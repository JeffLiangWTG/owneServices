using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobScheduleChange))]
	sealed class JobScheduleChangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParent()
		{
			Origin.JA_E_DEP = new ZDateTime(2000, 1, 1);
			Destination.JB_E_ARV = new ZDateTime(2000, 1, 1);
			Factory.Save();

			Origin.JA_E_DEP = new ZDateTime(2000, 2, 2);
			Destination.JB_E_ARV = new ZDateTime(2000, 2, 2);
			Factory.Save();

			JobScheduleChange originDateChange = Factory.LoadTop1<JobScheduleChange>(new ZQuery(JobScheduleChangeSchema.E7_DateType, ScheduleDateTypes.Codes.ETD));
			JobScheduleChange destinationDateChange = Factory.LoadTop1<JobScheduleChange>(new ZQuery(JobScheduleChangeSchema.E7_DateType, ScheduleDateTypes.Codes.ETA));

			AssertEquals("Parent is VoyageOrigin", Origin, originDateChange.Parent);
			AssertEquals("Parent is VoyageDestination", Destination, destinationDateChange.Parent);
		}

		public void TestVoyage()
		{
			Origin.JA_E_DEP = new ZDateTime(2000, 1, 1);
			Destination.JB_E_ARV = new ZDateTime(2000, 1, 1);
			Factory.Save();

			Origin.JA_E_DEP = new ZDateTime(2000, 2, 2);
			Destination.JB_E_ARV = new ZDateTime(2000, 2, 2);
			Factory.Save();

			JobScheduleChange originDateChange = Factory.LoadTop1<JobScheduleChange>(new ZQuery(JobScheduleChangeSchema.E7_DateType, ScheduleDateTypes.Codes.ETD));
			JobScheduleChange destinationDateChange = Factory.LoadTop1<JobScheduleChange>(new ZQuery(JobScheduleChangeSchema.E7_DateType, ScheduleDateTypes.Codes.ETA));

			AssertEquals("For VoyageOrigin date change", Voyage, originDateChange.Voyage);
			AssertEquals("For VoyageDestination date change", Voyage, destinationDateChange.Voyage);
		}

		public void TestDateTypeDescription()
		{
			JobScheduleChange change = Factory.New<JobScheduleChange>();
			change.E7_DateType = ScheduleDateTypes.Codes.FCLAvailable;
			AssertEquals("Availability", change.DateTypeDescription);
		}

		#region Implementation

		JobVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					voyage = Factory.New<JobVoyage>();
				}
				return voyage;
			}
		}
		JobVoyage voyage;

		VoyageOrigin Origin
		{
			get
			{
				if (origin == null)
				{
					origin = Voyage.Origins.AddNew();
				}
				return origin;
			}
		}
		VoyageOrigin origin;

		VoyageDestination Destination
		{
			get
			{
				if (destination == null)
				{
					destination = Voyage.Destinations.AddNew();
				}
				return destination;
			}
		}
		VoyageDestination destination;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<JobScheduleChange>();

			var now = ZDateTime.Now;
			result.E7_PreviousValue = now.AddHours(-5);
			result.E7_UpdatedValue = now;
			result.E7_ParentID = ZGuid.NewZGuid();
			result.E7_ParentTableCode = "JX";

			return result;
		}

		#endregion
	}
}
