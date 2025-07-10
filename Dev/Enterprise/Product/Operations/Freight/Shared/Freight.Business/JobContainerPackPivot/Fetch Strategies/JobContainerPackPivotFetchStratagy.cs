using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class JobContainerPackPivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public JobContainerPackPivotFetchStrategy(JobContainerPackPivot pivot)
			: base(pivot)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(JobContainerSchema.Constants.TableName, Pivot.J6_JC);
			Factory.AddFetchHint(JobPackLinesSchema.Constants.TableName, Pivot.J6_JL);
		}

		JobContainerPackPivot Pivot
		{
			get { return (JobContainerPackPivot)BusinessObject; }
		}
	}
}
