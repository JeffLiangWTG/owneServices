using System.Collections.Generic;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IPalletIDFromDocketIDGenerator
	{
		IEnumerable<GeneratedID> GenerateIDs(WhsDocket docket, int numberOfIds, int startId = 1);
	}
}
