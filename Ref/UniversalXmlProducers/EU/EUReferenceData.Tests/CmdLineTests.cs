using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	class CmdLineTests
	{
		[Test]
		public void TestEmptyArgument()
		{
			AssertArgumentError(string.Empty, "No arguments entered.");
		}

		[Test]
		public void TestInvalidFunction()
		{
			AssertArgumentError("NoFUNCTION", "Invalid argument entered: NOFUNCTION");
		}

		void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var processStartInfo = new ProcessStartInfo(Path.Combine(BinFilesPath, "CargoWise.RefDbRepo.EUReferenceData.CmdLine.exe"), parameters)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			processStartInfo.EnvironmentVariables.Add("RefDataRepoTesting", "Test");
			var message = ProcessRunner.RunProcess(processStartInfo, false);
			StringAssert.Contains(expectedErrorMessage, message);
		}

		string BinFilesPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
