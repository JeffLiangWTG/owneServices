using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor.Test
{
	[TestFixture]
	class ProgramTest
	{
		[Test]
		public void TestEmptyArgument()
		{
			AssertArgumentError(string.Empty, "Missing or invalid arguments; please specify the country to run against.");
		}

		[Test]
		public void TestInvalidParameter1()
		{
			AssertArgumentError("X!", "There is no CLASET Processor support for 'X!'.");
		}

		[Test]
		public void TestInvalidParameter2()
		{
			AssertArgumentError("HELLO WORLD", "Missing or invalid arguments; please specify the country to run against.");
		}

		void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var messageBuilder = new StringBuilder();
			var processStartInfo = new ProcessStartInfo(Path.Combine(BinFilesPath, "CargoWise.RefDbRepo.Staging.CLASETMessageProcessor.exe"), parameters)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			Process process = null;
			try
			{
				process = new Process
				{
					StartInfo = processStartInfo
				};
				process.ErrorDataReceived += (sender, eventArgs) => messageBuilder.AppendLine(eventArgs.Data);
				process.Start();
				process.BeginErrorReadLine();
				process.WaitForExit();
			}
			finally
			{
				if (process != null)
				{
					process.Dispose();
				}
			}

			StringAssert.Contains(expectedErrorMessage, messageBuilder.ToString());
		}

		string BinFilesPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
