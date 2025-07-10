using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyBookingContainerManyToManyCollection))]
	internal class AgencyBookingContainerManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		#region TestTestingCorrectCollection
		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(AgencyBookingContainerManyToManyCollection), GetCollectionToTest().GetType());
		}

		#endregion
		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			AgencyBookingPackLine packLine = Factory.New<AgencyBooking>().OuterPackLines.AddNew();
			return new AgencyBookingContainerManyToManyCollection(packLine);
		}
		#endregion
	}
}
