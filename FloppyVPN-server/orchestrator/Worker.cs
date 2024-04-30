namespace FloppyVPN
{
	/// <summary>
	/// Does periodical jobs like deleting data which is not needed anymore at all
	/// </summary>
	internal static class Worker
	{
		public static void Start()
		{
			Thread.Sleep(5000);
			for (; ; )
			{
				DeleteUnpaidAccounts();
				DeleteOldRequests();
				Provisioner.FlushConfigsOfDeletedAccounts();
				DeleteOldPayments();

				Thread.Sleep(15 * 1000 * 60); //minutes
			}
		}

		static void DeleteUnpaidAccounts()
		{

		}

		static void DeleteOldRequests()
		{

		}

		static void DeleteOldPayments()
		{

		}
	}
}