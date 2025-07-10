using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class DocketLineEnumerableExtensions
	{
		public static IEnumerable<ILineWithProductAndQuantity> ToProductAndQuantities(this IEnumerable<WhsDocketLine> lines)
		{
			Argument.NotNull(lines, nameof(lines));
			return lines.Select(l => new LineWithProductAndQuantity(l.WE_OP, l.WE_TransactionQuantity));
		}
	}
}
