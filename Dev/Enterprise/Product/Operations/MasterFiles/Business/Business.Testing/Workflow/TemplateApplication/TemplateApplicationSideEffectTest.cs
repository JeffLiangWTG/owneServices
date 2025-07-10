using CargoWise.Common;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TemplateApplicationSideEffectTest : TemplateApplicationTestCase
	{
		public void TestTaskAssignmentOnTemplateApplication()
		{
			var template = MakeTemplate();
			var tasks = new[] { MakeTask(template), MakeTask(template) };
			foreach (var task in tasks)
			{
				task.P9_Sequence = 1;
				task.P9_GS_NKAssignedStaffMember = "";
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			}
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals(GlbStaff.CurrentUser.GS_Code, dummy.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, dummy.WorkflowItems[1].P9_GS_NKAssignedStaffMember);
		}

		public void TestDeletingDuringTemplateApplicationReportsError()
		{
			var template = MakeTemplate();
			var task = MakeTask(template);
			Factory.Save();

			WorkflowItemCollectionView.OnItemCreated.Value = t => t.TemplateTask.Delete();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals("Should not be deleting a task in the middle of template application.", 2, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}
	}
}
