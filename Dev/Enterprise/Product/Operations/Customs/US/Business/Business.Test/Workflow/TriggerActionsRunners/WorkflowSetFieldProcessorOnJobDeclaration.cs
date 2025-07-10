using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class WorkflowSetFieldProcessorOnJobDeclaration : TestCaseWithFactory
	{
		public void TestIntermediateFieldChangeFromUniversalTemplate()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode);
			universalTemplate.P0_Name = "Dummy Universal Template";

			var templateTrigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			templateTrigger.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			templateTrigger.Description = "123";

			var action = (ProcessTaskNotification)templateTrigger.TriggerActions.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task1\").P9_Status>";
			action.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var job = Factory.NewWithValidTestData<JobDeclaration>();

			var task1 = job.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "task1";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			job.Logs.AddNew(Events.CustomisableEvent00);

			AssertEquals("No errors should be reported in this case", 0, ErrorReporter.TotalErrorCount);
		}
	}
}
