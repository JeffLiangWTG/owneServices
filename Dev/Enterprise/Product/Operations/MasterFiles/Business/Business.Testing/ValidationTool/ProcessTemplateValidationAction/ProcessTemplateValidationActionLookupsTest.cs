using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTemplateValidationActionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestActionSourceList()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "RPN";
			var templateValidation = template.ProcessTemplateValidations.AddNew();
			var templateValidationAction = templateValidation.ProcessTemplateValidationActions.AddNew();
			AssertEquals(true, templateValidationAction.Lookups.ActionSourceList.Count > 1);
			AssertEquals(true, templateValidationAction.Lookups.ActionSourceList.ContainsCode("SAV"));
		}
	}
}
