using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Module
{
	public class CRMGlbCompanyCampaignController : ZController, IGlbCompanyCampaignController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public CRMGlbCompanyCampaignController()
		{
		}

		#region Standard Controller Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GlbCompanyCampaign; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbCompanyCampaign; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbCompanyCampaign); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbCompanyCampaignForm((GlbCompanyCampaign)businessEntity);
		}

		#endregion

		#region Showing Forms

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			if (!RunAdditionalShowFormChecks(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowViewForm(sourceEntity);
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			if (!RunAdditionalShowFormChecks(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowEditForm(sourceEntity);
		}

		public IZForm ShowEditFormButFocusOnCampaignItem(IGlbCompanyCampaignItem campaignItem)
		{
			var form = (GlbCompanyCampaignForm)ShowEditForm((BusinessObject)campaignItem.Campaign);
			form.FocusOnCampaignItem(FocusOnTrackingTabTypes.CampaignItem, campaignItem);
			return form;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (!RunAdditionalShowFormChecks(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowDeleteForm(sourceEntity);
		}

		protected override IZForm ShowCopyForm(BusinessObject inMemorySourceEntity, CopyOfBusinessObject returnsNewBusinessEntity)
		{
			if (!RunAdditionalShowFormChecks(inMemorySourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowCopyForm(inMemorySourceEntity, returnsNewBusinessEntity);
		}

		static bool RunAdditionalShowFormChecks(BusinessObject sourceEntity)
		{
			var campaign = sourceEntity as GlbCompanyCampaign;
			if (campaign != null && !Env.Security.CampaignAllowSearchOutsideLoginCompany.IsAllowed)
			{
				return CheckLoginCompanyMatches(campaign);
			}

			return true;
		}

		static bool CheckLoginCompanyMatches(GlbCompanyCampaign campaign)
		{
			var company = campaign.Company;
			if (company != null && company.PK != Env.CurrentCompany.PK)
			{
				var countryDescription = company.Country != null ? company.Country.Description : ZString.Empty;
				Globals.Message.ShowError(
						ResString.GetMultilingualString("199ecb5f-f7fc-4c26-979b-803f23394f98", @"You do not have the appropriate security rights to view {0}. You are only allowed to view campaigns for your login company.

If you require access to this function please login to the relevant company ({1} ({2})), or ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{3}",
								campaign.HumanReadableName,
								countryDescription,
								company.GC_Code,
								Env.Security.CampaignAllowSearchOutsideLoginCompany.DisplayTextPathToSecurityRight),
						ResString.GetMultilingualString("976f52ee-c6ee-43ad-97fa-3852d6a83ac1", "Access Denied: {0}", Env.Security.CampaignAllowSearchOutsideLoginCompany.DisplayText)
					);

				return false;
			}

			return true;
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			var hrCompanyCampaign = businessEntity as GlbCompanyCampaign;
			hrCompanyCampaign.G0_IsSalesAndMarketing = true;

			return base.ShowFormForNewEntityCore(businessEntity);
		}

		#endregion

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CampaignManagementDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CampaignManagementEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CampaignManagementNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CampaignManagementView; }
		}

		#endregion

		#region CRM Security

		readonly GlbCompanyCampaignCRMSecurityProvider SecurityProvider = new GlbCompanyCampaignCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as GlbCompanyCampaign, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as GlbCompanyCampaign, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as GlbCompanyCampaign, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
