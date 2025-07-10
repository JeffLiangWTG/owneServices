using System;
using System.Diagnostics;
using System.Linq;

namespace TestConsoleApp
{
	class Program
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Test Code")]
		static void Main(string[] args)
		{
			Console.ReadLine(); // allow the test to add this process to the job object
			if (args.Length > 0)
			{
				Process.Start(args[0]);
			}
			Console.ReadLine();
		}
	}
}
