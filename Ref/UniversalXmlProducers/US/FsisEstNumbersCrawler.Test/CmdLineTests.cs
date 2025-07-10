using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace FsisEstNumbersCrawler.Test
{
	[TestFixture]
	public class CmdLineTests
	{
		[Test]
		public void TestFsisEstNumbersCrawlerProgram()
		{
			var processStartInfo = new ProcessStartInfo(Path.Combine(BinFilesPath, "CargoWise.RefDbRepo.FsisEstNumbersCrawler.exe"))
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			var error = ProcessRunner.RunProcess(processStartInfo, true);

			Assert.That(error, Does.Contain(@"The out put file path is ..\..\UxmlFiles\US_FSISEstNumbers.xml"));
			Assert.That(error, Does.Contain(@"Start searching the data file node in the web page..."));
		}

		string BinFilesPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
