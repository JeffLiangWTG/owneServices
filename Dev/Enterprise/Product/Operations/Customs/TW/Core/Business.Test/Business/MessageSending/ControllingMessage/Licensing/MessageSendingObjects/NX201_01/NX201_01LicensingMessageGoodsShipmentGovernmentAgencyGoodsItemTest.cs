using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_01LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem))]
	sealed class NX201_01LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSequenceNumeric()
		{
			var entryLine1 = Factory.New<CusEntryLine>();
			entryLine1.CL_LineNumber = 4;
			var entryLine2 = Factory.New<CusEntryLine>();
			entryLine2.CL_LineNumber = 5;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine1.AssignCMHeaderToInvoices(header);
			invoiceLine2.AssignCMHeaderToInvoices(header);
			CombineAssertions(() =>
			{
				IGovernmentAgencyGoodsItem goodsShipmentGovernmentAgencyGoodsItem = new NX201_01LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine1);
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItem.SequenceNumeric, NUnit.Framework.Is.EqualTo(4).Using(CustomComparers.TypeComparison), "entryLine1.CL_LineNumber");

				goodsShipmentGovernmentAgencyGoodsItem = new NX201_01LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine2);
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItem.SequenceNumeric, NUnit.Framework.Is.EqualTo(5).Using(CustomComparers.TypeComparison), "entryLine2.CL_LineNumber");
			});
		}
	}
}
