using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsDocketRelatedEntityOperationsRFStrategy : WhsDocketRelatedEntityOperationsStrategy
	{
		internal WhsDocketRelatedEntityOperationsRFStrategy(WhsDocket parent)
			: base(parent)
		{
		}

		#region EventLogExists

		protected override bool EventLogExistsCore(ZString code)
		{
			var query = GetLogFilter(code);
			query.FetchOnlyFromLocalCache = true;

			return (Parent.Factory.LoadTop1<StmALog>(query) != null || Parent.Factory.ExistsInDatabase(StmALogSchema.Constants.TableName, query));
		}

		#endregion

		#region GetCurrentMaxLineNo

		protected override ZShort GetCurrentMaxLineNoCore(ZShort maxLineNo)
		{
			var query = new ZQuery(WhsDocketLineSchema.WE_WD, Parent.PK);
			query.OrderBy = WhsDocketLineSchema.WE_LineNo.Name + OrderByClause.Descending;

			var result = Parent.Factory.LoadTop1<WhsDocketLine>(query);
			if (result != null)
			{
				maxLineNo = result.WE_LineNo;
			}

			return maxLineNo;
		}

		#endregion

	}
}
