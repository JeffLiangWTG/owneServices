using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.CHReferenceData.CmdLine;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests
{
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
			AssertArgumentError("NoFUNCTION", "Invalid argument entered: NOFUNCTION");
		}

		[TestCase("EXCHANGERATES", TestName = "CommandValidEXCHANGERATES")]
		[TestCase("TRADEGROUPS", TestName = "CommandValidTRADEGROUPS")]
		[TestCase("CODELISTS", TestName = "CommandValidCODELISTS")]
		[TestCase("CUSTOMSOFFICES", TestName = "CommandValidCUSTOMSOFFICES")]
		[TestCase("NOMENCLATUREGROUPS", TestName = "CommandValidNOMENCLATUREGROUPS")]
		[TestCase("EXPORTTARIFFS", TestName = "CommandValidEXPORTTARIFFS")]
		[TestCase("IMPORTTARIFFS", TestName = "CommandValidIMPORTTARIFFS")]
		[TestCase("PERMITITEMDETAILS", TestName = "CommandValidPERMITITEMDETAILS")]
		[TestCase("ADDITIONALTAXESTARIFFS", TestName = "CommandValidADDITIONALTAXESTARIFFS")]
		[TestCase("RATECODES", TestName = "CommandValidRATECODES")]
		[TestCase("PASSARCODELISTS", TestName = "CommandValidPASSARCODELISTS")]
		public void CommandValid(string programFunction)
		{
			Assert.That(Program.TryGetProgramFunction(programFunction), Is.Not.Null);
		}

		[TestCase("TEST ERROR OUTPUT", "TEST ERROR OUTPUT\r\n", TestName = "PrintErrorMessageWithContent")]
		[TestCase("", "", TestName = "PrintErrorMessageEmptyString")]
		[TestCase(null, "", TestName = "PrintErrorMessageNull")]
		public void PrintErrorMessage(string input, string expectedOutput)
		{
			using (var stringWriter = new StringWriter())
			using (new SavedConsoleStreams())
			{
				Console.SetOut(stringWriter);
				Console.SetError(stringWriter);
				Program.PrintErrorMessage(input);
				Assert.That(stringWriter.ToString(), Is.EqualTo(expectedOutput));
			}
		}

		static void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var processStartInfo = new ProcessStartInfo(Path.Combine(binPath, "CargoWise.RefDbRepo.CHReferenceData.CmdLine.exe"), parameters)
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

		class SavedConsoleStreams : IDisposable
		{
			TextWriter savedOut;
			TextWriter savedErr;

			internal SavedConsoleStreams()
			{
				savedOut = Console.Out;
				savedErr = Console.Error;
			}

			public void Dispose()
			{
				Console.SetOut(savedOut);
				Console.SetError(savedErr);
			}
		}
	}
}
