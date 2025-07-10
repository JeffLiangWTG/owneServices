using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class OpportunitySalesDashboardActivity : SalesDashboardActivity
	{
		public OpportunitySalesDashboardActivity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public OrgOpportunity ParentOpportunity
		{
			get { return Factory.Load<OrgOpportunity>(VSA_ParentId); }
		}

		public override ZString OverallActivityDispositionDescription
		{
			get { return ParentOpportunity?.OverallDispositionDescription ?? ZString.Empty; }
		}

		public override SalesDashboardProcessTaskCollectionView ActiveAndCompletedTasks
		{
			get { return tasks ?? (tasks = new SalesDashboardProcessTaskCollectionView(ParentOpportunity.WorkflowItems)); }
		}
		SalesDashboardProcessTaskCollectionView tasks;

		#region ISalesRelationActivity

		protected override ISalesRelationActivity ParentActivity
		{
			get { return ParentOpportunity; }
		}

		#endregion
	}
}
