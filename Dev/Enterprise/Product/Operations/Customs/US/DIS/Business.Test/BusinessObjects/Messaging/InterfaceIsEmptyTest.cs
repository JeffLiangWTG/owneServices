using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class InterfaceIsEmptyTest : TestCaseWithFactory
	{
		public void TestIsEmpty()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var invoiceMock = new Mock<ICommercialInvoiceDefault>();
			invoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("423987432"));
			invoiceMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var invoice = disDocument.Invoice;
			invoice.InvoiceNumber = "423987432";
			var invoiceWrapper = new DISInvoiceWrapper(invoice, hostWrapper);
			Assert(!invoiceWrapper.IsEmpty());
			invoice.InvoiceNumber = "";
			Assert(invoiceWrapper.IsEmpty());
			var invoiceLineMock = new Mock<IDISInvoiceLineDefault>();
			invoice.InvoiceNumber = "423987432";
			var invoiceLineRange = invoice.InvoiceLineRanges.AddNew();
			invoiceLineRange.InvoiceLineFrom = 1;
			Assert(!invoiceWrapper.IsEmpty());

			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			invoiceMock.VerifyAll();
			invoiceLineMock.VerifyAll();
		}
	}
}
