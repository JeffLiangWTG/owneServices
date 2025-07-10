using Enterprise.Accounting.Integration.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class ShipmentFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<ForwardingShipment>
	{
		protected override ForwardingShipment GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<ForwardingShipment>();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get { return ModuleIDs.JobShipment; }
		}
	}
}
