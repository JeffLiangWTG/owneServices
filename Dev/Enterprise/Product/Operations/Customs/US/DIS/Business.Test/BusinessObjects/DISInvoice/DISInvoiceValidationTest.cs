using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISInvoiceValidationTest : TestCaseWithFactory
	{
		public void TestCheckInvoiceNumber()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var invoiceMock = new Mock<ICommercialInvoiceDefault>();
			invoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("INV1"));
			invoiceMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			var invoiceLineMock = new Mock<IDISInvoiceLineDefault>();
			var invoiceLineMock2 = new Mock<IDISInvoiceLineDefault>();
			var invoice2Mock = new Mock<ICommercialInvoiceDefault>();
			var invoiceLineMock3 = new Mock<IDISInvoiceLineDefault>();
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.Invoice.InvoiceNumber = "INV3";
			AssertHasMessageErrorContaining(disDocument.Invoice.InvoiceNumberInfo, ListValidation.InvalidCodeMessageError);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			invoiceMock.VerifyAll();
			invoiceLineMock.VerifyAll();
			invoiceLineMock2.VerifyAll();
			invoice2Mock.VerifyAll();
			invoiceLineMock3.VerifyAll();
		}

		public void TestCheckInvoiceNumberWithLineRange()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(System.Array.Empty<ICommercialInvoiceDefault>());
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var invoice = disDocument.Invoice;
			invoice.InvoiceLineRanges.AddNew();
			invoice.InvoiceNumber = "";
			AssertHasMessageErrorContaining(disDocument.Invoice.InvoiceNumberInfo, DISInvoiceValidation.PleaseEnterInvoiceNumber);
			invoice.InvoiceNumber = "INV1";
			AssertNoMessageErrorContaining(disDocument.Invoice.InvoiceNumberInfo, DISInvoiceValidation.PleaseEnterInvoiceNumber);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
		}
	}
}
