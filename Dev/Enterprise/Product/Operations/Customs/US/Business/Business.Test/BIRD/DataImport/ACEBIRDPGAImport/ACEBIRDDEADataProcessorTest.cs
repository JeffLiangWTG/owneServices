using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDDEADataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_DEADisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Description = "TEST DEA DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			CombineAssertions(delegate
			{
				AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_DEAInd);
				AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_DEADisclaimReason);
				AssertEquals("TEST DEA DISCLAIMED", invoiceLineImported.JI_Description);
			});
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			declaration.US_FDAADTA = new ZDateTime(2016, 12, 31, 16, 2, 0);
			invoiceLine.JI_Description = "TEST DEA DESC";
			var deaHeader = invoiceLine.DEAHeaders.AddNew();
			deaHeader.US_CountryOfShipment = Core.Constants.CountryCodes.Mexico;
			deaHeader.US_PermitNumber = "123456";
			deaHeader.US_RegistrationNumber = "9876543";
			deaHeader.US_FormID = DEAFormTypeList.Codes.DEA35;

			var constituent1 = deaHeader.Constituents.AddNew();
			constituent1.US_ProductCode = "3999";
			constituent1.US_Weight = 100m;
			constituent1.US_WeightUQ = "KG";
			var constituent2 = deaHeader.Constituents.AddNew();
			constituent2.US_ProductCode = "4000";
			constituent2.US_Weight = 2000m;
			constituent2.US_WeightUQ = "MG";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			CombineAssertions(delegate
			{
				AssertEquals("DeclarationImported.US_FDAADTA", new ZDateTime(2016, 12, 31), DeclarationImported.US_FDAADTA);
				AssertEquals("invoiceLineImported.US_DEAInd", OGAIndicatorList.Codes.Declared, invoiceLineImported.US_DEAInd);
				AssertEquals("invoiceLineImported.JI_Description", "TEST DEA DESC", invoiceLineImported.JI_Description);
				AssertEquals("invoiceLineImported.DEAHeaders.Count", 1, invoiceLineImported.DEAHeaders.Count);

				var deaHeaderImported = invoiceLineImported.DEAHeaders[0];
				AssertEquals("deaHeaderImported.US_CountryOfShipment", Core.Constants.CountryCodes.Mexico, deaHeaderImported.US_CountryOfShipment);
				AssertEquals("deaHeaderImported.US_PermitNumber", "123456", deaHeaderImported.US_PermitNumber);
				AssertEquals("deaHeaderImported.US_RegistrationNumber", "9876543", deaHeaderImported.US_RegistrationNumber);
				AssertEquals("deaHeaderImported.US_FormID", DEAFormTypeList.Codes.DEA35, deaHeaderImported.US_FormID);
				AssertEquals("deaHeaderImported.Constituents.Count", 2, deaHeaderImported.Constituents.Count);

				var constituentImported0 = deaHeaderImported.Constituents[0];
				AssertEquals("constituentImported0.US_ProductCode", "3999", constituentImported0.US_ProductCode);
				AssertEquals("constituentImported0.US_Weight", 100m, constituentImported0.US_Weight);
				AssertEquals("constituentImported0.US_WeightUQ", "KG", constituentImported0.US_WeightUQ);
				var constituentImported1 = deaHeaderImported.Constituents[1];
				AssertEquals("constituentImported1.US_ProductCode", "4000", constituentImported1.US_ProductCode);
				AssertEquals("constituentImported1.US_Weight", 2000m, constituentImported1.US_Weight);
				AssertEquals("constituentImported1.US_WeightUQ", "MG", constituentImported1.US_WeightUQ);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
		}
	}
}
