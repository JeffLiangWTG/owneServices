namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class ExtensionsUsaEBond : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "EBD";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "eBond";
		public override string FeatureName => "US eBond - Single Transaction Bond Filing";
		public override string CompanyCode => "InBondDetails.GC_Code";
		public override string BranchCode => "InBondDetails.GB_Code";
		public override string TransactionDateUtc => "InBondDetails.EM_SystemCreateTimeUtc";
		public override string BillingReference1 => "InBondDetails.JE_DeclarationReference";
		public override string BillingReference2 => "InBondDetails.GC_RN_NKCountryCode";
		public override string BillingReference3 => "InBondDetails.CE_EntryNum";
		public override string GuidReference => "InBondDetails.JE_PK";
		public override string CreatingUserCode => "InBondDetails.EM_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"

(
	SELECT
        JE_PK, EM.EM_SystemCreateTimeUtc,
        GB.GB_Code,
        GC.GC_Code,
		GC.GC_RN_NKCountryCode,
		EM.EM_SystemCreateUser,
		JE.JE_DeclarationReference,
        EM.EM_LinkUniqueID, 
		EM.EM_PK,
		CE.CE_EntryNum
    FROM
        dbo.JobDeclaration JE
		INNER JOIN dbo.CusEntryNum CE ON CE_ParentID = JE_PK AND CE_EntryType ='ENS' AND CE_RN_NKCountryCode = 'US'
        INNER JOIN dbo.GlbBranch GB ON JE_GB = GB_PK
        INNER JOIN dbo.GlbCompany GC ON GB_GC = GC_PK AND GC_RN_NKCountryCode IN ('US', 'PR')
        INNER JOIN dbo.CusEntryHeader ON JE_ClusterKey = CH_ClusterKey AND CH_MessageType = 'ENS'
        INNER JOIN dbo.EDIMessage AS EM ON EM.EM_LinkUniqueID = CH_PK
    WHERE
		JE_MessageType = 'IMP'
        AND EM.EM_SystemCreateTimeUtc >= @StartDateTimeInclusive
        AND EM.EM_SystemCreateTimeUtc < @EndDateTimeExclusive
        AND EM_ApplicationCode = 'UXB'
        AND EM_MessageType = 'XDC'
        AND EM_MessageSubType = 'XUE'
        AND EM_ReceiveTransmit = 'RCV'
        AND EM_Status = 'RCV'
        AND EM_ApplicationReference = 'ACCEPTED'
) AS InBondDetails";
		public override string WhereClause => @"
NOT EXISTS(
       SELECT NULL
FROM dbo.EDIMessage O
WHERE O.EM_LinkUniqueID = InBondDetails.EM_LinkUniqueID
    AND O.EM_PK != InBondDetails.EM_PK
    AND O.EM_SystemCreateTimeUtc < InBondDetails.EM_SystemCreateTimeUtc
	AND O.EM_SystemCreateTimeUtc >= DATEADD(month, -6, InBondDetails.EM_SystemCreateTimeUtc)
    AND O.EM_ApplicationCode = 'UXB'
    AND O.EM_MessageType = 'XDC'
    AND O.EM_MessageSubType = 'XUE'
    AND O.EM_ReceiveTransmit = 'RCV'
    AND O.EM_Status = 'RCV'
    AND O.EM_ApplicationReference = 'ACCEPTED'
)";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.6.388";
	}
	#endregion
}
