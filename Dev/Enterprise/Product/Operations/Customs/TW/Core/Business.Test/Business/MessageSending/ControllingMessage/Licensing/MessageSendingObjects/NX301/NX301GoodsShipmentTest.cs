using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301GoodsShipment))]
	sealed class NX301GoodsShipmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItems()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(Header);
			var governmentAgencyGoodsItems = GoodsShipment.GovernmentAgencyGoodsItems.ToArray();
			NUnit.Framework.Assert.That(governmentAgencyGoodsItems.ElementAt(0), NUnit.Framework.Is.TypeOf<NX301GoodsShipmentGovernmentAgencyGoodsItem>());
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ??= Factory.NewWithValidTestData<JobDeclaration>();

		CusTWControllingMessageHeader header;
		CusTWControllingMessageHeader Header => header ??= Declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();

		IGoodsShipment goodsShipment;
		IGoodsShipment GoodsShipment => goodsShipment ??= new NX301GoodsShipment(Header);
	}
}
