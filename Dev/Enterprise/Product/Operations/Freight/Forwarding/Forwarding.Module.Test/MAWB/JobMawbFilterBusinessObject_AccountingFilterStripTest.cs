using Enterprise.Accounting.Integration.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class JobMawbFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<JobMawb>
	{
		protected override JobMawb GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<JobMawb>();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get { return ModuleIDs.JobMawb; }
		}

		protected override bool ShouldUseBillingFilters
		{
			get { return false; }
		}
	}
}
