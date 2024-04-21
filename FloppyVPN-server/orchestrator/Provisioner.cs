﻿namespace FloppyVPN
{
	/// <summary>
	/// Provides accounts with VPN configs and removes them from unpaid ones.
	/// Deals with vpn servers.
	/// </summary>
	internal static class Provisioner
	{
		/// <summary>
		/// Periodically checks and updates statuses of vpn servers
		/// </summary>
		public static void VpnServersWatchdog()
		{
			for (; ; )
			{
				DataTable vpnServers = DB.GetDataTable("SELECT * FROM `vpn_servers`;");

				foreach (DataRow vpnServer in vpnServers.Rows)
				{
					string vpnServerReply = Communicator.GetHttp($"http://{vpnServer["socket"]}/CheckAvailability", 
						"", out _, out bool isSuccessful);
					
					if (isSuccessful && vpnServerReply.Contains(" OK "))
					{
						DB.Execute("UPDATE `vpn_servers` SET `last_alive` = @now WHERE `id` = @id;",
							new Dictionary<string, object>()
							{
								{ "@now", DateTime.Now },
								{ "@id", (ulong)vpnServer["id"] }
							});
					}
				}

				Thread.Sleep(100000);
			}
		}

		/// <summary>
		/// Deletes vpn configs that belong to accounts that don't exist anymore
		/// </summary>
		public static void FlushConfigsOfDeletedAccounts()
		{
			string[] configsWithNoOwner = DB.FirstColumnAsArray("SELECT `id` FROM `vpn_configs` " +
				"WHERE `account` NOT IN (SELECT `id` FROM `accounts`);");

			if (configsWithNoOwner.Length > 0)
			{
				foreach (string configWithNoOwner in configsWithNoOwner)
				{
					DeleteConfig(ulong.Parse(configWithNoOwner));
				}
			}
		}

		/// <summary>
		/// Creates a config in a wished country
		/// </summary>
		/// <returns>ID of an obtained/created config.</returns>
		public static ulong GetConfig(ulong account_id, string country_code, int device_type)
		{
			DataTable suchConfigsFound = DB.GetDataTable("SELECT * FROM `vpn_configs` " +
				"WHERE `account` = @account_id AND `device_type` = @device_type AND `country_code` = @country_code;",
				new Dictionary<string, object>()
				{
					{ "@account_id", account_id },
					{ "@device_type", device_type },
					{ "@country_code", country_code }
				});

			bool returnFoundConfig = suchConfigsFound.Rows.Count > 0 && new Random().Next(1, 3) == 2;

			if (returnFoundConfig) //in 1/2 cases return found config without creating a new one
			{
				return (ulong)suchConfigsFound.Rows[0]["id"];
			}
			else
			{
				//clean previous configs of this device type:
				DeleteConfigs(account_id, country_code, device_type);
				Thread.Sleep(100);
			}

			// Find the suitable server (which has available clients amount that we need):
			string? vpnServerID = (DB.GetValue($"SELECT vs.id, vs.socket, vs.country_code, vs.max_configs " +
				$"FROM vpn_servers vs " +
				$"LEFT JOIN (SELECT server, COUNT(DISTINCT account) AS num_configs " +
				$"FROM vpn_configs " +
				$"GROUP BY server) vc ON vs.id = vc.server " +
				$"WHERE vs.country_code = '{country_code}' " +
				$"AND (vc.num_configs IS NULL OR vc.num_configs < vs.max_configs);") ?? "").ToString();
			if (string.IsNullOrEmpty(vpnServerID))
			{
				DB.Log("CreateConfig()", $"Lacking vpn servers in country code {country_code}.");
				return 0;
			}

			//create vpn config on vpn server itself:
			DataRow vpnServerInfo = DB.GetDataTable($"SELECT * FROM `vpn_servers` WHERE `id` = {vpnServerID};").Rows[0];

			//first create record to get an ID:
			ulong newConfigID = DB.InsertAndGetID($"INSERT INTO `vpn_configs` " +
				$"(`server`, `config`, `account`, `device_type`) " +
				$"VALUES(@server_id, '', @account_id, @device_type);",
				new Dictionary<string, object>()
				{
					{ "@server_id", (ulong)vpnServerInfo["id"] },
					{ "@account_id", account_id },
					{ "@device_type", device_type }
				});

			string newConfig = Communicator.PostHttp($"http://{vpnServerInfo["socket"]}/CreateConfig",
				body: newConfigID.ToString().EncodeBody(),
				"", "", out _, out bool isSuccessful);

			if (isSuccessful)
			{
				//write the new vpn config into the database:
				DB.Execute($"UPDATE `vpn_configs` SET `config` = @config WHERE `id` = @id;",
					new Dictionary<string, object>()
					{
						{ "@id", newConfigID },
						{ "@config", newConfig }
					});

				return newConfigID;
			}
			else
			{
				DB.Execute($"DELETE FROM `vpn_configs` WHERE `id` = {newConfigID};");

				DB.Log("CreateConfig()", $"Couldn't create config. Server: {vpnServerID}. Response: {newConfig}.");
				return 0;
			}
		}

		/// <summary>
		/// Deletes all configs belonging to a certain account and, if specified, of a certain country code and device type.
		/// </summary>
		/// <param name="country_code">Country code to delete configs in. 
		/// If not specified, all configs in all country codes will be deleted.</param>
		/// <param name="device_type">Device type to delete configs for.
		/// If not specified, configs on any device types will be deleted.</param>
		private static void DeleteConfigs(ulong account_id, string country_code = "all", int device_type = 0)
		{
			string countryCodeParameter = country_code == "all" ? "" : $" AND `country_code` = '{country_code}'";
			string deviceTypeParameter = device_type == 0 ? "" : $" AND `device_type` = {device_type}";

			string[] vpnConfigsOfAccountToDelete = DB.FirstColumnAsArray($"SELECT `id` FROM `vpn_configs` " +
				$"WHERE `account` = {account_id}{countryCodeParameter}{deviceTypeParameter};");

			foreach (string vpnConfigToDelete in vpnConfigsOfAccountToDelete)
			{
				DeleteConfig(ulong.Parse(vpnConfigToDelete));
			}
		}

		/// <summary>
		/// Deletes a certain vpn config both from the vpn server itself and from the database
		/// </summary>
		private static void DeleteConfig(ulong configID)
		{
			DataRow configInfo = DB.GetDataTable($"SELECT * FROM `vpn_configs` WHERE `id` = {configID};").Rows[0];
			string vpnServerSocket = DB.GetValue($"SELECT `socket` FROM `vpn_servers` WHERE `id` = {configInfo["server"]};").ToString();
			
			//delete config on the vpn server
			for (byte b = 0; b < 3; b++) //multiple retries
			{
				bool isSuccessful = false;
				try
				{
					Communicator.PostHttp($"http://{vpnServerSocket}/DeleteConfig", 
						body: "configID".EncodeBody(), 
						"", "", out _, out isSuccessful);
				}
				catch
				{
				}

				if (isSuccessful)
					break;
				else
					Thread.Sleep(13000);
			}

			DB.Execute($"DELETE FROM `vpn_configs` WHERE `id` = {configID};");
		}

		/// <summary>
		/// Deletes a vpn server and its configs from the system.
		/// </summary>
		static void FlushVpnServer(ulong vpn_server_id)
		{
			DB.Execute($"DELETE FROM `vpn_configs` WHERE `server` = {vpn_server_id};");
			DB.Execute($"DELETE FROM `vpn_servers` WHERE `id` = {vpn_server_id};");
			DB.Execute($"DELETE FROM `vpn_configs` WHERE `server` = {vpn_server_id};");
		}
	}
}