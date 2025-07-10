using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	class BasePackageFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public BasePackageFetchStrategy(BasePackage package)
			: base(package)
		{
		}

		BasePackage Package
		{
			get { return (BasePackage)base.BusinessObject; }
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			Factory.AddFetchHint(typeof(BasePackage), CusDecHouseContainerPackSchema.CW_CW_Parent, Package.PK);
			Factory.AddFetchHint(typeof(InvoiceLinePackagePivot), CusHouseContPackInvoiceLinePivotSchema.CHC_CW, Package.PK);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			if (Package.Declaration?.SupportsChcPivotBetweenInvoiceLineAndPacking ?? false)
			{
				Factory.AddFetchHint(typeof(InvoiceLinePackagePivot), CusHouseContPackInvoiceLinePivotSchema.CHC_CW, Package.PK);
			}

			if (Package.Declaration?.SupportsChzPivotBetweenInvoiceHeaderAndPacking ?? false)
			{
				Factory.AddFetchHint(typeof(InvoiceHeaderPackagePivot), CusHouseContPackInvoiceHeaderPivotSchema.CHZ_CW, Package.PK);
			}
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			if (Package.Declaration?.SupportsChcPivotBetweenInvoiceLineAndPacking ?? false)
			{
				Factory.AddFetchHint(typeof(InvoiceLinePackagePivot), CusHouseContPackInvoiceLinePivotSchema.CHC_CW, Package.PK);
			}
			if (Package.Declaration?.SupportsChzPivotBetweenInvoiceHeaderAndPacking ?? false)
			{
				Factory.AddFetchHint(typeof(InvoiceHeaderPackagePivot), CusHouseContPackInvoiceHeaderPivotSchema.CHZ_CW, Package.PK);
			}
		}
	}
}

