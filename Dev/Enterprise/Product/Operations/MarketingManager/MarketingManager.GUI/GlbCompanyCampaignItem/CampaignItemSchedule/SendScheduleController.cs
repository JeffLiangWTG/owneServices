using System;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public class SendScheduleController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public SendScheduleController()
		{
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

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbCompanyCampaignItemSchedule; }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.GlbCompanyCampaignItemSchedule;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbCompanyCampaignItemSchedule); }
		}

		protected override ZArchitecture.GUI.IZForm GetForm(CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return null;
		}

		protected override ZArchitecture.GUI.IZForm ShowLoadedForm(CargoWise.EntityFramework.IBusiness sourceEntity, ZArchitecture.GUI.FormAction action)
		{
			return null;
		}
	}
}
