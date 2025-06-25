namespace CargoWise.Billing.Collectors.Scripts.Customs.EU;

public class EUExitControlStandaloneDeclaration : RefStlScriptWithDefaults
{
	public override string FeatureCode => "XIT";

	public override string FeatureName => "Exit Control";

	public override string RoleName => "Customs & Country Specific Integrations";

	public override string ModuleName => "Customs & Other Government Communication";

	public override string FunctionName => "Special Entry Types";

	public override string TransactionDateUtc => "CXH_SystemCreateTimeUTC";

	public override string CreatingUserCode => "CXH_SystemCreateUser";

	public override string CompanyCode => "GC_Code";

	public override string BranchCode => "GB_Code";

	public override string BillingReference1 => "CXH_JobReference";

	public override string GuidReference => "CXH_PK";

	public override string FromClause => @"dbo.CusExitHeader
INNER JOIN dbo.GlbBranch ON GB_PK = CXH_GB_Branch
INNER JOIN dbo.GlbCompany ON GC_PK = CXH_GC_Company
LEFT JOIN dbo.JobShipment ON JS_PK = CXH_ParentId AND CXH_ParentTableCode = 'JS'
LEFT JOIN dbo.JobDeclaration ON JS_PK = JE_JS AND JE_GC = GC_PK
";

	public override string WhereClause => "CXH_ApplicationCode = 'XIT' AND CXH_ParentTableCode <> 'JE' AND JE_PK IS NULL";

	public override string MinCW1Version => "25.5.22.225";
}
