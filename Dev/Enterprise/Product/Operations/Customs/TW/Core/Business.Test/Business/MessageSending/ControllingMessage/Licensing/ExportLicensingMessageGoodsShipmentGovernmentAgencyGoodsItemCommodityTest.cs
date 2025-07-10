using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity))]
	sealed class ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodityTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGovernmentProcedureCurrentCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "31";
			invoiceLine.AssignCMHeaderToInvoices(header);
			ICommodity goodsShipmentGovernmentAgencyGoodsItemCommodity = new ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(header, invoiceLine);
			NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.GovernmentProcedure.CurrentCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "GovernmentProcedure.CurrentCode should be empty - should be [null] or [empty]");
		}
	}
}
