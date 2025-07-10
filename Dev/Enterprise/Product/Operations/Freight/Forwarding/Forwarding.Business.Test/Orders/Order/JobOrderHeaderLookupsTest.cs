using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class JobOrderHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestShipmentPrePlannings()
		{
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer3 = Factory.NewWithValidTestData<OrgHeader>();

			var preAdvice1 = Factory.New<JobShipmentPreplanning>();
			preAdvice1.BuyerPK = buyer1.PK;

			var preAdvice2 = Factory.New<JobShipmentPreplanning>();
			preAdvice2.BuyerPK = buyer2.PK;

			var order = Factory.New<Order>();
			var collection = order.Lookups.ShipmentPrePlannings;
			collection.Load();
			AssertEquals(2, collection.Count);
			AssertEquals(true, collection.Contains(preAdvice1));
			AssertEquals(true, collection.Contains(preAdvice2));

			order.BuyerPK = buyer1.PK;
			collection = order.Lookups.ShipmentPrePlannings;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals(preAdvice1, collection[0]);

			order.BuyerPK = buyer2.PK;
			collection = order.Lookups.ShipmentPrePlannings;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals(preAdvice2, collection[0]);

			order.BuyerPK = buyer3.PK;
			collection = order.Lookups.ShipmentPrePlannings;
			collection.Load();
			AssertEquals(0, collection.Count);
		}

		public void TestShipmentPrePlanningsErrors()
		{
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();

			var collection = new DummyJobShipmentPreplanningCollection(Factory, order);

			var preAdvice1 = collection.AddNew();
			preAdvice1.BuyerPK = buyer1.PK;

			var preAdvice2 = collection.AddNew();
			preAdvice2.BuyerPK = buyer2.PK;

			var errors = new StringCollectionX();

			collection.AddNotificationWhenAdditionalFilterNotMet(errors, preAdvice1);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), errors.ToArray());

			collection.AddNotificationWhenAdditionalFilterNotMet(errors, preAdvice2);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), errors.ToArray());

			order.BuyerPK = buyer1.PK;

			collection.AddNotificationWhenAdditionalFilterNotMet(errors, preAdvice1);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), errors.ToArray());

			collection.AddNotificationWhenAdditionalFilterNotMet(errors, preAdvice2);
			AssertContainsExactElementsInAnyOrder(
				new string[] { "You can only choose Pre Advices that are for the same buyer as the Order." },
				errors.ToArray());
		}

		#region Helper Classes

		class DummyJobShipmentPreplanningCollection : JobOrderHeaderLookups.OrderPreAdviseCollection
		{
			public DummyJobShipmentPreplanningCollection(BusinessObjectFactory factory, Order parent)
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
