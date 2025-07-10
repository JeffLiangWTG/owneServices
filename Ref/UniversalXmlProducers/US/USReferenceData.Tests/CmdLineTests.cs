using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	class CmdLineTests
	{
		[Test]
		public void TestEmptyArgument()
		{
			AssertArgumentError(new[] { string.Empty }, "No arguments entered.");
		}

		[Test]
		public void TestInvalidFunction()
		{
			AssertArgumentError(new[] { "NoFUNCTION" }, "Invalid argument entered: NOFUNCTION");
		}

		[Test]
		public void TestUnitedNationsStandardProductAndServiceCodesProgram()
		{
			var message = GetArgumentError(new[] { "UNSPC" });
			StringAssert.DoesNotContain("Invalid argument entered", message);
		}

		[Test]
		public void TestUSIncomingMessageQueryProgram()
		{
			var message = GetArgumentError(new[] { "USICMQ" });
			StringAssert.Contains("Invalid second argument. It should be F101 or F104 or F111.", message);

			message = GetArgumentError(new[] { "USICMQ", "F000" });
			StringAssert.Contains("Invalid second argument. It should be F101 or F104 or F111.", message);

			message = GetArgumentError(new[] { "USICMQ", "F101" });
			StringAssert.DoesNotContain("Invalid argument entered", message);
		}

		[Test]
		public void TestUSIncomingMessageDownloadProgram()
		{
			var message = GetArgumentError(new[] { "USICMD" });
			StringAssert.DoesNotContain("Invalid argument entered", message);
		}

		void AssertArgumentError(string[] parameters, string expectedErrorMessage)
		{
			var message = GetArgumentError(parameters);
			StringAssert.Contains(expectedErrorMessage, message);
		}

		string GetArgumentError(string[] parameters)
		{
			var parametersString = string.Join(" ", parameters);
			var processStartInfo = new ProcessStartInfo(Path.Combine(BinFilesPath, "CargoWise.RefDbRepo.USReferenceData.CmdLine.exe"), parametersString)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			processStartInfo.EnvironmentVariables.Add("RefDataRepoTesting", "Test");
			return ProcessRunner.RunProcess(processStartInfo, true);
		}

		string BinFilesPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
