using System.Diagnostics;
using System.IO;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace ReferenceDataUpdateService.Web.Test
{
	[TestFixture]
	public class NUnitJestTest
	{
		[Property("DAT:CapabilityRequirements", "NODEJS")]
		[Test]
		public void TestJestTests()
		{
			var rootPath = TestSourcePathHelper.DATTestSupplementaryContentPath;
			var referenceServiceWebPath = Path.Combine(rootPath, @"Web\ReferenceDataUpdateService.Web");

			var startInfo = new ProcessStartInfo("cmd.exe", "/C npm run test")
			{
				WorkingDirectory = referenceServiceWebPath,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			var errorMessage = ProcessRunner.RunProcess(startInfo, false, true);
			Assert.That(errorMessage, Is.Null.Or.Empty, errorMessage);
		}
	}
}
