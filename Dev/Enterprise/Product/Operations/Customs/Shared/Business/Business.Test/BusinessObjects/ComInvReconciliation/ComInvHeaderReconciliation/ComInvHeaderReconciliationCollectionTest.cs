using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ComInvHeaderReconciliationCollection))]
	sealed class ComInvHeaderReconciliationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ComInvHeaderReconciliationCollection>
	{
		public void TestIndexer()
		{
			ComInvHeaderReconciliationCollection collection = new ComInvHeaderReconciliationCollection(Factory);
			ComInvHeaderReconciliation comInvHead1 = GetNewComInvHeader("ORDER1", 1, new ZDateTime(2005, 6, 28));
			collection.Add(comInvHead1);
			ComInvHeaderReconciliation comInvHead2 = GetNewComInvHeader("ORDER1", 2, new ZDateTime(2005, 6, 29));
			collection.Add(comInvHead2);
			ComInvHeaderReconciliation comInvHead3 = GetNewComInvHeader("ORDER2", 1, new ZDateTime(2005, 6, 30));
			collection.Add(comInvHead3);

			AssertNotNull(collection[0]);
			AssertEquals(comInvHead1, collection[0]);
			AssertNotNull(collection[1]);
			AssertEquals(comInvHead2, collection[1]);
			AssertNotNull(collection[2]);
			AssertEquals(comInvHead3, collection[2]);

			ComInvOrderReconciliation comInvOrder = Factory.New<ComInvOrderReconciliation>();
			comInvOrder.SupplierPK = helper.Supplier.PK;
			comInvOrder.JD_InvoiceNumber = "DUMMY";
			comInvOrder.JD_InvoiceDate = new ZDateTime(2005, 6, 28);
			comInvOrder.JD_RX_NKOrderCurrency = helper.USDCurrency.RX_Code;
			comInvOrder.JD_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			ComInvHeaderReconciliation comInvHead = collection[comInvOrder];
			AssertNull(comInvHead);

			comInvOrder.JD_InvoiceNumber = "ORDER1";
			comInvHead = collection[comInvOrder];
			AssertNotNull(comInvHead);
			AssertEquals(comInvHead1, comInvHead);

			comInvOrder.JD_InvoiceDate = new ZDateTime(2005, 6, 29);
			comInvHead = collection[comInvOrder];
			AssertNotNull(comInvHead);
			AssertEquals(comInvHead2, comInvHead);

			comInvOrder.JD_InvoiceNumber = "ORDER2";
			comInvOrder.JD_InvoiceDate = new ZDateTime(2005, 6, 30);
			comInvHead = collection[comInvOrder];
			AssertNotNull(comInvHead);
			AssertEquals(comInvHead3, comInvHead);
		}

		public override void TestTypedAddNew()
		{
			TestAddNew();
		}

		public override void TestAddNew()
		{
			ComInvHeaderReconciliationCollection collection = new ComInvHeaderReconciliationCollection(Factory);
			ComInvHeaderReconciliation header = collection.AddNew();
			AssertNull(header);
		}

		public void TestAllowNew()
		{
			ComInvHeaderReconciliationCollection collection = new ComInvHeaderReconciliationCollection(Factory);
			AssertEquals(false, collection.AllowNew);
		}

		public void TestCopyInvoiceLinesPersistentValuesToAnotherFactory()
		{
			ComInvHeaderReconciliationCollection collection = new ComInvHeaderReconciliationCollection(Factory);
			ComInvHeaderReconciliation header = GetNewComInvHeader("INVOICENO", 1, new ZDateTime(2005, 6, 28));
			collection.Add(header);
			ComInvOrderReconciliation comInvOrder = header.ComInvOrders[0];
			ComInvOrderLineReconciliation comInvLine = comInvOrder.OrderLines[0];

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ComInvOrderLineReconciliation newComInvLine = newFactory.Load<ComInvOrderLineReconciliation>(comInvLine.PK);
			AssertEquals(comInvLine.JO_Quantity, newComInvLine.JO_Quantity);

			comInvLine.JO_Quantity = 23m;
			AssertEquals(false, comInvLine.JO_Quantity == newComInvLine.JO_Quantity);
			collection.CopyInvoiceLinesPersistentValuesToAnotherFactory(newFactory);
			AssertEquals(comInvLine.JO_Quantity, newComInvLine.JO_Quantity);

			ComInvOrderLineReconciliation comInvLine2 = Factory.New<ComInvOrderLineReconciliation>();
			comInvLine2.JO_JD = comInvOrder.PK;
			comInvLine2.JO_LineNo = 2;
			comInvLine2.JO_Quantity = 43m;
			header.InvoiceLines.Add(comInvLine2);

			ComInvOrderLineReconciliation newComInvLine2 = newFactory.Load<ComInvOrderLineReconciliation>(comInvLine2.PK);
			AssertNull(newComInvLine2);
			collection.CopyInvoiceLinesPersistentValuesToAnotherFactory(newFactory);
			newComInvLine2 = newFactory.Load<ComInvOrderLineReconciliation>(comInvLine2.PK);
			AssertNotNull(newComInvLine2);
			AssertEquals(comInvLine2.JO_Quantity, newComInvLine2.JO_Quantity);
			AssertEquals(43m, newComInvLine2.JO_Quantity);
		}

		#region Implementation

		ComInvHeaderReconciliation GetNewComInvHeader(ZString orderNo, ZByte orderSplit, ZDateTime orderDate)
		{
			ComInvHeaderReconciliation result = new ComInvHeaderReconciliation(Factory, helper.Supplier.PK, orderNo, orderDate, helper.USDCurrency.RX_Code, Constants.IncoTerms.FreeOnBoard);
			result.AddComInvOrder(GetNewComInvOrder(orderNo, orderSplit, orderDate));
			return result;
		}

		ComInvOrderReconciliation GetNewComInvOrder(ZString orderNo, ZByte orderSplit, ZDateTime orderDate)
		{
			Order newOrder = Factory.New<Order>();
			newOrder.BuyerPK = helper.Buyer.PK;
			newOrder.SupplierPK = helper.Supplier.PK;
			newOrder.JD_OrderNumber = orderNo;
			newOrder.JD_OrderDate = orderDate;
			newOrder.JD_OrderNumberSplit = orderSplit;
			newOrder.JD_InvoiceNumber = orderNo;
			newOrder.JD_InvoiceDate = orderDate;
			newOrder.JD_RX_NKOrderCurrency = helper.USDCurrency.RX_Code;

			OrderLine line = newOrder.OrderLines.AddNew();
			line.JO_Quantity = 5m;
			line.JO_ItemPrice = 10m;
			Factory.Save();

			var result = Factory.Load<ComInvOrderReconciliation>(newOrder.PK);
			var comInvLine = Factory.Load<ComInvOrderLineReconciliation>(line.PK);
			result.OrderLines.Add(comInvLine);

			return result;
		}

		protected override ComInvHeaderReconciliationCollection GetCollectionToTest()
		{
			return new ComInvHeaderReconciliationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComInvHeaderReconciliation(Factory, helper.Supplier.PK, "INVNO", ZDateTime.Now, helper.USDCurrency.RX_Code, Constants.IncoTerms.FreeOnBoard);
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
