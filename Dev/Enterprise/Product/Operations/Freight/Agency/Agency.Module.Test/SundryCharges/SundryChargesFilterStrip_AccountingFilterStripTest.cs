using Enterprise.Accounting.Integration.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class SundryChargesFilterStrip_AccountingFilterStripTest : AccountingFilterStripTest<SundryCharges>
	{
		protected override SundryCharges GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<SundryCharges>();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get
			{
				return ModuleIDs.AgencySundryCharges;
			}
		}
	}
}
