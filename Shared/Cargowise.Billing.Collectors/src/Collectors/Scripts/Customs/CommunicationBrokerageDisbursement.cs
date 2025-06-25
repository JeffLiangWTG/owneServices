namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class CommunicationBrokerageDisbursement : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "BRD";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "Formal Customs Entry Compliance";
		public override string FeatureName => "Brokerage Disbursement";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "jr.JR_SystemCreateTimeUtc";
		public override string BillingReference1 => "jh.JH_JobNum";
		public override string BillingReference2 => "jr.JR_RX_NKCostCurrency";
		public override string BillingReference3 => "jr.JR_OSCostAmt";
		public override string BillingReference4 => "jh.JH_Direction";
		public override string GuidReference => "jh.JH_PK";
		public override string CreatingUserCode => "jr.JR_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
JobCharge jr
INNER join dbo.JobHeader jh ON JH_PK = JR_JH AND JH_IsDisbursement = 1
INNER JOIN dbo.JobDeclaration je ON je.JE_PK = jh.JH_ParentID
INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = jr.JR_GB
INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
INNER JOIN dbo.AccTransactionLines al on al.AL_LineType = 'CST' AND al.AL_PK = jr.JR_AL_APLine
INNER JOIN dbo.AccTransactionHeader ah on ah.AH_PK = al.AL_AH AND ah.AH_TransactionType = 'JRJ' AND ah.AH_IsCancelled = 0 ";
		public override string WhereClause => @"
(jr.JR_AL_APLine IS NOT NULL
AND 
jr.JR_AC
IN
(
	SELECT AC_PK FROM dbo.AccChargeCode
	WHERE AC_Code IN
	(
		SELECT AC_Code FROM dbo.AccChargeCode
		WHERE AC_PK IN
		(
		  SELECT CAST(CAST(SD_BinaryValue AS NVARCHAR(MAX)) AS UNIQUEIDENTIFIER)
			FROM dbo.StmData WHERE SD_Name = 'ElectronicProcessingChargeCode'
		)
	)
))";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "24.11.28.88";
	}
	#endregion
}
