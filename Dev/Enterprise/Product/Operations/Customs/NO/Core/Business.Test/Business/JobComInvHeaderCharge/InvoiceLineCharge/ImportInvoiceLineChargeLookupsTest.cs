using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(ImportInvoiceLineChargeLookups))]
	sealed class ImportInvoiceLineChargeLookupsTest : InvoiceLineChargeLookupsAbstractTest<ImportInvoiceLineChargeLookups>
	{
		protected override string MessageType => JobMessageTypeList.Codes.Import;

		public void TestChargeTypeList()
		{
			var chargeCodes = lookups.ChargeTypeList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", chargeCodes, lookups.ChargeTypeList);
				AssertType<NOInvoiceChargeTypesImport>("Type", chargeCodes);
				AssertEquals("All codes", "DED, OFT, ONS, OTH, VGE", chargeCodes.CodesAsString);
				AssertEquals("OFT description", "International Freight", chargeCodes.GetDescriptionFromCode("OFT"));
				AssertEquals("ONS description", "International Insurance", chargeCodes.GetDescriptionFromCode("ONS"));
			});
		}
	}
}
