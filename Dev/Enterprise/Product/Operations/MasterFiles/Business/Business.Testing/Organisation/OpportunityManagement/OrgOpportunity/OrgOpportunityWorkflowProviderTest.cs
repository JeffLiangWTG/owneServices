using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunity))]
	sealed class OrgOpportunityWorkflowProviderTest : WorkflowProviderTest<OrgOpportunity, OpportunityProcessTasksCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return new OpportunityWorkflowDescriptor().Code; }
		}
	}
}
