using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsPickLinesExtensions
	{
		public static IEnumerable<IPickLinePair> GetOriginallyPickedPickLines(this IEnumerable<WhsPickLine> pickLines) => Argument.NotNull(pickLines, nameof(pickLines)).Select(pl => PickLinePair.New(pl));
	}
}
