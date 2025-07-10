using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	public abstract class IndexFilterHandlerBase : IIndexFilterHandler
	{
		protected IndexFilterHandlerBase(FilterStripBusinessObject parent)
		{
			Parent = parent;
		}

		public abstract IReadOnlyCollection<Type> ApplicableTypes { get; }

		public abstract string[] RequiredIndexSearchFields { get; }

		public bool IsApplicable()
		{
			return IsApplicableCore() && ApplicableTypes.Any(t => t.IsAssignableFrom(Parent.GetType()));
		}

		protected virtual bool IsApplicableCore() => true;

		public ModuleFilterCollection Add(ModuleFilterCollection filters)
		{
			if (!Parent.HasIndexSearchFields)
			{
				return filters;
			}

			if (!IsApplicable())
			{
				return filters;
			}

			foreach (var moduleFilter in GetFilters())
			{
				if (moduleFilter != default)
				{
					filters.AddFilter(moduleFilter);
				}
			}

			return filters;
		}

		public IEnumerable<ModuleFilter> GetFilters()
		{
			foreach (var requiredIndexSearchField in RequiredIndexSearchFields)
			{
				var searchField = Parent.IndexSearchFields[requiredIndexSearchField];
				if (searchField == null)
				{
					Parent.ReturnNoResultsQuery = true;
					return Array.Empty<ModuleFilter>();
				}
			}

			return GetFiltersCore();
		}

		public readonly FilterStripBusinessObject Parent;

		protected abstract IEnumerable<ModuleFilter> GetFiltersCore();
	}
}
