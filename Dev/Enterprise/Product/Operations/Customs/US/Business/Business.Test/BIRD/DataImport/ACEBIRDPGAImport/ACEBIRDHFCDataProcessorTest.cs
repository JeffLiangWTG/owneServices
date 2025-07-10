using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDHFCDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_HFCDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Description = "TEST HFC DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			CombineAssertions(delegate
			{
				AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_HFCInd);
				AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_HFCDisclaimReason);
				AssertEquals("TEST HFC DISCLAIMED", invoiceLineImported.JI_Description);
			});
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
			invoiceLine.JI_Description = "TEST HFC DESC";
			var hfcHeader1 = invoiceLine.USHFCHeaders.AddNew();
			hfcHeader1.US_ASHRAENumber = "A";
			hfcHeader1.US_CertifyingIndividual = EntityRoleCodeList.Codes.Consignee;
			hfcHeader1.US_HFCImageSent = true;
			hfcHeader1.US_NetWeight = 80;
			var hfcHeader2 = invoiceLine.USHFCHeaders.AddNew();
			hfcHeader2.US_CertifyingIndividual = EntityRoleCodeList.Codes.CustomsBroker;
			hfcHeader2.US_HFCImageSent = false;
			hfcHeader2.US_NetWeight = 90;
			var hfcDetail1 = hfcHeader2.USHFCDetails.AddNew();
			hfcDetail1.US_LPCONumber = "123";
			hfcDetail1.US_NameOfActiveIngredient = "ABC";
			hfcDetail1.US_ActiveIngredientPercentage = 10;
			var hfcDetail2 = hfcHeader2.USHFCDetails.AddNew();
			hfcDetail2.US_LPCONumber = "456";
			hfcDetail2.US_NameOfActiveIngredient = "DEF";
			hfcDetail2.US_ActiveIngredientPercentage = 20;
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
				var hfcHeaders = invoiceLineImported.USHFCHeaders.OfType<USHFCHeader>().OrderBy(x => x.US_NetWeight).ToArray();
				AssertEquals(2, hfcHeaders.Length);

				AssertEquals("Header1.US_ASHRAENumber", "A", hfcHeaders[0].US_ASHRAENumber);
				AssertEquals("Header1.US_CertifyingIndividual", EntityRoleCodeList.Codes.Consignee, hfcHeaders[0].US_CertifyingIndividual);
				AssertEquals("Header1.US_HFCImageSent", true, hfcHeaders[0].US_HFCImageSent);
				AssertEquals("Header1.US_NetWeight", 80m, hfcHeaders[0].US_NetWeight);
				AssertEquals("Header1.USHFCDetails", 0, hfcHeaders[0].USHFCDetails.Count);

				AssertEquals("Header2.US_ASHRAENumber", ZString.Empty, hfcHeaders[1].US_ASHRAENumber);
				AssertEquals("Header2.US_CertifyingIndividual", EntityRoleCodeList.Codes.CustomsBroker, hfcHeaders[1].US_CertifyingIndividual);
				AssertEquals("Header2.US_HFCImageSent", false, hfcHeaders[1].US_HFCImageSent);
				AssertEquals("Header2.US_NetWeight", 90m, hfcHeaders[1].US_NetWeight);
				AssertEquals("Header2.USHFCDetails", 2, hfcHeaders[1].USHFCDetails.Count);

				var hfcDetails = hfcHeaders[1].USHFCDetails.OfType<USHFCDetail>().OrderBy(x => x.US_ActiveIngredientPercentage).ToArray();
				AssertEquals("Detail1.US_LPCONumber", "123", hfcDetails[0].US_LPCONumber);
				AssertEquals("Detail1.US_NameOfActiveIngredient", "ABC", hfcDetails[0].US_NameOfActiveIngredient);
				AssertEquals("Detail1.US_ActiveIngredientPercentage", 10m, hfcDetails[0].US_ActiveIngredientPercentage);
				AssertEquals("Detail1.US_LPCONumber", "456", hfcDetails[1].US_LPCONumber);
				AssertEquals("Detail1.US_NameOfActiveIngredient", "DEF", hfcDetails[1].US_NameOfActiveIngredient);
				AssertEquals("Detail1.US_ActiveIngredientPercentage", 20m, hfcDetails[1].US_ActiveIngredientPercentage);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
		}
	}
}
