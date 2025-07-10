using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Confirmations.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Confirmations.Module
{
	public class ConsolidatedTransportBookingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ConsolidatedTransportBooking; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ConsolidatedTransportBooking; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CommonConsolidatedTransportBooking); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ConsolidatedTransportBookingForm((CommonConsolidatedTransportBooking)businessEntity);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ConsolidatedTransportBooking; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ConsolidatedTransportBookingNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ConsolidatedTransportBookingEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ConsolidatedTransportBookingDelete; }
		}

		#endregion

		#region CRM Security

		readonly ConsolidatedTransportBookingCRMSecurityProvider SecurityProvider = new ConsolidatedTransportBookingCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as CommonConsolidatedTransportBooking, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as CommonConsolidatedTransportBooking, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as CommonConsolidatedTransportBooking, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
