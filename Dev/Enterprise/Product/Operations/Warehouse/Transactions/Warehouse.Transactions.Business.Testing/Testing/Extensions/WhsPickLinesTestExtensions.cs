using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public static class WhsPickLinesTestExtensions
	{
		public static IPickLinePair[] ToPickLinePairs(this IEnumerable<WhsPickLine> pickLines) => pickLines.GetOriginallyPickedPickLines().ToArray();
	}
}
