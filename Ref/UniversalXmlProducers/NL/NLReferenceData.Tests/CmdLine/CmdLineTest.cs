using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.CmdLine.Tests
{
	sealed class CmdLineTest
	{
		[Test]
		public void EmptyArgument()
		{
			AssertArgumentError(string.Empty, "No arguments entered.");
		}

		[Test]
		public void InvalidFunctionArgument()
		{
			AssertArgumentError("InvalidArgument", "Invalid argument: INVALIDARGUMENT");
		}

		static void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var processStartInfo = new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CargoWise.RefDbRepo.NLReferenceData.CmdLine.exe"), parameters)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			processStartInfo.EnvironmentVariables.Add("RefDataRepoTesting", "Test");
			var message = ProcessRunner.RunProcess(processStartInfo, false);
			Assert.That(message, Does.Contain(expectedErrorMessage));
		}
	}
}
