using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingPackLineCollection))]
	public class TrackingPackLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			TrackingShipment shipment = Factory.New<TrackingShipment>();
			return shipment.OuterPackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			TrackingPackLine packLine = Factory.New<TrackingPackLine>();
			packLine.JL_FreightMode = FreightConstants.OuterPackType;

			return packLine;
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(TrackingPackLineCollection), GetCollectionToTest().GetType());
		}
	}
}
