namespace CargoWise.Billing.Collectors.Customs;

public class EUICS2ForwarderEntrySummaryDeclaration : RefStlScriptWithDefaults
{
	public override string FeatureCode => "ICF";
	public override string RoleName => "Customs & Country Specific Integrations";
	public override string ModuleName => "Pre Departure, Advanced Filing and Embargo";
	public override string FunctionName => "Embargo / Parties of Interest etc";
	public override string FeatureName => "EU ICS2 Pre-Loading Advance Cargo Information (PLACI) - Forwarder";
	public override string CompanyCode => "GC_Code";
	public override string BranchCode => "GB_Code";
	public override string TransactionDateUtc => "CE_SystemCreateTimeUtc";
	public override string GuidReference => "ABL_PK";
	public override string BillingReference1 => "'Manifest: ' + AMA_JobReference";
	public override string BillingReference2 => "'Bill: ' + ABL_BillNumber";
	public override string FromClause => @"
dbo.AsycudaManifestHeader
INNER JOIN dbo.CusEntryNum ON CE_ParentID = AMA_PK AND CE_EntryType = 'ASY'
INNER JOIN dbo.AsycudaBill ON AMA_ClusterKey = ABL_ClusterKey AND ABL_BolType <> 'BOL'
INNER JOIN dbo.GlbBranch ON GB_PK = AMA_GB
INNER JOIN dbo.GlbCompany ON GC_PK = GB_GC";
	public override string WhereClause => @"AMA_ManifestType = 'ENS'
AND AMA_ApplicationCode = 'NVC'
AND CE_SystemCreateTimeUtc >= @StartDateTimeInclusive
AND CE_SystemCreateTimeUtc < @EndDateTimeExclusive";
}
