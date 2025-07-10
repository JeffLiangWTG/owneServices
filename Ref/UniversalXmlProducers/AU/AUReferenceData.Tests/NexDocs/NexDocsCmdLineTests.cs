using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	[TestFixture]
	class NexDocsCmdLineTests
	{
		[Test]
		public void TestInvalidArgument()
		{
			AssertArgumentError("NexDocs INVALID ARGUMENT",
				@"Invalid arguments entered; valid arguments are:	-LASTUPDATE=yyyy-MM-dd	-EXCLUDE:[functions]. Valid Code List Types are	COMMODITY_CUSCODELIST_ATTRIBUTE_NAME	COMMODITY_CUSCODETYPE	ECM_ADD_TEXT_FORMAT	ECM_ATTACHMENT_TYPE	ECM_CN_CODE	ECM_DECLARATION	ECM_DOCUMENT_TYPE	ECM_NATURE_OF_COMMODITY	ECM_PACK_TYPE	ECM_PACKAGE_TYPE	ECM_PERMIT_TYPE	ECM_PRESERVATION_TYPE	ECM_PRINT_REGION	ECM_PROCESS_CATEGORY	ECM_PRODUCT_CATEGORY	ECM_PRODUCT_CATEGORY_AHECC	ECM_PRODUCT_TYPE	ECM_SUPPLEMENTARY_CODE	ECM_TREATMENT	ECM_UNIT_OF_MEASUREMENT	ECM_CODES");
		}

		[Test]
		public void TestInvalidCodeListType()
		{
			AssertArgumentError("NexDocs -EXCLUDE:NOTHING", "Invalid Code List Types to exclude entered: NOTHING");
		}

		[Test]
		public void TestTooManyArguments()
		{
			AssertArgumentError("NexDocs -EXCLUDE:ECM_SUPPLEMENTARY_CODE WOOPS",
				@"Invalid arguments entered; valid arguments are:	-LASTUPDATE=yyyy-MM-dd	-EXCLUDE:[functions]. Valid Code List Types are	COMMODITY_CUSCODELIST_ATTRIBUTE_NAME	COMMODITY_CUSCODETYPE	ECM_ADD_TEXT_FORMAT	ECM_ATTACHMENT_TYPE	ECM_CN_CODE	ECM_DECLARATION	ECM_DOCUMENT_TYPE	ECM_NATURE_OF_COMMODITY	ECM_PACK_TYPE	ECM_PACKAGE_TYPE	ECM_PERMIT_TYPE	ECM_PRESERVATION_TYPE	ECM_PRINT_REGION	ECM_PROCESS_CATEGORY	ECM_PRODUCT_CATEGORY	ECM_PRODUCT_CATEGORY_AHECC	ECM_PRODUCT_TYPE	ECM_SUPPLEMENTARY_CODE	ECM_TREATMENT	ECM_UNIT_OF_MEASUREMENT	ECM_CODES");
		}

		[Test]
		public void TestInValidArgumentWithValidLastUpdatedArgument()
		{
			AssertArgumentError("NEXDOCS -LASTUPDATE=2019-01-01 INVALID ARGUMENT",
				@"Invalid arguments entered; valid arguments are:	-LASTUPDATE=yyyy-MM-dd	-EXCLUDE:[functions]. Valid Code List Types are	COMMODITY_CUSCODELIST_ATTRIBUTE_NAME	COMMODITY_CUSCODETYPE	ECM_ADD_TEXT_FORMAT	ECM_ATTACHMENT_TYPE	ECM_CN_CODE	ECM_DECLARATION	ECM_DOCUMENT_TYPE	ECM_NATURE_OF_COMMODITY	ECM_PACK_TYPE	ECM_PACKAGE_TYPE	ECM_PERMIT_TYPE	ECM_PRESERVATION_TYPE	ECM_PRINT_REGION	ECM_PROCESS_CATEGORY	ECM_PRODUCT_CATEGORY	ECM_PRODUCT_CATEGORY_AHECC	ECM_PRODUCT_TYPE	ECM_SUPPLEMENTARY_CODE	ECM_TREATMENT	ECM_UNIT_OF_MEASUREMENT	ECM_CODES");
		}

		[Test]
		public void TestInValidArgumentWithValidExcludeArgument()
		{
			AssertArgumentError("NEXDOCS -EXCLUDE:ECM_DOCUMENT_TYPE ARGUMENTS",
				@"Invalid arguments entered; valid arguments are:	-LASTUPDATE=yyyy-MM-dd	-EXCLUDE:[functions]. Valid Code List Types are	COMMODITY_CUSCODELIST_ATTRIBUTE_NAME	COMMODITY_CUSCODETYPE	ECM_ADD_TEXT_FORMAT	ECM_ATTACHMENT_TYPE	ECM_CN_CODE	ECM_DECLARATION	ECM_DOCUMENT_TYPE	ECM_NATURE_OF_COMMODITY	ECM_PACK_TYPE	ECM_PACKAGE_TYPE	ECM_PERMIT_TYPE	ECM_PRESERVATION_TYPE	ECM_PRINT_REGION	ECM_PROCESS_CATEGORY	ECM_PRODUCT_CATEGORY	ECM_PRODUCT_CATEGORY_AHECC	ECM_PRODUCT_TYPE	ECM_SUPPLEMENTARY_CODE	ECM_TREATMENT	ECM_UNIT_OF_MEASUREMENT	ECM_CODES");
		}

		[Test]
		public void TestInValidExcludeArgumentsList()
		{
			AssertArgumentError("NEXDOCS -EXCLUDE:ABC", "Invalid Code List Types to exclude entered: ABC");
		}

		[Test]
		public void TestInValidExcludeArgumentEmptyList()
		{
			AssertArgumentError("NEXDOCS -EXCLUDE:", "Invalid Code List Types to exclude entered: ");
		}

		[Test]
		public void TestInValidLastUpdatedArgument()
		{
			AssertArgumentError("NEXDOCS -LASTUPDATE=ABC", "Invalid LastUpdated argument format. Valid argument should be in this format: -LASTUPDATE=yyyy-MM-dd");
		}

		[Test]
		public void TestInValidLastUpdatedArgumentEmpty()
		{
			AssertArgumentError("NEXDOCS -LASTUPDATE=15-09-2023", "Invalid LastUpdated argument format. Valid argument should be in this format: -LASTUPDATE=yyyy-MM-dd");
		}

		void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var processStartInfo = new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe"), parameters)
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
	}
}
