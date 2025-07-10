namespace Enterprise.ReportTesting.MasterFiles
{
	using System;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Registry.Business;
	using Enterprise.ReportTesting;
	using Enterprise.ZArchitecture.Core;

	[TemplateName("Workflow Exceptions")]
	public class TestWorkflowExceptionsTemplate : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			var items = WorkflowDataRegistry.Instance.TaskTypes.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), new Guid(), new Guid());
			CategorisedWorkflowTaskTypes item = items.AddNew();
			item.Code = "WAR";
			item.Description = (NoResString)"Huh!, what is it good for?";
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), new Guid(), new Guid(), items);
		}
	}

	public class TestWorkflowExceptionsReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.ProcessMgrReports(); }
		}

		public override string Hint
		{
			get
			{
				return @"Use this report to manage your workflow exceptions. It lists exceptions created within a nominated date range. 
Exceptions are triggered by Milestone failures across the operational jobs against which you actively choose to track workflow and milestone events.  An exception is a deviation from the normal flow of a process and identifies a service / workflow failure. 
To run the report you nominate an exception date range. Optional filters include limiting the report to exceptions for a particular job type, milestone event code, exception type, client or staff member. A ""description"" filter limits the report to only listing exceptions where the actual exception description includes the nominated text.
By default, the report excludes exceptions that have since been actioned / resolved.
Note: Exceptions are reported against the actual job record on which the exception was created. The date of an exception can be seen on the Workflow & Tracking > Exceptions tab of each job.";
			}
		}

		public override string MenuName
		{
			get { return @"Workflow Exceptions Report"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWorkflowExceptionsTemplate();
		}
	}
}
