using System.Linq;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentRoutingCollectionTest : BaseFreightTest
	{
		#region TestDeleteConsol

		public void TestDeleteConsol()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonConsol consol = shipment.Consols.AddNew();

			ShipmentRoutingCollection collection = new ShipmentRoutingCollection(shipment);

			AssertEquals("precondition:", 1, collection.Count);

			consol.Delete();
			AssertEquals("consol deleted", 0, collection.Count);
		}

		#endregion

		#region TestMonitorShipmentTransports

		public void TestMonitorShipmentTransports()
		{
			Transport transport1 = Shipment.Transports.AddNew();
			AssertEquals("count", 1, Collection.Count);
			AssertEquals("Contains transport1", true, Collection.Contains(transport1));

			Transport transport2 = Shipment.Transports.AddNew();
			AssertEquals("count", 2, Collection.Count);
			AssertEquals("Contains transport2", true, Collection.Contains(transport2));
		}

		public void TestMonitorShipmentTransportsMasterChanged()
		{
			var shipment = Factory.New<CommonShipment>();
			var shipment1 = shipment.CoLoadShipments.AddNew();
			var shipment2 = shipment.CoLoadShipments.AddNew();
			var shipment11 = shipment1.CoLoadShipments.AddNew();
			var shipment21 = shipment2.CoLoadShipments.AddNew();
			var shipment111 = shipment11.CoLoadShipments.AddNew();

			var transport = shipment.Transports.AddNew();
			var transport1 = shipment1.Transports.AddNew();
			var transport2 = shipment2.Transports.AddNew();
			var transport11 = shipment11.Transports.AddNew();
			var transport21 = shipment21.Transports.AddNew();
			var transport111 = shipment111.Transports.AddNew();

			AssertEquals(1, shipment111.Transports.Count);
			AssertEquals(transport111.PK, shipment111.Transports[0].PK);

			AssertEquals(4, shipment111.TransportsIncludingRelated.Count);
			AssertEquals(GetTransportsAsString(new[] { transport.PK, transport1.PK, transport11.PK, transport111.PK }), GetTransportsAsString(shipment111.TransportsIncludingRelated));

			shipment111.JS_JS_ColoadMasterShipment = shipment21.PK;

			AssertEquals(1, shipment111.Transports.Count);
			AssertEquals(transport111.PK, shipment111.Transports[0].PK);

			AssertEquals(4, shipment111.TransportsIncludingRelated.Count);
			AssertEquals(GetTransportsAsString(new[] { transport.PK, transport2.PK, transport21.PK, transport111.PK }), GetTransportsAsString(shipment111.TransportsIncludingRelated));

			shipment21.JS_JS_ColoadMasterShipment = shipment1.PK;

			AssertEquals(1, shipment111.Transports.Count);
			AssertEquals(transport111.PK, shipment111.Transports[0].PK);

			AssertEquals(4, shipment111.TransportsIncludingRelated.Count);
			AssertEquals(GetTransportsAsString(new[] { transport.PK, transport1.PK, transport21.PK, transport111.PK }), GetTransportsAsString(shipment111.TransportsIncludingRelated));
		}

		#endregion

		#region TestMonitorConsolTransports

		public void TestMonitorConsolTransports()
		{
			CommonConsol consol1 = Factory.New<CommonConsol>();
			Transport transport1 = consol1.Transports[0];
			AssertEquals("count", 0, Collection.Count);

			Shipment.Consols.Add(consol1);
			AssertEquals("count", 1, Collection.Count);
			AssertEquals("contains transports from consol1", true, Collection.Contains(transport1));

			Transport transport2 = consol1.Transports.AddNew();
			AssertEquals("count", 2, Collection.Count);
			AssertEquals("contains transports from consol1", true, Collection.Contains(transport2));

			CommonConsol consol2 = Shipment.Consols.AddNew();
			Transport transport3 = consol2.Transports[0];
			AssertEquals("count", 3, Collection.Count);
			AssertEquals("contains transports from consol2", true, Collection.Contains(transport3));

			Shipment.Consols.Remove(consol1);
			AssertEquals("count", 1, Collection.Count);
			AssertEquals("not contains transports from consol1", false, Collection.Contains(transport1));
			AssertEquals("not contains transports from consol2", false, Collection.Contains(transport2));

			Transport transport4 = consol1.Transports.AddNew();
			AssertEquals("collection shold not be monitored anymore.", 1, Collection.Count);
		}

		#endregion

		#region Implementation

		#region Collection

		string GetTransportsAsString(ZGuid[] collection)
		{
			return collection.Select(e => e.ToString()).OrderBy(e => e).Aggregate((current, next) => current + ", " + next);
		}

		string GetTransportsAsString(RoutingCollection collection)
		{
			return collection.Select(t => t.PK.ToString()).OrderBy(t => t).Aggregate((current, next) => current + ", " + next);
		}

		ShipmentRoutingCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new ShipmentRoutingCollection(Shipment);
				}

				return collection;
			}
		}

		ShipmentRoutingCollection collection;

		#endregion

		#region Shipment

		CommonShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.New<CommonShipment>();
				}

				return shipment;
			}
		}

		CommonShipment shipment;

		#endregion

		#endregion
	}
}
