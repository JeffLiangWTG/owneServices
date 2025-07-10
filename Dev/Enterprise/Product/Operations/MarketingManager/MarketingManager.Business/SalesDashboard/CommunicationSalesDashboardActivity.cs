using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class CommunicationSalesDashboardActivity : SalesDashboardActivity
	{
		public CommunicationSalesDashboardActivity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public OrgSalesCall SalesCall
		{
			get { return Factory.Load<OrgSalesCall>(VSA_ParentId); }
		}

		public override ZString OverallActivityDispositionDescription
		{
			get { return SalesCall?.OverallDispositionDescription ?? ZString.Empty; }
		}

		public override SalesDashboardProcessTaskCollectionView ActiveAndCompletedTasks
		{
			get { return tasks ?? (tasks = new SalesDashboardProcessTaskCollectionView(SalesCall.WorkflowItems)); }
		}
		SalesDashboardProcessTaskCollectionView tasks;

		#region ISalesRelationActivity

		protected override ISalesRelationActivity ParentActivity
		{
			get { return SalesCall; }
		}

		#endregion
	}
}
