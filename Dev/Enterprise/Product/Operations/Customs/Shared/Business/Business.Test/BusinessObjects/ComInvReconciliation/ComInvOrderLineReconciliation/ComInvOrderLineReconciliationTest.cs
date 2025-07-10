using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ComInvOrderLineReconciliation))]
	sealed class ComInvOrderLineReconciliationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			ComInvOrderLineReconciliation newComInvOrderLine = Factory.New<ComInvOrderLineReconciliation>();
			AssertNotNull(newComInvOrderLine);
			AssertEquals(0m, newComInvOrderLine.JO_Recon_Quantity);
			AssertEquals(0m, newComInvOrderLine.JO_Recon_QuantityInfo.Value);
			AssertEquals(0m, newComInvOrderLine.JO_Recon_ItemPrice);
			AssertEquals(0m, newComInvOrderLine.JO_Recon_ItemPriceInfo.Value);
			AssertEquals(0m, newComInvOrderLine.JO_Recon_LinePrice);
			AssertEquals(0m, newComInvOrderLine.JO_Recon_LinePriceInfo.Value);
			AssertEquals(ZString.Empty, newComInvOrderLine.JO_OrderNumberAndSplit);
			AssertEquals(ZString.Empty, newComInvOrderLine.JO_OrderNumberAndSplitInfo.Value);
			AssertEquals(ComInvReconciliationQuantityType.OrderQuantity, newComInvOrderLine.QuantityType);
		}

		public void TestJO_OrderNumberAndSplit()
		{
			AssertEquals(new ZString("orderno10-2"), bO.JO_OrderNumberAndSplit);
			bO.JO_JD = ZGuid.Empty;
			AssertEquals(ZString.Empty, bO.JO_OrderNumberAndSplit);
		}

		public void TestRecalculate()
		{
			AssertEquals("PreCondition: JO_Recon_Quantity", 5m, bO.JO_Recon_Quantity);
			AssertEquals("PreCondition: JO_Recon_ItemPrice", 10m, bO.JO_Recon_ItemPrice);
			AssertEquals("PreCondition: JO_Recon_LinePrice", 50m, bO.JO_Recon_LinePrice);

			bO.QuantityType = ComInvReconciliationQuantityType.InvoiceQuantity;
			AssertEquals("QuantityType.InvoiceQuantity: JO_Recon_Quantity", 4m, bO.JO_Recon_Quantity);
			AssertEquals("QuantityType.InvoiceQuantity: JO_Recon_ItemPrice", 10m, bO.JO_Recon_ItemPrice);
			AssertEquals("QuantityType.InvoiceQuantity: JO_Recon_LinePrice", 40m, bO.JO_Recon_LinePrice);

			bO.QuantityType = ComInvReconciliationQuantityType.ReceivedQuantity;
			AssertEquals("QuantityType.ReceivedQuantity: JO_Recon_Quantity", 3m, bO.JO_Recon_Quantity);
			AssertEquals("QuantityType.ReceivedQuantity: JO_Recon_ItemPrice", 10m, bO.JO_Recon_ItemPrice);
			AssertEquals("QuantityType.ReceivedQuantity: JO_Recon_LinePrice", 30m, bO.JO_Recon_LinePrice);

			bO.JO_Recon_Quantity = 6m;
			AssertEquals("JO_Recon_Quantity Changed to 6: JO_Recon_ItemPrice", 10m, bO.JO_Recon_ItemPrice);
			AssertEquals("JO_Recon_Quantity Changed to 6: JO_Recon_LinePrice", 60m, bO.JO_Recon_LinePrice);

			bO.JO_Recon_ItemPrice = 15m;
			AssertEquals("JO_Recon_ItemPrice Changed to 15: JO_Recon_Quantity", 6m, bO.JO_Recon_Quantity);
			AssertEquals("JO_Recon_ItemPrice Changed to 15: JO_Recon_LinePrice", 90m, bO.JO_Recon_LinePrice);

			bO.JO_Recon_LinePrice = 120m;
			AssertEquals("JO_Recon_LinePrice Changed to 120: JO_Recon_ItemPrice", 6m, bO.JO_Recon_Quantity);
			AssertEquals("JO_Recon_LinePrice Changed to 120: JO_Recon_ItemPrice", 20m, bO.JO_Recon_ItemPrice);

			bO.JO_Recon_LinePrice = 12.34m;
			AssertEquals("JO_Recon_LinePrice Changed to 120: JO_Recon_ItemPrice", 6m, bO.JO_Recon_Quantity);
			AssertEquals("JO_Recon_LinePrice Changed to 120: JO_Recon_ItemPrice", 2.0567m, bO.JO_Recon_ItemPrice);
		}

		public void TestJO_ReconItemPriceHas4DecimalsWhenNoCurrencyDecimals()
		{
			ComInvOrderLineReconciliation line = Factory.New<ComInvOrderLineReconciliation>();
			line.JO_Quantity = 5m;
			line.JO_ItemPrice = 10m;

			line.JO_Recon_ItemPrice = 10.1200m;
			AssertEquals("PreCondition: JO_Recon_ItemPrice", 10.1200m, line.JO_Recon_ItemPrice);

			line.JO_Recon_ItemPrice = 10.1234m;
			AssertEquals("JO_Recon_ItemPrice:", 10.1234m, line.JO_Recon_ItemPrice);
		}

		public void TestIsValid()
		{
			AssertEquals(true, bO.IsValid);
			bO.JO_Recon_Quantity = 0m;
			AssertEquals(false, bO.IsValid);
		}

		public void TesClone()
		{
			bO.QuantityType = ComInvReconciliationQuantityType.InvoiceQuantity;
			bO.JO_Recon_ItemPrice = 20m;

			ComInvOrderLineReconciliation bO2 = bO.Clone() as ComInvOrderLineReconciliation;

			AssertNotNull(bO2);
			AssertEquals(bO.PK, bO2.PK);
			AssertEquals(bO.QuantityType, bO2.QuantityType);
			AssertEquals(bO.JO_Recon_Quantity, bO2.JO_Recon_Quantity);
			AssertEquals(bO.JO_Recon_ItemPrice, bO2.JO_Recon_ItemPrice);
			AssertEquals(bO.JO_Recon_LinePrice, bO2.JO_Recon_LinePrice);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			helper = new TestHelper();
			bO = (ComInvOrderLineReconciliation)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ComInvOrderReconciliation dummyOrder = Factory.New<ComInvOrderReconciliation>();
			dummyOrder.JD_OrderNumber = "orderno10";
			dummyOrder.BuyerPK = helper.Buyer.PK;
			dummyOrder.SupplierPK = helper.Supplier.PK;
			dummyOrder.JD_OrderNumberSplit = 2;
			dummyOrder.JD_RX_NKOrderCurrency = helper.USDCurrency.RX_Code;

			ComInvOrderLineReconciliation dummyLine = Factory.New<ComInvOrderLineReconciliation>();
			dummyLine.JO_JD = dummyOrder.PK;
			dummyLine.JO_LineNo = 99;
			dummyLine.JO_Quantity = 5m;
			dummyLine.JO_ItemPrice = 10m;
			dummyLine.JO_QtyInvoiced = 4m;
			dummyLine.JO_QtyReceived = 3m;
			dummyLine.JO_Recon_ItemPrice = 10m;

			return Factory.Load(typeof(ComInvOrderLineReconciliation), dummyLine.PK);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			ComInvOrderLineReconciliation result = (ComInvOrderLineReconciliation)GetNewBusinessObject();
			result.Order.JD_OrderNumberSplit = 99;
			return result;
		}

		TestHelper helper;
		ComInvOrderLineReconciliation bO;

		//		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject BO)
		//		{
		//			ComInvOrderLineReconciliation Line = (ComInvOrderLineReconciliation)BO;
		//			return ComInvOrderLineReconciliation.NewCustomLabelsProvider(Line.Order);
		//		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new ComInvOrderLineReconciliationLightValidationTester(bizObjToTest);
		}

		class ComInvOrderLineReconciliationLightValidationTester : LightValidationTester
		{
			public ComInvOrderLineReconciliationLightValidationTester(BusinessObject bo)
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
