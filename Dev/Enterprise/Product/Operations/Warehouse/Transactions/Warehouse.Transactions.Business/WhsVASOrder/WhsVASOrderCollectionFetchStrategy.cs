using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public WhsVASOrderCollectionFetchStrategy(IBusinessObjectCollection collection)
			: base(collection)
		{
		}

		// Tested in VASOrderFilterControlTest.cs
		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);

			if (columns.Any(c => c.ColumnName == nameof(WhsVASOrder.WarehousePK)))
			{
				var subQuery = new ZDBOnlySubQuery(typeof(WhsArea), WhsAreaSchema.WA_WW_Whs);
				subQuery.AddToFilter(WhsAreaSchema.PK, businessObjects.Cast<WhsVASOrder>().Select(vo => vo.WVO_WA_ServiceArea).Distinct());

				var result = new ZDBOnlyQuery(typeof(WhsWarehouse));
				result.AddSubQuery(subQuery, JoinCondition.And);

				Collection.Factory.AddFetchHint(WhsWarehouseSchema.Instance, result);
			}
		}
	}
}
