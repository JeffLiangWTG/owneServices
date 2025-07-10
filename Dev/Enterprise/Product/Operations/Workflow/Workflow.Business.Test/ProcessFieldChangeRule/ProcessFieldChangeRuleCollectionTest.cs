using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessFieldChangeRuleCollection))]
	class ProcessFieldChangeRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessFieldChangeRuleCollection>
	{
		protected override ProcessFieldChangeRuleCollection GetCollectionToTest() => new ProcessFieldChangeRuleCollection(Factory);
	}
}
