using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BuildBulkJobSailingHelperBusinessObjectTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestBulkCopySchedulesWhenETANotSpecified()
		{
			Sailing.Voyage.Destinations[0].JB_A_ARV = ZDateTime.Empty;
			Sailing.Voyage.Destinations[0].JB_E_ARV = ZDateTime.Empty;
			Factory.Save();

			CriteriaForTest = CreateBulkCopyCriteria(Sailing, new ZDateTime(2006, 12, 12, 12, 0, 0),
													 new ZDateTime(2006, 12, 16, 12, 0, 0),
													 "Daily", "ETA", 2, 0, "");

			BuildBulkJobSailingHelper helper = new BuildBulkJobSailingHelper(CriteriaForTest);
			helper.BulkCopySchedule();
			AssertEquals("The Schedules collection should be empty", 0, CriteriaForTest.Schedules.Count);
		}

		[TestDate(2006, 12, 12, 12, 0, 0)]
		public void TestBulkCopySchedulesDaily()
		{
			CriteriaForTest = CreateBulkCopyCriteria(Sailing, new ZDateTime(2006, 12, 12, 12, 0, 0),
													 new ZDateTime(2006, 12, 12, 12, 0, 0),
													 "Daily", "ETD", 2, 0, "");

			BuildBulkJobSailingHelper helper = new BuildBulkJobSailingHelper(CriteriaForTest);

			AssertEquals("The Schedules collection should be empty", 0, CriteriaForTest.Schedules.Count);

			helper.BulkCopySchedule();

			AssertEquals("No Schedule is created as Schedule already existed", 0, CriteriaForTest.Schedules.Count);

			CriteriaForTest = CreateBulkCopyCriteria(Sailing, new ZDateTime(2006, 12, 12, 12, 0, 0),
													 new ZDateTime(2006, 12, 16, 12, 0, 0),
													 "Daily", "ETD", 2, 0, "");

			helper = new BuildBulkJobSailingHelper(CriteriaForTest);

			AssertEquals("The Schedules collection should be empty", 0, CriteriaForTest.Schedules.Count);

			helper.BulkCopySchedule();
			AssertEquals("2 new schedules are created", 2, CriteriaForTest.Schedules.Count);

			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[0]);
			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[1]);

			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[0], new TimeSpan(2, 0, 0, 0));
			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[1], new TimeSpan(4, 0, 0, 0));

			CriteriaForTest = CreateBulkCopyCriteria(Sailing, new ZDateTime(2006, 12, 12, 12, 0, 0),
													 new ZDateTime(2006, 12, 16, 12, 0, 0),
													 "Daily", "ETA", 2, 0, "");

			helper = new BuildBulkJobSailingHelper(CriteriaForTest);

			AssertEquals("The Schedules collection should be empty", 0, CriteriaForTest.Schedules.Count);

			helper.BulkCopySchedule();
			AssertEquals("1 new schedules are created", 1, CriteriaForTest.Schedules.Count);

			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[0]);

			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[0], new TimeSpan(2, 0, 0, 0));
		}

		[TestDate(2006, 12, 12, 12, 0, 0)]
		public void TestBulkCopySchedulesMonthly()
		{
			CriteriaForTest = CreateBulkCopyCriteria(Sailing, new ZDateTime(2007, 01, 01, 12, 0, 0),
													 new ZDateTime(2007, 04, 30, 12, 0, 0),
													 "Monthly", "ETD", 1, 31, "");

			BuildBulkJobSailingHelper helper = new BuildBulkJobSailingHelper(CriteriaForTest);

			AssertEquals("The Schedules collection should be empty", 0, CriteriaForTest.Schedules.Count);

			helper.BulkCopySchedule();
			AssertEquals("2 new schedules are created", 2, CriteriaForTest.Schedules.Count);

			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[0]);
			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[1]);

			TimeSpan difference = new ZDateTime(2007, 01, 31, 12, 0, 0) - Sailing.JX_JA_E_DEP;
			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[0], difference);

			difference = new ZDateTime(2007, 03, 31, 12, 0, 0) - Sailing.JX_JA_E_DEP;
			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[1], difference);

			CriteriaForTest = CreateBulkCopyCriteria(Sailing, new ZDateTime(2007, 02, 28, 12, 0, 0),
													 new ZDateTime(2007, 04, 30, 12, 0, 0),
													 "Monthly", "ETA", 2, 30, "");

			helper = new BuildBulkJobSailingHelper(CriteriaForTest);

			AssertEquals("The Schedules collection should be empty", 0, CriteriaForTest.Schedules.Count);

			helper.BulkCopySchedule();
			AssertEquals("1 new schedules are created", 1, CriteriaForTest.Schedules.Count);

			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[0]);

			difference = new ZDateTime(2007, 04, 30, 10, 59, 0) - Sailing.JX_JB_E_ARV;
			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[0], difference);
		}

		[TestDate(2006, 12, 12, 12, 0, 0)]
		public void TestBulkCopySchedulesWeekly()
		{
			CriteriaForTest = CreateBulkCopyCriteria(Sailing, new ZDateTime(2006, 12, 23, 12, 0, 0),
													 new ZDateTime(2006, 12, 31, 12, 0, 0),
													 "Weekly", "ETD", 1, 0, "0,3,5");

			BuildBulkJobSailingHelper helper = new BuildBulkJobSailingHelper(CriteriaForTest);

			AssertEquals("The Schedules collection should be empty", 0, CriteriaForTest.Schedules.Count);

			helper.BulkCopySchedule();
			AssertEquals("4 new schedules are created", 4, CriteriaForTest.Schedules.Count);

			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[0]);
			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[1]);
			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[2]);
			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[3]);

			TimeSpan difference = new ZDateTime(2006, 12, 24, 12, 0, 0) - Sailing.JX_JA_E_DEP;
			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[0], difference);

			difference = new ZDateTime(2006, 12, 27, 12, 0, 0) - Sailing.JX_JA_E_DEP;
			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[1], difference);

			difference = new ZDateTime(2006, 12, 29, 12, 0, 0) - Sailing.JX_JA_E_DEP;
			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[2], difference);

			difference = new ZDateTime(2006, 12, 31, 12, 0, 0) - Sailing.JX_JA_E_DEP;
			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[3], difference);

			CriteriaForTest = CreateBulkCopyCriteria(Sailing, new ZDateTime(2007, 01, 12, 12, 0, 0),
													 new ZDateTime(2007, 01, 23, 12, 0, 0),
													 "Weekly", "ETA", 1, 0, "2,6");

			helper = new BuildBulkJobSailingHelper(CriteriaForTest);

			AssertEquals("The Schedules collection should be empty", 0, CriteriaForTest.Schedules.Count);

			helper.BulkCopySchedule();
			AssertEquals("4 new schedules are created", 4, CriteriaForTest.Schedules.Count);

			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[0]);
			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[1]);
			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[2]);
			AssertCopyAllRelevantInfo(CriteriaForTest.Schedules[3]);

			difference = new ZDateTime(2007, 1, 13, 10, 59, 0) - Sailing.JX_JB_E_ARV;
			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[0], difference);

			difference = new ZDateTime(2007, 1, 16, 10, 59, 0) - Sailing.JX_JB_E_ARV;
			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[1], difference);

			difference = new ZDateTime(2007, 1, 20, 10, 59, 0) - Sailing.JX_JB_E_ARV;
			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[2], difference);

			difference = new ZDateTime(2007, 1, 23, 10, 59, 0) - Sailing.JX_JB_E_ARV;
			AssertAllScheduleDatesAreCorrect(CriteriaForTest.Schedules[3], difference);
		}

		[TestDate(2007, 11, 30, 9, 0, 0)]
		public void TestBulkCopySchedulesWeekly_FirstDayPriorToETD()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "QF1";
			voyage.JV_OH_Line = Carrier.PK;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2007, 11, 30);
			origin.JA_S_DEP = new ZDateTime(2007, 11, 29);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_S_ARV = new ZDateTime(2007, 12, 2);

			voyage.GenerateSailings();
			Factory.Save();

			CriteriaForTest = CreateBulkCopyCriteria(voyage.Sailings[0], new ZDateTime(2007, 11, 30),
													 new ZDateTime(2007, 12, 30),
													 "Weekly", "ETD", 1, 0, "2");

			BuildBulkJobSailingHelper helper = new BuildBulkJobSailingHelper(CriteriaForTest);
			helper.BulkCopySchedule();
			AssertEquals("First schedule is the first tuesday after the ETD", new ZDateTime(2007, 12, 4), CriteriaForTest.Schedules[0].JX_JA_E_DEP.Date);
			AssertEquals("STD should update date and time according to bulk copy", new ZDateTime(2007, 12, 3), CriteriaForTest.Schedules[0].JX_JA_S_DEP.Date);
			AssertEquals("STA should update date and time according to bulk copy", new ZDateTime(2007, 12, 6), CriteriaForTest.Schedules[0].JX_JB_S_ARV.Date);
		}

		public void TestFilterByPorts()
		{
			ZDateTime now = ZDateTime.Now;

			string[] ports = new string[]
			{
				"NZAKL", "AUBNE", "MYBAG",
			};

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "QF123";

			for (int i = 1; i < ports.Length; i++)
			{
				VoyageOrigin origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = ports[i - 1];
				origin.JA_E_DEP = now.AddHours(i * 2);

				VoyageDestination destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = ports[i];
				destination.JB_E_ARV = now.AddHours(i * 2 + 1);
			}

			voyage.GenerateSailings();

			Factory.Save();

			JobSailing sailing = voyage.Sailings.GetSailingFromLoadAndDischarge("NZAKL", "MYBAG");

			BulkCopyCriteria criteria = new BulkCopyCriteriaForTest(sailing.PK);

			// sort to make a random bug more consistent
			criteria.Sailing.Voyage.Origins.Sort(new SortInfo(JobVoyOriginSchema.Constants.JA_RL_NKPortOfLoading, ListSortDirection.Ascending));
			criteria.Sailing.Voyage.Destinations.Sort(new SortInfo(JobVoyDestinationSchema.Constants.JB_RL_NKPortOfDischarge, ListSortDirection.Ascending));

			RepititionSelection selection = criteria.RepititionSelection;
			selection.UseDailyPattern = true;
			selection.RecurrenceInterval = 1;
			selection.FromDate = now.AddDays(4);
			selection.ToDate = now.AddDays(4);

			BuildBulkJobSailingHelper helper = new BuildBulkJobSailingHelper(criteria);
			helper.FilteredOrigin = "NZAKL";
			helper.FilteredDestination = "MYBAG";
			helper.BulkCopySchedule();

			AssertEquals("should have made 1 copy", 1, criteria.Schedules.Count);
			AssertEquals("NZAKL", criteria.Schedules[0].JX_JA_RL_NKPortOfLoading);
			AssertEquals("MYBAG", criteria.Schedules[0].JX_JB_RL_NKPortOfDischarge);
		}

		public void TestFlightMatching()
		{
			ZDateTime now = ZDateTime.Now;
			now = new ZDateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage1.JV_VoyageFlight = "QF123";

			VoyageOrigin origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUBNE";
			origin1.JA_E_DEP = now;

			VoyageDestination destination1 = voyage1.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "MYBAG";
			destination1.JB_E_ARV = now.Add(new TimeSpan(0, 3, 0, 0));

			voyage1.GenerateSailings();

			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage2.JV_VoyageFlight = "QF123";

			VoyageOrigin origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUBNE";
			origin2.JA_E_DEP = now.Add(new TimeSpan(4, 1, 0, 0)); // out by 1 hour

			VoyageDestination destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "MYBAG";
			destination2.JB_E_ARV = now.Add(new TimeSpan(4, 4, 0, 0));

			voyage2.GenerateSailings();

			Factory.Save();

			JobSailing sailing = voyage1.Sailings[0];

			BulkCopyCriteria criteria = new BulkCopyCriteriaForTest(sailing.PK);

			RepititionSelection selection = criteria.RepititionSelection;
			selection.UseDailyPattern = true;
			selection.RecurrenceInterval = 2;
			selection.FromDate = now.AddDays(2);
			selection.ToDate = now.AddDays(6);

			BuildBulkJobSailingHelper helper = new BuildBulkJobSailingHelper(criteria);
			helper.FilteredOrigin = "AUBNE";
			helper.FilteredDestination = "MYBAG";
			helper.BulkCopySchedule();

			AssertContainsExactElementsInAnyOrder(string.Format("should skip '{0}' as it already exists", now.AddDays(4)),
				new ZDateTime[] { now.AddDays(2), now.AddDays(6) },
				Array.ConvertAll(criteria.Schedules.ToArray<JobSailing>(), (s) => s.JX_JA_E_DEP));
		}

		#region Implementation

		void AssertCopyAllRelevantInfo(BaseJobSailing newSailing)
		{
			AssertEquals("Transport Mode", Sailing.Voyage.JV_AirSeaRoad, newSailing.Voyage.JV_AirSeaRoad);
			AssertEquals("Carrier", Sailing.Voyage.JV_OH_Line, newSailing.Voyage.JV_OH_Line);
			AssertEquals("Discharge Port", Sailing.JX_JB_RL_NKPortOfDischarge, newSailing.JX_JB_RL_NKPortOfDischarge);
			AssertEquals("Load Port", Sailing.JX_JA_RL_NKPortOfLoading, newSailing.JX_JA_RL_NKPortOfLoading);
			AssertEquals("Flight No", Sailing.JX_JV_VoyageFlight, newSailing.JX_JV_VoyageFlight);
		}

		void AssertAllScheduleDatesAreCorrect(BaseJobSailing newSailing, TimeSpan interval)
		{
			AssertEquals("JX_DepotReceivalCommences", Sailing.JX_DepotReceivalCommences.Add(interval), newSailing.JX_DepotReceivalCommences);
			AssertEquals("JX_DepotCutOff", Sailing.JX_DepotCutOff.Add(interval), newSailing.JX_DepotCutOff);
			AssertEquals("JX_JB_CTOCutOff", Sailing.JX_JA_CTOCutOff.Add(interval), newSailing.JX_JA_CTOCutOff);
			AssertEquals("JX_JA_DocumentaryCutoff", Sailing.JX_JA_DocumentaryCutoff.Add(interval), newSailing.JX_JA_DocumentaryCutoff);
			AssertEquals("JX_DepotStorageDate", Sailing.JX_DepotStorageDate.Add(interval), newSailing.JX_DepotStorageDate);
			AssertEquals("JX_JA_E_DEP", Sailing.JX_JA_E_DEP.Add(interval), newSailing.JX_JA_E_DEP);
			AssertEquals("JX_JB_E_ARV", Sailing.JX_JB_E_ARV.Add(interval), newSailing.JX_JB_E_ARV);
			AssertEquals("JX_JA_S_DEP", Sailing.JX_JA_S_DEP.Add(interval), newSailing.JX_JA_S_DEP);
			AssertEquals("JX_JB_S_ARV", Sailing.JX_JB_S_ARV.Add(interval), newSailing.JX_JB_S_ARV);
			Assert("JB_A_ARV should not be specified.", newSailing.JX_JB_A_ARV.IsEmpty);
			Assert("JA_A_DEP should not be specified.", newSailing.JX_JA_A_DEP.IsEmpty);
			AssertEquals("JA_ReceivalCommences", Sailing.Origin.JA_ReceivalCommences.Add(interval), newSailing.Origin.JA_ReceivalCommences);
			AssertEquals("JA_DGReceivalCommences", Sailing.Origin.JA_DGReceivalCommences.Add(interval), newSailing.Origin.JA_DGReceivalCommences);
			AssertEquals("JA_DGCutOff", Sailing.Origin.JA_DGCutOff.Add(interval), newSailing.Origin.JA_DGCutOff);
			AssertEquals("JB_AvailabilityDate", Sailing.Destination.JB_AvailabilityDate.Add(interval), newSailing.Destination.JB_AvailabilityDate);
			AssertEquals("JB_StorageDate", Sailing.Destination.JB_StorageDate.Add(interval), newSailing.Destination.JB_StorageDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Sailing = CreateSailing();
			CriteriaForTest = new BulkCopyCriteriaForTest(Sailing.PK);
		}

		BaseJobSailing CreateSailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "CX123";
			voyage.JV_OH_Line = CreateCarrier().PK;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_ReceivalCommences = new ZDateTime(2006, 12, 10, 12, 0, 0);
			origin.JA_DGReceivalCommences = new ZDateTime(2006, 12, 11, 9, 0, 0);
			origin.JA_DGCutOff = new ZDateTime(2006, 12, 11, 12, 0, 0);
			origin.JA_E_DEP = new ZDateTime(2006, 12, 12, 12, 0, 0);
			origin.JA_A_DEP = new ZDateTime(2006, 12, 12, 13, 0, 0);
			origin.JA_CutOff = ZDateTime.Today;
			origin.JA_DocumentaryCutoff = new ZDateTime(2006, 12, 13, 13, 45, 0);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = new ZDateTime(2006, 12, 13, 10, 59, 0);
			destination.JB_A_ARV = ZDateTime.Now;
			destination.JB_AvailabilityDate = new ZDateTime(2006, 12, 13, 18, 0, 0);
			destination.JB_StorageDate = new ZDateTime(2006, 12, 14, 12, 0, 0);

			voyage.GenerateSailings();
			BaseJobSailing sailing = voyage.Sailings[0];
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sailing.JX_DepotReceivalCommences = new ZDateTime(2006, 10, 12, 12, 0, 0);
			sailing.JX_DepotCutOff = sailing.JX_DepotReceivalCommences;
			sailing.JX_DepotStorageDate = sailing.JX_JA_DocumentaryCutoff;
			Factory.Save();

			return sailing;
		}

		OrgHeader CreateCarrier()
		{
			Carrier = Factory.New<OrgHeader>();
			Carrier.OH_Code = "Carrier";
			Carrier.OH_IsShippingProvider = true;
			Carrier.OH_IsAirLine = true;

			return Carrier;
		}

		BulkCopyCriteria CreateBulkCopyCriteria(BaseJobSailing sailing, ZDateTime copyDateFrom, ZDateTime copyDateTo, ZString pattern, ZString recurringBasis,
						int recurringInterval, int dayOfTheMonth, ZString daysSelected)
		{
			BulkCopyCriteria criteria = new BulkCopyCriteriaForTest(sailing.PK);

			RepititionSelection selection = criteria.RepititionSelection;

			selection.FromDate = copyDateFrom;
			selection.ToDate = copyDateTo;
			selection.UseMonthlyPattern = (pattern == "Monthly");
			selection.UseWeeklyPattern = (pattern == "Weekly");
			selection.UseDailyPattern = (pattern == "Daily");
			selection.RecurrenceInterval = recurringInterval;

			if (recurringBasis == "ETA")
			{
				selection.FromFirstETA = true;
			}
			else
			{
				selection.FromFirstETD = true;
			}

			switch (pattern)
			{
				case ("Monthly"):
					selection.DayOfTheMonth = dayOfTheMonth;
					break;

				case ("Weekly"):
					UpdateDaysSelected(daysSelected, selection.DayOfTheWeek);
					break;
			}

			return criteria;
		}

		void UpdateDaysSelected(ZString daysSelected, ZBoolDescriptionPairList list)
		{
			ZString[] days = daysSelected.Split(',');

			foreach (ZString day in days)
			{
				list[int.Parse(day)].Value = true;
			}
		}

		OrgHeader Carrier;
		BaseJobSailing Sailing;
		BulkCopyCriteria CriteriaForTest;

		#endregion
	}
}
