using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401ExportLicensingMessageGoodsShipment))]
	sealed class NX401ExportLicensingMessageGoodsShipmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConsignment()
		{
			var importEntryHeader = CusEntryHeaderTest.GetImportFobEntryHeaderTestCase(Factory);
			var importDeclaration = importEntryHeader.Declaration;
			var importHeader = importDeclaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			IGoodsShipment goodsShipment = new NX401ExportLicensingMessageGoodsShipment(importHeader);
			NUnit.Framework.Assert.That(goodsShipment.Consignment, NUnit.Framework.Is.TypeOf<NX401ExportLicensingMessageGoodsShipmentConsignment>(), "Consignment Type");
		}

		[ExpectNoExceptions]
		public void TestLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			IGoodsShipment goodsShipment = new NX401ExportLicensingMessageGoodsShipment(header);
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.FirstOrDefault(), NUnit.Framework.Is.TypeOf<NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem>());
		}

		[ExpectNoExceptions]
		public void TestSeller()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			IGoodsShipment goodsShipment = new NX401ExportLicensingMessageGoodsShipment(header);
			NUnit.Framework.Assert.That(goodsShipment.Seller, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)), "Seller must be null - should be [null]");
		}
	}
}
