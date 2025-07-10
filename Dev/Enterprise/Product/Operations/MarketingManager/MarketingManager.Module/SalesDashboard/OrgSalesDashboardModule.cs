using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Module
{
	public class OrgSalesDashboardModule : ZFilterGridModule, IOrgSalesDashboardModule
	{
		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.OrgSalesDashboard; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.SalesDashboard);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new SalesDashboardFilterControl((SalesDashboardActivityCollection)GridCollection, (SalesDashboardFilterBusinessObject)FilterBusinessObject, showCurrentLoginUserInfo: false);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return Org != null ? new SalesDashboardActivityCollection(Factory, Org) : new SalesDashboardActivityCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new SalesDashboardFilterBusinessObject(Org);
		}

		protected override MenuItem[] GetNewAdditionalContextMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewAdditionalContextMenuItems());

			var campaignTracking = new ZMenuItem(Res.GetData("507b417a-8ece-4530-9fa3-6df999f98f7b", "Campaign Tracking", "Edit Campaign and set Tracking tab to contact"), EditCampaign);
			campaignTracking.Name = "CampaignTracking";
			result.Add(campaignTracking);
			return result.ToArray();
		}

		void EditCampaign(object sender, EventArgs args)
		{
			var currentCampaignActivity = CurrentBusinessObjectInGrid as CampaignSalesDashboardActivity;
			if (currentCampaignActivity != null)
			{
				var selectedItem = Factory.Load<GlbCompanyCampaignItem>(currentCampaignActivity.VSA_ParentId);
				controller = ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaignItem);
				controller.ShowEditForm(selectedItem);
			}
		}

		protected ZController controller;

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		#endregion

		#region Security Checkpoint

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.SalesDashboard; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.SalesDashboard; }
		}

		#endregion

		#region IOrgSalesDashboardModule Members

		public IOrgHeader Org
		{
			get;
			set;
		}

		#endregion
	}
}
