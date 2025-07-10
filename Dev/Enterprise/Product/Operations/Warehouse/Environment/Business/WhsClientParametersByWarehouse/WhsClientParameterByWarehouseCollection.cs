using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsClientParameterByWarehouseCollection : ActiveBusinessObjectCollection<WhsClientParameterByWarehouse>
	{
		#region Constructors

		public WhsClientParameterByWarehouseCollection(OrgHeader master)
			: this(master, null)
		{
		}

		public WhsClientParameterByWarehouseCollection(WhsWarehouse master)
			: base(master.Factory, master, null, WhsClientParameterByWarehouseSchema.WY_WW_Whs)
		{
		}

		public WhsClientParameterByWarehouseCollection(OrgHeader master, ZGuid warehouse)
			: this(master, new ZQuery(WhsClientParameterByWarehouseSchema.WY_WW_Whs, warehouse))
		{
		}

		WhsClientParameterByWarehouseCollection(OrgHeader master, ZQuery query)
			: base(master.Factory, master, query, WhsClientParameterByWarehouseSchema.WY_OH_Client)
		{
		}

		#endregion

		#region Methods

		public WhsClientParameterByWarehouse FindWithEmptyFallback(ZGuid clientPK, ZGuid warehousePK, ZString receiveCategoryCode)
		{
			var ranker = new ColumnValueRanker();
			ranker.Add(WhsClientParameterByWarehouseSchema.WY_OH_Client, clientPK, ZGuid.Empty);
			ranker.Add(WhsClientParameterByWarehouseSchema.WY_WW_Whs, warehousePK, ZGuid.Empty);
			ranker.Add(WhsClientParameterByWarehouseSchema.WY_ReceiveCategory, receiveCategoryCode, ZString.Empty);

			return ranker.GetBestMatch(this)?.FirstOrDefault();
		}

		#endregion
	}
}
