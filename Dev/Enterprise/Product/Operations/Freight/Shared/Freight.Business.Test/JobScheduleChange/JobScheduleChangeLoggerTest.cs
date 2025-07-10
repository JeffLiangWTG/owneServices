using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	public class JobScheduleChangeLoggerTest : TestCaseWithFactory
	{
		public void TestLogDateChange_FCLReceivalCommences()
		{
			AssertDateChangeLogged(ScheduleDateTypes.Codes.FCLReceivalCommences, Origin.JA_ReceivalCommencesInfo);
		}

		public void TestLogDateChange_FCLCutOff()
		{
			AssertDateChangeLogged(ScheduleDateTypes.Codes.FCLCutOff, Origin.JA_CutOffInfo);
		}

		public void TestLogDateChange_FCLAvailable()
		{
			AssertDateChangeLogged(ScheduleDateTypes.Codes.FCLAvailable, Destination.JB_AvailabilityDateInfo);
		}

		public void TestLogDateChange_FCLStorage()
		{
			AssertDateChangeLogged(ScheduleDateTypes.Codes.FCLStorage, Destination.JB_StorageDateInfo);
		}

		public void TestLogDateChange_ETD()
		{
			AssertDateChangeLogged(ScheduleDateTypes.Codes.ETD, Origin.JA_E_DEPInfo);
		}

		public void TestLogDateChange_ATD()
		{
			AssertDateChangeLogged(ScheduleDateTypes.Codes.ATD, Origin.JA_A_DEPInfo);
		}

		public void TestLogDateChange_ETA()
		{
			AssertDateChangeLogged(ScheduleDateTypes.Codes.ETA, Destination.JB_E_ARVInfo);
		}

		public void TestLogDateChange_ATA()
		{
			AssertDateChangeLogged(ScheduleDateTypes.Codes.ATA, Destination.JB_A_ARVInfo);
		}

		public void TestLogDateChange_EmptyReceivalCommences()
		{
			AssertDateChangeLogged(ScheduleDateTypes.Codes.EmptyReceivalCommences, Origin.JA_EmptyReceivalCommencesInfo);
		}

		public void TestLogDateChange_EmptyCutOff()
		{
			AssertDateChangeLogged(ScheduleDateTypes.Codes.EmptyCutOff, Origin.JA_EmptyCutOffInfo);
		}

		public void TestLogDateChange_ReeferReceivalCommences()
		{
			AssertDateChangeLogged(ScheduleDateTypes.Codes.ReeferReceivalCommences, Origin.JA_ReeferReceivalCommencesInfo);
		}

		public void TestLogDateChange_ReeferCutOff()
		{
			AssertDateChangeLogged(ScheduleDateTypes.Codes.ReeferCutOff, Origin.JA_ReeferCutOffInfo);
		}

		public void TestLogDateChange_WhenDateHasntChanged()
		{
			Sailing.Destination.JB_AvailabilityDate = new ZDateTime(2000, 1, 1);
			Factory.Save();
			AssertNoChangesLogged();

			Sailing.Destination.JB_AvailabilityDate = new ZDateTime(2000, 2, 2);
			Factory.Save();
			AssertLastDateChangeLogged(ScheduleDateTypes.Codes.FCLAvailable, new ZDateTime(2000, 1, 1), new ZDateTime(2000, 2, 2));

			Sailing.Destination.JB_AvailabilityDate = new ZDateTime(2000, 1, 1);
			Factory.Save();
			AssertNoChangesLogged();
		}

		public void TestLogDateChange_WhenSaveFails()
		{
			Sailing.Destination.JB_AvailabilityDate = new ZDateTime(2000, 1, 1);
			Factory.Save();

			Sailing.Destination.JB_AvailabilityDate = new ZDateTime(2000, 2, 2);
			SaveFactoryAndFail();

			AssertNoChangesLogged();
		}

		public void TestLogDateChangeWithDataProvider_ETD()
		{
			var originScheduleChangeEmailSupporter = Origin as IScheduleChangeEmailSupporter;
			if (originScheduleChangeEmailSupporter != null)
			{
				AssertDateChangeLogged(ScheduleDateTypes.Codes.ETD, Origin.JA_E_DEPInfo, FreightConstants.VesselDataProviders.DAKOSY, originScheduleChangeEmailSupporter);
			}
		}

		public void TestLogDateChangeWithDataProvider_ATD()
		{
			var originScheduleChangeEmailSupporter = Origin as IScheduleChangeEmailSupporter;
			if (originScheduleChangeEmailSupporter != null)
			{
				AssertDateChangeLogged(ScheduleDateTypes.Codes.ATD, Origin.JA_A_DEPInfo, FreightConstants.VesselDataProviders.DAKOSY, originScheduleChangeEmailSupporter);
			}
		}

		public void TestLogDateChangeWithDataProvider_FCLReceivalCommences()
		{
			var originScheduleChangeEmailSupporter = Origin as IScheduleChangeEmailSupporter;
			if (originScheduleChangeEmailSupporter != null)
			{
				AssertDateChangeLogged(ScheduleDateTypes.Codes.FCLReceivalCommences, Origin.JA_ReceivalCommencesInfo, FreightConstants.VesselDataProviders.DAKOSY, originScheduleChangeEmailSupporter);
			}
		}

		public void TestLogDateChangeWithDataProvider_FCLCutOff()
		{
			var originScheduleChangeEmailSupporter = Origin as IScheduleChangeEmailSupporter;
			if (originScheduleChangeEmailSupporter != null)
			{
				AssertDateChangeLogged(ScheduleDateTypes.Codes.FCLCutOff, Origin.JA_CutOffInfo, FreightConstants.VesselDataProviders.DAKOSY, originScheduleChangeEmailSupporter);
			}
		}

		public void TestLogDateChangeWithDataProvider_EmptyReceivalCommences()
		{
			var originScheduleChangeEmailSupporter = Origin as IScheduleChangeEmailSupporter;
			if (originScheduleChangeEmailSupporter != null)
			{
				AssertDateChangeLogged(ScheduleDateTypes.Codes.EmptyReceivalCommences, Origin.JA_EmptyReceivalCommencesInfo, FreightConstants.VesselDataProviders.DAKOSY, originScheduleChangeEmailSupporter);
			}
		}

		public void TestLogDateChangeWithDataProvider_EmptyCutOff()
		{
			var originScheduleChangeEmailSupporter = Origin as IScheduleChangeEmailSupporter;
			if (originScheduleChangeEmailSupporter != null)
			{
				AssertDateChangeLogged(ScheduleDateTypes.Codes.FCLCutOff, Origin.JA_CutOffInfo, FreightConstants.VesselDataProviders.DAKOSY, originScheduleChangeEmailSupporter);
			}
		}

		public void TestLogDateChangeWithDataProvider_ReeferReceivalCommences()
		{
			var originScheduleChangeEmailSupporter = Origin as IScheduleChangeEmailSupporter;
			if (originScheduleChangeEmailSupporter != null)
			{
				AssertDateChangeLogged(ScheduleDateTypes.Codes.ReeferReceivalCommences, Origin.JA_ReeferReceivalCommencesInfo, FreightConstants.VesselDataProviders.DAKOSY, originScheduleChangeEmailSupporter);
			}
		}

		public void TestLogDateChangeWithDataProvider_ReeferCutOff()
		{
			var originScheduleChangeEmailSupporter = Origin as IScheduleChangeEmailSupporter;
			if (originScheduleChangeEmailSupporter != null)
			{
				AssertDateChangeLogged(ScheduleDateTypes.Codes.ReeferCutOff, Origin.JA_ReeferCutOffInfo, FreightConstants.VesselDataProviders.DAKOSY, originScheduleChangeEmailSupporter);
			}
		}

		public void TestLogDateChangeWithDataProvider_ETA()
		{
			var destinationScheduleChangeEmailSupporter = Destination as IScheduleChangeEmailSupporter;
			if (destinationScheduleChangeEmailSupporter != null)
			{
				AssertDateChangeLogged(ScheduleDateTypes.Codes.ETA, Destination.JB_E_ARVInfo, FreightConstants.VesselDataProviders.OneStop, destinationScheduleChangeEmailSupporter);
			}
		}

		public void TestLogDateChangeWithDataProvider_ATA()
		{
			var destinationScheduleChangeEmailSupporter = Destination as IScheduleChangeEmailSupporter;
			if (destinationScheduleChangeEmailSupporter != null)
			{
				AssertDateChangeLogged(ScheduleDateTypes.Codes.ATA, Destination.JB_A_ARVInfo, FreightConstants.VesselDataProviders.OneStop, destinationScheduleChangeEmailSupporter);
			}
		}

		public void TestLogDateChangeWithDataProvider_FCLAvailable()
		{
			var destinationScheduleChangeEmailSupporter = Destination as IScheduleChangeEmailSupporter;
			if (destinationScheduleChangeEmailSupporter != null)
			{
				AssertDateChangeLogged(ScheduleDateTypes.Codes.FCLAvailable, Destination.JB_AvailabilityDateInfo, FreightConstants.VesselDataProviders.OneStop, destinationScheduleChangeEmailSupporter);
			}
		}

		public void TestLogDateChangeWithDataProvider_FCLStorage()
		{
			var destinationScheduleChangeEmailSupporter = Destination as IScheduleChangeEmailSupporter;
			if (destinationScheduleChangeEmailSupporter != null)
			{
				AssertDateChangeLogged(ScheduleDateTypes.Codes.FCLStorage, Destination.JB_StorageDateInfo, FreightConstants.VesselDataProviders.OneStop, destinationScheduleChangeEmailSupporter);
			}
		}

		void SaveFactoryAndFail()
		{
			OrgHeader failToSave = Factory.New<OrgHeader>();
			failToSave.OH_Code = "abc";
			failToSave.OH_ScreeningStatus = "abc"; // has a check constraint
			try
			{
				Factory.Save();
				Fail("Should not reach this point because org headers has invalid column. That org has no relevance to this test other than to ensure saving will fail.");
			}
			catch (ZSaveException)
			{
			}
		}

		#region Implementation

		void AssertDateChangeLogged(string dateType, ZPropertyInfo dateProperty)
		{
			dateProperty.Value = new ZDateTime(2000, 1, 1);
			Factory.Save();
			AssertNoChangesLogged();

			dateProperty.Value = new ZDateTime(2000, 2, 2);
			Factory.Save();
			AssertLastDateChangeLogged(dateType, new ZDateTime(2000, 1, 1), new ZDateTime(2000, 2, 2));

			dateProperty.Value = new ZDateTime(2000, 3, 3);
			Factory.Save();
			AssertLastDateChangeLogged(dateType, new ZDateTime(2000, 1, 1), new ZDateTime(2000, 3, 3));
		}

		void AssertDateChangeLogged(string dateType, ZPropertyInfo dateProperty, ZString dataProvider, IScheduleChangeEmailSupporter scheduleChangeEmailSupporter)
		{
			scheduleChangeEmailSupporter.AddOrUpdateJobScheduleChange(dateType, dataProvider);

			dateProperty.Value = new ZDateTime(2000, 1, 1);
			Factory.Save();
			AssertNoChangesLogged();

			dateProperty.Value = new ZDateTime(2000, 2, 2);
			Factory.Save();
			AssertDateChangeLogged(dateType, new ZDateTime(2000, 1, 1), new ZDateTime(2000, 2, 2), dataProvider);

			dateProperty.Value = new ZDateTime(2000, 3, 3);
			Factory.Save();
			AssertDateChangeLogged(dateType, new ZDateTime(2000, 1, 1), new ZDateTime(2000, 3, 3), dataProvider);
		}

		void AssertNoChangesLogged()
		{
			ZQuery query = new ZQuery();
			query.OrderBy = JobScheduleChangeSchema.E7_ChangedAt.Name + " DESC";
			query.AddToFilter(JobScheduleChangeSchema.E7_ChangedAt, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddSeconds(-10));

			JobScheduleChange lastChange = Factory.LoadTop1<JobScheduleChange>(query);
			AssertNull("Expected no sailing date changes", lastChange);
		}

		void AssertLastDateChangeLogged(string dateType, ZDateTime previousValue, ZDateTime updatedValue)
		{
			AssertLastDateChangeLogged(Factory, dateType, previousValue, updatedValue);
		}

		void AssertDateChangeLogged(string dateType, ZDateTime previousValue, ZDateTime updatedValue, ZString dataProvider)
		{
			AssertDateChangeLogged(Factory, dateType, previousValue, updatedValue, dataProvider);
		}

		public static void AssertLastDateChangeLogged(BusinessObjectFactory factory, string dateType, ZDateTime previousValue, ZDateTime updatedValue)
		{
			var query = new ZQuery();
			query.OrderBy = JobScheduleChangeSchema.E7_ChangedAt.Name + " DESC";
			query.AddToFilter(JobScheduleChangeSchema.E7_ChangedAt, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddSeconds(-10));

			var lastChange = factory.LoadTop1<JobScheduleChange>(query);
			AssertNotNull("Expected a sailing date change", lastChange);

			AssertEquals("E7_DateType", dateType, lastChange.E7_DateType);
			AssertEquals("E7_PreviousValue", previousValue, lastChange.E7_PreviousValue);
			AssertEquals("E7_UpdatedValue", updatedValue, lastChange.E7_UpdatedValue);
		}

		public static void AssertDateChangeLogged(BusinessObjectFactory factory, string dateType, ZDateTime previousValue, ZDateTime updatedValue, ZString dataProvider)
		{
			var query = new ZQuery();
			query.OrderBy = JobScheduleChangeSchema.E7_ChangedAt.Name + " DESC";
			query.AddToFilter(JobScheduleChangeSchema.E7_ChangedAt, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddSeconds(-10));
			query.AddToFilter(JobScheduleChangeSchema.E7_DateType, dateType);

			var scheduleChange = factory.LoadTop1<JobScheduleChange>(query);
			AssertNotNull("Expected a sailing date change", scheduleChange);

			AssertEquals("E7_PreviousValue", previousValue, scheduleChange.E7_PreviousValue);
			AssertEquals("E7_UpdatedValue", updatedValue, scheduleChange.E7_UpdatedValue);
			AssertEquals("E7_DataProvider", dataProvider, scheduleChange.E7_DataProvider);
		}

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

		JobSailing Sailing
		{
			get
			{
				if (sailing == null)
				{
					VoyageOrigin origin = this.Origin;
					VoyageDestination destination = this.Destination;
					sailing = Voyage.Sailings.AddNew();
					sailing.JX_JA = origin.PK;
					sailing.JX_JB = destination.PK;
				}
				return sailing;
			}
		}
		JobSailing sailing;

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

		#endregion
	}
}
