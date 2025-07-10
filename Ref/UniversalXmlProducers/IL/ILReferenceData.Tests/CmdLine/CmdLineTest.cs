using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.CmdLine
{
	[TestFixture]
	sealed class CmdLineTest
	{
		[Test]
		public void TestEmptyArgument()
		{
			AssertArgumentError(false, string.Empty, "No command line argument found.");
		}

		[Test]
		public void TestInvalidFunctionToRun()
		{
			AssertArgumentError(false, "BAD_PROGRAM_FUNCTION", "Invalid Function To Run.");
		}

		[Test]
		public void TestInvalidFunctionToRun_TableUpdateWithoutTableName()
		{
			AssertArgumentError(true, "CUSTOMS_TABLE_UPDATE", "Bad Command Line Arguments");
		}

		[Test]
		public void TestInvalidFunctionToRun_ExchangeRatesWithoutDates()
		{
			const string expectedMessageError = @"Invalid arguments.	Here's the correct format you should use:	CUSTOMS_EXCHANGE_RATE_UPDATE	Please note:	- Use this command to update all exchange rates for today and for the upcoming week.";

			AssertArgumentError(true, "CUSTOMS_EXCHANGE_RATE_UPDATE invalid", expectedMessageError);
		}

		[Test]
		public void TestInvalidFunctionToRun_CustomsTariffUpdateWithoutInputFolder()
		{
			AssertArgumentError(true, "CUSTOMS_TARIFF_UPDATE", "Invalid command line arguments. Path to the folder with Customs Tariff files not provided.");
		}

		void AssertArgumentError(bool combineOutputError, string parameters, string expectedErrorMessage)
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var processStartInfo = new ProcessStartInfo(Path.Combine(binPath, "CargoWise.RefDbRepo.ILReferenceData.CmdLine.exe"), parameters)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			processStartInfo.EnvironmentVariables.Add("RefDataRepoTesting", "Test");
			var message = ProcessRunner.RunProcess(processStartInfo, combineOutputError);
			Assert.That(message.Replace("\r\n", ""), Does.Contain(expectedErrorMessage.Replace("\r\n", "")));
		}
	}
}
