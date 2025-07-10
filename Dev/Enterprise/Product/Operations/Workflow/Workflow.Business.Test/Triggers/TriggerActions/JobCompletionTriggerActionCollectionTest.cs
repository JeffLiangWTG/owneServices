using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(JobCompletionTriggerActionCollection))]
	class JobCompletionTriggerActionCollectionTest : ActiveBusinessObjectCollectionTestCase<JobCompletionTriggerActionCollection>
	{
		protected override JobCompletionTriggerActionCollection GetCollectionToTest()
		{
			var trigger = WorkflowTestCase.CreateJobTriggerLink(Factory);

			return new JobCompletionTriggerActionCollection(trigger);
		}
	}
}
