using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TemplateProcessTask))]
	sealed class TemplateProcessTasksTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert(true);
		}

		public void TestParentTemplateID_TemplatesSharePK()
		{
			AssertEquals("This is so the Task Grid conveniently shows the template it belongs to.", ProcessTask.PK, ProcessTask.TemplateID);
		}

		public void TestParentTemplateID_TemplatesShareSoureTemplatePK()
		{
			AssertEquals("This is so the Task Grid conveniently shows the template it belongs to.", ProcessTask.Parent.PK, ProcessTask.SourceTemplatePK);
		}

		public void TestParentTemplateName_TemplatesShareSoureTemplateName()
		{
			AssertEquals("This is so the Task Grid conveniently shows the template it belongs to.", ProcessTask.Parent.P0_Name, ProcessTask.SourceTemplateName);
		}

		public void TestTemplateTaskStatusShouldNotAffectOtherProperties()
		{
			var task1 = Factory.New<ProcessTaskTemplate>().WorkflowItems.Tasks.AddNew();
			AssertEquals("Precondition", ZDateTime.Empty, task1.P9_ActualDate.ToZDateTime());
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task1.P9_Status);
			AssertEquals("Should not set Actual Date", ZDateTime.Empty, task1.P9_ActualDate.ToZDateTime());

			var task2 = Factory.New<ProcessTaskTemplate>().WorkflowItems.Tasks.AddNew();
			AssertEquals("Precondition", ZDateTime.Empty, task2.P9_ActualDuration);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
			AssertEquals("Should not set Actual Task Duration", ZDateTime.Empty, task2.P9_ActualDuration);
		}

		public void TestWorkflowTypeGetter_LineTriggerTypeIsNotSpecified_ReturnTemplateProcessType()
		{
			ProcessTask.P9_LineTriggerType = ZString.Empty;
			ProcessTaskTemplate.P0_ProcessType = "DUM";

			AssertEquals("WorkflowType", "DUM", ProcessTask.WorkflowType);
		}

		public void TestWorkflowTypeGetter_LineTriggerTypeIsSpecified_ReturnLineTriggerWorkflowType()
		{
			ProcessTaskTemplate.P0_ProcessType = "SSS";
			ProcessTask.P9_LineTriggerType = "DUM";

			AssertEquals("WorkflowType", "DUM", ProcessTask.WorkflowType);
		}

		public void TestWorkflowType()
		{
			ProcessTaskTemplate.P0_ProcessType = "ZZZ";
			AssertEquals("ZZZ", ProcessTask.WorkflowType);
		}

		public void TestRootTypesGetter()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			var task = template.WorkflowItems.Triggers.AddNew();
			task.P9_LineTriggerType = "SHP";

			var expectedTypes = new[] { typeof(DummyWithWorkflow), typeof(TemplateProcessTask) };
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			var actualTypes = ((IRootTypeProvider)task).RootTypes;
			AssertContainsExactElementsInAnyOrder("Root types list", expectedTypes, actualTypes.Skip(1));
		}

		public void TestRootsGeteer()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			var task = template.WorkflowItems.Triggers.AddNew();
			task.P9_LineTriggerType = "SHP";

			var expectedRoots = System.Array.Empty<BusinessObject>();
			var actualRoots = ((IRootTypeProvider)task).Roots;
			AssertContainsExactElementsInAnyOrder("Roots list", expectedRoots, actualRoots);
		}

		public void TestLookups()
		{
			AssertEquals(typeof(TemplateProcessTaskLookups), ProcessTask.Lookups.GetType());
		}

		#region Implementation

		TemplateProcessTask ProcessTask
		{
			get
			{
				if (processTask == null)
				{
					processTask = (TemplateProcessTask)ProcessTaskTemplate.WorkflowItems.AddNew();
				}
				return processTask;
			}
		}
		TemplateProcessTask processTask;

		ProcessTaskTemplate ProcessTaskTemplate
		{
			get
			{
				if (processTaskTemplate == null)
				{
					processTaskTemplate = Factory.New<ProcessTaskTemplate>();
				}
				return processTaskTemplate;
			}
		}
		ProcessTaskTemplate processTaskTemplate;

		protected override BusinessObject GetNewBusinessObject()
		{
			ProcessTaskTemplate template = Factory.New<ProcessTaskTemplate>();
			return template.WorkflowItems.AddNew();
		}

		#endregion
	}
}
