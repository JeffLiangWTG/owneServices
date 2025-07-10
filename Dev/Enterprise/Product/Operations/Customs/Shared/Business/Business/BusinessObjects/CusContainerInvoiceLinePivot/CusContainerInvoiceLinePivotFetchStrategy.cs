using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusContainerInvoiceLinePivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusContainerInvoiceLinePivotFetchStrategy(CusContainerInvoiceLinePivot pivot)
			: base(pivot)
		{
		}

		CusContainerInvoiceLinePivot Pivot
		{
			get { return BusinessObject as CusContainerInvoiceLinePivot; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusContainerSchema.Constants.TableName, Pivot.C2_CO);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobComInvoiceLineSchema.Constants.TableName, Pivot.C2_JI);
		}
	}
}
