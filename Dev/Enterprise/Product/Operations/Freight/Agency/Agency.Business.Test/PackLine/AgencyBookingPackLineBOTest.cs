using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyBookingPackLine))]
	internal class AgencyBookingPackLineBOTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<AgencyBooking>().OuterPackLines.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.New<AgencyBooking>().OuterPackLines.AddNew();
		}
		#endregion
	}
}
