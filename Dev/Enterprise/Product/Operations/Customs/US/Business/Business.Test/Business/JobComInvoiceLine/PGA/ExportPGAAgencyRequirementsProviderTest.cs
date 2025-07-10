using System.Linq;
using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	public class ExportPGAAgencyRequirementsProviderTest : TestCaseWithFactory
	{
		public void TestExportPGAAgencyRequirements()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var provider = new ExportPGAInvoiceLineRequirementsProvider(invoiceLine);
			var agencyProgramCodes = provider.GetGovernmentAgencyProgramCodeList();
			AssertEquals("AMS, EPA, NMFS, ATF, DEA, FWS, TTB are allowed for Export currently", 7, agencyProgramCodes.Count);

			foreach (var agencyProgram in agencyProgramCodes.OfType<CodeDescriptionPairList>())
			{
				var programCode = agencyProgramCodes.CodesAsString;
				AssertNotNull("Indicator property for " + programCode + " shouldn't be null in Export PGA.", provider.GetIndicatorInfo(programCode));
				AssertNull("Disclaim reason is not supported in Export PGA. ", provider.GetDisclaimReasonInfo(programCode));
			}
			Assert("Export TTB Can Disclaim", provider.CanDisclaim(GovernmentAgencyProgramCodeList.Codes.TTB));
			Assert("Export DEA Can Disclaim", provider.CanDisclaim(GovernmentAgencyProgramCodeList.Codes.DEA));
			Assert("Export EPA Can Disclaim", provider.CanDisclaim(GovernmentAgencyProgramCodeList.Codes.EPA));
		}

		public void TestExportProductPGAAgencyRequirements()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			var provider = new ExportPGAProductRequirementsProvider(pivot);
			var agencyProgramCodes = provider.GetGovernmentAgencyProgramCodeList();
			AssertEquals("AMS, EPA, NMFS, ATF, DEA, FWS, TTB are allowed for Export on product currently", 7, agencyProgramCodes.Count);

			foreach (var agencyProgram in agencyProgramCodes.OfType<CodeDescriptionPairList>())
			{
				var programCode = agencyProgramCodes.CodesAsString;
				AssertNotNull("Indicator property for " + programCode + " shouldn't be null in Export PGA on product.", provider.GetIndicatorInfo(programCode));
				AssertNull("Disclaim reason is not supported in Export PGA on product. ", provider.GetDisclaimReasonInfo(programCode));
			}
			Assert("Export TTB Can Disclaim", provider.CanDisclaim(GovernmentAgencyProgramCodeList.Codes.TTB));
			Assert("Export DEA Can Disclaim", provider.CanDisclaim(GovernmentAgencyProgramCodeList.Codes.DEA));
			Assert("Export EPA Can Disclaim", provider.CanDisclaim(GovernmentAgencyProgramCodeList.Codes.EPA));
		}
	}
}
