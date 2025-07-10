using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
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
			AssertArgumentError("NoFUNCTION", "Invalid argument entered: NOFUNCTION");
		}

		[Test]
		public void TestInValidArgumentsList()
		{
			AssertArgumentError("NEXDOCS TOO MANY ARGUMENTS",
				@"Invalid arguments entered; valid arguments are:	-LASTUPDATE=yyyy-MM-dd	-EXCLUDE:[functions]. Valid Code List Types are	COMMODITY_CUSCODELIST_ATTRIBUTE_NAME	COMMODITY_CUSCODETYPE	ECM_ADD_TEXT_FORMAT	ECM_ATTACHMENT_TYPE	ECM_CN_CODE	ECM_DECLARATION	ECM_DOCUMENT_TYPE	ECM_NATURE_OF_COMMODITY	ECM_PACK_TYPE	ECM_PACKAGE_TYPE	ECM_PERMIT_TYPE	ECM_PRESERVATION_TYPE	ECM_PRINT_REGION	ECM_PROCESS_CATEGORY	ECM_PRODUCT_CATEGORY	ECM_PRODUCT_CATEGORY_AHECC	ECM_PRODUCT_TYPE	ECM_SUPPLEMENTARY_CODE	ECM_TREATMENT	ECM_UNIT_OF_MEASUREMENT	ECM_CODES");
		}

		void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var processStartInfo = new ProcessStartInfo(Path.Combine(BinFilesPath, "CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe"), parameters)
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

		string BinFilesPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	}
}
