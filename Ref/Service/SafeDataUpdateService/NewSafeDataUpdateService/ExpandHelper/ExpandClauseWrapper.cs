using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.OData.UriParser;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class ExpandClauseWrapper : IExpandClauseWrapper
	{
		public ExpandClauseWrapper(SelectExpandClause expandClause)
		{
			Argument.NotNull(expandClause, nameof(expandClause));

			this.expandClause = expandClause;
		}

		readonly SelectExpandClause expandClause;

		public IEnumerable<IExpandedItemWrapper> GetExpandClauseWrapper()
		{
			var result = expandClause.SelectedItems?.OfType<ExpandedNavigationSelectItem>().Where(x => x != null).Select(x => new ExpandedItemWrapper(x));
			return result;
		}
	}
}
