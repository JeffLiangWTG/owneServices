using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(CompletionTriggerActionCollection))]
	class CompletionTriggerActionCollectionTest : ActiveBusinessObjectCollectionTestCase<CompletionTriggerActionCollection>
	{
		protected override CompletionTriggerActionCollection GetCollectionToTest()
		{
			var trigger = WorkflowTestCase.CreateTrigger(Factory);

			return new CompletionTriggerActionCollection(trigger);
		}
	}
}
