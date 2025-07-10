using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.OceanCarrier.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.OceanCarrier.Module
{
	public sealed class CarrierShipmentHeaderModule : GlowOnlyModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CarrierShipmentHeaderFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CarrierShipmentHeaderFilterControl(GridCollection, (CarrierShipmentHeaderFilterStripBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CarrierShipmentHeaderCollection(Factory);

		public override ModuleIdentifier ID => ModuleIDs.CarrierShipmentHeader;

		protected override ControllerID ControllerID => ControllerIDs.CarrierShipmentHeader;

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.LinerAndAgency;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ShippingManager;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CarrierShipmentHeaderWorkflowDescriptorCode;
	}
}
