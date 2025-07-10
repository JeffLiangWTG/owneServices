using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class RelatedTransportBookingsOfJobDeclarationFilter : ModuleGuidPivotFilter
	{
		public RelatedTransportBookingsOfJobDeclarationFilter(ZString description, GetList listDelegate)
				: base(description, ModuleIDs.DtbBooking, DtbBookingConsolidationSchema.PK, DtbBookingConsolidationSchema.KB_ParentID, listDelegate, typeof(BaseJobDeclaration), typeof(IDtbBookingConsolidation))
		{
			SetDefaultProperties();
		}

		void SetDefaultProperties()
		{
			MultilingualDescription = DefaultDescription;
		}

		static MultilingualString DefaultDescription => ResString.GetMultilingualString("4CA084EF-0816-43AC-9103-68C3E9563933", "Related Transport Bookings");
		protected override FilterCategory DefaultCategory => FilterCategories.Other;

		protected override SchemaColumn SubQueryColumn => DtbBookingSchema.KM_KB_Booking;

		protected override string ParentPkColumnNameForAllMatch => SetParentPkColumnNameForAllMatch;

		protected string SetParentPkColumnNameForAllMatch { get; set; }

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			var query = GetNewQueryForSelectedFilters();
			if (ComparisonOperator == ComparisonConstants.AllMatch)
			{
				if (filterBusinessObject.ActiveModuleFilters.Any())
				{
					var parametersFromSubQueries = new ZSqlParameterCollection();

					SetParentPkColumnNameForAllMatch = JobDeclarationSchema.PK.Name;
					var allMatchSQLForJE = CreateAllMatchSql(query, filterBusinessObject, parametersFromSubQueries);
					SetParentPkColumnNameForAllMatch = JobDeclarationSchema.JE_JS.Name;
					var allMatchSQLForJS = CreateAllMatchSql(query, filterBusinessObject, parametersFromSubQueries);
					var sql = string.Format(CultureInfo.InvariantCulture, @"
{0} = ALL
(
	{1}
)
AND
{2} = ALL
(
	{3}
)
", JobDeclarationSchema.PK.Name, allMatchSQLForJE, JobDeclarationSchema.JE_JS.Name, allMatchSQLForJS);

					query.AddFilterAndZSQLParameterCollection(sql, parametersFromSubQueries);
				}
			}
			else
			{
				var pivotSubQuery = GetPivotSubQuery(filterBusinessObject, subModuleFilter);

				query.AddSubQuery(pivotSubQuery, JoinCondition.And);
				if (ComparisonOperator == ComparisonConstants.AnyMatch)
				{
					query.AddSubQuery(JobDeclarationSchema.JE_JS, pivotSubQuery, JoinCondition.Or);
				}
				else if (ComparisonOperator == ComparisonConstants.NoneMatch)
				{
					var query2 = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
					query2.AddSubQuery(JobDeclarationSchema.JE_JS, pivotSubQuery, JoinCondition.And);
					query2.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_JS, null);
					query.AddToFilter(query2, JoinCondition.And);
				}
			}

			return query;
		}
	}
}
