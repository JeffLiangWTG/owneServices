using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Confirmations.Module
{
	public class PickupDeliveryConfirmController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.PickupDeliveryConfirm; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.PickupDeliveryConfirm; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CommonPickupDeliveryConfirm); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return ShowShipmentForm(businessEntity, (controller, shipment) => controller.ShowViewForm(shipment));
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			return ShowShipmentForm(sourceEntity, (controller, shipment) => controller.ShowEditForm(shipment));
		}

		IZForm ShowShipmentForm(IBusiness sourceEntity, Func<ZController, CommonShipment, IZForm> formToShow)
		{
			ChildEditableService.SetState(((CommonPickupDeliveryConfirm)sourceEntity).Factory, ChildEditableServiceStates.Shipment);
			var shipment = ((CommonPickupDeliveryConfirm)sourceEntity).FirstShipment;

			IZForm result = null;
			if (shipment != null)
			{
				var forwardingShipmentController = ZControllerFactory.Create(ControllerIDs.JobShipment);
				result = formToShow(forwardingShipmentController, shipment); //not sure about passing back a form that is already shown
			}

			return result;
		}

		public override IZForm ShowNewForm()
		{
			throw new ControllerShowNewFormNotSupportedException("New Confirmations are not supported");
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PickupDeliveryConfirmations; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.PickupDeliveryConfirmationsNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.PickupDeliveryConfirmationsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.PickupDeliveryConfirmationsDelete; }
		}

		#endregion
	}
}
