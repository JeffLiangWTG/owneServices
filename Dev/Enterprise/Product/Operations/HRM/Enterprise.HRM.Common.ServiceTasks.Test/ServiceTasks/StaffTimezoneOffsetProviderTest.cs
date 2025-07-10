using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.HRM.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.HRM.ServiceTasks.Testing
{
	class StaffTimezoneOffsetProviderTest : TestCaseWithFactory
	{
		public void TestReturnsStaffsCurrentTimezone()
		{
			var staff = GetStaff("BOB", "Bob");
			GetTimeZoneSet("Literal/One", GetTimeZone("LIT1", 1 * 60), null);
			GetTimeZoneSet("Literal/Two", GetTimeZone("LIT2", 2 * 60), null);

			AddTimezoneHistory(staff, new[]
			{
				(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero), "Literal/One"),
				(new DateTimeOffset(2100, 1, 1, 0, 0, 0, TimeSpan.Zero), "Literal/Two"),
			});

			Factory.Save();

			AssertEquals(TimeSpan.FromHours(1), StaffTimezoneOffsetProvider.GetOffset(Factory, staff.PK, new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)));
		}

		public void TestFallbackToHomeBranchTimezone()
		{
			var tz = GetTimeZoneSet("Literal/One", GetTimeZone("LIT1", 1 * 60), null);

			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_Code = "JCDBS";
			unloco.RL_R3 = tz.PK;

			var branch = GetBranch("BRR", "Some Branch");
			branch.GB_RL_NKHomePort = unloco.RL_Code;

			var staff = GetStaff("BOB", "Bob");
			staff.GS_GB_HomeBranch = branch.PK;

			Factory.Save();

			AssertEquals(TimeSpan.FromHours(1), StaffTimezoneOffsetProvider.GetOffset(Factory, staff.PK, new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)));
		}

		public void TestReturnInvalidMarkerWhenTimezoneUnknown()
		{
			var staff = GetStaff("BOB", "Bob");
			staff.GS_GB_HomeBranch = ZGuid.Empty;

			AssertEquals(new TimeSpan(13, 59, 0), StaffTimezoneOffsetProvider.GetOffset(Factory, staff.PK, new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)));
		}

		public void TestConsidersStaffTimezoneHistoryEffectiveDates()
		{
			var timezones = Enumerable.Range(0, 10)
				.Select(i => GetTimeZoneSet($"Literal/{i}", GetTimeZone($"JCD0{i}", (short)(i * 60)), null))
				.ToArray();

			var staffHistory = Enumerable.Range(0, 10).Select(i => (new DateTimeOffset(2010 + i, 1, 1, 0, 0, 0, TimeSpan.Zero), $"Literal/{i}")).ToArray();

			var staff = GetStaff("BOB", "Bob");
			Factory.Save();

			AddTimezoneHistory(staff, staffHistory);
			Factory.Save();

			for (var i = 0; i < 10; i++)
			{
				var offset = StaffTimezoneOffsetProvider.GetOffset(Factory, staff.PK, new DateTime(2010 + i, 6, 1));
				AssertEquals(TimeSpan.FromHours(i), offset);
			}
		}

		#region Datatable Helpers

		void AddTimezoneHistory(GlbStaff staff, IEnumerable<(DateTimeOffset Start, string TimezoneName)> histories)
		{
			for (var i = 0; i < histories.Count(); i++)
			{
				var row = Factory.NewWithValidTestData<GlbStaffTimezone>();

				row.GSZ_GS_Staff = staff.PK;
				row.GSZ_EffectiveDate = histories.ElementAt(i).Start;
				row.GSZ_R3_NKTimeZoneSetName = histories.ElementAt(i).TimezoneName;

				if (i < histories.Count() - 1)
				{
					row.GSZ_AutoEffectiveEndDate = histories.ElementAt(i + 1).Start;
				}
			}
		}

		RefTimeZoneSet GetTimeZoneSet(string timezoneName, RefTimeZone stdTz, RefTimeZone dstTz, bool updateAnyway = false)
		{
			var timezoneSet = Factory.LoadTop1<RefTimeZoneSet>(new ZQuery(RefTimeZoneSetSchema.R3_TimeZoneSetName, timezoneName));
			var tzSetNotFound = timezoneSet is null;

			if (tzSetNotFound)
			{
				timezoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			}

			if (updateAnyway || tzSetNotFound)
			{
				timezoneSet.R3_TimeZoneSetName = timezoneName;
				timezoneSet.R3_R2_StandardZone = stdTz.PK;
				timezoneSet.R3_R2_DaylightSavingZone = dstTz == null ? ZGuid.Empty : dstTz.PK;
			}

			return timezoneSet;
		}

		RefTimeZone GetTimeZone(string timezoneCode, short offsetMinutesFromUTC, bool updateAnyway = false)
		{
			var timezone = Factory.LoadTop1<RefTimeZone>(new ZQuery(RefTimeZoneSchema.R2_CivilianTimeZoneCode, timezoneCode));
			var tzNotFount = timezone is null;

			if (tzNotFount)
			{
				timezone = Factory.NewWithValidTestData<RefTimeZone>();
			}

			if (updateAnyway || tzNotFount)
			{
				timezone.R2_CivilianTimeZoneCode = timezoneCode;
				timezone.R2_OffsetMinutesFromUTC = offsetMinutesFromUTC;
			}

			return timezone;
		}

		GlbStaff GetStaff(string code, string name)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_FullName = name;

			return staff;
		}

		GlbBranch GetBranch(string code, string name)
		{
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, code));
			if (branch is null)
			{
				branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_Code = code;
				branch.GB_BranchName = name;
				branch.GB_GC = GetCompany("DUM", "Dummy Company").PK;
			}

			return branch;
		}

		GlbCompany GetCompany(string code, string name)
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, code));
			if (company is null)
			{
				company = Factory.NewWithValidTestData<GlbCompany>();

				company.GC_Code = code;
				company.GC_Name = name;
			}

			return company;
		}

		#endregion
	}
}

