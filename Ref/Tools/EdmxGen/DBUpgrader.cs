using System;
using System.Diagnostics;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Tools.Common;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public static class DBUpgrader
	{
		const string DefaultPathStagingUpgrader = @"..\..\..\bin\Staging\net8.0\CargoWise.RefDbRepo.Staging.DbUpgrader.exe";
		const string DefaultPathSafeUpgrader = @"..\..\..\bin\Server\net8.0\CargoWise.RefDbRepo.Service.UpgradeManagerRunner.exe";

		public static void UpgradeDatabase(bool safeOnly, bool stagingOnly)
		{
			if (!safeOnly && !stagingOnly)
			{
				RunSafe();
				RunStaging();
			}
			else if (safeOnly)
			{
				RunSafe();
			}
			else
			{
				RunStaging();
			}
		}

		static void RunStaging()
		{
			Console.WriteLine("Initialising Staging upgrade....");
			var stagingConnString = Application.StagingConnectionString;

			DBHelper.DropAndCreateNewDatabase(stagingConnString);

			var procStartInfo = new ProcessStartInfo(DefaultPathStagingUpgrader, $"{DBHelper.GetDbName(stagingConnString)} -force")
			{
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden
			};
			WinProcessor.RunProcess(procStartInfo);

			Console.WriteLine("Staging has been upgraded.");
		}

		static void RunSafe()
		{
			Console.WriteLine("Initialising Safe upgrade....");
			var safeConnString = Application.SafeConnectionString;

			DBHelper.DropAndCreateNewDatabase(safeConnString);

			var procStartInfo = new ProcessStartInfo(DefaultPathSafeUpgrader, $"{DBHelper.GetDbName(safeConnString)} -force")
			{
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden
			};
			WinProcessor.RunProcess(procStartInfo);

			Console.WriteLine("Safe has been upgraded");
		}
	}
}
