using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.CmdLine
{
	[TestFixture]
	public class ProgramTests
	{
		[Test]
		public void NoArguments()
		{
			var message = GetProcessOutput(string.Empty);
			Assert.That(message, Contains.Substring("No arguments entered."));
		}

		[Test]
		public void InvalidFunction()
		{
			var message = GetProcessOutput("M.T.F.B.W.Y.");
			Assert.That(message, Contains.Substring("Function not supported: M.T.F.B.W.Y."));
		}

		string GetProcessOutput(string parameters)
		{
			var processStartInfo = new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CargoWise.RefDbRepo.ZAReferenceData.CmdLine.exe"), parameters)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			processStartInfo.EnvironmentVariables.Add("RefDataRepoTesting", "Test");
			var message = ProcessRunner.RunProcess(processStartInfo, false);

			return message;
		}
	}
}
