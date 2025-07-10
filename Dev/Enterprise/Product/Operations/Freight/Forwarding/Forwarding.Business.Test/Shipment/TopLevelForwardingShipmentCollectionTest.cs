using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(TopLevelForwardingShipmentCollection))]
	sealed class TopLevelForwardingShipmentCollectionTest : TopLevelShipmentCollectionBOTest<TopLevelForwardingShipmentCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			return shipment;
		}

		protected override TopLevelForwardingShipmentCollection GetCollectionToTest()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			return new TopLevelForwardingShipmentCollection(new ConsolShipmentCollection(consol), consol);
		}
	}
}
