using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business
{
	[TestedType(typeof(AgencyBookingPackLineManyToManyCollection))]
	internal class AgencyBookingPackLineManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<AgencyBooking>().BookedContainers.AddNew().PackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<AgencyBooking>().OuterPackLines.AddNew();
		}
		#endregion
	}
}
