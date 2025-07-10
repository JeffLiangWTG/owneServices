using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IPalletIDFromSSCCGenerator
	{
		IEnumerable<ZString> GenerateIDs(ZString ssccPrefix, int numberOfIDs);
	}
}
