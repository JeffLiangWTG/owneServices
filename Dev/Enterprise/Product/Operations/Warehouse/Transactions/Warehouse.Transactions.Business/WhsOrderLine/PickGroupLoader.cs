using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class PickGroupLoader
	{
		public static ZShort GetDefaultPickGroup(RowFactory rowFactory, ZGuid productPK, ZGuid warehousePK, ZGuid clientPK)
		{
			ZShort result = 0;
			if (!productPK.IsEmpty && !warehousePK.IsEmpty && !clientPK.IsEmpty)
			{
				var query = new ZQuery(WhsProductParamsByWhsAndClientSchema.W3_OP, productPK);
				query.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_OH, clientPK);
				query.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_WW, warehousePK);
				query.MaximumRows = 1;
				var productParamsByWhsAndClient = rowFactory.Load(WhsProductParamsByWhsAndClientSchema.Constants.TableName, query);
				if (productParamsByWhsAndClient.Length == 1)
				{
					// W3_PickGroup is not nullable so we can cast safely
					result = (short)productParamsByWhsAndClient[0][WhsProductParamsByWhsAndClientSchema.Constants.W3_PickGroup];
				}
			}
			return result;
		}
	}
}
