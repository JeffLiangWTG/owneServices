using Enterprise.Accounting.Integration.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class ShipmentReceivalFilterStrip_AccountingFilterStripTest : AccountingFilterStripTest<CFSShipment>
	{
		protected override CFSShipment GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<CFSShipment>();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get { return ModuleIDs.ShipmentReceival; }
		}
	}
}
