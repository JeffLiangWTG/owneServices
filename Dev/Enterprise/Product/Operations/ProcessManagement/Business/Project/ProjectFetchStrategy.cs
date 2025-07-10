using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class ProjectFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public ProjectFetchStrategy(Project project)
			: base(project)
		{
		}

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			SalesRelationActivityFetchStrategyHelper.AddFetchHintsForView(Factory, (ISalesRelationActivity)BusinessObject, columns);
		}

		#endregion
	}
}
