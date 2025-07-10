using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders.Matchers
{
	class ShipmentMatchKeyComparerTest : TestCaseWithFactory
	{
		public void Test_Null()
		{
			var comparer = new ShipmentMatchKeyComparer() as IEqualityComparer<ShipmentMatchKey>;
			AssertEquals(true, comparer.Equals(null, null));
			AssertEquals(true, comparer.Equals(new ShipmentMatchKey(), new ShipmentMatchKey()));
			AssertEquals(false, comparer.Equals(null, new ShipmentMatchKey()));
			AssertEquals(false, comparer.Equals(new ShipmentMatchKey(), null));

			AssertEquals(comparer.GetHashCode(new ShipmentMatchKey()), comparer.GetHashCode(new ShipmentMatchKey()));
			AssertEquals(comparer.GetHashCode(null), comparer.GetHashCode(null));
		}

		public void Test_PlanningShipmentTool()
		{
			var comparer = new ShipmentMatchKeyComparer() as IEqualityComparer<ShipmentMatchKey>;

			var createKey = (ForwardingShipment shipment, ForwardingConsol allocatedConsol) =>
			{
				return new ShipmentMatchKey { PlannedShipment = shipment, PackedConsol = allocatedConsol };
			};

			var shipment1 = Factory.New<ForwardingShipment>();
			var consol1 = shipment1.Consols.AddNew();
			var shipment2 = Factory.New<ForwardingShipment>();
			var consol2 = shipment2.Consols.AddNew();

			var consol3 = Factory.New<ForwardingConsol>();

			CombineAssertions("check Equals and GetHashCode", () =>
			{
				AssertEquals("should be equal when allocating the same planning shipment to same attached consol", true, comparer.Equals(createKey(shipment1, consol1), createKey(shipment1, consol1)));
				AssertEquals("should be equal when allocating the same planning shipment to same unattached consol", true, comparer.Equals(createKey(shipment1, consol3), createKey(shipment1, consol3)));
				AssertEquals("should be not equal when allocating the same planning shipment to separately attached consol and unattached consol", false, comparer.Equals(createKey(shipment1, consol1), createKey(shipment1, consol2)));

				AssertEquals("should be not equal when allocating different planning shipments to separately attached consols", false, comparer.Equals(createKey(shipment1, consol1), createKey(shipment2, consol2)));
				AssertEquals("should be not equal when allocating different planning shipments to separately unattached consols", false, comparer.Equals(createKey(shipment1, consol3), createKey(shipment2, consol3)));

				AssertEquals("should be not equal when allocating planning shipment and no-planning shipment", false, comparer.Equals(createKey(shipment1, consol3), createKey(null, null)));

				AssertEquals("should have same hash when allocating the same planning shipment to same attached consol", comparer.GetHashCode(createKey(shipment1, consol1)), comparer.GetHashCode(createKey(shipment1, consol1)));
				AssertEquals("should have same hash when allocating the same planning shipment to same unattached consol", comparer.GetHashCode(createKey(shipment1, consol3)), comparer.GetHashCode(createKey(shipment1, consol3)));
				AssertEquals("should has same hash when allocating the same planning shipment to separately attached consol and unattached consol", comparer.GetHashCode(createKey(shipment1, consol1)), comparer.GetHashCode(createKey(shipment1, consol2)));
				AssertNotEquals("should have different hash when allocating different planning shipments to separately attached consol and unattached consols", comparer.GetHashCode(createKey(shipment1, consol1)), comparer.GetHashCode(createKey(shipment2, consol1)));
				AssertNotEquals("should have different hash when allocating planning shipment and no-planning shipment", comparer.GetHashCode(createKey(shipment1, consol1)), comparer.GetHashCode(createKey(null, null)));
			});
		}

		public void Test_MatchFields()
		{
			var comparer = new ShipmentMatchKeyComparer() as IEqualityComparer<ShipmentMatchKey>;

			var key1 = new ShipmentMatchKey();
			var key2 = new ShipmentMatchKey();

			AssertEquals(true, comparer.Equals(key1, key2));

			CheckField(key1, key2, key => key.LoadMode = "X1", key => key.LoadMode = "X2");

			var supplierBooking1 = Factory.New<JobSupplierBooking>();
			var supplierBooking2 = Factory.New<JobSupplierBooking>();
			CheckField(key1, key2, key => key.SupplierBooking = supplierBooking1, key => key.SupplierBooking = supplierBooking2);

			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			CheckField(key1, key2, key => key.PackedConsol = consol1, key => key.PackedConsol = consol2);

			var consigneeAddress1 = Factory.New<OrgHeader>().MainAddress;
			var consigneeAddress2 = Factory.New<OrgHeader>().MainAddress;
			CheckField(key1, key2, key => key.ConsigneeAddress = consigneeAddress1, key => key.ConsigneeAddress = consigneeAddress2);
		}

		static void CheckField(ShipmentMatchKey key1, ShipmentMatchKey key2, Action<ShipmentMatchKey> action1, Action<ShipmentMatchKey> action2)
		{
			var comparer = new ShipmentMatchKeyComparer() as IEqualityComparer<ShipmentMatchKey>;
			AssertEquals(true, comparer.Equals(key1, key2));
			AssertEquals(comparer.GetHashCode(key1), comparer.GetHashCode(key2));

			action1(key1);
			AssertEquals(false, comparer.Equals(key1, key2));

			action2(key2);
			AssertEquals(false, comparer.Equals(key1, key2));

			action1(key2);
			AssertEquals(true, comparer.Equals(key1, key2));
			AssertEquals(comparer.GetHashCode(key1), comparer.GetHashCode(key2));
		}
	}
}
