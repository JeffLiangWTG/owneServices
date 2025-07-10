using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Module
{
	public class CustomerServiceTicketOrganizationFilter : ModuleGuidFilter
	{
		public CustomerServiceTicketOrganizationFilter(GetList listDelegate)
			: base(new ZString(CustomerServiceTicketFilterBusinessObject.FilterDescriptions.Organization), ModuleIDs.Organisation, EmptyQuery, listDelegate)
		{
			MultilingualDescription = ResString.GetMultilingualString("ff1195cc-acf6-4482-bb34-3bf7bbfcd2b4", "Organization");
		}

		static ZQuery EmptyQuery(ZGuid pk)
		{
			throw new InvalidOperationException();
		}

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			ModuleTextFilter.ComparisonConstants.Exact,
			ModuleTextFilter.ComparisonConstants.NotEqual,
			ModuleTextFilter.ComparisonConstants.FiltersMatch,
		};

		public override bool HasComparisonOperator => true;

		protected override ZQuery GetQuery()
		{
			return IsFilterCollectionComparisonOperatorSelected() ? GetQueryForSelectedFilters() : GetSpecificItemQuery();
		}

		protected override void AddSelectedFiltersSubquery(FilterStripBusinessObject filterBusinessObject, ZDBOnlyQuery query, ZDBOnlySubQuery subQuery)
		{
			var orgContactSubQuery = GetOrgContactSubQuery(notIn: false);
			orgContactSubQuery.AddSubQuery(subQuery, JoinCondition.And);
			query.AddSubQuery(orgContactSubQuery, JoinCondition.And);
		}

		protected override ZDBOnlyQuery GetNewQueryForSelectedFilters()
		{
			return new ZDBOnlyQuery(typeof(WorkRequest));
		}

		protected override SchemaColumn SubQueryColumn => OrgContactSchema.OC_OH;

		ZQuery GetSpecificItemQuery()
		{
			if (IsEmpty)
			{
				return new ZQuery();
			}

			var isNotEqual = ComparisonOperator == ModuleTextFilter.ComparisonConstants.NotEqual;
			var query = GetNewQueryForSelectedFilters();
			var subQuery = GetOrgContactSubQuery(notIn: isNotEqual);
			subQuery.AddToFilter(OrgContactSchema.OC_OH, Property);

			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		static ZDBOnlySubQuery GetOrgContactSubQuery(bool notIn)
		{
			return new ZDBOnlySubQuery(typeof(OrgContact), WorkRequestSchema.WKR_OC_Client, notIn);
		}
	}
}
