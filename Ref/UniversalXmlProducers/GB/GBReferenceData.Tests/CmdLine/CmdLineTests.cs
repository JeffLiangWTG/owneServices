using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.GBReferenceData.Business;
using CargoWise.RefDbRepo.GBReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests
{
	[TestFixture]
	class CmdLineTests
	{
		[Test]
		public void TestEmptyArgument()
		{
			AssertArgumentError(string.Empty, "No arguments entered.");
		}

		[Test]
		public void TestInvalidFunction()
		{
			AssertArgumentError("DoSomeNuclearPhysics", "Invalid argument: DOSOMENUCLEARPHYSICS");
		}

		void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var processStartInfo = new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CargoWise.RefDbRepo.GBReferenceData.CmdLine.exe"), parameters)
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

		[TestCase(Constants.ProgramFunctions.CDSStandingData, "CargoWise.RefDbRepo.GBReferenceData.CmdLine.CdsStandingDataProgram.Run")]
		[TestCase(Constants.ProgramFunctions.ChiefHarmonisedDeclarationCode, "CargoWise.RefDbRepo.GBReferenceData.CmdLine.ChiefHarmonisedDeclarationCodeProgram.Run")]
		[TestCase(Constants.ProgramFunctions.CDSTariffData, "CargoWise.RefDbRepo.GBReferenceData.CmdLine.CDSTariffDataProgram.Run")]
		[TestCase(Constants.ProgramFunctions.GvmsReferenceData, "CargoWise.RefDbRepo.GBReferenceData.CmdLine.GVMSReferenceDataProgram.Run")]
		[TestCase(Constants.ProgramFunctions.CDSPortData, "CargoWise.RefDbRepo.GBReferenceData.CmdLine.CDSPortDataProgram.Run")]
		[TestCase(Constants.ProgramFunctions.ExchangeRates, "CargoWise.RefDbRepo.GBReferenceData.CmdLine.ExchangeRateProgram.Run")]
		[TestCase(Constants.ProgramFunctions.CDSProcedureData, "CargoWise.RefDbRepo.GBReferenceData.CmdLine.CDSProcedureDataProgram.Run")]
		[TestCase(Constants.ProgramFunctions.UKOfficeCodes, "CargoWise.RefDbRepo.GBReferenceData.CmdLine.UKOfficeCodesProgram.Run")]
		public void GetProgramFunction(string funcCode, string expectedFunction)
		{
			var func = Program.GetProgramFunction(funcCode);
			Assert.NotNull(func);
			Assert.That($"{func.Method.DeclaringType.FullName}.{func.Method.Name}", Is.EqualTo(expectedFunction));
		}
	}
}
