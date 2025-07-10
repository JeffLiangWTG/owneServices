using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity))]
	sealed class NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodityTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAdValoremTaxBaseAmount()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			ICommodity goodsShipmentGovernmentAgencyGoodsItemCommodity = new NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(header, invoiceLine);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			invoiceLine.CusEntryLine.CL_CustomsValue = 101m;
			NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.DutyTaxFee.AdValoremTaxBaseAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "DutyTaxFee.AdValoremTaxBaseAmount");
		}
	}
}
