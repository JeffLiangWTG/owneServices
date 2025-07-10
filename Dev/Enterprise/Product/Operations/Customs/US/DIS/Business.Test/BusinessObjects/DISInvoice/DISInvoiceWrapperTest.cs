using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISInvoiceWrapperTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestInterface()
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
			var invoiceLineMock3 = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock3.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(3));
			invoiceLineMock3.Setup(m => m.CommodityDetails).Returns((IDISCommodityLine)null);
			invoiceMock.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLineDefault[] { invoiceLineMock.Object, invoiceLineMock2.Object, invoiceLineMock3.Object });
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var invoice = disDocument.Invoice;
			invoice.InvoiceNumber = "423987432";
			var invoiceLineRange = invoice.InvoiceLineRanges.AddNew();
			invoiceLineRange.InvoiceLineFrom = 1;
			var invoiceWrapper = (IDISInvoice)new DISInvoiceWrapper(invoice, hostWrapper);
			AssertEquals("423987432", invoiceWrapper.InvoiceNumber);
			AssertEquals(DISInvoiceType.CommercialInvoice, invoiceWrapper.InvoiceType);
			AssertEquals(1, invoiceWrapper.InvoiceLines.Count());
			AssertEquals(invoiceLineMock.Object, invoiceWrapper.InvoiceLines.ElementAt(0));
			invoiceLineRange.InvoiceLineFrom = 2;
			invoiceLineRange.InvoiceLineTo = 3;
			AssertEquals(2, invoiceWrapper.InvoiceLines.Count());
			AssertEquals(invoiceLineMock2.Object, invoiceWrapper.InvoiceLines.ElementAt(0));
			AssertEquals(invoiceLineMock3.Object, invoiceWrapper.InvoiceLines.ElementAt(1));
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			invoiceMock.VerifyAll();
			invoiceLineMock.VerifyAll();
			invoiceLineMock2.VerifyAll();
			invoiceLineMock3.VerifyAll();
		}

		public void TestInterfaceWhenNoInvoiceLineRangeIsSelected()
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
			var invoiceLineMock3 = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock3.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(3));
			invoiceLineMock3.Setup(m => m.CommodityDetails).Returns((IDISCommodityLine)null);
			invoiceMock.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLineDefault[] { invoiceLineMock.Object, invoiceLineMock2.Object, invoiceLineMock3.Object });
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var invoice = disDocument.Invoice;
			invoice.InvoiceNumber = "423987432";
			var invoiceWrapper = (IDISInvoice)new DISInvoiceWrapper(invoice, hostWrapper);
			AssertEquals("423987432", invoiceWrapper.InvoiceNumber);
			AssertEquals(DISInvoiceType.CommercialInvoice, invoiceWrapper.InvoiceType);
			AssertEquals(3, invoiceWrapper.InvoiceLines.Count());
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			invoiceMock.VerifyAll();
			invoiceLineMock.VerifyAll();
			invoiceLineMock2.VerifyAll();
			invoiceLineMock3.VerifyAll();
		}
	}
}
