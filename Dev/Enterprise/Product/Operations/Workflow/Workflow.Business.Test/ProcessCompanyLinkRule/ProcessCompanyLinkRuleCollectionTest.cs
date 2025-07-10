using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessCompanyLinkRuleCollection))]
	class ProcessCompanyLinkRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessCompanyLinkRuleCollection>
	{
		protected override ProcessCompanyLinkRuleCollection GetCollectionToTest() => new ProcessCompanyLinkRuleCollection(Factory);
	}
}
