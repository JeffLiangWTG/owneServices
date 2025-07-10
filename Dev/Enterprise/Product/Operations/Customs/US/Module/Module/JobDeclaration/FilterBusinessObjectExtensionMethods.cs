using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	static class FilterBusinessObjectExtensionMethods
	{
		public static ZQuery GetLiquidationDateQuery(this FilterStripBusinessObject filter, DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var liquidationDateQuery = new ZQuery();
			filter.AddDateRange(liquidationDateQuery, comparisonOperator, JoinCondition.And, CusLiquidationSchema.B8_LiquidationDate, date1.Date, date2.Date);
			var liquidationDateClause = liquidationDateQuery.LiteralTextSqlFormatted;
			if (!string.IsNullOrEmpty(liquidationDateClause))
			{
				liquidationDateClause = "WHERE " + liquidationDateClause;
			}

			var liquidationDateQueryText = string.Format($@"
JE_PK IN (
	SELECT JE_PK
	FROM dbo.JobDeclaration 
	LEFT JOIN
	(
		SELECT B8_PK, B8_ClusterKey, B8_JE, B8_LiquidationDate, ROW_NUMBER() OVER (PARTITION BY B8_JE ORDER BY B8_SystemCreateDate DESC) rowNumber 
		FROM dbo.CusLiquidation
		WHERE B8_GC = @LiquidationCompanyPK
	) latestLiquid ON JE_PK = latestLiquid.B8_JE AND JE_ClusterKey = latestLiquid.B8_ClusterKey AND latestLiquid.rowNumber = 1
	{liquidationDateClause}
)");

			result.AddFilterAndZSQLParameterCollection(liquidationDateQueryText, new ZSqlParameterCollection(ZSqlParameter.New("@LiquidationCompanyPK", GlbCompany.CurrentCompany.PK, CusLiquidationSchema.B8_GC)));

			return result;
		}

		public static ZQuery GetStatementNoQuery(this FilterStripBusinessObject filter, SQLComparisonOperator filterOperator, ZString refNo)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var prefix = filterOperator == SpecialComparisonOperator.IsBlank ? "NOT" : "";

			var queryText = @"JE_PK " + // direct query required because no FK in CusStatementHeader and CusStatementLine
				prefix + @" IN
				( 
					SELECT JE_PK FROM dbo.JobDeclaration 
					INNER JOIN dbo.GenAddOnColumn  ON XA_ParentID = JE_PK AND XA_Name = @FieldName 
					INNER JOIN dbo.CusEntryNum  ON CE_ParentID = JE_PK AND CE_EntryType = @EntryType
					INNER JOIN dbo.CusStatementLine  ON CE_EntryNum = B3_EntryNum AND B3_Status != @Status AND XA_Data = B3_EntryFilerCode";

			if (!refNo.IsEmpty)
			{
				var param = ZSqlParameter.New("@RefNo", refNo, CusStatementHeaderSchema.B2_StatementNumber, filterOperator);
				queryText += " INNER JOIN dbo.CusStatementHeader  ON B3_B2 = B2_PK WHERE " + param.LiteralTextADO;
			}

			queryText += ")";

			var queryParams = new ZSqlParameterCollection(
				ZSqlParameter.New("@FieldName", USAddInfoSchema.US_EntryFilerCode.Name, GenAddOnColumnSchema.XA_Name),
				ZSqlParameter.New("@EntryType", CusEntryHeaderMessageTypeList.Codes.EntrySummary, CusEntryNumSchema.CE_EntryType),
				ZSqlParameter.New("@Status", StatementLineStatusList.Codes.Deleted, CusStatementLineSchema.B3_Status));

			result.AddFilterAndZSQLParameterCollection(queryText, queryParams);
			return result;
		}
	}
}
