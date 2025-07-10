using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Confirmations.Module
{
	public class PickupDeliveryConfirmModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.PickupDeliveryConfirm; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.PickupDeliveryConfirm);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new PickupDeliveryConfirmFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CommonPickupDeliveryConfirmCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new PickupDeliveryConfirmFilterBusinessObject();
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		#region Security / Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Forwarder; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.PickupDeliveryConfirmations; }
		}

		#endregion
	}
}
