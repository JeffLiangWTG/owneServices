using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobVoyCountryValidationTest : BusinessObjectValidationTestCase
	{
		#region NoNotifications by default

		public void TestNoNotifications()
		{
			AssertNoNotifications("No Notifications on default VoyageCountry", Country);
		}

		#endregion

		#region Implementation

		JobVoyage Voyage;
		VoyageCountry Country;

		protected override void SetUp()
		{
			base.SetUp();

			Voyage = Factory.New<JobVoyage>();
			Country = Voyage.CurrentCountry;
		}

		#endregion
	}
}
