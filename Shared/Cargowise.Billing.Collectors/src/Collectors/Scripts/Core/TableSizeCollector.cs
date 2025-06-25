namespace CargoWise.Billing.Collectors.Core
{
	#region SuppressResourceStringsCheckRegion
	public class TableSizeCollector : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "TBS";
		public override string RoleName => "Hosting";
		public override string ModuleName => "WiseCloud";
		public override string FunctionName => "Database Table Size";
		public override string FeatureName => "Table size, index size and row count";
		public override string DataGranularity => RefStlItemGrain.MonthlyCurrentDataOnly;
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string TransactionCount => "1";
		public override string BillingReference1 => "[Database]";
		public override string BillingReference2 => "[Schema]";
		public override string BillingReference3 => "[Table]";
		public override string PreparationScript => Constants.MonthAsDateVariableScript + @"
				;WITH agg AS
				(   -- Get info for Tables, Indexed Views, etc
				    SELECT  ps.[object_id] AS [ObjectID],
				            ps.index_id AS [IndexID],
				            NULL AS [ParentIndexID],
				            NULL AS [PassThroughIndexName],
				            NULL AS [PassThroughIndexType],
				            SUM(ps.in_row_data_page_count) AS [InRowDataPageCount],
				            SUM(ps.used_page_count) AS [UsedPageCount],
				            SUM(ps.reserved_page_count) AS [ReservedPageCount],
				            SUM(ps.row_count) AS [RowCount],
				            SUM(ps.lob_used_page_count + ps.row_overflow_used_page_count)
				                    AS [LobAndRowOverflowUsedPageCount]
				    FROM    sys.dm_db_partition_stats ps
				    GROUP BY    ps.[object_id],
				                ps.[index_id]
				    UNION ALL
				    -- Get info for FullText indexes, XML indexes, Spatial indexes, etc
				    SELECT  sit.[parent_id] AS [ObjectID],
				            sit.[object_id] AS [IndexID],
				            sit.[parent_minor_id] AS [ParentIndexID],
				            sit.[name] AS [PassThroughIndexName],
				            sit.[internal_type_desc] AS [PassThroughIndexType],
				            0 AS [InRowDataPageCount],
				            SUM(ps.used_page_count) AS [UsedPageCount],
				            SUM(ps.reserved_page_count) AS [ReservedPageCount],
				            0 AS [RowCount],
				            0 AS [LobAndRowOverflowUsedPageCount]
				    FROM    sys.dm_db_partition_stats ps
				    INNER JOIN  sys.internal_tables sit
				            ON  sit.[object_id] = ps.[object_id]
				    WHERE   sit.internal_type IN
				               (202, 204, 207, 211, 212, 213, 214, 215, 216, 221, 222, 236)
				    GROUP BY    sit.[parent_id],
				                sit.[object_id],
				                sit.[parent_minor_id],
				                sit.[name],
				                sit.[internal_type_desc]
				), spaceused AS
				(
					SELECT  agg.[ObjectID],
					        agg.[IndexID],
					        agg.[ParentIndexID],
					        agg.[PassThroughIndexName],
					        agg.[PassThroughIndexType],
					        OBJECT_SCHEMA_NAME(agg.[ObjectID]) AS [Schema],
					        OBJECT_NAME(agg.[ObjectID]) AS [Table],
					        SUM(CASE
					                WHEN (agg.IndexID < 2) THEN agg.[RowCount]
					                ELSE 0
					            END) AS [Rows],
					        SUM(agg.ReservedPageCount) AS [ReservedPageCount],
					        SUM(CASE
					                WHEN (agg.IndexID < 2) THEN (agg.LobAndRowOverflowUsedPageCount + agg.InRowDataPageCount)
					                ELSE 0
					            END)  AS [DataPageCount],
					        SUM(CASE
					                WHEN (agg.IndexID < 2) THEN  agg.UsedPageCount - agg.LobAndRowOverflowUsedPageCount - agg.InRowDataPageCount
					                ELSE agg.UsedPageCount
					            END) AS [IndexPageCount],
					        SUM(agg.ReservedPageCount - agg.UsedPageCount) AS [UnusedPageCount],
					        SUM(agg.UsedPageCount) AS [UsedPageCount]
					FROM    agg
					GROUP BY    agg.[ObjectID],
					            agg.[IndexID],
					            agg.[ParentIndexID],
					            agg.[PassThroughIndexName],
					            agg.[PassThroughIndexType],
					            OBJECT_SCHEMA_NAME(agg.[ObjectID]),
					            OBJECT_NAME(agg.[ObjectID])
				), spacedusedsum AS 
				(
					SELECT DB_NAME() AS [Database],
						[Schema], 
						[Table], 
						SUM([Rows]) AS [Rows],
						SUM([ReservedPageCount]) AS [ReservedPageCount],
						SUM([DataPageCount]) AS [DataPageCount],
						SUM([IndexPageCount]) AS [IndexPageCount]
					FROM (
						SELECT sp.[Schema],
							   sp.[Table],
							   sp.[Rows],
							   sp.[ReservedPageCount],
							   sp.[DataPageCount],
							   sp.[IndexPageCount],
							   sp.[UnusedPageCount],
							   sp.[UsedPageCount]
						FROM   spaceused sp
						INNER JOIN sys.all_objects so
							   ON so.[object_id] = sp.ObjectID
						LEFT JOIN  sys.indexes si
							   ON si.[object_id] = sp.ObjectID
							   AND (si.[index_id] = sp.IndexID
							   OR si.[index_id] = sp.[ParentIndexID])
						WHERE so.is_ms_shipped = 0 and so.[type_desc] = 'USER_TABLE' AND sp.[Schema] <> 'cdc') t
					GROUP BY [Schema], [Table]
				)";

		public override string FromClause => "spacedusedsum";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (
					SELECT [Database], [Schema], [Table], [Rows] AS [RowCount], 
						[ReservedPageCount] AS [TotalPages],
						CAST(([ReservedPageCount] * 8.)/1024 AS decimal(28,3)) AS [TotalMB],
						CAST(([ReservedPageCount] * 8.)/1024/1024 AS decimal(28,3)) AS [TotalGB],
						[DataPageCount] AS [TotalDataPages],
						CAST(([DataPageCount] * 8.)/1024 AS decimal(28,3)) AS [TotalDataMB],
						CAST(([DataPageCount] * 8.)/1024/1024 AS decimal(28,3)) AS [TotalDataGB],
						[IndexPageCount] AS [TotalIndexPages],
						CAST(([IndexPageCount] * 8.)/1024 AS decimal(28,3)) AS [TotalIndexMB],
						CAST(([IndexPageCount] * 8.)/1024/1024 AS decimal(28,3)) AS [TotalIndexGB]
					FOR JSON PATH, WITHOUT_ARRAY_WRAPPER))
				)";
		public override string WhereClause => string.Empty;
		public override string ActiveOn => "ALL";
	}
	#endregion
}
