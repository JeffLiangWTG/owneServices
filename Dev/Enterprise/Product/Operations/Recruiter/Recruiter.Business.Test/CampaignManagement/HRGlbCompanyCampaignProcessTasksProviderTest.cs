using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRGlbCompanyCampaign))]
	sealed class HRGlbCompanyCampaignProcessTasksProviderTest : WorkflowProviderTest<HRGlbCompanyCampaign, HRCampaignProcessTasksCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return new HRCampaignWorkflowDescriptor().Code; }
		}

		protected override HRGlbCompanyCampaign GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<HRGlbCompanyCampaign>();
		}
	}
}
