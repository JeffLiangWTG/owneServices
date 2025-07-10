using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ComInvHeaderReconciliation))]
	sealed class ComInvHeaderReconciliationTest : NonPersistentBusinessObjectTestCase
	{
		#region Constructor

		[ExpectNoExceptions]
		public void TestConstructor_InvalidInvoiceNumber()
		{
			new ComInvHeaderReconciliation(Factory, helper.Supplier.PK, null, new ZDateTime(2005, 6, 28), helper.USDCurrency.RX_Code, Constants.IncoTerms.FreeOnBoard);
		}

		[ExpectNoExceptions]
		public void TestConstructor_EmptyInvoiceNumber()
		{
			new ComInvHeaderReconciliation(Factory, helper.Supplier.PK, ZString.Empty, new ZDateTime(2005, 6, 28), helper.USDCurrency.RX_Code, Constants.IncoTerms.FreeOnBoard);
		}

		[ExpectNoExceptions]
		public void TestConstructor_InvalidInvoiceDate()
		{
			new ComInvHeaderReconciliation(Factory, helper.Supplier.PK, "INVNO", ZDateTime.Invalid, helper.USDCurrency.RX_Code, Constants.IncoTerms.FreeOnBoard);
		}

		[ExpectNoExceptions]
		public void TestConstructor_EmptyInvoiceDate()
		{
			new ComInvHeaderReconciliation(Factory, helper.Supplier.PK, "INVNO", ZDateTime.Empty, helper.USDCurrency.RX_Code, Constants.IncoTerms.FreeOnBoard);
		}

		public void TestConstructor()
		{
			var supplierPK = helper.Supplier.PK;
			var invoiceNumber = "INVOICENO";
			var invoiceDate = new ZDateTime(2005, 6, 28);
			var currencyCode = helper.USDCurrency.RX_Code;
			var incoTerm = Constants.IncoTerms.FreeOnBoard;

			var comInvHeader = new ComInvHeaderReconciliation(Factory, supplierPK, invoiceNumber, invoiceDate, currencyCode, incoTerm);
			AssertNotNull(comInvHeader);
			AssertEquals(supplierPK, comInvHeader.SupplierPK);
			AssertEquals(supplierPK, comInvHeader.Supplier.PK);
			AssertEquals(helper.Supplier.OH_Code, comInvHeader.SupplierCode);
			AssertEquals(helper.Supplier.OH_Code, comInvHeader.SupplierCodeInfo.Value);
			AssertEquals(invoiceNumber, comInvHeader.InvoiceNumber);
			AssertEquals(invoiceNumber, comInvHeader.InvoiceNumberInfo.Value);
			AssertEquals(invoiceDate, comInvHeader.InvoiceDate);
			AssertEquals(invoiceDate, comInvHeader.InvoiceDateInfo.Value);
			AssertEquals(currencyCode, comInvHeader.CurrencyCode);
			AssertEquals(currencyCode, comInvHeader.Currency.RX_Code);
			AssertEquals(helper.USDCurrency.RX_Code, comInvHeader.CurrencyCode);
			AssertEquals(helper.USDCurrency.RX_Code, comInvHeader.CurrencyCodeInfo.Value);
			AssertEquals(incoTerm, comInvHeader.IncoTerm);
			AssertEquals(incoTerm, comInvHeader.IncoTermInfo.Value);
			AssertEquals(0m, comInvHeader.InvoiceTotal);
			AssertEquals(0m, comInvHeader.InvoiceTotalInfo.Value);
			AssertEquals(0m, comInvHeader.InvoiceLinesTotal);
			AssertNotNull(comInvHeader.InvoiceLines);
			AssertEquals(0, comInvHeader.InvoiceLines.Count);
			AssertNotNull(comInvHeader.ComInvOrders);
			AssertEquals(0, comInvHeader.ComInvOrders.Count);
		}

		#endregion

		public void TestAddComInvOrder()
		{
			var supplierPK = helper.Supplier.PK;
			var invoiceNumber = "INVOICENO";
			var invoiceDate = new ZDateTime(2005, 6, 28);
			var currencyCode = helper.USDCurrency.RX_Code;
			var incoTerm = Constants.IncoTerms.FreeOnBoard;

			var comInvHeader = new ComInvHeaderReconciliation(Factory, supplierPK, invoiceNumber, invoiceDate, currencyCode, incoTerm);
			comInvHeader.AddComInvOrder(ComInvOrder);
			AssertEquals(50m, comInvHeader.InvoiceTotal);
			AssertEquals(50m, comInvHeader.InvoiceLinesTotal);
			AssertEquals(1, comInvHeader.InvoiceLines.Count);
			AssertNotNull(comInvHeader.ComInvOrders);
			AssertEquals(1, comInvHeader.ComInvOrders.Count);
			AssertEquals(ComInvOrder, comInvHeader.ComInvOrders[0]);
		}

		public void TestInvoiceLinesTotal()
		{
			var supplierPK = helper.Supplier.PK;
			var invoiceNumber = "INVOICENO";
			var invoiceDate = new ZDateTime(2005, 6, 28);
			var currencyCode = helper.USDCurrency.RX_Code;
			var incoTerm = Constants.IncoTerms.FreeOnBoard;

			var comInvHeader = new ComInvHeaderReconciliation(Factory, supplierPK, invoiceNumber, invoiceDate, currencyCode, incoTerm);
			comInvHeader.AddComInvOrder(ComInvOrder);
			AssertEquals(50m, comInvHeader.InvoiceLinesTotal);
			AssertEquals(50m, comInvHeader.InvoiceLinesTotalInfo.Value);
			AssertEquals(1, comInvHeader.InvoiceLines.Count);
			ComInvOrderLine.JO_Recon_LinePrice = 60m;
			AssertEquals(60m, comInvHeader.InvoiceLinesTotal);
			AssertEquals(60m, comInvHeader.InvoiceLinesTotalInfo.Value);
		}

		public void TestInvoiceTotal()
		{
			var supplierPK = helper.Supplier.PK;
			var invoiceNumber = "INVOICENO";
			var invoiceDate = new ZDateTime(2005, 6, 28);
			var currencyCode = helper.USDCurrency.RX_Code;
			var incoTerm = Constants.IncoTerms.FreeOnBoard;
			var sumTotalWarning = "Invoice Header amount does not equal the Total Lines Amount. It is out by ";

			var comInvHeader = new ComInvHeaderReconciliation(Factory, supplierPK, invoiceNumber, invoiceDate, currencyCode, incoTerm);
			comInvHeader.AddComInvOrder(ComInvOrder);
			CombineAssertions(() =>
			{
				AssertEquals("Invoice Total", 50m, comInvHeader.InvoiceTotal);
				AssertNoWarnings("InitalWarnings", comInvHeader.InvoiceTotalInfo);

				comInvHeader.InvoiceTotal = 60m;
				AssertHasWarning("Out by 10", comInvHeader.InvoiceTotalInfo, sumTotalWarning + "10.00");

				ComInvOrderLine.JO_Recon_LinePrice = 70m;
				AssertHasWarning("Out by -10", comInvHeader.InvoiceTotalInfo, sumTotalWarning + "-10.00");

				ComInvOrderLine.JO_Recon_LinePrice = 60m;
				AssertNoWarnings("Equal", comInvHeader.InvoiceTotalInfo);

				ComInvOrderLine.JO_Recon_LinePrice = 50m;
				AssertHasWarning("Line price out by 10", comInvHeader.InvoiceTotalInfo, sumTotalWarning + "10.00");
			});
		}

		#region Implementation

		ComInvOrderReconciliation ComInvOrder
		{
			get
			{
				if (fComInvOrder == null)
				{
					fComInvOrder = Factory.Load<ComInvOrderReconciliation>(GetNewOrderPK());
				}

				return fComInvOrder;
			}
		}

		ComInvOrderReconciliation fComInvOrder;

		ZGuid GetNewOrderPK()
		{
			var newFactory = Factory;
			var newOrder = newFactory.NewWithValidTestData<Order>();
			var line = newOrder.OrderLines.AddNew();
			line.JO_Quantity = 5m;
			line.JO_ItemPrice = 10m;
			newFactory.Save();
			return newOrder.PK;
		}

		ComInvOrderLineReconciliation ComInvOrderLine
		{
			get { return ComInvOrder.OrderLines[0]; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ComInvHeaderReconciliation(Factory, helper.Supplier.PK, "INVNO", new ZDateTime(2005, 6, 28), helper.USDCurrency.RX_Code, Constants.IncoTerms.FreeOnBoard);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new TestHelper();
		}

		TestHelper helper;

		#endregion
	}
}
