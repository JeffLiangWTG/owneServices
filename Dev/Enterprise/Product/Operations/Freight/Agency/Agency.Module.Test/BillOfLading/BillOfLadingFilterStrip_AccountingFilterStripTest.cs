using Enterprise.Accounting.Integration.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class BillOfLadingFilterStrip_AccountingFilterStripTest : AccountingFilterStripTest<BillOfLading>
	{
		#region Implementation
		protected override BillOfLading GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<BillOfLading>();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get
			{
				return ModuleIDs.AgencyBillOfLading;
			}
		}
		#endregion
	}
}
