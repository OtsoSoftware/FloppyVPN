namespace FloppyVPN
{
    internal static class Program
    {
		static void Main()
        {
			//single instance:
			Mutex mutex = new(true, "floppyvpn_vpn_L2Neli82o5I8P1hsqVtOHoq67htydhyth", out bool result);
			if (!result)
			{
				Console.WriteLine("The same server is already running so it won't start again.");
				Environment.Exit(0);
			}
			GC.KeepAlive(mutex);

			Config.EnsureFileIntegrity();

			new Thread(() => Config.CacheRefresher()).Start();

			Vpn.CreateServerConfigIfNotYet();

			Vpn.DownInterface();
			Thread.Sleep(1000);
			Vpn.UpInterface();

			new Thread(() => Listener.Start()).Start();

			//action on ctrl+c keypress:
			Console.CancelKeyPress += delegate {
				Vpn.DownInterface();
				Task.Delay(777).GetAwaiter().GetResult();
				Environment.Exit(0);
			};
		}
	}
}