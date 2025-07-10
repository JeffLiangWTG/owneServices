using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessFieldChangeRuleFieldCollection))]
	class ProcessFieldChangeRuleFieldCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessFieldChangeRuleFieldCollection>
	{
		protected override ProcessFieldChangeRuleFieldCollection GetCollectionToTest() => new ProcessFieldChangeRuleFieldCollection(Factory.NewWithValidTestData<ProcessFieldChangeRule>());
	}
}
