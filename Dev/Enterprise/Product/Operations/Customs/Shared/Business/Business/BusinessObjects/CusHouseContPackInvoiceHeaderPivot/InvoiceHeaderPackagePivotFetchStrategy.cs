using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class InvoiceHeaderPackagePivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public InvoiceHeaderPackagePivotFetchStrategy(InvoiceHeaderPackagePivot pivot)
			: base(pivot)
		{
		}

		InvoiceHeaderPackagePivot Pivot
		{
			get { return BusinessObject as InvoiceHeaderPackagePivot; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusDecHouseContainerPackSchema.Constants.TableName, Pivot.CHZ_CW);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(JobComInvoiceHeaderSchema.Constants.TableName, Pivot.CHZ_JZ);
		}
	}
}
