using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ComInvOrderReconciliation))]
	sealed class ComInvOrderReconciliationTest : EnterpriseBusinessObjectTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.Order);
			}
		}

		#endregion

		public void TestConstructor()
		{
			ComInvOrderReconciliation comInvOrder = Factory.New<ComInvOrderReconciliation>();
			AssertNotNull(comInvOrder);
			AssertEquals(ComInvReconciliationQuantityType.OrderQuantity, comInvOrder.QuantityType);
		}

		public void TestOrderLines()
		{
			AssertNotNull(bO.OrderLines);
			AssertEquals(typeof(ComInvOrderLineReconciliationCollection), bO.OrderLines.GetType());
			AssertEquals(1, bO.OrderLines.Count);
		}

		public void TestSplitOrder()
		{
			ComInvOrderReconciliation comInvOrderSplit = bO.SplitOrder(CreateOrderType.Split);
			AssertNotNull(comInvOrderSplit.OrderLines);
			AssertEquals(1, comInvOrderSplit.OrderLines.Count);
			ComInvOrderLineReconciliation line = comInvOrderSplit.OrderLines[0];
			AssertEquals(2m, line.JO_Quantity);
		}

		#region InvoiceNumber

		public void TestInvoiceNumber()
		{
			ZString invoiceNumber = "TESTINVOICENO";
			bO.JD_OrderNumber = "TESTORDERNO";
			bO.JD_OrderNumberSplit = 3;
			bO.JD_InvoiceNumber = invoiceNumber;
			AssertEquals(invoiceNumber, bO.InvoiceNumber);
			bO.JD_InvoiceNumber = ZString.Empty;
			AssertEquals("TESTORDERNO-3", bO.InvoiceNumber.ToString());
		}

		#endregion

		#region InvoiceDate

		[TestDate(2005, 6, 30)]
		public void TestInvoiceDate()
		{
			ZDateTime invoiceDate = new ZDateTime(2005, 5, 29);
			bO.JD_InvoiceDate = invoiceDate;
			AssertEquals(invoiceDate, bO.InvoiceDate);
			bO.JD_InvoiceDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Now.Date, bO.InvoiceDate.Date);
		}

		#endregion

		#region QuantityType

		public void TestQuantityType()
		{
			CheckQuantityType(bO, ComInvReconciliationQuantityType.InvoiceQuantity);
			CheckQuantityType(bO, ComInvReconciliationQuantityType.ReceivedQuantity);
			CheckQuantityType(bO, ComInvReconciliationQuantityType.OrderQuantity);
		}

		void CheckQuantityType(ComInvOrderReconciliation comInvOrder, ComInvReconciliationQuantityType quantityType)
		{
			comInvOrder.QuantityType = quantityType;
			foreach (ComInvOrderLineReconciliation line in comInvOrder.OrderLines)
			{
				AssertEquals(quantityType, line.QuantityType);
			}
		}

		#endregion

		public void TesClone()
		{
			bO.QuantityType = ComInvReconciliationQuantityType.InvoiceQuantity;

			ComInvOrderReconciliation comInvOrder2 = bO.Clone() as ComInvOrderReconciliation;

			AssertNotNull(comInvOrder2);
			AssertEquals(bO.PK, comInvOrder2.PK);
			AssertEquals(bO.QuantityType, comInvOrder2.QuantityType);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			ComInvOrderReconciliation result = (ComInvOrderReconciliation)GetNewBusinessObject();
			result.JD_OrderNumberSplit = 99;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ComInvOrderReconciliation dummyOrder = Factory.New<ComInvOrderReconciliation>();
			dummyOrder.JD_OrderNumber = "orderno" + Guid.NewGuid().ToString().Substring(0, 5);
			dummyOrder.BuyerPK = helper.Buyer.PK;
			dummyOrder.SupplierPK = helper.Supplier.PK;
			dummyOrder.JD_OrderNumberSplit = 2;

			ComInvOrderLineReconciliation dummyLine = dummyOrder.OrderLines.AddNew();
			dummyLine.JO_JD = dummyOrder.PK;
			dummyLine.JO_LineNo = 99;
			dummyLine.JO_Quantity = 5m;
			dummyLine.JO_ItemPrice = 10m;
			dummyLine.JO_QtyInvoiced = 4m;
			dummyLine.JO_QtyReceived = 3m;
			dummyLine.JO_Recon_ItemPrice = 10m;

			return Factory.Load(typeof(ComInvOrderReconciliation), dummyOrder.PK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new TestHelper();
			bO = (ComInvOrderReconciliation)GetNewBusinessObject();
		}

		ComInvOrderReconciliation bO;
		TestHelper helper;

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new ComInvOrderReconciliationLightValidationTester(bizObjToTest);
		}

		class ComInvOrderReconciliationLightValidationTester : LightValidationTester
		{
			public ComInvOrderReconciliationLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return propertyName != "E2_AddressType" && propertyName != "E2_AddressOverride" && propertyName != "E2_AddressSequence" && propertyName != "E2_OA_Address";
			}
		}

		#endregion
	}
}
