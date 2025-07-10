using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	class WhsAreaFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsAreaFetchStrategy(WhsArea area)
			: base(area)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			bool requireTransitClient = false;
			bool requireWarehouse = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case WhsAreaSchema.Constants.WA_OH_TransitClient:
						requireTransitClient = true;
						break;
					case WhsAreaSchema.Constants.WA_WW_Whs:
						requireWarehouse = true;
						break;
					default:
						break;
				}
			}

			if (requireTransitClient)
			{
				Factory.AddFetchHint(OrgHeaderSchema.PK, Parent.WA_OH_TransitClient);
			}

			if (requireWarehouse)
			{
				Factory.AddFetchHint(WhsWarehouseSchema.PK, Parent.WA_WW_Whs);
			}
		}

		WhsArea Parent
		{
			get { return (WhsArea)BusinessObject; }
		}
	}
}
