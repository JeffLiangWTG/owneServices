namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class CaptureORNExportOutwardReportsCount : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "CTO";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FunctionName => "Forwarder/CFS/CTO/CY/Land Transport Message";
		public override string FeatureName => "Import/Export Air Cargo CFS/CTO Customs Functions";
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string TransactionDateUtc => "ce.CE_SystemCreateTimeUtc";
		public override string CreatingUserCode => "ce.CE_SystemCreateUser";
		public override string GuidReference => "ce.CE_PK";
		public override string BillingReference1 => "jk.JK_UniqueConsignRef";
		public override string BillingReference2 => "jk.JK_MasterBillNum";
		public override string TransactionCount => "1";
		public override string FromClause => @"
								CusEntryNum ce
								WITH (INDEX(NR_RX__CE_SystemCreateTimeUtc))
								INNER JOIN dbo.JobConsol jk ON jk.JK_PK = ce.CE_ParentID
								LEFT JOIN dbo.EDIMessage em ON em.EM_LinkUniqueID = jk.JK_PK
									AND em.EM_LinkTable = 'JobConsol'
									AND em.EM_ApplicationCode = 'NZC'
									AND em.EM_ReceiveTransmit = 'TRX'
									AND em.EM_MessageType = 'OCR'
									AND em.EM_MessageSubType = 'ORG'
								LEFT JOIN GlbBranch gb ON gb.GB_PK = em.EM_GB
								LEFT JOIN GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
								ce.CE_RN_NKCountryCode = 'NZ'
								AND ce.CE_ParentTable = 'JobConsol'
								AND ce.CE_EntryType = 'ORN'
								AND ce.CE_EntryNum <> '00000000'";
		public override bool WithOptionRecompile => false;
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string CompanyCode => "ISNULL(gc.GC_Code, (SELECT TOP 1 GC_Code FROM GlbCompany WHERE GC_RN_NKCountryCode = 'NZ' AND GC_IsActive = 1))";
		public override string BranchCode => "gb.GB_Code";
	}

	#endregion // SuppressResourceStringsCheckRegion
}
