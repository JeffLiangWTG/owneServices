using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbWorkTimeCollection))]
	sealed class GlbWorkTimeCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbWorkTimeCollection>
	{
		#region Test Database Save

		public void TestSettingDefaultHoursOnNewStaffStoresDataInDb()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;

			DataRegistry.Instance.RawRegistry.StandardWorkingHours.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "40:00");

			Factory.Save();

			var mondayFilter = new ZQuery(GlbWorkTimeSchema.GW_ParentID, staff.PK);
			mondayFilter.AddToFilter(GlbWorkTimeSchema.GW_DayOfWeek, "MON");
			mondayFilter.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, "GS");

			var mondayDefault = Factory.Load<GlbWorkTime>(mondayFilter);

			CombineAssertions(() =>
			{
				AssertEquals(2, mondayDefault.Length);

				AssertEquals(GlbWorkTime.CreateTime(8, 30), mondayDefault[0].GW_StartTime);
				AssertEquals(GlbWorkTime.CreateTime(12, 30), mondayDefault[0].GW_EndTime);

				AssertEquals(GlbWorkTime.CreateTime(13, 00), mondayDefault[1].GW_StartTime);
				AssertEquals(GlbWorkTime.CreateTime(17, 00), mondayDefault[1].GW_EndTime);

				AssertEquals("                 ******** ********", staff.WorkTimes.MondayWorkingHours);
			});

			var sundayFilter = new ZQuery(GlbWorkTimeSchema.GW_ParentID, staff.PK);
			sundayFilter.AddToFilter(GlbWorkTimeSchema.GW_DayOfWeek, "SUN");
			sundayFilter.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, "GS");

			var sundayDefault = Factory.LoadTop1<GlbWorkTime>(sundayFilter);

			AssertNull(sundayDefault);
			AssertEquals("", staff.WorkTimes.SundayWorkingHours);
		}

		public void TestSettingDefaultHoursOnNewStaffStoresDataInDb_AllowedSaturday()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;

				DataRegistry.Instance.RawRegistry.StandardWorkingHours.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "80:00");

				Factory.Save();

				var satdayFilter = new ZQuery(GlbWorkTimeSchema.GW_ParentID, staff.PK);
				satdayFilter.AddToFilter(GlbWorkTimeSchema.GW_DayOfWeek, "Sat");
				satdayFilter.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, "GS");

				var saturdayDefault = Factory.Load<GlbWorkTime>(satdayFilter);

				CombineAssertions(() =>
				{
					AssertEquals(2, saturdayDefault.Length);
					AssertEquals(GlbWorkTime.CreateTime(8, 30), saturdayDefault[0].GW_StartTime);
					AssertEquals(GlbWorkTime.CreateTime(12, 30), saturdayDefault[0].GW_EndTime);

					AssertEquals(GlbWorkTime.CreateTime(13, 00), saturdayDefault[1].GW_StartTime);
					AssertEquals(GlbWorkTime.CreateTime(22, 00), saturdayDefault[1].GW_EndTime);

					AssertEquals("                 ******** ******************", staff.WorkTimes.SaturdayWorkingHours);
				});
			}
		}

		public void TestWillRetrieveNewWorkTimeAssignedToDb()
		{
			var staff = Factory.New<GlbStaff>();

			// Delete Defaulted Working hours
			var workTimesToDelete = Factory.Load<GlbWorkTime>(new ZQuery(GlbWorkTimeSchema.GW_ParentID, staff.PK));
			foreach (var workTime in workTimesToDelete)
			{
				workTime.Delete();
			}

			var newWorkTime = Factory.New<GlbWorkTime>();
			newWorkTime.GW_ParentID = staff.PK;
			newWorkTime.GW_ParentTableCode = "GS";
			newWorkTime.GW_DayOfWeek = "MON";
			newWorkTime.GW_StartTime = GlbWorkTime.CreateTime(0, 0);
			newWorkTime.GW_EndTime = GlbWorkTime.CreateTime(0, 0, isNextDay: true);
			Factory.Save();

			var loaded = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staff.PK));

			AssertEquals(loaded.WorkTimes.MondayWorkingHours, "************************************************");
		}

		public void TestUpdateWorkingHoursOnNewStaffStoresDataInDb()
		{
			var staff = Factory.New<GlbStaff>();

			// Delete Defaulted Working hours
			var workTimesToDelete = Factory.Load<GlbWorkTime>(new ZQuery(GlbWorkTimeSchema.GW_ParentID, staff.PK));
			foreach (var workTime in workTimesToDelete)
			{
				workTime.Delete();
			}

			var newWorkTime = Factory.New<GlbWorkTime>();
			newWorkTime.GW_ParentID = staff.PK;
			newWorkTime.GW_ParentTableCode = "GS";
			newWorkTime.GW_DayOfWeek = "MON";
			newWorkTime.GW_StartTime = GlbWorkTime.CreateTime(0, 0);
			newWorkTime.GW_EndTime = GlbWorkTime.CreateTime(2, 0);
			Factory.Save();

			var loaded = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staff.PK));
			loaded.WorkTimes.MondayWorkingHours = "*** **";

			CombineAssertions(() =>
			{
				AssertEquals("*** **", loaded.WorkTimes.MondayWorkingHours);
			});
		}

		[TestDate(2020, 9, 1, 3, 10, 5)]
		public void TestUpdateWorkingHoursOnNewStaffStoresDataInDb_AddNewInterval()
		{
			var staff = Factory.New<GlbStaff>();

			// Delete Defaulted Working hours
			var workTimesToDelete = Factory.Load<GlbWorkTime>(new ZQuery(GlbWorkTimeSchema.GW_ParentID, staff.PK));
			foreach (var workTime in workTimesToDelete)
			{
				workTime.Delete();
			}

			var newWorkTime = Factory.New<GlbWorkTime>();
			newWorkTime.GW_ParentID = staff.PK;
			newWorkTime.GW_ParentTableCode = "GS";
			newWorkTime.GW_DayOfWeek = "MON";
			newWorkTime.GW_StartTime = GlbWorkTime.CreateTime(0, 0);
			newWorkTime.GW_EndTime = GlbWorkTime.CreateTime(2, 0);
			Factory.Save();

			var loadedStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staff.PK));
			loadedStaff.WorkTimes.MondayWorkingHours = "**** ***";

			var staffWorkTimes = Factory.Load<GlbWorkTime>(new ZQuery(GlbWorkTimeSchema.GW_ParentID, staff.PK));

			CombineAssertions(() =>
			{
				AssertEquals("**** ***", loadedStaff.WorkTimes.MondayWorkingHours);

				AssertEquals(2, staffWorkTimes.Length);

				AssertEquals("MON", staffWorkTimes[0].GW_DayOfWeek);
				AssertEquals(GlbWorkTime.CreateTime(0, 0), staffWorkTimes[0].GW_StartTime);
				AssertEquals(GlbWorkTime.CreateTime(2, 0), staffWorkTimes[0].GW_EndTime);

				AssertEquals("MON", staffWorkTimes[1].GW_DayOfWeek);
				AssertEquals(GlbWorkTime.CreateTime(2, 30), staffWorkTimes[1].GW_StartTime);
				AssertEquals(GlbWorkTime.CreateTime(4, 0), staffWorkTimes[1].GW_EndTime);
			});
		}

		[TestDate(2020, 9, 1, 3, 10, 5)]
		public void TestUpdateWorkingHoursOnNewStaffStoresDataInDb_UpdateExistingInterval()
		{
			var staff = Factory.New<GlbStaff>();

			// Delete Defaulted Working hours
			var workTimesToDelete = Factory.Load<GlbWorkTime>(new ZQuery(GlbWorkTimeSchema.GW_ParentID, staff.PK));
			foreach (var workTime in workTimesToDelete)
			{
				workTime.Delete();
			}

			var newWorkTime1 = Factory.New<GlbWorkTime>();
			newWorkTime1.GW_ParentID = staff.PK;
			newWorkTime1.GW_ParentTableCode = "GS";
			newWorkTime1.GW_DayOfWeek = "MON";
			newWorkTime1.GW_StartTime = GlbWorkTime.CreateTime(0, 0);
			newWorkTime1.GW_EndTime = GlbWorkTime.CreateTime(2, 0);

			var newWorkTime2 = Factory.New<GlbWorkTime>();
			newWorkTime2.GW_ParentID = staff.PK;
			newWorkTime2.GW_ParentTableCode = "GS";
			newWorkTime2.GW_DayOfWeek = "MON";
			newWorkTime2.GW_StartTime = GlbWorkTime.CreateTime(2, 30);
			newWorkTime2.GW_EndTime = GlbWorkTime.CreateTime(4, 0);

			Factory.Save();

			var loadedStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staff.PK));
			loadedStaff.WorkTimes.MondayWorkingHours = "****  ****";

			var staffWorkTimes = Factory.Load<GlbWorkTime>(new ZQuery(GlbWorkTimeSchema.GW_ParentID, staff.PK));

			CombineAssertions(() =>
			{
				AssertEquals("****  ****", loadedStaff.WorkTimes.MondayWorkingHours);

				AssertEquals(2, staffWorkTimes.Length);

				AssertEquals("MON", staffWorkTimes[0].GW_DayOfWeek);
				AssertEquals(GlbWorkTime.CreateTime(0, 0), staffWorkTimes[0].GW_StartTime);
				AssertEquals(GlbWorkTime.CreateTime(2, 0), staffWorkTimes[0].GW_EndTime);

				AssertEquals("MON", staffWorkTimes[1].GW_DayOfWeek);
				AssertEquals(GlbWorkTime.CreateTime(3, 0), staffWorkTimes[1].GW_StartTime);
				AssertEquals(GlbWorkTime.CreateTime(5, 0), staffWorkTimes[1].GW_EndTime);
			});
		}

		[TestDate(2020, 9, 1, 3, 10, 5)]
		public void TestUpdateWorkingHoursOnNewStaffStoresDataInDb_DeleteExistingInterval()
		{
			var staff = Factory.New<GlbStaff>();

			// Delete Defaulted Working hours
			var workTimesToDelete = Factory.Load<GlbWorkTime>(new ZQuery(GlbWorkTimeSchema.GW_ParentID, staff.PK));
			foreach (var workTime in workTimesToDelete)
			{
				workTime.Delete();
			}

			var newWorkTime1 = Factory.New<GlbWorkTime>();
			newWorkTime1.GW_ParentID = staff.PK;
			newWorkTime1.GW_ParentTableCode = "GS";
			newWorkTime1.GW_DayOfWeek = "MON";
			newWorkTime1.GW_StartTime = GlbWorkTime.CreateTime(0, 0);
			newWorkTime1.GW_EndTime = GlbWorkTime.CreateTime(2, 0);

			var newWorkTime2 = Factory.New<GlbWorkTime>();
			newWorkTime2.GW_ParentID = staff.PK;
			newWorkTime2.GW_ParentTableCode = "GS";
			newWorkTime2.GW_DayOfWeek = "MON";
			newWorkTime2.GW_StartTime = GlbWorkTime.CreateTime(2, 30);
			newWorkTime2.GW_EndTime = GlbWorkTime.CreateTime(4, 0);

			Factory.Save();

			var loadedStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staff.PK));
			loadedStaff.WorkTimes.MondayWorkingHours = "****";

			var staffWorkTimes = Factory.Load<GlbWorkTime>(new ZQuery(GlbWorkTimeSchema.GW_ParentID, staff.PK));

			CombineAssertions(() =>
			{
				AssertEquals("****", loadedStaff.WorkTimes.MondayWorkingHours);

				AssertEquals(1, staffWorkTimes.Length);

				AssertEquals("MON", staffWorkTimes[0].GW_DayOfWeek);
				AssertEquals(GlbWorkTime.CreateTime(0, 0), staffWorkTimes[0].GW_StartTime);
				AssertEquals(GlbWorkTime.CreateTime(2, 0), staffWorkTimes[0].GW_EndTime);
			});
		}

		public void TestWorkingHoursString_ClearInterval()
		{
			var staff = Factory.New<GlbStaff>();
			staff.WorkTimes.TuesdayWorkingHours = "";

			AssertEquals("", staff.WorkTimes.TuesdayWorkingHours);
		}

		public void TestWorkingHoursString_AddNewIntervals()
		{
			var staff = Factory.New<GlbStaff>();
			staff.WorkTimes.TuesdayWorkingHours = "";
			staff.WorkTimes.TuesdayWorkingHours = "                * ***************** *";

			AssertEquals("                * ***************** *", staff.WorkTimes.TuesdayWorkingHours);
		}

		public void TestWorkingHoursString_UpdateIntervals()
		{
			var staff = Factory.New<GlbStaff>();
			staff.WorkTimes.TuesdayWorkingHours = "";
			staff.WorkTimes.TuesdayWorkingHours = "                * ***************** *";
			staff.WorkTimes.TuesdayWorkingHours = "                * *** ***";

			AssertEquals("                * *** ***", staff.WorkTimes.TuesdayWorkingHours);
		}

		public void TestWorkingHoursString_DeactiveAllIntervals()
		{
			var staff = Factory.New<GlbStaff>();
			staff.WorkTimes.TuesdayWorkingHours = "";
			staff.WorkTimes.TuesdayWorkingHours = "                * ***************** *";
			staff.WorkTimes.TuesdayWorkingHours = "";

			AssertEquals("", staff.WorkTimes.TuesdayWorkingHours);
		}

		public void TestRepeatedlySettingOneDay()
		{
			var staff = Factory.New<GlbStaff>();
			Factory.Save();

			var collection = new GlbWorkTimeCollection(Factory, staff.PK, staff.TablePrefix);

			collection.DeleteAll();
			Factory.Save();

			AssertEquals("PRE: Should be empty", 0, collection.Count);

			collection.MondayWorkingHours = "*";
			collection.MondayWorkingHours = "**";
			collection.MondayWorkingHours = "***";
			collection.MondayWorkingHours = "****";
			collection.MondayWorkingHours = "*****";

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => Factory.Save());
				AssertEquals("Only the final interval should have made it to the database", 1, collection.Count);
				AssertEquals("*****", collection.MondayWorkingHours);
			});
		}

		public void TestRepeatedlySettingOneDay_WithExistingIntervals()
		{
			var staff = Factory.New<GlbStaff>();
			var collection = new GlbWorkTimeCollection(Factory, staff.PK, staff.TablePrefix);

			collection.DeleteAll();
			collection.MondayWorkingHours = "* **";
			Factory.Save();

			AssertEquals("PRE: Should have 2 intervals", 2, collection.Count);

			collection.MondayWorkingHours = "* ** *";
			collection.MondayWorkingHours = "** * ** **";
			collection.MondayWorkingHours = "***";
			collection.MondayWorkingHours = "**** ****";
			collection.MondayWorkingHours = "***** * * *";

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => Factory.Save());
				AssertEquals("Only the final interval should have made it to the database", 4, collection.Count);
				AssertEquals("***** * * *", collection.MondayWorkingHours);
			});
		}

		#endregion

		#region Test Constructor Throws

		public void TestThrowsExceptionOnInvalidQuery_NoParentId()
		{
			var query = new ZQuery(GlbWorkTimeSchema.GW_ParentTableCode, GlbStaffSchema.Constants.Prefix);

			AssertExceptionThrown<ArgumentNullException>(() => new GlbWorkTimeCollection(Factory, query));
		}

		public void TestThrowsExceptionOnInvalidQuery_NoParentTableCode()
		{
			var query = new ZQuery(GlbWorkTimeSchema.GW_ParentID, ZGuid.NewZGuid());

			AssertExceptionThrown<ArgumentNullException>(() => new GlbWorkTimeCollection(Factory, query));
		}

		public void TestThrowsExceptionOnInvalidQuery_EmptyParentId()
		{
			AssertExceptionThrown<ArgumentException>(() => new GlbWorkTimeCollection(Factory, ZGuid.Empty, GlbStaffSchema.Constants.Prefix));
		}

		public void TestThrowsExceptionOnInvalidQuery_EmptyParentTableCode()
		{
			AssertExceptionThrown<ArgumentException>(() => new GlbWorkTimeCollection(Factory, ZGuid.NewZGuid(), ZString.Empty));
		}

		#endregion
	}
}
