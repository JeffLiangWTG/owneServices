using Enterprise.Accounting.Integration.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	public class CartageFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<CommonCartage>
	{
		protected override CommonCartage GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<CommonCartage>();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get
			{
				return ModuleIDs.Cartage;
			}
		}
	}
}
