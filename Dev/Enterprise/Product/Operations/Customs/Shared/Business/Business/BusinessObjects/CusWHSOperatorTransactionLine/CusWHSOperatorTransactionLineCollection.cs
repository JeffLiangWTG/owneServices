using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusWHSOperatorTransactionLineCollection : BusinessObjectCollection<CusWHSOperatorTransactionLine>
	{
		public CusWHSOperatorTransactionLineCollection(BusinessObjectFactory factory, CusWHSOperatorTransaction transaction)
			: base(factory, GetQueryForCusWHSOperatorTransactionLine(factory, transaction))
		{
			this.transaction = transaction;
		}

		static ZQuery GetQueryForCusWHSOperatorTransactionLine(BusinessObjectFactory factory, CusWHSOperatorTransaction transaction)
		{
			Argument.NotNull(transaction, nameof(transaction));
			var query = new ZQuery();
			if (transaction.IsOrder)
			{
				query.AddToFilter(CusWHSOperatorTransactionLineSchema.WOL_WOT_WHSOperatorTransactionOrder, transaction.PK);
			}
			else
			{
				query.AddToFilter(CusWHSOperatorTransactionLineSchema.WOL_WOT_WHSOperatorTransactionReceipt, transaction.PK);
			}
			return query;
		}

		public CusWHSOperatorTransaction Transaction => transaction;

		readonly CusWHSOperatorTransaction transaction;
	}
}
