namespace CargoWise.Billing.Collectors.MasterData
{
	public class ComplianceAlertViewCollector : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;

		public override string FeatureCode => "VCA";

		public override string ModuleName => "Compliance Risk";

		public override string FeatureName => "Compliance Alert Viewed";

		public override string FunctionName => "View Compliance Alert";

		public override string RoleName => "Commodity Compliance Alert Viewed";

		public override string DataGranularity => RefStlItemGrain.Daily;

		public override string CompanyCode => string.Empty;

		public override string BranchCode => string.Empty;

		public override string TransactionDateUtc => Constants.StartDateTimeInclusiveParamName;

		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.EndDateTimeExclusiveParamName})*10000) + (DATEPART(month, {Constants.EndDateTimeExclusiveParamName})*100) + DATEPART(day, {Constants.EndDateTimeExclusiveParamName})as varbinary(16)) as uniqueidentifier)";

		public override string BillingReference1 => "";

		public override string TransactionCount => "TransactionCount";

		public override string AdditionalRefs => "CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), ViewedInformation))";

		public override string FromClause => "VCALogsResults";

		const int OneMessageMaxRecordNumber = 1000;

		public override string WhereClause => "";

		public override string MinCW1Version => "24.7.12.0";

		public override string DateType => RefStlDateType.DateTimeOffset;

		public override string PreparationScript => $@"
		WITH VCALogs AS
		(
			SELECT CONVERT(datetimeoffset, CONVERT(VARCHAR(10), [SCE_EventTimeOffset], 120)) AS ViewedTime,
			[TH_QuoteNumber] AS JobID,
			'TH' as JobType,
			CASE
				WHEN CHARINDEX('|ORG=', [SCE_EventReference]) > 0
					THEN LEFT([SCE_EventReference], CHARINDEX('|ORG=', [SCE_EventReference]) - 1)
				ELSE
					[SCE_EventReference]
			END as HSCode,
			CASE
				WHEN CHARINDEX('|ORG=', [SCE_EventReference]) > 0
					THEN RIGHT([SCE_EventReference], LEN([SCE_EventReference]) - CHARINDEX('|ORG=', [SCE_EventReference]) - 4)
				ELSE
					''
			END as OriginOfGoods
			FROM
			[dbo].[StmComplianceEvent]
			JOIN [dbo].[RatingHeader] ON [TH_PK] = [SCE_ParentID]
			AND [SCE_EventType] = 'BWI' AND [SCE_EventSubType] = 'LBV'
			LEFT JOIN [dbo].[JobShipment] ON [TH_PK] = [JS_TH_OneTimeQuote]
			WHERE [SCE_EventTimeOffset] >= {Constants.StartDateTimeInclusiveParamName}
			AND [SCE_EventTimeOffset] < {Constants.EndDateTimeExclusiveParamName}

			UNION ALL

			SELECT CONVERT(datetimeoffset, CONVERT(VARCHAR(10), [SCE_EventTimeOffset], 120)) AS ViewedTime,
			[JS_UniqueConsignRef] as JobID,
			'SHP' as JobType,
			CASE
				WHEN CHARINDEX('|ORG=', [SCE_EventReference]) > 0
					THEN LEFT([SCE_EventReference], CHARINDEX('|ORG=', [SCE_EventReference]) - 1)
				ELSE
					[SCE_EventReference]
			END as HSCode,
			CASE
				WHEN CHARINDEX('|ORG=', [SCE_EventReference]) > 0
					THEN RIGHT([SCE_EventReference], LEN([SCE_EventReference]) - CHARINDEX('|ORG=', [SCE_EventReference]) - 4)
				ELSE
					''
			END as OriginOfGoods
			FROM
			[dbo].[StmComplianceEvent]
			JOIN [dbo].[JobShipment] ON [JS_PK] = [SCE_ParentID]
			AND [SCE_EventType] = 'BWI' AND [SCE_EventSubType] = 'LBV'
			WHERE [SCE_EventTimeOffset] >= {Constants.StartDateTimeInclusiveParamName}
			AND [SCE_EventTimeOffset] < {Constants.EndDateTimeExclusiveParamName}
		),
		VCALogsWithRowNumber AS
		(
			SELECT ViewedTime, HSCode, JobType, OriginOfGoods,
			(ROW_NUMBER() OVER (PARTITION BY ViewedTime ORDER BY ViewedTime))/{OneMessageMaxRecordNumber} AS RowNum
			FROM VCALogs
		),
		VCALogsResults AS
		(
			SELECT ViewedTime,
			RowNum,
			COUNT(ViewedTime) AS TransactionCount,
			(
				SELECT
				(
					SELECT 
						vca.HSCode AS HSCode,
						COUNT(vca.HSCode) AS Count,
						vca.JobType AS JobType,
						vca.OriginOfGoods AS OriginOfGoods
					FROM VCALogsWithRowNumber vca	
					WHERE vca.ViewedTime = vcaNum.ViewedTime
					AND vca.RowNum = vcaNum.RowNum
					GROUP BY HSCode, JobType, OriginOfGoods
					FOR JSON PATH
				) AS ViewedInformation
				FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
			) AS ViewedInformation
			FROM VCALogsWithRowNumber vcaNum
			GROUP BY
			ViewedTime, RowNum
		)";
	}
}
