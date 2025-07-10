using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public partial class QuotationStatusesCodeList : CodeDescriptionPairList
	{
		public bool IsOpenStatus(string status)
		{
			return OpenStatuses.Contains(status);
		}

		public IList<string> OpenStatuses
		{
			get
			{
				if (openStatuses == null)
				{
					openStatuses = new List<string>();
					openStatuses.Add(Codes.Active);
					openStatuses.Add(Codes.Finalized);
					openStatuses.Add(Codes.Approved);
				}
				return openStatuses;
			}
		}
		IList<string> openStatuses;
	}

	public class QuotationSalesDashboardActivity : SalesDashboardActivity
	{
		public QuotationSalesDashboardActivity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public BusinessObject Parent => Factory.Load<Quote>(VSA_ParentId);

		public override ZString OverallActivityDispositionDescription => (quotationStatuses ?? (quotationStatuses = new QuotationStatusesCodeList())).IsOpenStatus(VSA_ActivityStatus)
			? OrgSalesCallOverallDispositionList.Descriptions.Open
			: OrgSalesCallOverallDispositionList.Descriptions.Closed;
		QuotationStatusesCodeList quotationStatuses;

		public override SalesDashboardProcessTaskCollectionView ActiveAndCompletedTasks => tasks
			?? (tasks = new SalesDashboardProcessTaskCollectionView((Parent as IWorkflowProvider)?.WorkflowItems));
		SalesDashboardProcessTaskCollectionView tasks;

		protected override ISalesRelationActivity ParentActivity => Parent as ISalesRelationActivity;
	}
}
