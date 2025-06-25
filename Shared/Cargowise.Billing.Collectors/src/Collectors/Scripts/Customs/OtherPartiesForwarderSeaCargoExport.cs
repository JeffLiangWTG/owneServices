namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class OtherPartiesForwarderSeaCargoExport : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "SCE";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Forwarder/CFS/CTO Functions";
		public override string FeatureName => "Import/Export Sea Cargo Report/Port Community Messaging (Export)";
		public override string TransactionDateUtc => "ce.CE_SystemCreateTimeUtc";
		public override string BillingReference1 => "ce.CE_EntryNum";
		public override string BillingReference2 => "jk.JK_UniqueConsignRef";
		public override string GuidReference => "ce.CE_PK";
		public override string CreatingUserCode => "ce.CE_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
				CusEntryNum ce 
					INNER JOIN dbo.JobConsol jk on jk.JK_PK = ce.CE_ParentID
				LEFT JOIN dbo.EDIMessage em on jk.JK_PK = em.EM_LinkUniqueID
								AND em.EM_LinkTable = 'JobConsol'
								AND em.EM_ApplicationCode = 'CMR'
								AND em.EM_ReceiveTransmit = 'TRX'
								AND em.EM_MessageType = 'ESM'
				LEFT JOIN ( GlbBranch gb
								INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
				) ON gb.GB_PK = em.EM_GB";
		public override string WhereClause => @"
					jk.JK_TransportMode = 'SEA'
					AND ce.CE_EntryNum <> ''
					AND ce.CE_RN_NKCountryCode = 'AU'
					AND ce.CE_Category = 'CUS'
					AND ce.CE_EntryType in ('CCN', 'CRN')";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string CompanyCode => "ISNULL(gc.GC_Code, (SELECT TOP 1 GC_Code FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'AU' AND GC_IsActive = 1))";
		public override string BranchCode => "gb.GB_Code";
		public override string MinCW1Version => "22.12.8.541";
	}
	#endregion
}
