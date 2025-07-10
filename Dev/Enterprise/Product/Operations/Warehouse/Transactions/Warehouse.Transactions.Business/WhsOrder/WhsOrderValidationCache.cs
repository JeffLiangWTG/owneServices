using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderValidationCache
	{
		public WhsOrderValidationCache(IEnumerable<WhsDocketLine> orderLines)
		{
			Argument.NotNull(orderLines, nameof(orderLines));

			LazyLinesContainHeldInventory = new Lazy<bool>(() => orderLines.Any(l => !l.WE_WHC_NKOrderedHeldCode.IsEmpty));
			LazyLinesContainAvailableInventory = new Lazy<bool>(() => orderLines.Any(l => l.WE_WHC_NKOrderedHeldCode.IsEmpty));
		}

		public bool LinesContainHeldInventory() => LazyLinesContainHeldInventory.Value;

		public bool LinesContainAvailableInventory() => LazyLinesContainAvailableInventory.Value;

		Lazy<bool> LazyLinesContainHeldInventory { get; }
		Lazy<bool> LazyLinesContainAvailableInventory { get; }
	}
}
