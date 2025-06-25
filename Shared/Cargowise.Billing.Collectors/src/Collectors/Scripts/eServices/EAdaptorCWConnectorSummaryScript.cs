namespace CargoWise.Billing.Collectors.eServices
{
	public class EAdaptorCWConnectorSummaryScript : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "ECS";
		public override string RoleName => "eServices";
		public override string ModuleName => "eAdaptor CWConnector";
		public override string FunctionName => "eAdaptor CWConnector summary";
		public override string FeatureName => "eAdaptor CWConnector summary";
		public override string ActiveOn => "ALL";
		public override string DataGranularity => "DAY";
		public override string FromClause => $@"
(
	SELECT	gc.GC_Code CompanyCode,
			MIN(gb.GB_Code) BranchCode,
			EM_ReceiveTransmit,
			EM_ApplicationCode,
			EM_MessageType,
			EM_MessageSubType,
			SUM(CASE WHEN EM_MessageData is NULL THEN DATALENGTH(EM_MessageText) ELSE DATALENGTH(EM_MessageData) END)/1024.0 TotalKB,
			MIN(CASE WHEN EM_MessageData is NULL THEN DATALENGTH(EM_MessageText) ELSE DATALENGTH(EM_MessageData) END)/1024.0 MinKB,
			MAX(CASE WHEN EM_MessageData is NULL THEN DATALENGTH(EM_MessageText) ELSE DATALENGTH(EM_MessageData) END)/1024.0 MaxKB,
			Count(*) MessageCount
	FROM dbo.EDIMessage

	LEFT JOIN (
		dbo.GlbBranch gb 
		INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
	) ON gb.GB_PK = EM_GB

	INNER JOIN (
		dbo.EDICommunicationPartyConfig  ecc
		INNER JOIN dbo.EDICommunicationParty ecp on ecc.ECC_ECP_Party = ecp.ECP_PK
	) ON ecc.ECC_PK = EM_ECC_CommunicationPartyConfig

	WHERE ecp.ECP_Name LIKE '%CWConnector%' AND EM_SystemCreateTimeUtc >= {Constants.StartDateTimeInclusiveParamName} AND EM_SystemCreateTimeUtc < {Constants.EndDateTimeExclusiveParamName}

	GROUP BY gc.GC_Code, EM_ReceiveTransmit, EM_ApplicationCode, EM_MessageType, EM_MessageSubType
) AggregatedMessage";
		public override string TransactionDateUtc => $"DATEFROMPARTS(YEAR({Constants.EndDateTimeExclusiveParamName}), MONTH({Constants.EndDateTimeExclusiveParamName}), DAY({Constants.EndDateTimeExclusiveParamName}))";
		public override string BillingReference1 => "AggregatedMessage.EM_ReceiveTransmit";
		public override string BillingReference2 => "AggregatedMessage.EM_ApplicationCode";
		public override string BillingReference3 => "AggregatedMessage.EM_MessageType + '.' + AggregatedMessage.EM_MessageSubType";
		public override string BillingReference4 => "FORMAT(AggregatedMessage.TotalKB, 'N2')";
		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.EndDateTimeExclusiveParamName})*10000) + (DATEPART(month, {Constants.EndDateTimeExclusiveParamName})*100) + DATEPART(day, {Constants.EndDateTimeExclusiveParamName})as varbinary(16)) as uniqueidentifier)";
		public override string CompanyCode => "AggregatedMessage.CompanyCode";
		public override string BranchCode => "AggregatedMessage.BranchCode";
		public override string TransactionCount => "AggregatedMessage.MessageCount";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(
	SELECT
		Direction = AggregatedMessage.EM_ReceiveTransmit,
		AppCode = AggregatedMessage.EM_ApplicationCode,
		MsgType = AggregatedMessage.EM_MessageType,
		MsgSubType = AggregatedMessage.EM_MessageSubType,
		MsgQty = AggregatedMessage.MessageCount,
		TotalKB = AggregatedMessage.TotalKB,
		MinKB = AggregatedMessage.MinKB,
		MaxKB = AggregatedMessage.MaxKB
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)))";
		public override string WhereClause => string.Empty;
	}
}
