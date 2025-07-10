using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyBookingContainerDependentCollection))]
	internal class AgencyBookingContainerDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTestingCorrectCollection()
		{
			AssertEquals(typeof(AgencyBookingContainerDependentCollection), GetCollectionToTest().GetType());
		}

		public void TestAllowNew()
		{
			AssertEquals(true, new AgencyBookingContainerDependentCollection(Factory.New<AgencyBooking>(), ContainerBookedStatus.Codes.Booked).AllowNew);
			AssertEquals(false, new AgencyBookingContainerDependentCollection(Factory.New<AgencyBooking>(), ContainerBookedStatus.Codes.Real).AllowNew);
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<AgencyBooking>().BookedContainers;
		}
		#endregion
	}
}
