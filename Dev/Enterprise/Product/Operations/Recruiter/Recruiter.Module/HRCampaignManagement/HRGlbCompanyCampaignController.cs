using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module
{
	public class HRGlbCompanyCampaignController : ZController, IGlbCompanyCampaignController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public HRGlbCompanyCampaignController()
		{
		}

		#region Standard Controller Overrides

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(HRGlbCompanyCampaign); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.HRGlbCompanyCampaign; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.HRGlbCompanyCampaign; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new HRGlbCompanyCampaignForm((HRGlbCompanyCampaign)businessEntity);
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
			if (campaign != null && !Env.Security.HRCampaignAllowSearchOutsideLoginCompany.IsAllowed)
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
						ResString.GetMultilingualString("8A81848E-114A-41BF-AA72-7FE42F67C4E2", @"You do not have the appropriate security rights to view {0}. You are only allowed to view campaigns for your login company.

If you require access to this function please login to the relevant company ({1} ({2})), or ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{3}",
								campaign.HumanReadableName,
								countryDescription,
								company.GC_Code,
								Env.Security.HRCampaignAllowSearchOutsideLoginCompany.DisplayTextPathToSecurityRight),
						ResString.GetMultilingualString("56D53E05-5715-4819-B2D8-22A950295D49", "Access Denied: {0}", Env.Security.HRCampaignAllowSearchOutsideLoginCompany.DisplayText)
					);

				return false;
			}

			return true;
		}

		public IZForm ShowEditFormButFocusOnCampaignItem(IGlbCompanyCampaignItem campaignItem)
		{
			var form = (HRGlbCompanyCampaignForm)ShowEditForm((BusinessObject)campaignItem.Campaign);
			form.FocusOnCampaignItem(FocusOnTrackingTabTypes.CampaignItem, campaignItem);
			return form;
		}

		#endregion

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.HRCampaignManagementDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.HRCampaignManagementEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.HRCampaignManagementNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.HRCampaignManagementView; }
		}

		#endregion
	}
}
