namespace CargoWise.Billing.Collectors.eServices
{
	public class EDIClientsScript : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "EDC";
		public override string RoleName => "CW Messaging";
		public override string ModuleName => "EDI Messaging";
		public override string FunctionName => "EDI Client Details";
		public override string FeatureName => "EDI Client Inbound and Outbound details";
		public override string ActiveOn => "ALL";
		public override string DataGranularity => "MAH";
		public override string FromClause => $@"
(
SELECT
	ECP.ECP_PK,
	ECP.ECP_Name,
	ECP.ECP_ApplicationCode,
	CASE WHEN ECP.ECP_IsActive = 1 THEN 'true' ELSE 'false' END AS IsActive,
	ECP.ECP_Summary,
	OC.OC_ContactName,
	IN_ECC.ECC_Direction AS ECC_Direction_IN,
	OUT_ECC.ECC_Direction AS ECC_Direction_OUT,
	IN_ECC.ECC_IsActive AS In_ECC_IsActive,
	OUT_ECC.ECC_IsActive AS Out_ECC_IsActive,
	IN_ECA.ECA_AuthorizationMode AS In_ECA_AuthorizationMode,
	OUT_ECA.ECA_AuthorizationMode AS Out_ECA_AuthorizationMode,
	IN_ECC.ECC_IsSelfManaged AS In_ECC_IsSelfManaged,
	BR.GB_Code AS In_ECC_GB_Branch,
	DE.GE_Code AS In_ECC_GE_Department,
	OUT_ECC.ECC_Endpoint AS Out_ECC_Endpoint,
	OUT_ECA.ECA_FlowCode AS Out_ECA_FlowCode,
	(
		SELECT MAX(dt)
		FROM (VALUES
			(ECP.ECP_SystemLastEditTimeUtc),
			(IN_ECC.ECC_SystemLastEditTimeUtc),
			(OUT_ECC.ECC_SystemLastEditTimeUtc),
			(IN_ECA.ECA_SystemLastEditTimeUtc),
			(OUT_ECA.ECA_SystemLastEditTimeUtc)
		) AS AllDates(dt)
	) AS LastUpdateTime,
	CASE 
		WHEN ECP.ECP_SystemCreateTimeUtc BETWEEN {Constants.StartDateTimeInclusiveParamName} AND {Constants.EndDateTimeExclusiveParamName} THEN 'NEW'
		WHEN 
			IN_ECA.ECA_SystemCreateTimeUtc BETWEEN {Constants.StartDateTimeInclusiveParamName} AND {Constants.EndDateTimeExclusiveParamName} OR
			IN_ECA.ECA_SystemLastEditTimeUtc BETWEEN {Constants.StartDateTimeInclusiveParamName} AND {Constants.EndDateTimeExclusiveParamName} OR
			IN_ECC.ECC_SystemCreateTimeUtc BETWEEN {Constants.StartDateTimeInclusiveParamName} AND {Constants.EndDateTimeExclusiveParamName} OR
			IN_ECC.ECC_SystemLastEditTimeUtc BETWEEN {Constants.StartDateTimeInclusiveParamName} AND {Constants.EndDateTimeExclusiveParamName} OR
			OUT_ECA.ECA_SystemCreateTimeUtc BETWEEN {Constants.StartDateTimeInclusiveParamName} AND {Constants.EndDateTimeExclusiveParamName} OR
			OUT_ECA.ECA_SystemLastEditTimeUtc BETWEEN {Constants.StartDateTimeInclusiveParamName} AND {Constants.EndDateTimeExclusiveParamName} OR
			OUT_ECC.ECC_SystemCreateTimeUtc BETWEEN {Constants.StartDateTimeInclusiveParamName} AND {Constants.EndDateTimeExclusiveParamName} OR
			OUT_ECC.ECC_SystemLastEditTimeUtc BETWEEN {Constants.StartDateTimeInclusiveParamName} AND {Constants.EndDateTimeExclusiveParamName} OR
			ECP.ECP_SystemLastEditTimeUtc BETWEEN {Constants.StartDateTimeInclusiveParamName} AND {Constants.EndDateTimeExclusiveParamName}
		THEN 'EDT'
		ELSE ''
	END AS ChangeType

FROM dbo.EDICommunicationParty ECP
LEFT JOIN dbo.EDICommunicationPartyConfig IN_ECC ON IN_ECC.ECC_ECP_Party = ECP.ECP_PK AND IN_ECC.ECC_Direction = 'IN'
LEFT JOIN dbo.EDICommunicationAuth IN_ECA ON IN_ECA.ECA_PK = IN_ECC.ECC_ECA_Auth
LEFT JOIN dbo.GlbDepartment DE ON IN_ECC.ECC_GE_Department = GE_PK
LEFT JOIN dbo.GlbBranch BR ON IN_ECC.ECC_GB_Branch = GB_PK
LEFT JOIN dbo.EDICommunicationPartyConfig OUT_ECC ON OUT_ECC.ECC_ECP_Party = ECP.ECP_PK AND OUT_ECC.ECC_Direction = 'OUT'
LEFT JOIN dbo.EDICommunicationAuth OUT_ECA ON OUT_ECA.ECA_PK = OUT_ECC.ECC_ECA_Auth
LEFT JOIN dbo.OrgContact OC ON ECP_OC_TechnicalContact = OC_PK
) AggregatedMessage";
		public override string TransactionDateUtc => "AggregatedMessage.LastUpdateTime";
		public override string BillingReference1 => "AggregatedMessage.ChangeType";
		public override string BillingReference2 => "AggregatedMessage.ECP_ApplicationCode";
		public override string BillingReference3 => "AggregatedMessage.ECP_Name + '.' + AggregatedMessage.ECP_Summary";
		public override string BillingReference4 => "AggregatedMessage.IsActive";
		public override string GuidReference => "AggregatedMessage.ECP_PK";
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;

		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(
	SELECT
		TechnicalContact = AggregatedMessage.OC_ContactName,
		InboundActive = AggregatedMessage.In_ECC_IsActive,
		InboundAuthorizationType = AggregatedMessage.In_ECA_AuthorizationMode,
		InboundSelfManagedOAuth = AggregatedMessage.In_ECC_IsSelfManaged,
		InboundBranch = ISNULL(AggregatedMessage.In_ECC_GB_Branch, ''),
		InboundDepartment = ISNULL(AggregatedMessage.In_ECC_GE_Department, ''),
		OutboundActive = AggregatedMessage.Out_ECC_IsActive,
		OutboundAuthorizationType = AggregatedMessage.Out_ECA_AuthorizationMode,
		OutboundEndpoint = AggregatedMessage.Out_ECC_Endpoint,
		OutboundGrantType = AggregatedMessage.Out_ECA_FlowCode
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)))";

		public override string WhereClause => string.Empty;

		public override string DateType => RefStlDateType.SmallDateTime;

		public override string MinCW1Version => "25.3.19.236";

	}
}
