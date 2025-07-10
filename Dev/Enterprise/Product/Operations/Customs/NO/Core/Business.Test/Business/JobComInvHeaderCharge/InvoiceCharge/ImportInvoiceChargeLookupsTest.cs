using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class ImportInvoiceChargeLookupsTest : InvoiceChargeLookupsAbstractTest
	{
		protected override string MessageType => JobMessageTypeList.Codes.Import;

		public void TestChargeTypeList()
		{
			var chargeCodes = charge.Lookups.ChargeTypeList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", chargeCodes, charge.Lookups.ChargeTypeList);
				AssertType<NOInvoiceChargeTypesImport>("Type", chargeCodes);
				AssertEquals("All codes", "DED, OFT, ONS, OTH, VGE", chargeCodes.CodesAsString);
				AssertEquals("OFT description", "International Freight", chargeCodes.GetDescriptionFromCode("OFT"));
				AssertEquals("ONS description", "International Insurance", chargeCodes.GetDescriptionFromCode("ONS"));
				AssertEquals("VGE description", "Value of Goods Exported", chargeCodes.GetDescriptionFromCode("VGE"));
			});
		}
	}
}
