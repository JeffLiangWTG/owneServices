using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISInvoiceLineRange))]
	sealed class DISInvoiceLineRangeTest : XmlSerializableNonPersistentBusinessObjectTest<DISInvoiceLineRange>
	{
		public void TestInvoiceLineFromCopiesToInvoiceLineTo()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(System.Array.Empty<ICommercialInvoiceDefault>());
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var invoiceLineRange = disDocument.Invoice.InvoiceLineRanges.AddNew();
			invoiceLineRange.InvoiceLineFrom = 1;
			AssertEquals(1, invoiceLineRange.InvoiceLineTo);
			invoiceLineRange.InvoiceLineFrom = 2;
			AssertEquals(2, invoiceLineRange.InvoiceLineTo);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
		}

		public void TestInvoiceLines()
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
			invoiceLineRange.InvoiceLineFrom = 1;
			invoiceLineRange.InvoiceLineTo = 2;
			var lines = invoiceLineRange.InvoiceLines;
			AssertEquals(2, lines.Count());
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			invoiceMock.VerifyAll();
			invoiceLineMock.VerifyAll();
			invoiceLineMock2.VerifyAll();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDeclaration = new TestHelper(Factory).GetJobDeclaration();
			var hostWrapper = new DISHostWrapper((IUSDISHost)jobDeclaration);
			var disDocument = new DISDocument(hostWrapper);
			var invoice = new DISInvoice(disDocument);
			return new DISInvoiceLineRange(invoice);
		}
	}
}
