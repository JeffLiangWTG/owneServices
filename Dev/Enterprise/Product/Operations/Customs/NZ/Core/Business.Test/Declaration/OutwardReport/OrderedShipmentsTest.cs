
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport.Testing
{
	using Enterprise.Freight.Forwarding.Business;
	using NUnit.Framework;

	[TestedType(typeof(OrderedShipments))]
	public class OrderedShipmentsTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			ConsolShipmentCollection unorderedShipments = new ConsolShipmentCollection(consol);

			return new OrderedShipments(unorderedShipments);
		}

		public void TestBuildOnConstruction()
		{
			CommonShipment shipment1 = shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SC0000000";
			CommonShipment shipment2 = shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SA0000000";
			CommonShipment shipment3 = shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "SB0000000";

			OrderedShipments ordered = new OrderedShipments(shipments);
			AssertEquals("Three shipments in the ordered collection", 3, ordered.Count);
		}

		public void TestSort()
		{
			CommonShipment shipment1 = shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SC0000000";
			CommonShipment shipment2 = shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SA0000000";
			CommonShipment shipment3 = shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "SB0000000";

			OrderedShipments ordered = new OrderedShipments(shipments);
			AssertEquals("First Shipment after sorted", shipment2, ordered[0]);
			AssertEquals("Second Shipment after sorted", shipment3, ordered[1]);
			AssertEquals("Third Shipment after sorted", shipment1, ordered[2]);
		}

		ForwardingConsol consol;
		ConsolShipmentCollection shipments;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			shipments = consol.Shipments;
		}
	}
}
