using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.CmdLine.Tests
{
	sealed class CmdLineTests
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

		static void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var processStartInfo = new ProcessStartInfo(Path.Combine(binPath, "CargoWise.RefDbRepo.NOReferenceData.CmdLine.exe"), parameters)
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
	}
}
