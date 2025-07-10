using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class InvoiceLinePackagePivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public InvoiceLinePackagePivotFetchStrategy(InvoiceLinePackagePivot pivot)
			: base(pivot)
		{
		}

		InvoiceLinePackagePivot Pivot
		{
			get { return BusinessObject as InvoiceLinePackagePivot; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusDecHouseContainerPackSchema.Constants.TableName, Pivot.CHC_CW);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobComInvoiceLineSchema.Constants.TableName, Pivot.CHC_JI);
		}
	}
}
