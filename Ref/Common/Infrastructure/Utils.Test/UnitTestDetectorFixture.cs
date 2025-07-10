using System;
using System.Diagnostics;
using System.IO;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class UnitTestDetectorFixture
	{
		[Test]
		public void IsRunningTests()
		{
			var isRunningTest = UnitTestDetector.IsRunningTests.Value;
			Assert.True(isRunningTest);

			var programExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestProgram.exe");
			var processStartInfo = new ProcessStartInfo(programExePath)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			processStartInfo.EnvironmentVariables.Add("RefDataRepoTesting", "Test");
			Assert.True(ProcessRunner.RunProcess(processStartInfo, true).Contains("Test"));
		}
	}
}
