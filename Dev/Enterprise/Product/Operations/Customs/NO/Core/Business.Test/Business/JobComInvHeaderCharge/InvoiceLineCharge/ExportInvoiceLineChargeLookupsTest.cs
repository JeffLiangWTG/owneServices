using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(ExportInvoiceLineChargeLookups))]
	sealed class ExportInvoiceLineChargeLookupsTest : InvoiceLineChargeLookupsAbstractTest<ExportInvoiceLineChargeLookups>
	{
		protected override string MessageType => JobMessageTypeList.Codes.Export;

		public void TestChargeTypeList()
		{
			var chargeCodes = lookups.ChargeTypeList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", chargeCodes, lookups.ChargeTypeList);
				AssertType<NOInvoiceChargeTypesExport>("Type", chargeCodes);
				AssertEquals("All codes", "DED, OFT, ONS, OTH", chargeCodes.CodesAsString);
				AssertEquals("OFT description", "Freight", chargeCodes.GetDescriptionFromCode("OFT"));
				AssertEquals("ONS description", "Insurance", chargeCodes.GetDescriptionFromCode("ONS"));
			});
		}
	}
}
