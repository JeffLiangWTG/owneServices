using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	public class BillContainersController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ControllerID ID
		{
			get { return ControllerIDs.AgencyBillContainers; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AgencyBillContainers; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BillOfLadingContainer); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new BillOfLadingForm((BillOfLading)businessEntity);
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			if (sourceEntity is BillOfLading)
			{
				return sourceEntity;
			}
			AgencyShipmentContainer container = (AgencyShipmentContainer)sourceEntity;
			return Factory.Load<BillOfLading>(container.JC_JS_FCLBookingOnlyLink);
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.AgencyBillOfLading.ToString();
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			var form = (BillOfLadingForm)base.ShowLoadedForm(sourceEntity, action);
			if (form != null)
			{
				form.SelectAndShowContainer((sourceEntity as BusinessObject).PK);
				form.DisableNewAction();
			}

			return form;
		}

		#region Show Form

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Delete not supported from this DetentionManagement.");
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			if (PrincipalSecurityCheck(sourceEntity))
			{
				return base.ShowEditForm(sourceEntity);
			}
			else
			{
				ShowPrincipalSecurityError();
				return null;
			}
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Template copy not supported from DetentionManagement.");
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			if (PrincipalSecurityCheck(sourceEntity))
			{
				return base.ShowViewForm(sourceEntity);
			}
			else
			{
				ShowPrincipalSecurityError();
				return null;
			}
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("New not supported from DetentionManagement.");
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AgencyBillOfLading; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AgencyBillOfLadingNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AgencyBillOfLadingEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AgencyBillOfLadingDelete; }
		}

		#endregion

		#region Implementation

		bool PrincipalSecurityCheck(IBusiness sourceEntity)
		{
			var container = sourceEntity as AgencyShipmentContainer;
			AgencyShipment shipment;
			OrgHeader principal;

			return container == null
				|| (shipment = container.Booking) == null
				|| (principal = shipment.Principal) == null
				|| ShipsAgencyPrincipalCollectionWithSecurityCheck.AllowedAccessTo(principal);
		}

		void ShowPrincipalSecurityError()
		{
			Globals.Message.ShowError(Res.GetString("d78c2ef5-efef-4e80-91a8-2af32dbaf1af", "You are not authorized to access shipments for this principal"));
		}

		#endregion
	}
}


