using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(NX101ControllingMessageHeaderSectionBodyDocumentWrapperCollection))]
	sealed class NX101ControllingMessageHeaderSectionBodyDocumentWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NX101ControllingMessageHeaderSectionBodyDocumentWrapperCollection>
	{
		[ExpectNoExceptions]
		public void TestLoadFromInvoiceHeader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var fINX101 = new NX101MessageSendingObject(header);

			var collection = new NX101ControllingMessageHeaderSectionBodyDocumentWrapperCollection(null, ZString.Empty, Factory);
			collection.AddLinesFrom(fINX101.GoodsShipment.GovernmentAgencyGoodsItems.OfType<NX101GoodsShipmentGovernmentAgencyGoodsItem>());
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(0), "collection.Count");

			invoiceLine.AssignCMHeaderToInvoices(header);
			collection = new NX101ControllingMessageHeaderSectionBodyDocumentWrapperCollection(null, ZString.Empty, Factory);
			collection.AddLinesFrom(fINX101.GoodsShipment.GovernmentAgencyGoodsItems.OfType<NX101GoodsShipmentGovernmentAgencyGoodsItem>());
			NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(1), "collection.Count");
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			var fINX101 = new NX101MessageSendingObject(header);
			return new NX101ControllingMessageHeaderSectionBodyDocumentWrapper(null, fINX101.GoodsShipment.GovernmentAgencyGoodsItems.OfType<NX101GoodsShipmentGovernmentAgencyGoodsItem>().FirstOrDefault(), CertificateTypeList.Codes.Code18, Factory);
		}

		protected override NX101ControllingMessageHeaderSectionBodyDocumentWrapperCollection GetCollectionToTest()
		{
			return new NX101ControllingMessageHeaderSectionBodyDocumentWrapperCollection(null, ZString.Empty, Factory);
		}
	}
}
