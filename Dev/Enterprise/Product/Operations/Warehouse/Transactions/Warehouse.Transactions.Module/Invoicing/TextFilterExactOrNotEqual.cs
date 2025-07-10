using System.Collections;
using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class TextFilterExactOrNotEqual : ModuleTextFilter
	{
		public TextFilterExactOrNotEqual(string filterName, SchemaStringColumn filterColumn, IList list)
			: base(filterName, filterColumn, list)
		{
		}

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
			{
				ComparisonConstants.Exact,
				ComparisonConstants.NotEqual,
			};
	}
}
