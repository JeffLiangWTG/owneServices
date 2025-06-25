namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class AdditionalServicesSGAirlineMessaging : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "SGM";
		public override string RoleName => "Additional Services";
		public override string ModuleName => "Additional Services";
		public override string FunctionName => "Messaging and Electronic Submission";
		public override string FeatureName => "Airlines Messaging (FWB/FHL/FSR/FSU/FSA/FMA/FNA/CMD/Forward Air)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "em.MinDate";
		public override string BillingReference1 => "jk.JK_UniqueConsignRef";
		public override string BillingReference2 => "js.JS_UniqueConsignRef";
		public override string BillingReference3 => "js.JS_HouseBill";
		public override string BillingReference4 => "jk.JK_MasterBillNum";
		public override string GuidReference => "js.JS_PK";
		public override string CreatingUserCode => "em.EM_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
(
SELECT em2.EM_LinkUniqueID, em2.EM_ApplicationReference, em2.EM_LinkTable, MAX(em2.EM_GB) AS EM_GB, MIN(em2.EM_SystemCreateTimeUtc) AS MinDate, MAX(em2.EM_SystemCreateUser) AS EM_SystemCreateUser
FROM 
	dbo.EDIMessage em2
WHERE 
	em2.EM_LinkUniqueID IN (
		SELECT em0.EM_LinkUniqueID
		FROM 
			dbo.EDIMessage em0
		WHERE
			em0.EM_SystemCreateTimeUtc >= @StartDateTimeInclusive
			AND em0.EM_SystemCreateTimeUtc < @EndDateTimeExclusive
			AND em0.EM_LinkUniqueID is not null
			AND em0.EM_ReceiveTransmit = 'TRX'
			AND em0.EM_MessageType IN ('CMD')
			AND em0.EM_ApplicationCode = 'CMD'
			AND em0.EM_Status = 'SNT'
		)

	AND em2.EM_LinkUniqueID is not null
	AND em2.EM_ReceiveTransmit = 'TRX'
	AND em2.EM_MessageType IN ('CMD')
	AND em2.EM_ApplicationCode = 'CMD'
	AND em2.EM_Status = 'SNT'
	GROUP BY em2.EM_LinkUniqueID, em2.EM_ApplicationReference, em2.EM_LinkTable
) AS em

INNER JOIN dbo.JobShipment js ON js.JS_PK = em.EM_LinkUniqueID
LEFT JOIN (
	JobConShipLink jn 
	INNER JOIN dbo.JobConsol jk ON jk.JK_PK = jn.JN_JK
	) ON jn.JN_JS = js.JS_PK AND jk.JK_UniqueConsignRef = em.EM_ApplicationReference

LEFT JOIN (
GlbBranch gb 
INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
) ON gb.GB_PK = em.EM_GB";
		public override string WhereClause => "isnull(gc.GC_RN_NKCountryCode, '') <> 'GB'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.13.467";
	}

	#endregion
}
