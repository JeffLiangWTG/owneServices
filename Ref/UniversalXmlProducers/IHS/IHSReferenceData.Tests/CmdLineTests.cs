using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IHSReferenceData.Tests
{
	[TestFixture]
	class CmdLineTests
	{
		[Test]
		public void EmptyArgument()
		{
			AssertArgumentError(string.Empty, "No arguments entered.");
		}

		[Test]
		public void InvalidFunction()
		{
			AssertArgumentError("nothing", "Invalid argument entered: NOTHING");
		}

		internal static void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var processStartInfo = new ProcessStartInfo(Path.Combine(binPath, "CargoWise.RefDbRepo.IHSReferenceData.CmdLine.exe"), parameters)
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
