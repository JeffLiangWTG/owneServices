using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(UniversalValidationRuleSetCollection))]
	class UniversalValidationRuleSetCollectionTest : ActiveBusinessObjectCollectionTestCase<UniversalValidationRuleSetCollection>
	{
		protected override UniversalValidationRuleSetCollection GetCollectionToTest() => new UniversalValidationRuleSetCollection(Factory);
	}
}
