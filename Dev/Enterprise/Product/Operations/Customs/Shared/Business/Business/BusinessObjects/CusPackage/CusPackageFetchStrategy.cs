using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	sealed class CusPackageFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusPackageFetchStrategy(CusPackage cusPackage)
			: base(cusPackage)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(CusPackage.RTUSLabelPrinterPK):
						Factory.AddFetchHint(StmDefaultPrinterSchema.SDP_SubjectID, BusinessObject.PK);
						break;
					case nameof(CusPackage.NetWeight):
					case nameof(CusPackage.UnitNetWeight):
						Factory.AddFetchHint(PkgPackageItemDivotSchema.KI_KP_Package, BusinessObject.PK);
						break;
				}
			}
		}

		new CusPackage BusinessObject => (CusPackage)base.BusinessObject;
	}
}
