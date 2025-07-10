using System;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.GUI
{
	public class HRGlbCompanyCampaignContactController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.HRGlbCompanyCampaignContact; }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.HRGlbCompanyCampaignContact;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CampaignContact); }
		}

		#region Security

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.HRCampaignManagementDelete; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.HRCampaignManagementEdit; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.HRCampaignManagementNew; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.HRCampaignManagementView; }
		}

		#endregion

		#region GetForm

		#region Controllers

		ZController GlbStaffController
		{
			get { return ZControllerFactory.Create(ControllerIDs.GlbStaff); }
		}

		ZController HRJobApplicantController
		{
			get { return ZControllerFactory.Create(ControllerIDs.HRJobApplicant); }
		}

		#endregion

		protected override ZArchitecture.GUI.IZForm GetForm(CargoWise.EntityFramework.IBusiness businessEntity)
		{
			throw new NotSupportedException("Can not invoke GetForm() method, use ShowViewForm(), ShowEditForm(), ShowNewForm() or ShowDeleteForm() instead");
		}

		public override ZArchitecture.GUI.IZForm ShowViewForm(CargoWise.EntityFramework.BusinessObject sourceEntity)
		{
			var contactType = sourceEntity as CampaignContact;
			if (contactType.VCC_TableCode == GlbStaffSchema.Constants.Prefix)
			{
				return LastShownForm = GlbStaffController.ShowViewForm(Factory.Load<GlbStaff>(contactType.PK));
			}
			else if (contactType.VCC_TableCode == HRJobApplicantSchema.Constants.Prefix)
			{
				return LastShownForm = HRJobApplicantController.ShowViewForm(Factory.Load<HRJobApplicant>(contactType.PK));
			}

			return null;
		}

		public override ZArchitecture.GUI.IZForm ShowEditForm(CargoWise.EntityFramework.BusinessObject sourceEntity)
		{
			var contactType = sourceEntity as CampaignContact;
			if (contactType.VCC_TableCode == GlbStaffSchema.Constants.Prefix)
			{
				return LastShownForm = GlbStaffController.ShowEditForm(Factory.Load<GlbStaff>(contactType.PK));
			}
			else if (contactType.VCC_TableCode == HRJobApplicantSchema.Constants.Prefix)
			{
				return LastShownForm = HRJobApplicantController.ShowEditForm(Factory.Load<HRJobApplicant>(contactType.PK));
			}

			return null;
		}

		public override ZArchitecture.GUI.IZForm ShowNewForm()
		{
			Globals.Message.Show(Res.GetString("fd757378-75cf-46ea-8552-22679eafc96a", "You are not allowed to add a new campaign contact"));
			return null;
		}

		public override ZArchitecture.GUI.IZForm ShowDeleteForm(CargoWise.EntityFramework.BusinessObject sourceEntity)
		{
			Globals.Message.Show(Res.GetString("f1ddf601-2fc8-41a3-81b1-dd9219278d89", "You are not allowed to delete a campaign contact"));
			return null;
		}

		public override ZArchitecture.GUI.IZForm ShowTemplateCopyForm(CargoWise.EntityFramework.BusinessObject inMemorySourceEntity)
		{
			Globals.Message.Show(Res.GetString("7e5fa35e-699c-4c9a-8a1c-0e265dd7f0d1", "You are not allowed to copy a campaign contact"));
			return null;
		}

		#endregion
	}
}
