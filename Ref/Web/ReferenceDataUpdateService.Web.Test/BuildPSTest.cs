using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace ReferenceDataUpdateService.Web.Test
{
	[TestFixture]
	public class BuildPSTest
	{
		const string CommandFromPs1StartPattern = "& ";

		[Test]
		public void BuildExitWhenErrorIsFound()
		{
			var rootPath = TestSourcePathHelper.DATTestSupplementaryContentPath;
			var directoryPath = Path.Combine(rootPath, @"Web\ReferenceDataUpdateService.Web");
			var buildPs1File = Directory.GetFiles(directoryPath).FirstOrDefault(x => x.EndsWith("build.ps1"));

			if (!File.Exists(buildPs1File))
			{
				Assert.Fail($"build.ps1 file does not exist in the directory {directoryPath} or has been renamed");
			}

			var buildPs1FileContentAsArray = File.ReadAllLines(buildPs1File);
			var shouldCheckNextLine = false;
			foreach (var line in buildPs1FileContentAsArray)
			{
				if (shouldCheckNextLine && !string.IsNullOrEmpty(line.Trim()))
				{
					Assert.That(line, Does.StartWith("if ($lastExitCode -ne 0)"), "After execution of commands in PS, we should check for the $lastExitCode not being equal to 0");
					Assert.That(line, Does.Contain("exit 1"), "We must exit with code 1 to catch the error on build.ps1 caller");
					shouldCheckNextLine = false;
				}
				else if (line.StartsWith(CommandFromPs1StartPattern))
				{
					shouldCheckNextLine = true;
				}
			}
		}
	}
}
