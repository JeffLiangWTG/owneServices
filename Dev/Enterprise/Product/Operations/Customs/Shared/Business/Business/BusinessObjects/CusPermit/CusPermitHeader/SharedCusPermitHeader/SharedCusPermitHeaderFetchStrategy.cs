using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	class SharedCusPermitHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public SharedCusPermitHeaderFetchStrategy(SharedCusPermitHeader baseCusPermitHeader)
			: base(baseCusPermitHeader)
		{
		}

		protected new SharedCusPermitHeader BusinessObject
		{
			get { return (SharedCusPermitHeader)base.BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(SharedCusPermitHeader.QuantityBalance):
					case nameof(SharedCusPermitHeader.ValueBalance):
					case nameof(SharedCusPermitHeader.AuthLatestQuantityBalance):
					case nameof(SharedCusPermitHeader.AuthLatestValueBalance):
					case nameof(SharedCusPermitHeader.AuthLatestValueBalanceWithMsg):
					case nameof(SharedCusPermitHeader.AuthLatestQuantityBalanceWithMsg):
						Factory.AddFetchHint(CusPermitLineTransactionSchema.CPL_CPH_PermitHeader, BusinessObject.PK);
						break;
				}
			}
		}
	}
}
