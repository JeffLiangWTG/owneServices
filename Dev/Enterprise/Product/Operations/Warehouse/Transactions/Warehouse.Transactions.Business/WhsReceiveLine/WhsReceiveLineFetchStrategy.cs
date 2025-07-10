using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsReceiveLineFetchStrategy : WhsDocketLineFetchStrategy
	{
		public WhsReceiveLineFetchStrategy(WhsReceiveLine docketLine)
			: base(docketLine)
		{
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(WhsInventoryViewSchema.WI_WE_InDocketLine, BusinessObject.PK);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var line = (WhsDocketLine)BusinessObject;
			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case WhsReceiveLine.Schema.SplitQuantity:
						Factory.AddFetchHint(typeof(WhsInventoryView), new ZQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, line.PK));
						break;

					case WhsReceiveLine.Schema.ConsigneeNameOrPK:
						Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, line.PK);
						Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, line.PK);
						break;
				}
			}
		}
	}
}
