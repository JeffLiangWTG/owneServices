using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruitment.Copyback;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Copyback
{
	sealed class ColumnCopybackProcessorImplTests : TestCase
	{
		[UseSnapshotProtection]
		public void TestCopyBackStaffWorkingBasis()
		{
			// arrange
			var hrmConnection = Db.NewExtraUnrestrictedWriterConnection(
				Db.ServerName, Db.DatabaseName);
			var hrmFactory = new BusinessObjectFactory(hrmConnection);
			var gsw = hrmFactory.NewWithValidTestData<GlbStaffWorkingBasis>();
			hrmFactory.Save();

			var factory = new BusinessObjectFactory();
			var staff = factory.Load<GlbStaff>(gsw.GSW_GS_Staff);

			// pre-assert
			AssertNotEquals(staff.GS_EmploymentBasis, gsw.GSW_WorkingBasis);

			// act
			ColumnCopybackProcessorImpl.StaffWorkingBasis(factory, staff, null);
			factory.Save();

			// assert
			AssertEquals(staff.GS_EmploymentBasis, gsw.GSW_WorkingBasis);
		}

		[UseSnapshotProtection]
		public void TestCopyBackEmploymentHistory()
		{
			// arrange
			var factory = new BusinessObjectFactory();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Title = "Orange Peeler";

			var geh = factory.NewWithValidTestData<GlbEmploymentHistory>();
			geh.GEH_JobTitle = "Mandarine Skinner";
			geh.GEH_GS_Staff = staff.PK;

			factory.Save();

			// pre-assert
			AssertNotEquals(staff.GS_Title, geh.GEH_JobTitle);

			// act
			ColumnCopybackProcessorImpl.EmploymentHistory(factory, staff, null);
			factory.Save();

			// assert
			AssertEquals(staff.GS_Title, geh.GEH_JobTitle);
		}

		[UseSnapshotProtection]
		public void TestCopyBackEmploymentHistory_WhenEmploymentHistoryTitleIsEmpty()
		{
			// arrange
			var factory = new BusinessObjectFactory();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			var originalTitle = "Orange Peeler";
			staff.GS_Title = originalTitle;

			var geh = factory.NewWithValidTestData<GlbEmploymentHistory>();
			geh.GEH_JobTitle = "";
			geh.GEH_GS_Staff = staff.PK;

			factory.Save();

			// pre-assert
			AssertNotEquals(staff.GS_Title, geh.GEH_JobTitle);

			// act
			ColumnCopybackProcessorImpl.EmploymentHistory(factory, staff, null);
			factory.Save();

			// assert
			AssertEquals(staff.GS_Title, originalTitle);
		}

		[UseSnapshotProtection]
		public void TestCopyBackEmploymentHistory_WhenNotApproved_ShouldKeepSameTitle()
		{
			var factory = new BusinessObjectFactory();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Title = "Rotator";

			var employmentHistory = factory.NewWithValidTestData<GlbEmploymentHistory>();
			employmentHistory.GEH_JobTitle = "Junior Dude";
			employmentHistory.GEH_GS_Staff = staff.PK;
			employmentHistory.GEH_EffectiveDate = DateTime.Now.AddMonths(-1);
			employmentHistory.GEH_IsApproved = false;

			factory.Save();

			AssertEquals(staff.GS_Title, "Rotator");
			ColumnCopybackProcessorImpl.EmploymentHistory(factory, staff, null);
			factory.Save();

			AssertEquals(staff.GS_Title, "Rotator");
		}

		[UseSnapshotProtection]
		public void TestCopyBackWorkPattern()
		{
			// arrange
			var factory = new BusinessObjectFactory();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Title = "Orange Peeler";

			// work pattern linked to staff
			var gwpBefore = factory.NewWithValidTestData<GlbWorkPattern>();
			gwpBefore.GWP_GS_Staff = staff.PK;
			gwpBefore.GWP_EffectiveDate = ZDateTimeOffset.Now.AddDays(-3);

			factory.Save();

			// check work times has 5 initial entries for staff (made in NewWithValidTestData<GlfStaff>)
			var currentWorkTimesForStaff = factory.Load<GlbWorkTime>(new ZQuery()).Where(cwt => cwt.GW_ParentID == staff.PK);
			AssertEquals(5, currentWorkTimesForStaff.Count());

			// copy those entries to work times for the work pattern we just made
			foreach (var cwt in currentWorkTimesForStaff)
			{
				var currentWorkTimesForWorkPattern = factory.NewWithValidTestData<GlbWorkTime>();
				currentWorkTimesForWorkPattern.GW_ParentID = gwpBefore.PK;
				currentWorkTimesForWorkPattern.GW_ParentTableCode = GlbWorkPatternSchema.Constants.Prefix;
				currentWorkTimesForWorkPattern.GW_DayOfWeek = cwt.GW_DayOfWeek;
				currentWorkTimesForWorkPattern.GW_StartTime = cwt.GW_StartTime;
				currentWorkTimesForWorkPattern.GW_EndTime = cwt.GW_EndTime;
			}

			factory.Save();

			// this is their new pattern, setting the effective date to be a few minutes ago to simulate
			// we just hit this date, received an event, and are now triggering copyback off that event
			var gwpAfter = factory.NewWithValidTestData<GlbWorkPattern>();
			gwpAfter.GWP_GS_Staff = staff.PK;
			gwpAfter.GWP_EffectiveDate = ZDateTimeOffset.UtcNow.AddMinutes(-2);

			// this is their new work time in the new work pattern
			var newWorkTimesForWorkPattern = new List<GlbWorkTime>();
			foreach (var cwt in currentWorkTimesForStaff)
			{
				var newWorkTime = factory.NewWithValidTestData<GlbWorkTime>();
				newWorkTime.GW_ParentID = gwpAfter.PK;
				newWorkTime.GW_ParentTableCode = GlbWorkPatternSchema.Constants.Prefix;
				newWorkTime.GW_DayOfWeek = cwt.GW_DayOfWeek;
				newWorkTime.GW_StartTime = new ZDateTime(1900, 1, 1, cwt.GW_StartTime.Hour + 1, 30, 0);
				newWorkTime.GW_EndTime = new ZDateTime(1900, 1, 1, cwt.GW_EndTime.Hour + 1, 25, 0);
				newWorkTimesForWorkPattern.Add(newWorkTime);
			}

			factory.Save();

			// act
			ColumnCopybackProcessorImpl.WorkPattern(factory, staff, null);
			factory.Save();

			// assert
			var copybackWorkTimesForWorkPattern = factory
				.Load<GlbWorkTime>(new ZQuery())
				.Where(workTime => workTime.GW_ParentTableCode == GlbStaffSchema.Constants.Prefix && workTime.GW_ParentID == staff.PK);

			AssertEquals(5, copybackWorkTimesForWorkPattern.Count());

			// join the 5 work times for GWP with the 5 from GS and assert they're equal, apart from ParentID and ParentTableCode
			foreach (var match in newWorkTimesForWorkPattern.Join(
				copybackWorkTimesForWorkPattern,
				newWT => newWT.GW_DayOfWeek,
				cbWT => cbWT.GW_DayOfWeek,
				(newWT, cbWT) => new
				{
					expected = newWT,
					actual = cbWT,
				}))
			{
				AssertEquals(GlbStaffSchema.Constants.Prefix, match.actual.GW_ParentTableCode);
				AssertEquals(staff.PK, match.actual.GW_ParentID);
				AssertEquals(match.expected.GW_DayOfWeek, match.actual.GW_DayOfWeek);
				AssertEquals(match.expected.GW_StartTime, match.actual.GW_StartTime);
				AssertEquals(match.expected.GW_EndTime, match.actual.GW_EndTime);
			}
		}

		[UseSnapshotProtection]
		public void TestCopyBackManagerUpdatesLastEditDate()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var managee = factory.NewWithValidTestData<GlbStaff>();
			var manager = factory.NewWithValidTestData<GlbStaff>();
			var gsm = factory.NewWithValidTestData<GlbStaffManager>();
			gsm.GSM_GS_Staff = managee.PK;
			gsm.GSM_GS_Manager = manager.PK;
			gsm.GSM_ManagerType = "DRM";
			factory.Save();

			// pre-assert
			AssertEquals(managee.GS_SystemLastEditTimeUtc, managee.GS_SystemCreateTimeUtc);
			var prevLastTimeEdit = managee.GS_SystemLastEditTimeUtc;

			// act
			ColumnCopybackProcessorImpl.StaffManager(factory, managee, null);
			factory.Save();

			// assert
			AssertNotEquals(managee.GS_SystemLastEditTimeUtc, prevLastTimeEdit);
		}

		[UseSnapshotProtection]
		public void TestCopyBackEmptyDB()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Title = "Orange Peeler";
			factory.Save();

			// act+assert
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => ColumnCopybackProcessorImpl.EmploymentHistory(factory, staff, null));
				AssertNoExceptionThrown(() => ColumnCopybackProcessorImpl.StaffWorkingBasis(factory, staff, null));

				AssertNoExceptionThrown(() => ColumnCopybackProcessorImpl.WorkPattern(factory, staff, null));
				AssertEquals("CannotFindWorkPattern", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}
	}
}
