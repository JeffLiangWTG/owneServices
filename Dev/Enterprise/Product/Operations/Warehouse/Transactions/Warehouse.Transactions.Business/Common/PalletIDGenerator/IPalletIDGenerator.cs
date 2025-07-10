using System.Collections.Generic;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPalletIDGenerator
	{
		IEnumerable<GeneratedID> GenerateIDs(WhsDocket docket, int numberOfIDs, bool shouldPrompt, int startID = 1);
	}
}
