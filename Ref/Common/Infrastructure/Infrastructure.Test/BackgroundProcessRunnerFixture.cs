using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[TestFixture]
	public class BackgroundProcessRunnerFixture
	{
		[Test]
		public void ShouldStartTheProcessInBackground()
		{
			var programExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestProgram.exe");
			var processStartInfo = new ProcessStartInfo(programExePath)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			processStartInfo.EnvironmentVariables.Add("RefDataRepoTesting", "Test");

			var backgroundProcessRunner = new BackgroundProcessRunner(processStartInfo);
			backgroundProcessRunner.Run();
			Thread.Sleep(5000);

			Assert.That(backgroundProcessRunner.OutputLog.ToString(), Contains.Substring("Test"));
		}

		[Test]
		public void ShouldKillTheProcessAutomaticallyAfterTimeout()
		{
			var programExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestProgram.exe");
			var processStartInfo = new ProcessStartInfo(programExePath)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			processStartInfo.EnvironmentVariables.Add("RefDataRepoTesting", "Test");

			var backgroundProcessRunner = new BackgroundProcessRunner(processStartInfo, 5000);
			backgroundProcessRunner.Run();
			Thread.Sleep(7000);

			Assert.Throws<InvalidOperationException>(
				() => backgroundProcessRunner.Process.Kill(), "No process is associated with this object."
			);
		}
	}
}
