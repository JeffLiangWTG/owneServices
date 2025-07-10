using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	class WhsWarehouseFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsWarehouseFetchStrategy(WhsWarehouse warehouse)
			: base(warehouse)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			// Tested in WarehouseFilterControlDbHitsTest.cs
			var requireAddress = false;
			var requireArea = false; 

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(WhsWarehouse.CountryCode):
						requireAddress = true;
						break;
					case nameof(WhsWarehouse.IsWarehouseFreeStoreEnabled):
					case nameof(WhsWarehouse.IsWarehouseBondEnabled):
					case nameof(WhsWarehouse.IsWarehouseExciseEnabled):
					case nameof(WhsWarehouse.IsInwardProcessingEnabled):
						requireArea = true;
						break;
					default:
						break;
				}
			}

			if (requireAddress)
			{
				Factory.AddFetchHint(OrgAddressSchema.PK, Parent.WW_OA_WarehouseAddress);
			}

			if (requireArea)
			{
				Factory.AddFetchHint(WhsAreaSchema.WA_WW_Whs, Parent.PK);
			}
		}

		WhsWarehouse Parent => (WhsWarehouse)BusinessObject;
	}
}
