using System;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public class GlbCompanyCampaignClickController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public GlbCompanyCampaignClickController()
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
			get { return ControllerIDs.GlbCompanyCampaignClick; }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.GlbCompanyCampaignClick;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbCompanyCampaignClick); }
		}

		protected override ZArchitecture.GUI.IZForm GetForm(CargoWise.EntityFramework.IBusiness businessEntity)
		{
			var bizO = businessEntity as GlbCompanyCampaignClick;
			return new GlbCompanyCampaignItemForm(bizO.CampaignItem);
		}
	}
}
