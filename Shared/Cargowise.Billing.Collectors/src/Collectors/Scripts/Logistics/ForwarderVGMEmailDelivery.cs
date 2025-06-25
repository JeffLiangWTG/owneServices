namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class ForwarderVGMEmailDelivery : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "VGE";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "General Forwarding Engine";
		public override string FeatureName => "Carrier VGM Email Tracking";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "pj.SP_RunDateTime";
		public override string BillingReference1 => "cr.SPR_EmailAddress";
		public override string BillingReference2 => @"
					CASE WHEN oc.OK_CustomsRegNo IS NULL THEN '' ELSE oc.OK_CustomsRegNo + ', ' END +
					oh.OH_FullName +
					', LoadPort: ' + jc.JK_RL_NKLoadPort +
					', ' + oa.OA_Address1 +
					CASE oa.OA_Address2 WHEN '' THEN '' ELSE ', ' + oa.OA_Address2 END +
					CASE oa.OA_City WHEN '' THEN '' ELSE ', ' + oa.OA_City END +
					CASE oa.OA_PostCode WHEN '' THEN '' ELSE ', ' + oa.OA_PostCode END +
					', ' + oa.OA_RN_NKCountryCode";
		public override string GuidReference => "pj.SP_PK";
		public override string CreatingUserCode => "pj.SP_GS_NKJobSubmittedBy";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					StmPrintJob pj
					JOIN dbo.JobDocumentData jdd ON jdd.JDD_PK = pj.SP_ParentGuid
					JOIN dbo.JobConsol jc ON jc.JK_PK = jdd.JDD_ParentID
					JOIN dbo.OrgAddress oa ON jc.JK_OA_ShippingLineAddress = oa.OA_PK
					JOIN dbo.OrgHeader oh ON oa.OA_OH = oh.OH_PK
					JOIN dbo.GlbBranch gb ON pj.SP_GB = gb.GB_PK
					JOIN dbo.GlbCompany gc ON gb.GB_GC = gc.GC_PK
					JOIN dbo.StmPrintJobCopyRecipient cr ON cr.SPR_SP = pj.SP_PK
					LEFT JOIN dbo.OrgCusCode oc ON oh.OH_PK= oc.OK_OH AND oc.OK_RN_NKCodeCountry = 'US' AND oc.OK_CodeType = 'CCC'";
		public override string WhereClause => @"
					pj.SP_JobType = 'EML'
					AND pj.SP_DocumentName = 'Verified Gross Container Weight'
					AND cr.SPR_EmailAddress <> ''";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.1.20.174";
	}

	#endregion
}
