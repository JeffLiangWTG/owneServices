using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(JobCompletionTriggerAction))]
	class JobCompletionTriggerActionTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported() => false;
	}
}
