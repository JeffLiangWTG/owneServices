using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ShipmentCollectionView))]
	sealed class ShipmentCollectionViewTest : BusinessObjectCollectionViewTestCase<ShipmentCollectionView>
	{
		public void TestFiltering()
		{
			DummyBusinessObjectCollection dummyCollection = new DummyBusinessObjectCollection(Factory);
			dummyCollection.AddNew();
			dummyCollection.AddNew();

			ShipmentCollectionView shipmentsView = new ShipmentCollectionView(dummyCollection, (shipment) => shipment.IsAir);
			AssertContainsExactElementsInAnyOrder("Not a shipment collection to filter", System.Array.Empty<CommonShipment>(), shipmentsView);

			CommonShipment shipment1 = CreateShipment(Constants.TransportModes.Air, "AAA");
			CommonShipment shipment2 = CreateShipment(Constants.TransportModes.Air, "ABC");
			CommonShipment shipment3 = CreateShipment(Constants.TransportModes.Air, "Brolli");
			CommonShipment shipment4 = CreateShipment(Constants.TransportModes.Sea, "Arrgh");
			CommonShipment shipment5 = CreateShipment(Constants.TransportModes.Sea, "Dee");

			ShipmentCollection shipments = new ShipmentCollection(Factory);
			shipments.Add(shipment1);
			shipments.Add(shipment2);
			shipments.Add(shipment3);
			shipments.Add(shipment4);
			shipments.Add(shipment5);

			shipmentsView = new ShipmentCollectionView(shipments, null);
			AssertContainsExactElementsInAnyOrder("No predicate => no shipments, that's the rule", System.Array.Empty<CommonShipment>(), shipmentsView);

			shipmentsView = new ShipmentCollectionView(shipments, (shipment) => shipment.IsAir);
			AssertContainsExactElementsInAnyOrder(new CommonShipment[] { shipment1, shipment2, shipment3 }, shipmentsView);

			shipmentsView = new ShipmentCollectionView(shipments, (shipment) => shipment.IsSea);
			AssertContainsExactElementsInAnyOrder(new CommonShipment[] { shipment4, shipment5 }, shipmentsView);

			shipmentsView = new ShipmentCollectionView(shipments, (shipment) => shipment.JS_UniqueConsignRef.StartsWith("A"));
			AssertContainsExactElementsInAnyOrder(new CommonShipment[] { shipment1, shipment2, shipment4 }, shipmentsView);

			shipmentsView = new ShipmentCollectionView(shipments, (shipment) => shipment.JS_UniqueConsignRef.StartsWith("B"));
			AssertContainsExactElementsInAnyOrder(new CommonShipment[] { shipment3 }, shipmentsView);

			shipmentsView = new ShipmentCollectionView(shipments, (shipment) => shipment.JS_UniqueConsignRef.StartsWith("C"));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<CommonShipment>(), shipmentsView);

			CommonShipment shipment6 = CreateShipment(Constants.TransportModes.Sea, "Cool");
			shipments.Add(shipment6);

			shipment5.JS_UniqueConsignRef = "Cucumber";
			AssertContainsExactElementsInAnyOrder(new CommonShipment[] { shipment5, shipment6 }, shipmentsView);
		}

		#region Implementation

		CommonShipment CreateShipment(string transportMode, string uniqueRef)
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_UniqueConsignRef = uniqueRef;

			return shipment;
		}

		protected override ShipmentCollectionView GetCollectionToTest()
		{
			ShipmentCollection shipments = new ShipmentCollection(Factory);
			return new ShipmentCollectionView(shipments, (shipment) => true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CommonShipment>();
		}

		#endregion
	}
}
