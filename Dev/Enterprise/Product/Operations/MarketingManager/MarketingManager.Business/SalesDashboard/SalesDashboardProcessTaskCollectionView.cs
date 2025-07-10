using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class SalesDashboardProcessTaskCollectionView : ProcessTaskCollectionView
	{
		public SalesDashboardProcessTaskCollectionView(ProcessTaskCollection collection)
			: base(collection)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var task = element as ProcessTask;
			return base.IsThisPartOfTheCollection(element) && (task.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled && task.P9_Status != ProcessTaskStatusCodeList.Codes.Open);
		}
	}
}
