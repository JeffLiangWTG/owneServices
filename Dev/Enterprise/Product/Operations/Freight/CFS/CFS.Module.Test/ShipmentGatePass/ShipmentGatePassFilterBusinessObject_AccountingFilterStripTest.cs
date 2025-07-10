using CargoWise.Types;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class ShipmentGatePassFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<GatePassShipment>
	{
		protected override GatePassShipment GetNewBusinessObjectForFilterCollection()
		{
			GatePassShipment shipment = Factory.NewWithValidTestData<GatePassShipment>();
			shipment.JS_IsCFSRegistered = ZBool.True;
			shipment.JS_TranshipToOtherCFS = ZBool.True;
			return shipment;
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get { return ModuleIDs.ShipmentGatePass; }
		}

		protected override bool ShouldUseBillingFilters
		{
			get { return false; }
		}
	}
}
