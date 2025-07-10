using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyBookingContainer))]
	internal class AgencyBookingContainerBOTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<AgencyBooking>().BookedContainers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.NewWithValidTestData<AgencyBookingContainer>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var booking = factory.New<AgencyBooking>();
			return booking.BookedContainers.AddNew();
		}
		#endregion
	}
}
