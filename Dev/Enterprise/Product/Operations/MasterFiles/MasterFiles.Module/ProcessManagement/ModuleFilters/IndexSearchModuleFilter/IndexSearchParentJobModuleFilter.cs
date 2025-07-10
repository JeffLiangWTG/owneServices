using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Module
{
	public class IndexSearchParentJobModuleFilter : ParentJobModuleFilter, IIndexSearchModuleFilter
	{
		public IndexSearchParentJobModuleFilter(SearchField searchField, SchemaGuidColumn filterColumn, BusinessObjectFactory factory) : base(searchField.FieldName, filterColumn, factory)
		{
			SearchField = searchField ?? throw new ArgumentNullException(nameof(searchField));
		}

		SearchField SearchField { get; }

		public IGlowQuery GetGlowIndexQuery()
		{
			if(!Property.IsValid)
			{
				return new EmptyQuery();
			}

			var term = new Term(SearchField.FieldName, Property.ToString());

			return (string)ComparisonOperator switch
			{
				ComparisonConstants.Exact => new EqualQuery(term, useQuotes: false),
				ComparisonConstants.NotEqual => new NotEqualQuery(term, useQuotes: false),
				_ => new EmptyQuery(),
			};
		}
	}
}
