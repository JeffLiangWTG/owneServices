using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public class GlbCompanyCampaignClickModule : ZFilterGridModule, IGlbCompanyCampaignClickModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaignClick);
		}

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			var campaignItem = (GlbCompanyCampaignItem)CampaignItem;
			if (campaignItem != null)
			{
				return new GlbCompanyCampaignClickFilterBusinessObject(campaignItem.CompanyCampaign);
			}
			return new GlbCompanyCampaignClickFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbCompanyCampaignClickFilterControl(GridCollection, (GlbCompanyCampaignClickFilterBusinessObject)FilterBusinessObject, (GlbCompanyCampaignItem)CampaignItem);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return CampaignItem != null ? new GlbCompanyCampaignClickCollection(((GlbCompanyCampaignItem)CampaignItem).CompanyCampaign) : new GlbCompanyCampaignClickCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbCompanyCampaignClick; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.RelationshipCampaignManager; }
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CampaignManagement; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		public override bool AllowNew
		{
			get
			{
				return false;
			}
		}

		public override bool AllowDelete
		{
			get
			{
				return false;
			}
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

#if DEBUG
		public
#endif
 IZForm LastShownForm;

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			LastShownForm = base.ShowViewForm(selectedBusinessObject);
			return LastShownForm;
		}

		#region IGlbCompanyCampaignClickModule Members

		public MasterFiles.Integration.IGlbCompanyCampaignItem CampaignItem
		{
			get;
			set;
		}

		#endregion
	}
}
