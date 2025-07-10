using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportConsignment.Module
{
	public abstract class DtbChildFilterBusinessObject : FilterStripBusinessObject
	{
		protected DtbChildFilterBusinessObject()
			: base()
		{
		}

		public abstract SchemaColumn FieldOnRunsheet { get; }
		public abstract SchemaColumn ChildBizOPKOrNK { get; }
		public abstract Type TypeOfBusinessObjectToQuery { get; }
		public abstract IEnumerable<ModuleFilter> ActiveFilters { get; }

		public DtbRoutePlannerFilterBusinessObject routePlannerFilterBusinessObject;

		public override ZQuery Filter
		{
			get
			{
				var query = new ZDBOnlyQuery(TypeOfBusinessObjectToQuery);
				var orCategoriesWithFilters = GetActiveChildFiltersGroupedByOrCategory();
				var whereClauseForUserDefinedFilters = GetWhereClauseForUserDefinedChildFilters(orCategoriesWithFilters);
				return query.AddFilterAndZSQLParameterCollection(whereClauseForUserDefinedFilters, new ZSqlParameterCollection());
			}
		}

		Dictionary<FilterOrCategory, List<ModuleFilter>> GetActiveChildFiltersGroupedByOrCategory()
		{
			var result = new Dictionary<FilterOrCategory, List<ModuleFilter>>();

			foreach (var moduleFilter in ActiveFilters)
			{
				List<ModuleFilter> filters;
				if (result.TryGetValue(moduleFilter.OrCategory, out filters))
				{
					filters.Add(moduleFilter);
				}
				else
				{
					result.Add(moduleFilter.OrCategory, new List<ModuleFilter> { moduleFilter });
				}
			}

			return result;
		}

		static string GetWhereClauseForUserDefinedChildFilters(Dictionary<FilterOrCategory, List<ModuleFilter>> orCategoriesWithFilters)
		{
			var whereClauseBuilder = new ZStringBuilder();
			var beginning = true;

			foreach (var category in orCategoriesWithFilters)
			{
				bool firstFilter = true;

				foreach (var moduleFilter in category.Value.Where(mf => !mf.Query.IsEmpty))
				{
					string prefix;

					if (beginning && firstFilter)
					{
						prefix = (category.Key != FilterOrCategory.None) ? " (" : string.Empty; // Part of SQL expression.
						beginning = false;
						firstFilter = false;
					}
					else if (firstFilter)
					{
						prefix = (category.Key != FilterOrCategory.None) ? (NoResString)"AND ( " : "AND "; // Part of SQL expression.
						firstFilter = false;
					}
					else
					{
						prefix = category.Key == FilterOrCategory.None ? " AND " : " OR ";
					}

					whereClauseBuilder.Append(string.Format(Culture.Invariant, "{0} ( {1} )", prefix, moduleFilter.Query.LiteralTextSqlFormatted.TrimEnd()));
				}

				if (category.Key != FilterOrCategory.None)
				{
					whereClauseBuilder.Append(")");
				}
			}

			return whereClauseBuilder.ToString();
		}
	}
}
