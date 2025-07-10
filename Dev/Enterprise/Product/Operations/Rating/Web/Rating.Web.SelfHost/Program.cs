using System;
using Enterprise.Rating.Web.Configuration;
using Microsoft.Owin.Hosting;

namespace Enterprise.Rating.Web.SelfHost
{
	class Program
	{
		#region SuppressResourceStringsCheckRegion
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Console I/O of a console-based application.")]
		static void Main()
		{
			//Run from CMD: netsh http add urlacl url="http://+:7800/" user=everyone
			Console.Write("Starting Rating APIs Service ... ");
			var port = 7800;
			using (WebApp.Start($"http://+:{port}", new RatingAPIsStartup().Configuration))
			{
				Console.WriteLine("OK");
				Console.WriteLine("Service is listening at port \"{0}\"", port);
				Console.WriteLine();
				Console.WriteLine($"The API documentation is available here: http://localhost:{port}/swagger");
				Console.WriteLine();
				Console.WriteLine("Press any key to stop the service ...");
				Console.ReadKey();
			}
		}
		#endregion
	}
}
