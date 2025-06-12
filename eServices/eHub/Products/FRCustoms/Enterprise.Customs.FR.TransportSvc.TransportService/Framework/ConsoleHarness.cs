using System;
using System.Globalization;

namespace Enterprise.Customs.FR.TransportSvc.TransportService.Framework
{
	public static class ConsoleHarness
	{
		// Run a service from the console given a service implementation
		public static void Run(string[] args, IWindowsService service)
		{
			var serviceName = service.GetType().Name;
			var isRunning = true;

			// simulate starting the windows service
			service.OnStart(args);

			// let it run as long as Q is not pressed
			while (isRunning)
			{
				WriteToConsole(ConsoleColor.Yellow, "Enter either [Q]uit, [P]ause, [R]esume : ");
				isRunning = HandleConsoleInput(service, Console.ReadLine());
			}

			// stop and shutdown
			service.OnStop();
			service.OnShutdown();
		}

		// Private input handler for console commands.
		static bool HandleConsoleInput(IWindowsService service, string line)
		{
			var canContinue = true;

			// check input
			if (line != null)
			{
				switch (line.ToUpper(CultureInfo.InvariantCulture))
				{
					case "Q":
						WriteToConsole(ConsoleColor.Green, "Service is shutting down...");
						canContinue = false;
						break;

					case "P":
						WriteToConsole(ConsoleColor.Green, "Service is pausing...");
						service.OnPause();
						WriteToConsole(ConsoleColor.Green, "Service paused.");
						break;

					case "R":
						WriteToConsole(ConsoleColor.Red, "Warning: changes done to the configuration don't apply when the service is resumed. Chose [Q]uit and restart the service if the configuration changed.");
						service.OnResume();
						WriteToConsole(ConsoleColor.Green, "Service resumed.");
						break;

					default:
						WriteToConsole(ConsoleColor.Red, "Did not understand that input, try again.");
						break;
				}
			}

			return canContinue;
		}

		// Helper method to write a message to the console at the given foreground color.
		internal static void WriteToConsole(ConsoleColor foregroundColor, string format, params object[] formatArguments)
		{
			var originalColor = Console.ForegroundColor;
			Console.ForegroundColor = foregroundColor;

			Console.WriteLine(format, formatArguments);

			Console.Out.Flush();

			Console.ForegroundColor = originalColor;
		}
	}
}
