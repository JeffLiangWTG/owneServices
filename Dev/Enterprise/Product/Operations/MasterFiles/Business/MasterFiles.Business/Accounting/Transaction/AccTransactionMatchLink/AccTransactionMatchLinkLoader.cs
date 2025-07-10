using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class AccTransactionMatchLinkLoader
	{
		public static AccTransactionMatchLink[] LoadByAccTransactionHeader(AccTransactionHeader transaction)
		{
			var query = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, transaction.PK);
			query.FetchOnlyFromLocalCache = !transaction.IsInDatabase;
			return transaction.Factory.Load<AccTransactionMatchLink>(query);
		}
	}
}
