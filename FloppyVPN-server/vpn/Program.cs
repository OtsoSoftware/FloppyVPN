namespace FloppyVPN
{
    internal static class Program
    {
		internal static string masterKey = ""; //vpn servers do NOT store master key in config file because it would add some risk.

		static void Main()
        {
			//single instance:
			Mutex mutex = new(true, "floppyvpn_vpn_L2Neli82o5I8P1hsqVtOHoq67htydhythtdeyj8AuWYE", out bool result);
			if (!result)
			{
				Console.WriteLine("The same server is already running so it won't start again.");
				Environment.Exit(0);
			}
			GC.KeepAlive(mutex);

			Config.EnsureFileIntegrity();

			//ask for master key (yep, on every launch :)
			Console.Write("Enter the master key: ");
			while (true)
			{
				var key = System.Console.ReadKey(true);
				if (key.Key == ConsoleKey.Enter)
					break;
				masterKey += key.KeyChar;
			}
			masterKey = masterKey.Replace("\n", "").Replace(" ", "");

			Vpn.GenerateServerConfigIfNotYet();

			new Thread(() => Config.CacheRefresher()).Start();
			new Thread(() => Listener.Start()).Start();
        }
    }
}