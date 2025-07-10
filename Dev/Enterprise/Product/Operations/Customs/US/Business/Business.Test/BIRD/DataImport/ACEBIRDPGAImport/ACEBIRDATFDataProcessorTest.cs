using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDATFDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			Assert("Disclaim messages are not supported for ATF.", true);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			declaration.US_EnableCRL = false;
			declaration.US_SchDEntry = "1101";
			declaration.US_FDAADTA = new ZDateTime(2016, 7, 22);

			invoiceLine.JI_Description = "ATF TEST";
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;

			var atfLine = invoiceLine.ATFLines.AddNew();
			atfLine.US_Quantity = 100m;
			atfLine.US_CategoryCode = "API";
			atfLine.US_ExtendedDescription = "DISCRIPTION";
			atfLine.US_FFLNumber = "1-23-456-78-9A-01234";
			atfLine.US_FELNumber = "1-23-456-78-9A-01235";
			atfLine.US_PermitNumber = "123456789";
			atfLine.US_AECANumber = "A-12-345-6789";
			atfLine.US_Model = "MODEL";
			atfLine.US_CaliberGaugeSize = "CALIBER";
			atfLine.US_BarrelLength = 1800m;
			atfLine.US_OverallLength = 2000m;

			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);
			AssertEquals(new ZDateTime(2016, 7, 22), DeclarationImported.US_FDAADTA);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_ATFInd);
			AssertEquals(Core.Constants.CountryCodes.Canada, invoiceLineImported.US_UC_NKCountryOfExport);
			AssertEquals("ATF TEST", invoiceLineImported.JI_Description);
			AssertEquals(1, invoiceLineImported.ATFLines.Count);

			var atfImported = invoiceLineImported.ATFLines[0];
			CombineAssertions(() =>
			{
				AssertEquals(100m, atfImported.US_Quantity);
				AssertEquals("API", atfImported.US_CategoryCode);
				AssertEquals("DISCRIPTION", atfImported.US_ExtendedDescription);
				AssertEquals("1-23-456-78-9A-01234", atfImported.US_FFLNumber);
				AssertEquals("1-23-456-78-9A-01235", atfImported.US_FELNumber);
				AssertEquals("123456789", atfImported.US_PermitNumber);
				AssertEquals("A-12-345-6789", atfImported.US_AECANumber);
				AssertEquals("MODEL", atfImported.US_Model);
				AssertEquals("CALIBER", atfImported.US_CaliberGaugeSize);
				AssertEquals(1800m, atfImported.US_BarrelLength);
				AssertEquals(2000m, atfImported.US_OverallLength);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
		}
	}
}
