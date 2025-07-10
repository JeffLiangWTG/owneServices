using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class JobShipmentPreplanningLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrdersRefreshedWhenBuyerChanges()
		{
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();
			order.BuyerPK = buyer1.PK;

			var preAdvice = Factory.New<JobShipmentPreplanning>();
			AssertEquals(0, preAdvice.Lookups.Orders.Count);

			preAdvice.BuyerPK = buyer1.PK;
			AssertEquals(1, preAdvice.Lookups.Orders.Count);

			preAdvice.BuyerPK = buyer2.PK;
			AssertEquals(0, preAdvice.Lookups.Orders.Count);
		}

		public void TestOrdersExcludesAttachedOrders()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			Order standAloneOrder = Factory.New<Order>();
			Order attachedOrder = preAdvice.Orders.AddNew();

			JobShipmentPreplanning preAdvice2 = Factory.New<JobShipmentPreplanning>();
			AssertCollectionContains(standAloneOrder, preAdvice2.Lookups.Orders);
			AssertCollectionNotContains(attachedOrder, preAdvice2.Lookups.Orders);
		}

		public void TestOrderErrors()
		{
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.New<ForwardingShipment>();

			var preAdvice = Factory.New<JobShipmentPreplanning>();
			preAdvice.BuyerPK = buyer1.PK;
			preAdvice.EF_JS = shipment.PK;

			var collection = new DummyPreadviceOrderCollection(Factory, preAdvice);

			var order1 = collection.AddNew();
			order1.BuyerPK = buyer1.PK;

			var order2 = collection.AddNew();
			order2.BuyerPK = buyer2.PK;

			var preAdvice2 = Factory.New<JobShipmentPreplanning>();
			var order3 = collection.AddNew();
			order3.BuyerPK = buyer1.PK;
			order3.JD_EF_ShipmentPrePlanning = preAdvice2.PK;

			var order4 = collection.AddNew();
			order4.BuyerPK = buyer1.PK;
			order4.JD_JS = shipment.PK;

			var order5 = collection.AddNew();
			order5.BuyerPK = buyer1.PK;
			order5.JD_JS = Factory.New<ForwardingShipment>().PK;

			Action<Order, string> assertOrderNotification = (order, expectedError) =>
				{
					StringCollectionX errors = new StringCollectionX();
					collection.AddNotificationWhenAdditionalFilterNotMet(errors, order);

					string[] expectedErrors = expectedError != null ? new string[] { expectedError } : Array.Empty<string>();
					AssertContainsExactElementsInAnyOrder(expectedErrors, errors.ToArray());
				};

			assertOrderNotification(order1, null);
			assertOrderNotification(order2, "You can only choose Orders that are for the same buyer as the Pre Advice.");
			assertOrderNotification(order3, "You can only choose Orders that are not already linked to a Shipment or Shipment Pre Advice.");
			assertOrderNotification(order4, null);
			assertOrderNotification(order5, "You can only choose Orders that are not already linked to a Shipment or Shipment Pre Advice.");
		}

		#region Helper Classes

		class DummyPreadviceOrderCollection : JobShipmentPreplanningLookups.PreadviceOrderCollection
		{
			public DummyPreadviceOrderCollection(BusinessObjectFactory factory, JobShipmentPreplanning parent)
				: base(factory, parent)
			{
			}

			public new void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
			{
				base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			}
		}

		#endregion
	}
}
