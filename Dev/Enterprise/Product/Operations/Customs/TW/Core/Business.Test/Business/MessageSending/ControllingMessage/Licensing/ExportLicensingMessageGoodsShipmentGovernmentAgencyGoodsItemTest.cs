using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem))]
	sealed class ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItemCommodityType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			IGovernmentAgencyGoodsItem goodsShipmentGovernmentAgencyGoodsItem = new ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
			NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItem.Commodity, NUnit.Framework.Is.TypeOf<ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity>());
		}
	}
}
