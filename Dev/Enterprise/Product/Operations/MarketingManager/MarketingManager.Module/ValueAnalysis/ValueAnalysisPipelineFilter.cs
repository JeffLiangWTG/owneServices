using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	class ValueAnalysisPipelineFilter : ModuleNumberRangeFilter
	{
		public enum Context { Any, PipelineValue, UnsuccessfulValue }

		public ValueAnalysisPipelineFilter(ZString description, SchemaNumericColumn column, Context context = Context.Any)
			: base(description, delegate
			{ return new ZQuery(); })
		{
			Column = column;
			FilterContext = context;
		}

		readonly SchemaNumericColumn Column;
		readonly Context FilterContext;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it is sql statement")]
		protected override ZQuery GetQuery()
		{
			var result = new ZDBOnlyQuery(typeof(ViewValueAnalysis));
			var parameters = new ZSqlParameterCollection();

			var jobCountMultiplier = Column.Name == OrgTradePeriodSchema.Constants.PAS_RepeatsMnth ? string.Empty : " (CASE WHEN PAS_RepeatsMnth = 0 THEN 1 ELSE PAS_RepeatsMnth END) *";

			var having = string.Format(CultureInfo.InvariantCulture, @"SUM({0} *{1}
			(CASE
				WHEN PAP_RecurrenceType = 'ONE' THEN 1
				WHEN PAP_RecurrenceType = 'YR' THEN 1
				WHEN PAP_RecurrenceType = 'MTH' THEN 12
				WHEN PAP_RecurrenceType = 'WK' THEN 52
			END))", Column.Name, jobCountMultiplier);

			if (IsGreaterThanOrEqualToSearch)
			{
				parameters.Add("@Property1", Property1, Column);
				having += ">=@Property1";
			}
			else if (IsLessThanOrEqualToSearch)
			{
				parameters.Add("@Property2", Property2, Column);
				having += "<=@Property2";
			}
			else if (IsEqualToSearch)
			{
				parameters.Add("@Property1", Property1, Column);
				having += "=@Property1";
			}
			else // IsBetweenSearch
			{
				parameters.Add("@Property1", Property1, Column);
				parameters.Add("@Property2", Property2, Column);
				having += string.Format(CultureInfo.InvariantCulture, ">=@Property1 AND {0}<=@Property2", having);
			}

			var tables = Column.TableSchema.SqlSchemaName + "." + Column.TableName;

			tables += " JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgTradeProspect ON PAP_PA=PA_PK JOIN dbo.OrgSales ON PA_OW=OW_PK";
			tables += " AND OW_IsTraded = 0 AND PAS_Period IS NULL";

			if (FilterContext == Context.PipelineValue)
			{
				tables += " AND PA_Status = 'ACT'";
			}
			else if (FilterContext == Context.UnsuccessfulValue)
			{
				tables += " AND PA_Status = 'UNS'";
			}

			string sql = string.Format(CultureInfo.InvariantCulture, "EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM {0} GROUP BY PA_PK, PAS_OH_Client HAVING {1}) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)", tables, having);
			result.AddFilterAndZSQLParameterCollection(sql, parameters);
			return result;
		}
	}
}
