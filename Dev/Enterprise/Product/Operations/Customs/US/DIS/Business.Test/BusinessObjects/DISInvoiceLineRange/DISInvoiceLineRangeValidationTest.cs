using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISInvoiceLineRangeValidationTest : TestCaseWithFactory
	{
		public void TestCheckInvoiceLineFrom()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var invoiceMock = new Mock<ICommercialInvoiceDefault>();
			invoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("423987432"));
			invoiceMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			var invoiceLineMock = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(1));
			invoiceLineMock.Setup(m => m.CommodityDetails).Returns((IDISCommodityLine)null);
			var invoiceLineMock2 = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock2.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(2));
			invoiceLineMock2.Setup(m => m.CommodityDetails).Returns((IDISCommodityLine)null);
			invoiceMock.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLineDefault[] { invoiceLineMock.Object, invoiceLineMock2.Object });
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var invoice = disDocument.Invoice;
			invoice.InvoiceNumber = "423987432";
			var invoiceLineRange = invoice.InvoiceLineRanges.AddNew();
			invoiceLineRange.InvoiceLineFrom = 0;
			AssertHasMessageErrorContaining(invoiceLineRange.InvoiceLineFromInfo, DISInvoiceLineRangeValidation.PleaseEnterValidInvoiceLineRange);
			invoiceLineRange.InvoiceLineFrom = -1;
			AssertHasMessageErrorContaining(invoiceLineRange.InvoiceLineFromInfo, DISInvoiceLineRangeValidation.PleaseEnterValidInvoiceLineRange);
			invoiceLineRange.InvoiceLineFrom = 3;
			AssertNoMessageErrorContaining(invoiceLineRange.InvoiceLineFromInfo, DISInvoiceLineRangeValidation.PleaseEnterValidInvoiceLineRange);
			AssertHasMessageErrorContaining(invoiceLineRange.InvoiceLineFromInfo, "does not exist on Invoice No.");
			invoiceLineRange.InvoiceLineFrom = 2;
			AssertNoMessageErrorContaining(invoiceLineRange.InvoiceLineFromInfo, "does not exist on Invoice No.");
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			invoiceMock.VerifyAll();
			invoiceLineMock.VerifyAll();
			invoiceLineMock2.VerifyAll();
		}

		public void TestCheckInvoiceLineTo()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var invoiceMock = new Mock<ICommercialInvoiceDefault>();
			invoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("423987432"));
			invoiceMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			var invoiceLineMock = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(1));
			invoiceLineMock.Setup(m => m.CommodityDetails).Returns((IDISCommodityLine)null);
			var invoiceLineMock2 = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock2.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(2));
			invoiceLineMock2.Setup(m => m.CommodityDetails).Returns((IDISCommodityLine)null);
			invoiceMock.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLineDefault[] { invoiceLineMock.Object, invoiceLineMock2.Object });
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var invoice = disDocument.Invoice;
			invoice.InvoiceNumber = "423987432";
			var invoiceLineRange = invoice.InvoiceLineRanges.AddNew();
			invoiceLineRange.InvoiceLineFrom = 3;
			invoiceLineRange.InvoiceLineTo = 2;
			AssertHasMessageErrorContaining(invoiceLineRange.InvoiceLineToInfo, DISInvoiceLineRangeValidation.InvalidInvoiceLineTo);
			invoiceLineRange.InvoiceLineFrom = 2;
			invoiceLineRange.InvoiceLineTo = 3;
			AssertNoMessageErrorContaining(invoiceLineRange.InvoiceLineToInfo, DISInvoiceLineRangeValidation.InvalidInvoiceLineTo);
			AssertHasMessageErrorContaining(invoiceLineRange.InvoiceLineToInfo, "does not exist on Invoice No.");
			invoiceLineRange.InvoiceLineTo = 2;
			AssertNoMessageErrorContaining(invoiceLineRange.InvoiceLineToInfo, "does not exist on Invoice No.");
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			invoiceMock.VerifyAll();
			invoiceLineMock.VerifyAll();
			invoiceLineMock2.VerifyAll();
		}
	}
}
