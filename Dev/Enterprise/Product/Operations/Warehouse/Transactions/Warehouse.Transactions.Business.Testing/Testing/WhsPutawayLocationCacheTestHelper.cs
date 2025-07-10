using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	static class WhsPutawayLocationCacheTestHelper
	{
		public static IEnumerable<DataRow> GetAllCacheRecords(BusinessObjectFactory factory, ZGuid warehousePK)
		{
			var whsParameter = ZSqlParameter.New("@WhsPK", warehousePK, WhsPutawayLocationCacheSchema.WPC_WW_Warehouse);
			var paramNameFactory = new ParameterNameFactory();
			whsParameter.Rename(paramNameFactory.GetParameterName(whsParameter));

			var sql = $@"SELECT * FROM dbo.WhsPutawayLocationCache WHERE WPC_WW_Warehouse = {whsParameter.ParameterName}";
			return DataRowLoader.Load(factory, sql, cmd => cmd.AddParameter(whsParameter));
		}
	}
}
