using System;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class GlbCompanyCampaignContactController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public GlbCompanyCampaignContactController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbCompanyCampaignContact; }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.GlbCompanyCampaignContact;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CampaignContact); }
		}

		#region Security

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

		#endregion

		#region GetForm

		#region Controllers

		ZController OrgContactController
		{
			get { return ZControllerFactory.Create(ControllerIDs.OrgContacts); }
		}

		ZController InquiryController
		{
			get { return ZControllerFactory.Create(ControllerIDs.SalesEnquiry); }
		}

		#endregion

		protected override ZArchitecture.GUI.IZForm GetForm(CargoWise.EntityFramework.IBusiness businessEntity)
		{
			throw new NotSupportedException("Can not invoke GetForm() method, use ShowViewForm(), ShowEditForm(), ShowNewForm() or ShowDeleteForm() instead");
		}

		public override ZArchitecture.GUI.IZForm ShowViewForm(CargoWise.EntityFramework.BusinessObject sourceEntity)
		{
			CampaignContact contactType = sourceEntity as CampaignContact;
			if (contactType.VCC_TableCode == OrgContactSchema.Constants.Prefix)
			{
				return LastShownForm = OrgContactController.ShowViewForm(Factory.Load<OrgContact>(contactType.PK));
			}
			else if (contactType.VCC_TableCode == OrgColdCallRegisterSchema.Constants.Prefix)
			{
				return LastShownForm = InquiryController.ShowViewForm(Factory.Load<SalesEnquiry>(contactType.PK));
			}

			return null;
		}

		public override ZArchitecture.GUI.IZForm ShowEditForm(CargoWise.EntityFramework.BusinessObject sourceEntity)
		{
			CampaignContact contactType = sourceEntity as CampaignContact;
			if (contactType.VCC_TableCode == OrgContactSchema.Constants.Prefix)
			{
				return LastShownForm = OrgContactController.ShowEditForm(Factory.Load<OrgContact>(contactType.PK));
			}
			else if (contactType.VCC_TableCode == OrgColdCallRegisterSchema.Constants.Prefix)
			{
				return LastShownForm = InquiryController.ShowEditForm(Factory.Load<SalesEnquiry>(contactType.PK));
			}

			return null;
		}

		public override ZArchitecture.GUI.IZForm ShowNewForm()
		{
			Globals.Message.Show(Res.GetString("2dd0902c-29f3-4c5e-92e8-68af16715a51", "You are not allowed to add a new campaign contact"));
			return null;
		}

		public override ZArchitecture.GUI.IZForm ShowDeleteForm(CargoWise.EntityFramework.BusinessObject sourceEntity)
		{
			Globals.Message.Show(Res.GetString("224c13ae-c158-424b-8317-fa38237b11ba", "You are not allowed to delete a campaign contact"));
			return null;
		}

		public override ZArchitecture.GUI.IZForm ShowTemplateCopyForm(CargoWise.EntityFramework.BusinessObject inMemorySourceEntity)
		{
			Globals.Message.Show(Res.GetString("4f564bf8-3918-468e-9a7d-ef005ebd6275", "You are not allowed to copy a campaign contact"));
			return null;
		}

		#endregion
	}
}
