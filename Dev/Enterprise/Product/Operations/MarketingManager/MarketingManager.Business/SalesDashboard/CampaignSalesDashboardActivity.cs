using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class CampaignSalesDashboardActivity : SalesDashboardActivity
	{
		public CampaignSalesDashboardActivity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public BusinessObject ParentCampaign => Factory.Load<GlbCompanyCampaignItem>(VSA_ParentId)?.CompanyCampaign;

		public override ZString OverallActivityDispositionDescription => VSA_ActivityStatus == TrackingStatusCodes.Codes.QUE
			? OrgSalesCallOverallDispositionList.Descriptions.Open
			: OrgSalesCallOverallDispositionList.Descriptions.Closed;

		public override SalesDashboardProcessTaskCollectionView ActiveAndCompletedTasks => tasks
			?? (tasks = new SalesDashboardProcessTaskCollectionView((ParentCampaign as IWorkflowProvider)?.WorkflowItems));
		SalesDashboardProcessTaskCollectionView tasks;

		protected override ISalesRelationActivity ParentActivity => ParentCampaign as ISalesRelationActivity;
	}
}
