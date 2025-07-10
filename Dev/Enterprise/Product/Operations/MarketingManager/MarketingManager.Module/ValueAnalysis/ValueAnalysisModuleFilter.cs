using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	public class ValueAnalysisModuleFilter : ModuleGuidForeignCollectionFilter
	{
		protected ValueAnalysisModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ValueAnalysisModuleFilter(ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(moduleID.Description.GetUnresolvedString(), moduleID, primaryKeyColumn, ViewValueAnalysisSchema.PK, list, parentBusinessObjectType)
		{
		}

		public ValueAnalysisModuleFilter(ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, BusinessObjectFactory factory, Type parentBusinessObjectType)
			: base(moduleID.Description.GetUnresolvedString(), moduleID, primaryKeyColumn, ViewValueAnalysisSchema.PK, () => new ViewValueAnalysisCollection(factory), parentBusinessObjectType)
		{
		}

		public override void UpdateSelectedFilters(FilterStripBusinessObject newSelectedFilters)
		{
			if (newSelectedFilters is ValueAnalysisFilterBusinessObject newBO && SelectedFilters is ValueAnalysisFilterBusinessObject thisBO)
			{
				thisBO.ModuleContext = newBO.ModuleContext;
			}
			base.UpdateSelectedFilters(newSelectedFilters);
		}

		protected override ZQuery GetQueryForSelectedFilters()
		{
			return SelectedFilters != null ? GetQueryForSelectedFiltersCore(SelectedFilters, SelectedFilters.Filter) : base.GetQueryForSelectedFilters();
		}

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			return subModuleFilter.IsEmpty ? new ZQuery() : GetParameterizedQuerySubModuleFilter(subModuleFilter);
		}

		ZQuery GetParameterizedQuerySubModuleFilter(ZQuery subModuleFilter)
		{
			var whereClause = subModuleFilter.ParameterisedText.ParameterisedQueryText;
			var parametersFromSubQueries = new ZSqlParameterCollection();
			var parameterIndex = 0;

			var parametersList = subModuleFilter.ParameterisedText.Parameters.OrderByDescending(p => p.ParameterName).ToList();
			foreach (var parameter in parametersList)
			{
				var parameterName = $"@P{parameterIndex}_{parameter.SchemaColumn.Name}"; // Sql parameter
				parametersFromSubQueries.Add(parameterName, parameter.ValueForSql, parameter.SchemaColumn);

				whereClause = whereClause.Replace(parameter.ParameterName, parameterName);
				parameterIndex++;
			}

			whereClause = ZQueryFormatter.GetFormattedText(whereClause);

			var subQueryText = FilterColumn.Name == OrgOpportunitySchema.PK.Name
				? GetOrgOpportunitySubQueryText(whereClause)
				: GetOrgHeaderSubQueryText(whereClause);

			var query = GetNewQueryForSelectedFilters();
			query.AddFilterAndZSQLParameterCollection(subQueryText, parametersFromSubQueries);

			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Sql statement")]
		string GetOrgOpportunitySubQueryText(string whereClause)
		{
			return ComparisonOperator == ModuleTextFilter.ComparisonConstants.AllMatch
				? string.Format(CultureInfo.InvariantCulture, @"
NOT EXISTS
(
	SELECT VVA_PK FROM dbo.ViewValueAnalysis JOIN dbo.OrgSalesValueAssociationPivot ON VVA_PK = SVP_TradeId WHERE SVP_ActivityId = {0}
	EXCEPT SELECT VVA_PK FROM dbo.ViewValueAnalysis JOIN dbo.OrgSalesValueAssociationPivot ON VVA_PK = SVP_TradeId WHERE SVP_ActivityId = {0} AND ({1})
)
AND {0} IN
(
	SELECT SVP_ActivityId FROM dbo.ViewValueAnalysis JOIN dbo.OrgSalesValueAssociationPivot ON VVA_PK = SVP_TradeId WHERE {1}
)", FilterColumn.Name, whereClause)

				: string.Format(CultureInfo.InvariantCulture, @"
{0} {1}IN
(
	SELECT SVP_ActivityId FROM dbo.ViewValueAnalysis JOIN dbo.OrgSalesValueAssociationPivot ON VVA_PK = SVP_TradeId WHERE {2}
)", FilterColumn.Name, UsesNotInQuery ? "NOT " : string.Empty, whereClause);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Sql statement")]
		string GetOrgHeaderSubQueryText(string whereClause)
		{
			return ComparisonOperator == ModuleTextFilter.ComparisonConstants.AllMatch
				? string.Format(CultureInfo.InvariantCulture, @"
NOT EXISTS
(
	SELECT VVA_PK FROM dbo.ViewValueAnalysis WHERE VVA_OH_Primary = {0}
	EXCEPT SELECT VVA_PK FROM dbo.ViewValueAnalysis WHERE VVA_OH_Primary = {0} AND ({1})
)
AND {0} IN
(
	SELECT VVA_OH_Primary FROM dbo.ViewValueAnalysis WHERE {1}
)", FilterColumn.Name, whereClause)

				: string.Format(CultureInfo.InvariantCulture, @"
{0} {1}IN
(
	SELECT VVA_OH_Primary FROM dbo.ViewValueAnalysis WHERE {2}
)", FilterColumn.Name, UsesNotInQuery ? "NOT " : string.Empty, whereClause);
		}
	}
}
