using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaign))]
	sealed class GlbCompanyCampaignProcessTasksProviderTest : WorkflowProviderTest<GlbCompanyCampaign, ProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return new CRMCampaignWorkflowDescriptor().Code; }
		}
	}
}
