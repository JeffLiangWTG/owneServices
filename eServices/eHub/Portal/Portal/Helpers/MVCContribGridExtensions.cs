using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcContrib.UI.Grid;

namespace CargoWise.eHub.Portal.Helpers
{
	public static class MVCContribGridExtensions
	{
		public static IGridColumn<T> EditAction<T>(this IGridColumn<T> column, Func<T, string> editAction)
		{
			column.CustomItemRenderer = (context, item) => context.Writer.Write("<td>" + editAction(item) + "</td>");
			return column;
		}
	}
}