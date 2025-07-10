using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class ExportInvoiceChargeLookupsTest : InvoiceChargeLookupsAbstractTest
	{
		protected override string MessageType => JobMessageTypeList.Codes.Export;

		public void TestChargeTypeList()
		{
			var chargeCodes = charge.Lookups.ChargeTypeList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", chargeCodes, charge.Lookups.ChargeTypeList);
				AssertType<NOInvoiceChargeTypesExport>("Type", chargeCodes);
				AssertEquals("All codes", "DED, OFT, ONS, OTH", chargeCodes.CodesAsString);
				AssertEquals("OFT description", "Freight", chargeCodes.GetDescriptionFromCode("OFT"));
				AssertEquals("ONS description", "Insurance", chargeCodes.GetDescriptionFromCode("ONS"));
			});
		}
	}
}
