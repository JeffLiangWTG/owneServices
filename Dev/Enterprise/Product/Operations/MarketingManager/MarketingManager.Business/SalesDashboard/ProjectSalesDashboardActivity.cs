using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.MarketingManager.Business
{
	public class ProjectSalesDashboardActivity : SalesDashboardActivity
	{
		public ProjectSalesDashboardActivity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public BusinessObject Parent => Factory.Load<IProject>(VSA_ParentId) as BusinessObject;

		public override ZString OverallActivityDispositionDescription => VSA_ActivityStatus == ProcessTaskStatusCodeList.Codes.Closed || VSA_ActivityStatus == ProcessTaskStatusCodeList.Codes.Cancelled
			? OrgSalesCallOverallDispositionList.Descriptions.Closed
			: OrgSalesCallOverallDispositionList.Descriptions.Open;

		public override SalesDashboardProcessTaskCollectionView ActiveAndCompletedTasks => tasks
			?? (tasks = new SalesDashboardProcessTaskCollectionView((Parent as IWorkflowProvider)?.WorkflowItems));
		SalesDashboardProcessTaskCollectionView tasks;

		protected override ISalesRelationActivity ParentActivity => Parent as ISalesRelationActivity;
	}
}
