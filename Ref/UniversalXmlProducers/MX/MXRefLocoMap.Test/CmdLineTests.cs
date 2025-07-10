using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.MXRefLocoMap.Business.Test
{
	[TestFixture]
	public class CmdLineTests
	{
		[Test]
		public void TestFsisEstNumbersCrawlerProgram()
		{
			var processStartInfo = new ProcessStartInfo(Path.Combine(BinFilesPath, "CargoWise.RefDbRepo.MXRefLocoMap.exe"))
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			var error = ProcessRunner.RunProcess(processStartInfo, true);

			Assert.That(error, Does.Contain(@"The out put file path is ..\UxmlFiles\RefLocoMap.xml"));
			Assert.That(error, Does.Contain(@"Start read excel file..."));
		}

		string BinFilesPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
