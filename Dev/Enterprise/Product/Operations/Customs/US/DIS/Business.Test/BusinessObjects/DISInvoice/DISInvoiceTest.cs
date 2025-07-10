using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISInvoice))]
	sealed class DISInvoiceTest : XmlSerializableNonPersistentBusinessObjectTest<DISInvoice>
	{
		public void TestDefaultInvoiceList()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var defaultInvoiceMock = new Mock<ICommercialInvoiceDefault>();
			defaultInvoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("423987432"));
			defaultInvoiceMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { defaultInvoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var invoice = disDocument.Invoice;
			var list = invoice.DefaultInvoiceList;
			AssertEquals(1, list.Count);
			AssertEquals("423987432", list[0].Code);
			AssertEquals("Blah", list[0].Description);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			defaultInvoiceMock.VerifyAll();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDeclaration = new TestHelper(Factory).GetJobDeclaration();
			var hostWrapper = new DISHostWrapper((IUSDISHost)jobDeclaration);
			var disDocument = new DISDocument(hostWrapper);
			return new DISInvoice(disDocument);
		}
	}
}
