using System.Collections;
using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Invoicing.Module
{
	public class TextFilterExactOrNotEqual : ModuleTextFilter
	{
		public TextFilterExactOrNotEqual(string filterName, SchemaStringColumn filterColumn, IList list)
			: base(filterName, filterColumn, list)
		{
		}

		public override IReadOnlyList<string> AllowedComparisonOperators =>
			[
				ComparisonConstants.Exact,
				ComparisonConstants.NotEqual,
			];
	}
}
