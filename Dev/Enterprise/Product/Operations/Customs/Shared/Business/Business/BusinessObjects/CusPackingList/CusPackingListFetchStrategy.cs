using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	sealed class CusPackingListFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusPackingListFetchStrategy(CusPackingList cusPackingList)
			: base(cusPackingList)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(CusPackingList.TotalNetWeight):
						Factory.AddFetchHint(CusPackableItemSchema.CUI_CUL, BusinessObject.PK);
						break;
				}
			}
		}

		new CusPackingList BusinessObject => (CusPackingList)base.BusinessObject;
	}
}
