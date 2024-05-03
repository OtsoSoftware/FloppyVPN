namespace FloppyVPN
{
	/// <summary>
	/// Does periodical jobs like keeping the system clean of unneeded data
	/// </summary>
	internal static class Worker
	{
		public static void Start()
		{
			Thread.Sleep(5000);
			for (; ; )
			{
				DeleteUnpaidAccounts();
				Provisioner.FlushConfigsOfDeletedAccounts();
				DeleteOldRequests();
				DeleteOldPayments();

				Thread.Sleep(5 * 1000 * 60); //minutes
			}
		}



		/// <summary>
		/// Deletes accounts that:
		/// a) Have their paid time over
		/// b) Are just registered but not paid during a day after registration
		/// </summary>
		static void DeleteUnpaidAccounts()
		{
			try
			{
				DB.Execute(@"
DELETE FROM accounts
WHERE	(NOW() > paid_till AND when_registered != paid_till)
OR		(when_registered = paid_till AND paid_till > NOW() + INTERVAL 1 DAY);"
				);
			}
			catch (Exception ex)
			{
				DB.Log("DeleteUnpaidAccounts()", ex.Message);
			}
		}

		/// <summary>
		/// Deletes requests used for karma that are actually not needed anymore
		/// </summary>
		static void DeleteOldRequests()
		{
			try
			{
				ushort olderThanDays = 14;
				DB.Execute($@"
	DELETE FROM `requests` 
	WHERE `date_time` < (NOW() - INTERVAL {olderThanDays} DAY);");
			}
			catch (Exception ex)
			{
				DB.Log("DeleteOldRequests()", ex.Message);
			}
		}

		/// <summary>
		/// Deletes old payments that are not needed anymore
		/// More likely to delete recent unpaid and expired payments
		/// while keeping older successful ones
		/// </summary>
		static void DeleteOldPayments()
		{
			try
			{
				ushort olderThanDays_unpaid = 1;
				ushort olderThanDays_paid = 3;
				DB.Execute($@"
	DELETE FROM `payments` 
	WHERE	((`to_be_paid_till` < (NOW() - INTERVAL {olderThanDays_unpaid} DAY)) AND (`is_paid` = 0)) 
	OR		((`when_created` < (NOW() - INTERVAL {olderThanDays_paid} DAY)) AND (`is_paid` = 1));");
			}
			catch (Exception ex)
			{
				DB.Log("DeleteOldPayments()", ex.Message);
			}
		}
	}
}