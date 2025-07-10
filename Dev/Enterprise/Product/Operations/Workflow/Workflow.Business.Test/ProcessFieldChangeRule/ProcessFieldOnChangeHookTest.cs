using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test
{
	sealed class ProcessFieldOnChangeHookTest : TestCaseWithFactory
	{
		public void TestNoProcessFieldChangeHookOnNullObjects()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode;
			rule.PFR_GroupName = "JEChange";
			rule.PFR_SE_NKEvent = AutoEvents.CustomisableEvent00Code;

			rule.Fields.AddNew().PFL_FieldName = JobDeclarationSchema.JE_MessageType.Name;
			Factory.Save();

			AssertNoExceptionThrown(() =>
				{
					var record = Factory.GetNull<JobDeclaration>();
					record.JE_MessageType = "";
				}
			);
		}
	}
}
