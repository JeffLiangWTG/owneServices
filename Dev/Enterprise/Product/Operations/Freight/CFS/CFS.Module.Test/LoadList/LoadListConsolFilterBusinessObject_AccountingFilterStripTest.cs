using Enterprise.Accounting.Integration.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module
{
	sealed class LoadListConsolFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<CFSLoadListConsol>
	{
		protected override CFSLoadListConsol GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<CFSLoadListConsol>();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get { return ModuleIDs.LoadListConsol; }
		}
	}
}
