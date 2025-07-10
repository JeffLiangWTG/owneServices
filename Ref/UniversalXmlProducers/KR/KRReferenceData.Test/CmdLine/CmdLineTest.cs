using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.XmlProducer.Common;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	class CmdLineTest
	{
		[Test]
		public void TestEmptyArgument()
		{
			AssertArgumentError(string.Empty, "No arguments entered.");
			AssertErrorStatus(string.Empty, ProducerStatus.Failure);
		}

		[Test]
		public void TestInvalidFunction()
		{
			AssertArgumentError("NoFUNCTION", "Invalid argument entered: NOFUNCTION");
			AssertErrorStatus("NoFUNCTION", ProducerStatus.Failure);
		}

		[Test]
		public void TestMissingConfigFile()
		{
			var jsonText = File.ReadAllText(TestHelper.ConfigJsonFilePath);
			try
			{
				var configDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
				configDictionary["OGAExportConfigFileInputPath"] = "";
				File.WriteAllText(TestHelper.ConfigJsonFilePath, JsonConvert.SerializeObject(configDictionary, Formatting.Indented));

				AssertArgumentError("OGAEXPORT", "No config file(.xml) exists.");
				AssertErrorStatus("OGAEXPORT", ProducerStatus.Failure);
			}
			finally
			{
				File.WriteAllText(TestHelper.ConfigJsonFilePath, jsonText);
			}
		}

		internal static void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var processStartInfo = new ProcessStartInfo(Path.Combine(binPath, "CargoWise.RefDbRepo.KRReferenceData.CmdLine.exe"), parameters)
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

		internal static void AssertErrorStatus(string parameters, ProducerStatus expectedErrorStatus)
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var processStartInfo = new ProcessStartInfo(Path.Combine(binPath, "CargoWise.RefDbRepo.KRReferenceData.CmdLine.exe"), parameters)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			processStartInfo.EnvironmentVariables.Add("RefDataRepoTesting", "Test");
			using (var process = new Process())
			{
				process.StartInfo = processStartInfo;
				Assert.That(process.Start(), Is.True, "Failed to start Process");
				if (!process.WaitForExit(60000))
				{
					try
					{
						process.Kill();
					}
					catch (InvalidOperationException e)
					{
						Assert.Fail(e.Message);
					}
				}
				process.WaitForExit();
				Assert.That((int)expectedErrorStatus == process.ExitCode, "When it failed, the process exit code to 1");
			}
		}
	}
}
