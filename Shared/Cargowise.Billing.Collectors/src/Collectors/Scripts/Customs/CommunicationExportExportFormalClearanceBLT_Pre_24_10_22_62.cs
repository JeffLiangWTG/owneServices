namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class CommunicationExportExportFormalClearanceBLT_Pre_24_10_22_62 : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "EF2";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "Export Formal Clearance/Fiscal Report";
		public override string FeatureName => "Export Formal Clearance (BLT)";
		public override string CompanyCode => "jech.GC_Code";
		public override string BranchCode => "jech.GB_Code";
		public override string TransactionDateUtc => "jech.EffectiveTransDate";
		public override string BillingReference1 => "jech.JE_DeclarationReference";
		public override string BillingReference2 => "jech.BGMReference";
		public override string BillingReference3 => "jech.GC_RN_NKCountryCode";
		public override string GuidReference => "jech.JE_PK";
		public override string CreatingUserCode => "jech.JE_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
(
SELECT
	je.JE_PK,
	je.JE_DeclarationReference,
	je.JE_ApplicationCode,
	je.JE_MessageType,
	je.JE_SystemCreateUser,
	je.JE_GB,
	BGMReference = null,
	EffectiveTransDate = je.JE_SystemCreateTimeUtc,
	GC_Code,
	GC_RN_NKCountryCode,
	GB_Code
FROM
	dbo.JobDeclaration je
	INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = je.JE_GB
	INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC AND gc.GC_RN_NKCountryCode NOT IN ('US', 'PR', 'SG', 'AU', 'NZ', 'GB', 'CA')
WHERE
	je.JE_SystemCreateTimeUtc >= @StartDateTimeInclusive
	AND je.JE_SystemCreateTimeUtc < @EndDateTimeExclusive
	AND (je.JE_ApplicationCode = 'BLT' OR (gc.GC_RN_NKCountryCode = 'FR' AND je.JE_ApplicationCode IN ('DG', 'DI')))
	AND je.JE_MessageType = 'EXP'
UNION ALL
SELECT
	je.JE_PK,
	je.JE_DeclarationReference,
	je.JE_ApplicationCode,
	je.JE_MessageType,
	je.JE_SystemCreateUser,
	je.JE_GB,
	BGMReference = cec.CH_BGMReference,
	EffectiveTransDate = cec.CH_EntrySubmittedDate,
	GC_Code,
	GC_RN_NKCountryCode,
	GB_Code
FROM
	dbo.JobDeclaration je
	INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = je.JE_GB
	INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC AND gc.GC_RN_NKCountryCode NOT IN ('US', 'PR', 'SG', 'AU', 'NZ', 'CA') 
	INNER JOIN (
		SELECT
			ch.CH_JE,
			ch.CH_EntrySubmittedDate,
			ch.CH_BGMReference,
			EntrySubmissionRank = RANK() OVER (PARTITION BY ch.CH_JE ORDER BY ch.CH_EntrySubmittedDate, ch.CH_PK)
		FROM
			dbo.CusEntryHeader ch
		WHERE
			ch.CH_JE IS NOT NULL
			AND ch.CH_EntrySubmittedDate >= @StartDateTimeInclusive
			AND ch.CH_EntrySubmittedDate < @EndDateTimeExclusive
	)cec ON cec.CH_JE = je.JE_PK
WHERE
	je.JE_SystemCreateTimeUtc < @EndDateTimeExclusive
	AND 
	(	
		(je.JE_ApplicationCode = 'CDS' AND gc.GC_RN_NKCountryCode = 'GB' AND je.JE_MessageType = 'EXP')
		OR 
		((je.JE_ApplicationCode = 'BLT' OR (gc.GC_RN_NKCountryCode = 'FR' AND je.JE_ApplicationCode IN ('DG', 'DI'))) AND je.JE_MessageType = 'EXP')
	)
	AND cec.EntrySubmissionRank > 1
) jech
";
		public override string WhereClause => "";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.11.21.50";
		public override string MaxCW1Version => "24.10.22.18";
	}
	#endregion
}
