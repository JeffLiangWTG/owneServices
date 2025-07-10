using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public class GlbCompanyCampaignItemController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Overrides

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CampaignManagementDelete; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CampaignManagementEdit; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CampaignManagementNew; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CampaignManagementView; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbCompanyCampaignItemForm((GlbCompanyCampaignItem)businessEntity);
		}

		#region Show New Form not allowed

		public override IZForm ShowNewForm()
		{
			ErrorReporter.ReportOnce("CannotCreateNewCampaignItem", "The operator tried to create a new Company Campaign Item, which can only happen when a Campaign is sent");
			return null;
		}

		#endregion

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbCompanyCampaignItem; }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.GlbCompanyCampaignItem;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbCompanyCampaignItem); }
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			var campaignItem = sourceEntity as GlbCompanyCampaignItem;
			if (ParentModule is GlbCompanyCampaignItemModule)
			{
				if (campaignItem.RecipientAsOrgContact != null)
				{
					return LastShownForm = OrgContactController.ShowViewForm(Factory.Load<OrgContact>(campaignItem.G8_RecipientID));
				}
				else if (campaignItem.RecipientAsSalesEnquiry != null)
				{
					return LastShownForm = InquiryController.ShowViewForm(Factory.Load<SalesEnquiry>(campaignItem.G8_RecipientID));
				}
				else if (campaignItem.RecipientAsHRJobApplicant != null)
				{
					return LastShownForm = ApplicantController.ShowViewForm((BusinessObject)Factory.Load<IHRJobApplicant>(campaignItem.G8_RecipientID));
				}
				else if (campaignItem.RecipientAsGlbStaff != null)
				{
					return LastShownForm = StaffController.ShowViewForm(Factory.Load<GlbStaff>(campaignItem.G8_RecipientID));
				}
			}
			else
			{
				LastShownForm = CampaignController.ShowViewForm(Factory.Load<GlbCompanyCampaign>(campaignItem.G8_G0));
				FocusOnCampaignItem(LastShownForm, campaignItem);
				return LastShownForm;
			}

			return null;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			GlbCompanyCampaignItem campaignItem = sourceEntity as GlbCompanyCampaignItem;
			if (ParentModule is GlbCompanyCampaignItemModule)
			{
				if (campaignItem.RecipientAsOrgContact != null)
				{
					return LastShownForm = OrgContactController.ShowEditForm(Factory.Load<OrgContact>(campaignItem.G8_RecipientID));
				}
				else if (campaignItem.RecipientAsSalesEnquiry != null)
				{
					return LastShownForm = InquiryController.ShowEditForm(Factory.Load<SalesEnquiry>(campaignItem.G8_RecipientID));
				}
				else if (campaignItem.RecipientAsHRJobApplicant != null)
				{
					return LastShownForm = ApplicantController.ShowEditForm((BusinessObject)Factory.Load<IHRJobApplicant>(campaignItem.G8_RecipientID));
				}
				else if (campaignItem.RecipientAsGlbStaff != null)
				{
					return LastShownForm = StaffController.ShowEditForm(Factory.Load<GlbStaff>(campaignItem.G8_RecipientID));
				}
			}
			else
			{
				LastShownForm = CampaignController.ShowEditForm(Factory.Load<GlbCompanyCampaign>(campaignItem.G8_G0));
				FocusOnCampaignItem(LastShownForm, campaignItem);
				return LastShownForm;
			}

			return null;
		}

		void FocusOnCampaignItem(IZForm form, GlbCompanyCampaignItem campaignItem)
		{
			if (LastShownForm != null)
			{
				GlbCompanyCampaignForm campaignForm = form as GlbCompanyCampaignForm;
				campaignForm.FocusOnCampaignItem(FocusOnTrackingTabTypes.CampaignItem, campaignItem);
			}
		}

		#endregion

		#region Controllers

		ZController OrgContactController
		{
			get { return ZControllerFactory.Create(ControllerIDs.OrgContacts); }
		}

		ZController InquiryController
		{
			get { return ZControllerFactory.Create(ControllerIDs.SalesEnquiry); }
		}

		ZController CampaignController
		{
			get { return ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaign); }
		}

		ZController ApplicantController
		{
			get { return ZControllerFactory.Create(ControllerIDs.HRJobApplicant); }
		}

		ZController StaffController
		{
			get { return ZControllerFactory.Create(ControllerIDs.GlbStaff); }
		}

		#endregion
	}
}
