﻿using MySql.Data.MySqlClient;

namespace FloppyVPN
{
	internal static class DB
	{
		private static readonly string connectionString = $"Server={Config.Get("db_host")};Port={Config.Get("db_port")};Database={Config.Get("db_name")};User={Config.Get("db_user")};Password={Config.Get("db_password")};AllowPublicKeyRetrieval=true;";


		public static void Log(string sender, string message)
		{
			DateTime now = DateTime.Now;
			Console.WriteLine($"{now.ToDateTime()}  {sender}\t{message}");

			try
			{
				DB.Execute("INSERT INTO `logs` (`date_time`, `sender`, `message`) " +
					"VALUES (@date_time, @sender, @message);", 
					new Dictionary<string, object>()
					{
						{ "date_time", now },
						{ "sender", sender },
						{ "message", message }
					});
			}
			catch (Exception ex)
			{
				Console.WriteLine("Something unexpected happens - could not even write log into the DB!");
			}
		}


		public static void Execute(string query, Dictionary<string, object> parameters = null)
		{
			using (MySqlConnection connection = new MySqlConnection(connectionString))
			{
				connection.Open();

				using (MySqlCommand command = new MySqlCommand(query, connection))
				{
					if (parameters != null)
					{
						foreach (var param in parameters)
						{
							command.Parameters.AddWithValue(param.Key, param.Value);
						}
					}

					command.ExecuteNonQuery();
				}
			}
		}


		/// <summary>
		/// To be used with INSERT commands only.
		/// </summary>
		/// <returns>ID of the newly created record</returns>
		public static ulong InsertAndGetID(string query, Dictionary<string, object> parameters = null)
		{
			using (MySqlConnection connection = new MySqlConnection(connectionString))
			{
				connection.Open();

				using (MySqlCommand cmd = new MySqlCommand(query, connection))
				{
					if (parameters != null)
					{
						foreach (var param in parameters)
						{
							cmd.Parameters.AddWithValue(param.Key, param.Value);
						}
					}

					cmd.ExecuteNonQuery();
				}

				// Getting ID of last inserted row
				ulong lastInsertedId;
				using (MySqlCommand getIdCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", connection))
				{
					lastInsertedId = Convert.ToUInt64(getIdCmd.ExecuteScalar());
				}

				return lastInsertedId;
			}
		}



		public static object GetValue(string query, Dictionary<string, object> parameters = null)
		{
			using (MySqlConnection connection = new MySqlConnection(connectionString))
			{
				connection.Open();

				using (MySqlCommand command = new MySqlCommand(query, connection))
				{
					if (parameters != null)
					{
						foreach (var param in parameters)
						{
							command.Parameters.AddWithValue(param.Key, param.Value);
						}
					}

					return command.ExecuteScalar();
				}
			}
		}

		public static DataTable GetDataTable(string query, Dictionary<string, object> parameters = null)
		{
			DataTable dataTable = new();

			using (MySqlConnection connection = new(connectionString))
			using (MySqlCommand command = new(query, connection))
			{
				if (parameters != null)
				{
					foreach (var param in parameters)
					{
						command.Parameters.AddWithValue(param.Key, param.Value);
					}
				}

				using (MySqlDataAdapter adapter = new(command))
				{
					adapter.Fill(dataTable);
				}
			}

			return dataTable;
		}

		public static string[] FirstColumnAsArray(string query, Dictionary<string, object> parameters = null)
		{
			List<string> array = new();

			DataTable dt;
			if (parameters != null)
				dt = GetDataTable(query, parameters);
			else
				dt = GetDataTable(query);

			foreach (DataRow row in dt.Rows)
			{
				array.Add(row[0].ToString());
			}

			return array.ToArray();
		}
	}
}
