using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketJobPivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsDocketJobPivotFetchStrategy(WhsDocketJobPivot pivot)
			: base(pivot)
		{
		}

		WhsDocketJobPivot Pivot
		{
			get { return (WhsDocketJobPivot)BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(WhsDocketSchema.PK, Pivot.WV_WD_Docket);
		}
	}
}
