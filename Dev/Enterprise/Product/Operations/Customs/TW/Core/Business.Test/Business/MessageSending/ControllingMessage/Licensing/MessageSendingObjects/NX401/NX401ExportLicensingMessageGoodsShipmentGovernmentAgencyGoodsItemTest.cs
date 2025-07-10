using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem))]
	sealed class NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCommodityType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			IGovernmentAgencyGoodsItem goodsShipmentGovernmentAgencyGoodsItem = new NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItem.Commodity, NUnit.Framework.Is.TypeOf<NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity>());
		}
	}
}
