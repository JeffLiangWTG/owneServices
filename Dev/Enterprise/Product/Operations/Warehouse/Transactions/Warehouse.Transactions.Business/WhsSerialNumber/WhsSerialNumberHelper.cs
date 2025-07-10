using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsSerialNumberHelper
	{
		public static void AddWhsSerialNumberAndPivotFetchHint(BusinessObjectFactory factory, IEnumerable<ZGuid> parentPKs)
		{
			var queryPivot = new ZQuery(WhsSerialNumberPivotSchema.WSV_ParentID, parentPKs);
			factory.AddFetchHint(WhsSerialNumberPivotSchema.Instance, queryPivot);

			var subQuery = new ZDBOnlySubQuery(typeof(WhsSerialNumberPivot), WhsSerialNumberPivotSchema.WSV_WSN_SerialNumber);
			subQuery.AddToFilter(queryPivot);

			var query = new ZDBOnlyQuery(typeof(WhsSerialNumber));
			query.AddSubQuery(WhsSerialNumberSchema.PK, subQuery, JoinCondition.And);
			factory.AddFetchHint(WhsSerialNumberSchema.Instance, query);
		}
	}
}
