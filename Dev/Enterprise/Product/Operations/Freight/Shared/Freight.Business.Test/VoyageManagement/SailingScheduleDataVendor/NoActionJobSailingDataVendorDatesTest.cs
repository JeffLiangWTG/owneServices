using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class NoActionJobSailingDataVendorDatesTest : TestCaseWithFactory
	{
		public void TestIsEnabled()
		{
			AssertEquals("Country with no data vendor is never enabled", false, NoActionVendor.IsEnabled);
		}

		public void TestIsVendorDataCurrent()
		{
			AssertEquals("Country with no data vendor never has current data", false, NoActionVendor.IsVendorDataCurrent);
		}

		public void TestStatus()
		{
			AssertEquals("Country with no data vendor has no data vendor status available", "", NoActionVendor.Status);
		}

		[ExpectNoExceptions]
		public void TestUpdateAllVoyageSailings()
		{
			NoActionVendor.UpdateAllVoyageSailings(Voyage);
		}

		[ExpectNoExceptions]
		public void TestUpdateVoyageOrigin()
		{
			NoActionVendor.UpdateVoyageOrigin(Origin);
		}

		[ExpectNoExceptions]
		public void TestUpdateVoyageDestination()
		{
			NoActionVendor.UpdateVoyageDestination(Destination);
		}

		#region Implementation

		JobVoyage Voyage
		{
			get
			{
				if (fVoyage == null)
				{
					fVoyage = Factory.New<JobVoyage>();
					fVoyage.Origins.AddNew();
					fVoyage.Destinations.AddNew();
				}
				return fVoyage;
			}
		}
		JobVoyage fVoyage;

		VoyageOrigin Origin
		{
			get { return Voyage.Origins[0]; }
		}

		VoyageDestination Destination
		{
			get { return Voyage.Destinations[0]; }
		}

		NoActionSailingScheduleDataVendor NoActionVendor
		{
			get
			{
				if (fNoActionVendor == null)
				{
					fNoActionVendor = new NoActionSailingScheduleDataVendor();
				}
				return fNoActionVendor;
			}
		}
		NoActionSailingScheduleDataVendor fNoActionVendor;

		#endregion
	}
}
