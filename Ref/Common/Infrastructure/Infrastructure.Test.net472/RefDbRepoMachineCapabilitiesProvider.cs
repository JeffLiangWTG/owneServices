using System;
using System.Diagnostics;
using System.IO;
using Dat.Integration;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test.net472
{
	public class RefDbRepoMachineCapabilitiesProvider : IMachineCapabilitiesProvider
	{
		public int GetMachineCapabilities()
		{
			return CanConnectToOdbc();
		}

		static int CanConnectToOdbc()
		{
			var rootLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
			var directory = Path.GetDirectoryName(rootLocation);
			var odbcPath = GetODBCCapabilityCheckLocation(directory);

			var proccStartInfo = new ProcessStartInfo(odbcPath)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden
			};

			using (var process = new Process { StartInfo = proccStartInfo, EnableRaisingEvents = true })
			{
				process.Start();
				process.WaitForExit(120_000);
				return process.ExitCode == 0 ? (int)RefDbRepoMachineCapabilityRequirements.CanConnectToOdbc : 0;
			}
		}

		static string GetODBCCapabilityCheckLocation(string path)
		{
			var programExe = @"net8.0\CargoWise.RefDbRepo.Common.ODBCCapabilityCheck.exe";
			var result = Path.Combine(path, programExe);
			if (File.Exists(result))
			{
				return result;
			}
			throw new InvalidOperationException($"ODBCCapabilityCheck is not available on path {result}");
		}
	}
}
