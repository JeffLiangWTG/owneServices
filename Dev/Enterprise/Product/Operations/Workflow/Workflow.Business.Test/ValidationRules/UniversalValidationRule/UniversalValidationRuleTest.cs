using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(UniversalValidationRule))]
	class UniversalValidationRuleTest : EnterpriseBusinessObjectTestCase
	{
		UniversalValidationRule CreateRuleWithParent(BusinessObjectFactory factory)
		{
			var parent = factory.NewWithValidTestData<UniversalValidationRuleSet>();
			return parent.Rules.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => CreateRuleWithParent(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateRuleWithParent(factory);
	}
}
