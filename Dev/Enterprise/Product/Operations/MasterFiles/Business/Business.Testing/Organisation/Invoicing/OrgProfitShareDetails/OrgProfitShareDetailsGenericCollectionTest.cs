using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgProfitShareDetailsGenericCollection))]
	sealed class OrgProfitShareDetailsGenericCollectionTest : OrgProfitShareDetailsDependentCollectionTest
	{
		protected override OrgProfitShareDetailsDependentCollection CreateCollectionForTest(OrgAgentRelationship agent)
		{
			return new OrgProfitShareDetailsGenericCollection(agent);
		}
	}
}
