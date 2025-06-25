namespace CargoWise.Billing.Collectors.MasterData
{
	public class CommodityRiskCheckDeclinedCollector : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;

		public override string FeatureCode => "CCD";

		public override string ModuleName => "Compliance Risk";

		public override string FeatureName => "Commodity Risk Check Declined";

		public override string FunctionName => "Decline Commodity Risk Check";

		public override string RoleName => "Commodity Compliance Risk Check Declined";

		public override string DataGranularity => RefStlItemGrain.Daily;

		public override string CompanyCode => string.Empty;

		public override string BranchCode => string.Empty;

		public override string TransactionDateUtc => Constants.StartDateTimeInclusiveParamName;

		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.EndDateTimeExclusiveParamName})*10000) + (DATEPART(month, {Constants.EndDateTimeExclusiveParamName})*100) + DATEPART(day, {Constants.EndDateTimeExclusiveParamName})as varbinary(16)) as uniqueidentifier)";

		public override string BillingReference1 => "";

		public override string TransactionCount => "TransactionCount";

		public override string AdditionalRefs => "CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), DeclinedInformation))";

		public override string FromClause => "CADResults";

		public override string WhereClause => "";

		public override string MinCW1Version => "24.7.12.0";

		public override string DateType => RefStlDateType.DateTimeOffset;

		public override string PreparationScript => $@"
		WITH CADLogs AS
		(
			SELECT CONVERT(datetimeoffset, CONVERT(VARCHAR(10), [SCE_EventTimeOffset], 120)) AS DeclinedTime,
			CAST(CONCAT(YEAR(TH_SystemCreateTimeUtc), '-', MONTH(TH_SystemCreateTimeUtc), '-01') AS DATETIME) AS JobCreatedTime,
			'TH' AS JobType
			FROM
			[dbo].[StmComplianceEvent]
			JOIN [dbo].[RatingHeader] ON [TH_PK] = [SCE_ParentID]
			AND [SCE_EventType] = 'CRI' AND [SCE_EventSubType] = 'CAD'
			WHERE [SCE_EventTimeOffset] >= {Constants.StartDateTimeInclusiveParamName}
			AND [SCE_EventTimeOffset] < {Constants.EndDateTimeExclusiveParamName}

			UNION ALL

			SELECT CONVERT(datetimeoffset, CONVERT(VARCHAR(10), [SCE_EventTimeOffset], 120)) AS DeclinedTime,
			CAST(CONCAT(YEAR(JS_SystemCreateTimeUtc), '-', MONTH(JS_SystemCreateTimeUtc), '-01') AS DATETIME) AS JobCreatedTime,
			'SHP' AS JobType
			FROM
			[dbo].[StmComplianceEvent]
			JOIN [dbo].[JobShipment] ON [JS_PK] = [SCE_ParentID]
			AND [SCE_EventType] = 'CRI' AND [SCE_EventSubType] = 'CAD'
			WHERE [SCE_EventTimeOffset] >= {Constants.StartDateTimeInclusiveParamName}
			AND [SCE_EventTimeOffset] < {Constants.EndDateTimeExclusiveParamName}
		),
		CADResults AS
		(
			SELECT DeclinedTime,
				COUNT(DeclinedTime) as TransactionCount,
				(
					SELECT
					(
						SELECT
							logs2.JobCreatedTime AS JobCreatedTime,
							COUNT(logs2.JobCreatedTime) AS Count,
							logs2.JobType AS JobType
						FROM CADLogs logs2 WHERE logs2.DeclinedTime = logs1.DeclinedTime
						GROUP BY JobCreatedTime, JobType
						FOR JSON PATH
					) AS DeclinedInformation
					FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
				) AS DeclinedInformation
			FROM CADLogs logs1
			GROUP BY DeclinedTime
		)";
	}
}
