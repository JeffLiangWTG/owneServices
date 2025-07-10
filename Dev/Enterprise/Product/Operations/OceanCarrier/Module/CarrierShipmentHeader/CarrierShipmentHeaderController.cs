using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.OceanCarrier.Business;
using Enterprise.OceanCarrier.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.OceanCarrier.Module
{
	public sealed class CarrierShipmentHeaderController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true; //TODO : investigate

		public override ControllerID ID => ControllerIDs.CarrierShipmentHeader;

		public override ModuleIdentifier ModuleID => ModuleIDs.CarrierShipmentHeader;

		public override Type TypeOfTopLevelBusinessObject => typeof(CarrierShipmentHeader);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var carrierShipmentHeader = (CarrierShipmentHeader)businessEntity;
			return new CarrierShipmentHeaderForm(carrierShipmentHeader);
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			var message = Res.GetString("7AF31CD5-BA7B-4088-ABFF-219157D6ADF6",
				"You cannot create a Carrier Shipment on the {0} Desktop, please go to the Ocean Carrier Portal.", "CargoWise");
			Globals.Message.ShowError(message);
			return null;
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		#endregion
	}
}
