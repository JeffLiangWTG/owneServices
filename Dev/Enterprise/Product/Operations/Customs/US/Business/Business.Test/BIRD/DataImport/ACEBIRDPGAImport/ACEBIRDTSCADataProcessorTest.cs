using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDTSCADataProcessorTest : ACEBIRDODSOrTSCADataProcessor
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_TSCADisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Description = "TEST TSCA DISCLAIMED";
			Factory.Save();

			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_TSCAInd);
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_TSCADisclaimReason);
			AssertEquals("TEST TSCA DISCLAIMED", invoiceLineImported.JI_Description);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			invoiceLine.JI_Description = "TEST TSCA DESC";
			invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;
			Factory.Save();

			return new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, GetAction(declaration), UpdateActionCode.Add).PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			CombineAssertions(() =>
			{
				AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_TSCAInd);
				AssertEquals("TEST TSCA DESC", invoiceLineImported.JI_Description);
				AssertEquals(TSCAIndicatorList.Codes.TSCAPositive, invoiceLineImported.US_TSCACertification);
				AssertEquals(PartyTypeList.Codes.Importer, invoiceLineImported.US_TSCAODSCertIndividual);
				AssertEquals("IAN TEST BROKER", invoiceLineImported.US_FDAContactName);
				AssertEquals("164285648734", invoiceLineImported.US_FDAContactPhoneNo);
				AssertEquals("ABCDEFG@TEST.COM", invoiceLineImported.US_FDAContactEmail);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
		}
	}
}
