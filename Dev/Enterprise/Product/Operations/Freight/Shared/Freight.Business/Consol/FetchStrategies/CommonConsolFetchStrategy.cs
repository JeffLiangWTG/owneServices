using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class CommonConsolFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CommonConsolFetchStrategy(CommonConsol consol)
			: base(consol)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID, BusinessObject.PK);
			Factory.AddFetchHint(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK, BusinessObject.PK);
			Factory.AddFetchHint(typeof(CommonContainer), JobContainerSchema.JC_JK, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();

			Factory.AddFetchHint(JobCartageSchema.JJ_ParentID, BusinessObject.PK);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case CommonConsol.Schema.JK_Calc_ContainerCount:
						Factory.AddFetchHint(JobContainerSchema.JC_JK, BusinessObject.PK);
						break;

					case CommonConsol.Schema.JK_CRN:
						Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
						break;
				}
			}
		}
	}
}
