using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Testing
{
	class UniversalValidationRuleSetValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDataContext()
		{
			var ruleSet = Factory.New<UniversalValidationRuleSet>();
			var info = ruleSet.VRS_DataContextInfo;
			ruleSet.Validation.ValidateVRS_DataContext();
			AssertHasErrors("blank is an error", info);

			ruleSet.VRS_DataContext = "not a data context";
			AssertHasErrors("error if not in the list of data contexts", info);

			ruleSet.VRS_DataContext = nameof(UniversalDataBuss.Integration.DataContextType.ForwardingShipment);
			AssertNoNotifications(info);
		}
	}
}
