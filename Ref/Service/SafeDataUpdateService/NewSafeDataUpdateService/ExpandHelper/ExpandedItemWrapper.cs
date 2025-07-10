using System;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class ExpandedItemWrapper : IExpandedItemWrapper
	{
		public ExpandedItemWrapper(ExpandedNavigationSelectItem expandedItem)
		{
			Argument.NotNull(expandedItem, nameof(expandedItem));
			this.expandedItem = expandedItem;
		}

		readonly ExpandedNavigationSelectItem expandedItem;

		public Type GetExpandType()
		{
			var elementType = (EdmEntityType)((EdmCollectionType)expandedItem.NavigationSource?.Type)?.ElementType?.Definition;
			var typeName = elementType.Namespace + "." + elementType.Name;
			var result = typeof(RefCusTariff).Assembly.GetType(typeName);
			return result;
		}

		public IExpandClauseWrapper GetExpandClause()
		{
			return expandedItem.SelectAndExpand != null ? new ExpandClauseWrapper(expandedItem.SelectAndExpand) : null;
		}
	}
}
