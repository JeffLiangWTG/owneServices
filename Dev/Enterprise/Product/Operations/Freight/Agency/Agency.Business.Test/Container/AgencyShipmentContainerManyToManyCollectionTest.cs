using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentContainerManyToManyCollection))]
	internal class AgencyShipmentContainerManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			AgencyShipmentPackLine packLine = Factory.New<AgencyShipment>().OuterPackLines.AddNew();
			return new AgencyShipmentContainerManyToManyCollection(packLine);
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(AgencyShipmentContainerManyToManyCollection), GetCollectionToTest().GetType());
		}
	}
}
