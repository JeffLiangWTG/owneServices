using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDDDTCDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			Assert("Disclaim messages are not supported for DDTC.", true);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			invoiceLine.JI_Description = "ABCDEFG DESC";
			invoiceLine.US_DDTCLicenseType = DDTCLicenseTypeCodes.Codes.S73;
			invoiceLine.US_DDTCLicenseNo = "SG12323";
			invoiceLine.US_DDTCRegistrationNo = "RE3234";
			invoiceLine.US_DDTCExemptionCode = ACEDDTCExemptionCodes.Codes._12318A2;
			invoiceLine.US_DDTCArrivalDate = new ZDateTime(2016, 7, 22, 12, 30, 0);
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("ABCDEFG DESC", invoiceLineImported.JI_Description);
				AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_DDTCInd);
				AssertEquals(DDTCLicenseTypeCodes.Codes.S73, invoiceLineImported.US_DDTCLicenseType);
				AssertEquals("SG12323", invoiceLineImported.US_DDTCLicenseNo);
				AssertEquals("RE3234", invoiceLineImported.US_DDTCRegistrationNo);
				AssertEquals(ACEDDTCExemptionCodes.Codes._12318A2, invoiceLineImported.US_DDTCExemptionCode);
				AssertEquals(new ZDateTime(2016, 7, 22, 12, 30, 0), invoiceLineImported.US_DDTCArrivalDate);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
		}
	}
}
