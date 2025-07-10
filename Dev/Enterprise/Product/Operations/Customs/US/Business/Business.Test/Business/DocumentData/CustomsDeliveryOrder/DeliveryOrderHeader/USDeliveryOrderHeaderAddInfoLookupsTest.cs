using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USDeliveryOrderHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPrepaidCollectList()
		{
			AssertEquals(typeof(DeliveryOrderPrepaidCollectTypeList), DeliveryOrderHeader.AddInfoLookups.PrepaidCollectList.GetType());
		}

		public void TestOrganisations()
		{
			AssertEquals(typeof(OrgHeaderCollection), DeliveryOrderHeader.AddInfoLookups.Organisations.GetType());
		}

		public void TestBillToParties()
		{
			AssertEquals(typeof(OrgHeaderCollection), DeliveryOrderHeader.AddInfoLookups.BillToParties.GetType());
		}

		public void TestInlandCarriers()
		{
			AssertEquals(typeof(LocalTransportCollection), DeliveryOrderHeader.AddInfoLookups.InlandCarriers.GetType());
		}

		public void TestOrderReferencesList()
		{
			DeliveryOrderHeader.Declaration.DocsAndCartage.JP_OrderItemsAsString = "Orders1, Orders1";
			DeliveryOrderHeader.Declaration.JE_OwnerRef = "OwnerRef";
			var order = DeliveryOrderHeader.Declaration.AttachedOrders.AddNew();
			order.JD_OrderNumber = "P000001";
			order.JD_OrderNumberSplit = 1;
			AssertEquals("OwnerRef", DeliveryOrderHeader.AddInfoLookups.OrderReferenceList[0].Code);
			AssertEquals("Orders1", DeliveryOrderHeader.AddInfoLookups.OrderReferenceList[1].Code);
			AssertEquals("P000001-1", DeliveryOrderHeader.AddInfoLookups.OrderReferenceList[2].Code);
		}

		DeliveryOrderHeader deliveryOrderHeader;
		DeliveryOrderHeader DeliveryOrderHeader
		{
			get
			{
				if (deliveryOrderHeader == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					deliveryOrderHeader = declaration.DeliveryOrderHeaders.AddNew();
				}
				return deliveryOrderHeader;
			}
		}
	}
}
