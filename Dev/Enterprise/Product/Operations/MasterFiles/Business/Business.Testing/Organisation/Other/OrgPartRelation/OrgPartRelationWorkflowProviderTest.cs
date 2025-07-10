using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartRelation))]
	sealed class OrgPartRelationWorkflowProviderTest : WorkflowProviderTest<OrgPartRelation, OrgPartRelationProcessTasksCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.OrgPartRelationWorkflowDescriptorCode; }
		}
	}
}
