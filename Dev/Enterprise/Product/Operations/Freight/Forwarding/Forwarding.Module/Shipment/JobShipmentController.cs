using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	/// <summary>
	/// Module Controller for JobShipment.
	/// </summary>
	public class JobShipmentController : TemplateRecordZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public JobShipmentController()
		{
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.JobShipment;
		public override ControllerID ID => ControllerIDs.JobShipment;

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ForwardingShipment); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var bizO = (ForwardingShipment)businessEntity;
			bizO.IsRoot = true;

			var form = (ZForm)GetFormCore(businessEntity);
			return form;
		}

		protected virtual IZForm GetFormCore(IBusiness businessEntity)
		{
			return new ShipmentForm((ForwardingShipment)businessEntity);
		}

		#region SetStrategyProvider

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			ChildEditableService.SetState(factory, ChildEditableServiceStates.Shipment);
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.MaintainShipment;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.MaintainShipmentNew;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.MaintainShipmentEdit;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.MaintainShipmentDelete;

		#endregion

		#region CRM Security

		protected virtual JobShipmentCRMSecurityProvider SecurityProvider => new JobShipmentCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as ForwardingShipment, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as ForwardingShipment, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as ForwardingShipment, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
