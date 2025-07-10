using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.eManifest.Module
{
	public class eManifestShipmentModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.US.eManifestShipment; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.US.eManifestShipment);
		}

		#region Filter

		protected override IFilterControl GetNewFilterControl()
		{
			return new eManifestShipmentFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ActiveBusinessObjectCollection<Shipment>(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new eManifestShipmentFilterStrip();
		}

		#endregion

		#region Checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.USeManifest; }
		}

		#endregion

		#region Allow

		public override ZBool HasActions
		{
			get { return false; }
		}

		public override bool AllowView
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		#endregion
	}
}
