using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	class WhsRowFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsRowFetchStrategy(WhsRow row)
			: base(row)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			var warehousePK = ((WhsRow)BusinessObject).WR_WW_Whs;

			Factory.AddFetchHint(WhsWarehouseSchema.PK, warehousePK);

			// OrgAddress query
			var orgAddressQuery = new ZDBOnlyQuery(typeof(OrgAddress));
			var warehouseSubQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsWarehouseSchema.WW_OA_WarehouseAddress);
			warehouseSubQuery.AddToFilter(WhsWarehouseSchema.PK, warehousePK);
			orgAddressQuery.AddSubQuery(warehouseSubQuery, JoinCondition.And);
			Factory.AddFetchHint(OrgAddressSchema.Instance, orgAddressQuery);

			// OrgHeader query
			var dbOnlyOrgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
			var warheouseSubQueryForOrgHeader = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsWarehouseSchema.WW_OA_WarehouseAddress);
			warheouseSubQueryForOrgHeader.AddToFilter(WhsWarehouseSchema.PK, warehousePK);
			orgAddressSubQuery.AddSubQuery(warheouseSubQueryForOrgHeader, JoinCondition.And);
			Factory.AddFetchHint(OrgHeaderSchema.Instance, dbOnlyOrgHeaderQuery);
		}
	}
}
