using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Testing
{
	class UniversalValidationRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestVR_BusinessRuleIsMandatory()
		{
			var rule = Factory.NewWithValidTestData<UniversalValidationRule>();
			rule.VR_BusinessRule = "";
			Factory.Save();
			AssertEquals(rule.Notifications.First().Message, "Error - VR_BusinessRule: Please enter a Business Rule.");
		}

		public void TestVR_StatusIsMandatory()
		{
			var rule = Factory.NewWithValidTestData<UniversalValidationRule>();
			rule.VR_Status = "";
			Factory.Save();
			AssertEquals(rule.Notifications.First().Message, "Error - VR_Status: Please enter a Status.");
		}
	}
}
