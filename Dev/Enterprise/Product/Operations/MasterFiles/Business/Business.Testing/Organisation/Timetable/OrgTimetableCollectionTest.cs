using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTimetableCollection))]
	sealed class OrgTimetableCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgTimetableCollection>
	{
		OrgAddress address;
		OrgTimetableCollection orgTimeTableCollection;
		bool hasFiredOnTriedToDeleteLastTimeTableEvent;

		protected override OrgTimetableCollection GetCollectionToTest()
		{
			return new OrgTimetableCollection(Factory.New<OrgAddress>());
		}

		protected override void SetUp()
		{
			base.SetUp();

			var orgheader = Factory.NewWithValidTestData<OrgHeader>();
			address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = orgheader.PK;
			address.Address1 = "ABCDEF";
			orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.OnTriedToDeleteLastTimeTable += delegate
			{ hasFiredOnTriedToDeleteLastTimeTableEvent = true; };
		}

		public void TestOrgTimetableCollectionOrder()
		{
			orgTimeTableCollection.AdvancedRange = true;

			OrgTimetable t1 = Factory.New<OrgTimetable>();
			t1.OTT_OA = address.PK;
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			t1.DayOfWeek = "FRI";
			t1.OTT_TimeFrom = new DateTime(1900, 1, 1, 8, 0, 0);
			t1.OTT_TimeTo = new DateTime(1900, 1, 1, 9, 0, 0);
			orgTimeTableCollection.Add(t1);

			OrgTimetable t2 = Factory.New<OrgTimetable>();
			t2.OTT_OA = address.PK;
			t2.OTT_Type = OrgTimetableType.Codes.Pickup;
			t2.DayOfWeek = "WED";
			t2.OTT_TimeFrom = new DateTime(1900, 1, 1, 8, 0, 0);
			t2.OTT_TimeTo = new DateTime(1900, 1, 1, 9, 30, 0);
			orgTimeTableCollection.Add(t2);

			OrgTimetable t3 = Factory.New<OrgTimetable>();
			t3.OTT_OA = address.PK;
			t3.OTT_Type = OrgTimetableType.Codes.Pickup;
			t3.DayOfWeek = "MON";
			t3.OTT_TimeFrom = new DateTime(1900, 1, 1, 9, 0, 0);
			t3.OTT_TimeTo = new DateTime(1900, 1, 1, 9, 30, 0);
			orgTimeTableCollection.Add(t3);

			OrgTimetable t4 = Factory.New<OrgTimetable>();
			t4.OTT_OA = address.PK;
			t4.OTT_Type = OrgTimetableType.Codes.Pickup;
			t4.DayOfWeek = "MON";
			t4.OTT_TimeFrom = new DateTime(1900, 1, 1, 8, 0, 0);
			t4.OTT_TimeTo = new DateTime(1900, 1, 1, 8, 30, 0);
			orgTimeTableCollection.Add(t4);

			OrgTimetable t5 = Factory.New<OrgTimetable>();
			t5.OTT_OA = address.PK;
			t5.OTT_Type = OrgTimetableType.Codes.Deliver;
			t5.DayOfWeek = "TUE";
			t5.OTT_TimeFrom = new DateTime(1900, 1, 1, 14, 0, 0);
			t5.OTT_TimeTo = new DateTime(1900, 1, 1, 14, 30, 0);
			orgTimeTableCollection.Add(t5);

			IComparer comparer = new OrgTimetableComparer();
			orgTimeTableCollection.ApplySort(comparer);

			AssertOrgTimetable(orgTimeTableCollection[0], OrgTimetableType.Codes.Pickup, "MON", new DateTime(1900, 1, 1, 8, 0, 0), new DateTime(1900, 1, 1, 8, 30, 0));
			AssertOrgTimetable(orgTimeTableCollection[1], OrgTimetableType.Codes.Pickup, "MON", new DateTime(1900, 1, 1, 9, 0, 0), new DateTime(1900, 1, 1, 9, 30, 0));
			AssertOrgTimetable(orgTimeTableCollection[2], OrgTimetableType.Codes.Deliver, "TUE", new DateTime(1900, 1, 1, 14, 0, 0), new DateTime(1900, 1, 1, 14, 30, 0));
			AssertOrgTimetable(orgTimeTableCollection[3], OrgTimetableType.Codes.Pickup, "WED", new DateTime(1900, 1, 1, 8, 0, 0), new DateTime(1900, 1, 1, 9, 30, 0));
			AssertOrgTimetable(orgTimeTableCollection[4], OrgTimetableType.Codes.Pickup, "FRI", new DateTime(1900, 1, 1, 8, 0, 0), new DateTime(1900, 1, 1, 9, 0, 0));
		}

		public void TestInitializeRangeType()
		{
			orgTimeTableCollection.InitializeRangeType();
			Assert(orgTimeTableCollection.DefaultRange);
			AssertEquals(OrgTimeTableRangeType.Default, orgTimeTableCollection.RangeType);

			orgTimeTableCollection.DeleteAllAndIgnoreDeleteEvent();
			orgTimeTableCollection.Add(CreateWeekdayTimeTable());
			orgTimeTableCollection.InitializeRangeType();
			Assert(orgTimeTableCollection.WeekdayRange);
			AssertEquals(OrgTimeTableRangeType.Weekday, orgTimeTableCollection.RangeType);

			orgTimeTableCollection.DeleteAllAndIgnoreDeleteEvent();
			CreateEverydayTimeTable();
			orgTimeTableCollection.InitializeRangeType();
			Assert(orgTimeTableCollection.AdvancedRange);
			AssertEquals(OrgTimeTableRangeType.Advanced, orgTimeTableCollection.RangeType);

			orgTimeTableCollection.DeleteAllAndIgnoreDeleteEvent();
			CreateAdvancedTimeTable();
			orgTimeTableCollection.InitializeRangeType();
			Assert(orgTimeTableCollection.AdvancedRange);
			AssertEquals(OrgTimeTableRangeType.Advanced, orgTimeTableCollection.RangeType);

			orgTimeTableCollection.DeleteAllAndIgnoreDeleteEvent();
			CreateNotApplicableTimeTable();
			orgTimeTableCollection.InitializeRangeType();
			Assert(orgTimeTableCollection.NotApplicable);
			AssertEquals(OrgTimeTableRangeType.NotApplicable, orgTimeTableCollection.RangeType);
		}

		public void TestReInitializeRangeType()
		{
			orgTimeTableCollection.ReInitializeRangeType();
			Assert(orgTimeTableCollection.DefaultRange);
			AssertEquals(OrgTimeTableRangeType.Default, orgTimeTableCollection.RangeType);

			orgTimeTableCollection.DeleteAllAndIgnoreDeleteEvent();
			orgTimeTableCollection.Add(CreateWeekdayTimeTable());
			orgTimeTableCollection.ReInitializeRangeType();
			Assert(orgTimeTableCollection.WeekdayRange);
			AssertEquals(OrgTimeTableRangeType.Weekday, orgTimeTableCollection.RangeType);

			orgTimeTableCollection.DeleteAllAndIgnoreDeleteEvent();
			CreateEverydayTimeTable();
			orgTimeTableCollection.ReInitializeRangeType();
			Assert(orgTimeTableCollection.AdvancedRange);
			AssertEquals(OrgTimeTableRangeType.Advanced, orgTimeTableCollection.RangeType);

			orgTimeTableCollection.DeleteAllAndIgnoreDeleteEvent();
			CreateAdvancedTimeTable();
			orgTimeTableCollection.ReInitializeRangeType();
			Assert(orgTimeTableCollection.AdvancedRange);
			AssertEquals(OrgTimeTableRangeType.Advanced, orgTimeTableCollection.RangeType);

			orgTimeTableCollection.DeleteAllAndIgnoreDeleteEvent();
			CreateNotApplicableTimeTable();
			orgTimeTableCollection.ReInitializeRangeType();
			Assert(orgTimeTableCollection.NotApplicable);
			AssertEquals(OrgTimeTableRangeType.NotApplicable, orgTimeTableCollection.RangeType);
		}

		public void TestNumberOfFactories()
		{
			var count1 = BusinessObjectFactory._NextInstance;

			orgTimeTableCollection.InitializeRangeType();
			var count2 = BusinessObjectFactory._NextInstance;
			Assert("Only increased two factories.", count1 + 2 == count2);

			orgTimeTableCollection.InitializeRangeType();
			var count3 = BusinessObjectFactory._NextInstance;
			Assert("Should not increase factory again.", count2 == count3);
		}

		public void TestSetRangeTypeDoesNotThrowNullReferenceException()
		{
			var defaultCollection = new DefaultOrgTimetableSettingsCollection();

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				AssertNoExceptionThrown("Should not throw an exception when there's no time table", () => orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.Default));

				Assert(orgTimeTableCollection.DefaultRange);
				AssertEquals(OrgTimeTableRangeType.Default, orgTimeTableCollection.RangeType);
			}
		}

		public void TestSetRangeType()
		{
			var defaultCollection = new DefaultOrgTimetableSettingsCollection();
			var settings = defaultCollection.AddNew();
			settings.Timetables.AddNewWithProperty(OrgTimetableType.Codes.Pickup, new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0), new ZDateTime(ZDateTime.Now.Year, 1, 1, 17, 0, 0), "MON", settings.Timetables);
			settings.Timetables.AddNewWithProperty(OrgTimetableType.Codes.Deliver, new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0), new ZDateTime(ZDateTime.Now.Year, 1, 1, 17, 0, 0), "MON", settings.Timetables);

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.Default);

				Assert(orgTimeTableCollection.DefaultRange);
				AssertEquals(OrgTimeTableRangeType.Default, orgTimeTableCollection.RangeType);
				AssertOrgTimetable(orgTimeTableCollection[0], OrgTimetableType.Codes.Pickup, "MON", new DateTime(1900, 1, 1, 9, 0, 0), new DateTime(1900, 1, 1, 17, 0, 0));
				AssertOrgTimetable(orgTimeTableCollection[1], OrgTimetableType.Codes.Deliver, "MON", new DateTime(1900, 1, 1, 9, 0, 0), new DateTime(1900, 1, 1, 17, 0, 0));
			}

			orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.Advanced);
			Assert(orgTimeTableCollection.AdvancedRange);
			AssertEquals(OrgTimeTableRangeType.Advanced, orgTimeTableCollection.RangeType);
			AssertOrgTimetable(orgTimeTableCollection[0], OrgTimetableType.Codes.Pickup, "MON", new DateTime(1900, 1, 1, 9, 0, 0), new DateTime(1900, 1, 1, 17, 0, 0));
			AssertOrgTimetable(orgTimeTableCollection[1], OrgTimetableType.Codes.Deliver, "MON", new DateTime(1900, 1, 1, 9, 0, 0), new DateTime(1900, 1, 1, 17, 0, 0));
			Assert(!orgTimeTableCollection[0].IsDefaultFromRegistry);
			Assert(!orgTimeTableCollection[0].IsDefaultFromRegistry);

			orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.NotApplicable);
			Assert(orgTimeTableCollection.NotApplicable);
			AssertEquals(OrgTimeTableRangeType.NotApplicable, orgTimeTableCollection.RangeType);
			AssertOrgTimetable(orgTimeTableCollection[0], OrgTimetableType.Codes.Pickup, "", ZDateTime.Empty, ZDateTime.Empty);
			AssertOrgTimetable(orgTimeTableCollection[1], OrgTimetableType.Codes.Deliver, "", ZDateTime.Empty, ZDateTime.Empty);
		}

		public void TestSetRangeTypeToWeekdayShouldDecreaseNumberOfRowsToTwo()
		{
			CreateAdvancedTimeTable();
			CreateAdvancedTimeTable();
			CreateAdvancedTimeTable();
			CreateAdvancedTimeTable();
			Assert("Precondition", orgTimeTableCollection.Count == 4);
			orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.Weekday);
			AssertEquals("Number of rows should decrease to two.", 2, orgTimeTableCollection.Count);
		}

		public void TestHasNoChanges()
		{
			orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.Default);
			Assert(!orgTimeTableCollection.HasChanges);

			orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.Advanced);
			Assert(!orgTimeTableCollection.HasChanges);

			orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.NotApplicable);
			Assert(!orgTimeTableCollection.HasChanges);
		}

		public void TestHasChangesWhenChangeOrgTimetableType()
		{
			CreateAdvancedTimeTable();
			Factory.Save();

			orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.Advanced);
			Assert("Precondition", !orgTimeTableCollection.HasChanges);
			var table = orgTimeTableCollection[0];
			table.OTT_Type = OrgTimetableType.Codes.Deliver;
			Assert(orgTimeTableCollection.HasChanges);
		}

		public void TestHasChangesWhenChangeFromTime()
		{
			CreateAdvancedTimeTable();
			Factory.Save();

			orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.Advanced);
			Assert("Precondition", !orgTimeTableCollection.HasChanges);
			var table = orgTimeTableCollection[0];
			table.OTT_TimeFrom = ZDateTime.BrettsBirthday;
			Assert(orgTimeTableCollection.HasChanges);
		}

		public void TestHasChangesWhenChangeToTime()
		{
			CreateAdvancedTimeTable();
			Factory.Save();

			orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.Advanced);
			Assert("Precondition", !orgTimeTableCollection.HasChanges);
			var table = orgTimeTableCollection[0];
			table.OTT_TimeTo = ZDateTime.BrettsBirthday;
			Assert(orgTimeTableCollection.HasChanges);
		}

		public void TestHasChangesWhenChangeDayOfWeek()
		{
			orgTimeTableCollection.DeleteAll();
			CreateAdvancedTimeTable();
			Factory.Save();

			Assert("Precondition", !orgTimeTableCollection.HasChanges);
			var table = orgTimeTableCollection[0];
			table.DayOfWeek = AutoDayOfWeekCodeList.Codes.Tuesday;
			Assert(orgTimeTableCollection.HasChanges);
		}

		public void TestHasChangesWhenChangeSavedData_Weekday()
		{
			orgTimeTableCollection.DeleteAll();
			orgTimeTableCollection.Add(CreateWeekdayTimeTable());
			Factory.Save();
			orgTimeTableCollection.InitializeRangeType();
			var table = orgTimeTableCollection[0];
			AssertEquals("Precondition", OrgTimetableType.Codes.Pickup, table.OTT_Type);
			Assert("Precondition", !orgTimeTableCollection.HasChanges);

			table.OTT_Type = OrgTimetableType.Codes.Deliver;
			Assert(orgTimeTableCollection.HasChanges);
		}

		public void TestHasChangesWhenChangeSavedData_Everyday()
		{
			CreateEverydayTimeTable();
			Factory.Save();
			orgTimeTableCollection.InitializeRangeType();
			var table = orgTimeTableCollection[0];
			AssertEquals("Precondition", 0, table.OTT_TimeTo.Minute);
			Assert("Precondition", !orgTimeTableCollection.HasChanges);

			table.OTT_TimeTo = new DateTime(1900, 1, 1, 9, 30, 0);
			Assert(orgTimeTableCollection.HasChanges);
		}

		public void TestHasChangesWhenChangeSavedData_Advanced()
		{
			CreateAdvancedTimeTable();
			Factory.Save();
			orgTimeTableCollection.InitializeRangeType();
			var table = orgTimeTableCollection[0];
			Assert("Precondition", table.OTT_Monday);
			Assert("Precondition", !orgTimeTableCollection.HasChanges);

			table.OTT_Monday = false;
			Assert(orgTimeTableCollection.HasChanges);
		}

		public void TestOnTriedToDeleteLastTimeTableEvent()
		{
			CreateAdvancedTimeTable();
			Factory.Save();
			orgTimeTableCollection.InitializeRangeType();
			orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.Advanced);
			Assert(!hasFiredOnTriedToDeleteLastTimeTableEvent);
		}

		public void TestNewlyAddedTimetablesShouldHaveWeekdayTypeIfRangeTypeIsWeekday()
		{
			orgTimeTableCollection.SetRangeType(OrgTimeTableRangeType.Weekday);
			orgTimeTableCollection.DeleteAll();
			var table = Factory.New<OrgTimetable>();
			table.OTT_IsForAllWeekDays = false;
			orgTimeTableCollection.Add(table);
			AssertEquals("OTT_IsForAllWeekday should be set", true, table.OTT_IsForAllWeekDays);
		}

		void CreateEverydayTimeTable()
		{
			var t1 = Factory.New<OrgTimetable>();
			t1.OTT_OA = address.PK;
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			t1.OTT_Monday = true;
			t1.OTT_Tuesday = true;
			t1.OTT_Wednesday = true;
			t1.OTT_Thursday = true;
			t1.OTT_Friday = true;
			t1.OTT_Saturday = true;
			t1.OTT_Sunday = true;
			t1.OTT_TimeFrom = new DateTime(1900, 1, 1, 8, 0, 0);
			t1.OTT_TimeTo = new DateTime(1900, 1, 1, 9, 0, 0);
			orgTimeTableCollection.Add(t1);
		}

		OrgTimetable CreateWeekdayTimeTable()
		{
			var t1 = Factory.New<OrgTimetable>();
			t1.OTT_OA = address.PK;
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			t1.OTT_IsForAllWeekDays = true;
			t1.OTT_TimeFrom = new DateTime(1900, 1, 1, 8, 0, 0);
			t1.OTT_TimeTo = new DateTime(1900, 1, 1, 9, 0, 0);
			return t1;
		}

		void CreateAdvancedTimeTable()
		{
			var t1 = Factory.New<OrgTimetable>();
			t1.OTT_OA = address.PK;
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			t1.OTT_Monday = true;
			t1.OTT_Tuesday = false;
			t1.OTT_Wednesday = false;
			t1.OTT_Thursday = false;
			t1.OTT_Friday = false;
			t1.OTT_Saturday = false;
			t1.OTT_Sunday = false;
			t1.OTT_TimeFrom = new DateTime(1900, 1, 1, 8, 0, 0);
			t1.OTT_TimeTo = new DateTime(1900, 1, 1, 9, 0, 0);
			orgTimeTableCollection.Add(t1);
		}

		void CreateNotApplicableTimeTable()
		{
			var t1 = Factory.New<OrgTimetable>();
			using (t1.GetValidationSuspender())
			{
				t1.OTT_OA = address.PK;
				t1.OTT_Type = OrgTimetableType.Codes.Pickup;
				t1.OTT_Monday = false;
				t1.OTT_Tuesday = false;
				t1.OTT_Wednesday = false;
				t1.OTT_Thursday = false;
				t1.OTT_Friday = false;
				t1.OTT_Saturday = false;
				t1.OTT_Sunday = false;
			}
			orgTimeTableCollection.Add(t1);

			var t2 = Factory.New<OrgTimetable>();
			using (t2.GetValidationSuspender())
			{
				t2.OTT_OA = address.PK;
				t2.OTT_Type = OrgTimetableType.Codes.Pickup;
				t2.OTT_Monday = false;
				t2.OTT_Tuesday = false;
				t2.OTT_Wednesday = false;
				t2.OTT_Thursday = false;
				t2.OTT_Friday = false;
				t2.OTT_Saturday = false;
				t2.OTT_Sunday = false;
			}
			orgTimeTableCollection.Add(t2);
		}

		void AssertOrgTimetable(OrgTimetable timetable, string type, string day, ZDateTime from, ZDateTime to)
		{
			AssertEquals(type, timetable.OTT_Type);
			AssertEquals(day, timetable.DayOfWeek);
			AssertEquals(from, timetable.OTT_TimeFrom);
			AssertEquals(to, timetable.OTT_TimeTo);
		}
	}
}
