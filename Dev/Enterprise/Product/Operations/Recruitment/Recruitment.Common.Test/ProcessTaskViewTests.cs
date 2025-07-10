using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Module
{
	[TestedType(typeof(ProcessTaskView))]
	sealed class ProcessTaskViewTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new ProcessTaskView(Factory.NewWithValidTestData<ProcessTask>());

		public void TestStage()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var pt = factory.NewWithValidTestData<ProcessTask>();
			var ptv = new ProcessTaskView(pt);

			AssertEquals(pt.P9_Description, ptv.Stage);

			// act
			ptv.Stage = "Hi there";

			// assert
			AssertEquals("Hi there", pt.P9_Description);
		}

		public void TestSequence()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var pt = factory.NewWithValidTestData<ProcessTask>();
			var ptv = new ProcessTaskView(pt);

			AssertEquals(pt.P9_Sequence, ptv.Sequence);

			// act
			ptv.Sequence = 9;

			// assert
			AssertEquals(9, pt.P9_Sequence);
		}

		public void TestStatus_Get()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var app = factory.NewWithValidTestData<HRJobApplication>();
			var pt = app.WorkflowItems.AddNew();
			var ptv = new ProcessTaskView(pt);
			// act

			// assert
			pt.P9_Status = "CLS";
			AssertEquals("Skipped", ptv.Status);
			pt.P9_Status = "CAN";
			AssertEquals("Skipped", ptv.Status);
			pt.P9_Status = "ASN";
			AssertEquals("Incomplete", ptv.Status);

			pt.P9_Status = "CLS";
			pt.P9_Outcome = ProcessTaskView.TaskP9OutcomeFail;
			factory.Save();
			AssertEquals("Failed", ptv.Status);

			pt.P9_Status = "CLS";
			pt.P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			factory.Save();
			AssertEquals("Passed", ptv.Status);

			pt.P9_Status = "CAN";
			pt.P9_Outcome = string.Empty;
			factory.Save();
			AssertEquals("Skipped", ptv.Status);

			pt.P9_Status = "ASN";
			pt.P9_Outcome = string.Empty;
			factory.Save();
			AssertEquals("Incomplete", ptv.Status);

			pt.P9_Status = "ASN";
			pt.P9_Outcome = ProcessTaskView.TaskP9OutcomePass;
			factory.Save();
			AssertEquals("Incomplete", ptv.Status);
		}

		public void TestStateMaxLengthMatchesP9Description()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var app = factory.NewWithValidTestData<HRJobApplication>();
			var pt = app.WorkflowItems.AddNew();
			var ptv = new ProcessTaskView(pt);

			AssertEquals(ProcessTasksSchema.P9_Description.MaxLength, ptv.StageInfo.MaxLength);
		}

		public void TestStatus_Set()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var app = factory.NewWithValidTestData<HRJobApplication>();
			var pt = app.WorkflowItems.AddNew();
			var ptv = new ProcessTaskView(pt);

			// assert
			// act
			ptv.Status = "Failed";
			AssertEquals("CLS", pt.P9_Status);
			AssertEquals(ProcessTaskView.TaskP9OutcomeFail, pt.P9_Outcome);
			AssertEquals(
				pt.PK.ToString(),
				app.Logs
					.Find(l => l.SL_Parent == app.PK)
					.OrderByDescending(l => l.SL_PostedTimeUtc)
					.Single(l => l.Event.SE_Code == AutoEvents.RecruitmentCheckpointFailed.Code).Parameters["PK"]);

			ptv.Status = "Passed";
			AssertEquals("CLS", pt.P9_Status);
			AssertEquals(ProcessTaskView.TaskP9OutcomePass, pt.P9_Outcome);
			AssertEquals(
				pt.PK.ToString(),
				app.Logs
					.Find(l => l.SL_Parent == app.PK)
					.OrderByDescending(l => l.SL_PostedTimeUtc)
					.Single(l => l.Event.SE_Code == AutoEvents.RecruitmentCheckpointPassed.Code).Parameters["PK"]);

			ptv.Status = "Skipped";
			AssertEquals("CAN", pt.P9_Status);

			ptv.Status = "Incomplete";
			AssertEquals("ASN", pt.P9_Status);
		}

		public void TestStatus_Set_OverrideStaffCode()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var app = factory.NewWithValidTestData<HRJobApplication>();
			var pt = app.WorkflowItems.AddNew();
			pt.P9_GS_NKAssignedStaffMember = "TST";
			factory.Save();

			var ptv = new ProcessTaskView(pt);

			ptv.Status = "Skipped";
			AssertEquals("CAN", pt.P9_Status);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, pt.P9_GS_NKAssignedStaffMember);

			pt.P9_GS_NKAssignedStaffMember = string.Empty;
			factory.Save();

			ptv.Status = "Passed";
			AssertEquals("CLS", pt.P9_Status);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, pt.P9_GS_NKAssignedStaffMember);
		}

		public void TestStmALog_Set_Multiple()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var app = factory.NewWithValidTestData<HRJobApplication>();
			var pt1 = app.WorkflowItems.AddNew();
			var pt2 = app.WorkflowItems.AddNew();
			var pt3 = app.WorkflowItems.AddNew();
			var ptv1 = new ProcessTaskView(pt1);
			var ptv2 = new ProcessTaskView(pt2);
			ptv1.Status = "Passed";
			ptv2.Status = "Passed";

			// Change should be exclusively applied per ptv
			// assert1
			ptv1.Status = "Failed";
			AssertEquals("CLS", pt1.P9_Status);
			AssertEquals(
				pt1.PK.ToString(),
				app.Logs
					.Find(l => l.SL_Parent == app.PK)
					.OrderByDescending(l => l.SL_PostedTimeUtc)
					.Single(l => l.Event.SE_Code == AutoEvents.RecruitmentCheckpointFailed.Code).Parameters["PK"]);

			AssertEquals("Passed", ptv2.Status);
			AssertEquals(
			pt2.PK.ToString(),
			app.Logs
				.Find(l => l.SL_Parent == app.PK)
				.OrderByDescending(l => l.SL_PostedTimeUtc)
				.Single(l => l.Event.SE_Code == AutoEvents.RecruitmentCheckpointPassed.Code).Parameters["PK"]);

			// assert2
			ptv2.Status = "Incomplete";
			AssertEquals("ASN", pt2.P9_Status);

			AssertEquals("Failed", ptv1.Status);
			AssertEquals(
			pt1.PK.ToString(),
			app.Logs
				.Find(l => l.SL_Parent == app.PK)
				.OrderByDescending(l => l.SL_PostedTimeUtc)
				.Single(l => l.Event.SE_Code == AutoEvents.RecruitmentCheckpointFailed.Code).Parameters["PK"]);
		}
	}
}
