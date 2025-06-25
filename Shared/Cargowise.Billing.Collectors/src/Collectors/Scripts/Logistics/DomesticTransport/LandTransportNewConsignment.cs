namespace CargoWise.Billing.Collectors.Logistics
{
	public class LandTransportNewConsignment : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override bool UsedInBilling => true;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string FeatureCode => "LNC";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Domestic Transport";
		public override string FunctionName => "Land Transport";
		public override string FeatureName => "Land Transport New Consignment";
		public override string MinCW1Version => "21.8.31.0";
		public override string TransactionDateUtc => "ltc.LTC_SystemCreateTimeUtc";
		public override string GuidReference => "ltc.LTC_PK";
		public override string CompanyCode => "c.GC_Code";
		public override string BranchCode => "b.GB_Code";
		public override string BillingReference1 => "ltc.LTC_JobID";
		public override string BillingReference2 => "ltc.LTC_ConnoteNumber";
		public override string BillingReference3 => "ltc.LTC_JobType";
		public override string CreatingUserCode => "ltc.LTC_SystemCreateUser";
		public override string FromClause => @"DtbConsignment ltc
LEFT JOIN dbo.GlbBranch AS b ON b.GB_PK = ltc.LTC_GB_Branch
LEFT JOIN dbo.GlbCompany AS c ON b.GB_GC = c.GC_PK 
CROSS APPLY (SELECT TOP 1 LTS_LTC_Consignment, LTS_PK FROM dbo.DtbConsignmentAddress WHERE LTS_InstructionType = 'PIC' AND ltc.LTC_PK = LTS_LTC_Consignment ORDER BY LTS_SystemCreateTimeUtc ASC) lts_pic
CROSS APPLY (SELECT TOP 1 LTS_LTC_Consignment, LTS_PK FROM dbo.DtbConsignmentAddress WHERE LTS_InstructionType = 'DLV' AND ltc.LTC_PK = LTS_LTC_Consignment ORDER BY LTS_SystemCreateTimeUtc DESC) lts_dlv
LEFT JOIN dbo.JobDocAddress e2p on e2p.E2_ParentID = lts_pic.LTS_PK AND e2p.E2_ParentTableCode = 'LTS'
LEFT JOIN dbo.JobDocAddress e2d on e2d.E2_ParentID = lts_dlv.LTS_PK AND e2d.E2_ParentTableCode = 'LTS'
LEFT JOIN dbo.OrgAddress oap on oap.OA_PK = e2p.E2_OA_Address
LEFT JOIN dbo.OrgAddress oad on oad.OA_PK = e2d.E2_OA_Address
";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(SELECT
PCity=IIF(e2p.E2_AddressOverride=1 OR e2p.E2_OA_Address IS NULL,e2p.E2_City,oap.OA_City),
PState=IIF(e2p.E2_AddressOverride=1 OR e2p.E2_OA_Address IS NULL,e2p.E2_State,oap.OA_State),
PCountry=IIF(e2p.E2_AddressOverride=1 OR e2p.E2_OA_Address IS NULL,e2p.E2_RN_NKCountryCode,oap.OA_RN_NKCountryCode),
PGeoloc=IIF(e2p.E2_AddressOverride=1 OR e2p.E2_OA_Address IS NULL,e2p.E2_GeoLocation,oap.OA_GeoLocation).ToString(),
DCity=IIF(e2d.E2_AddressOverride=1 OR e2d.E2_OA_Address IS NULL,e2d.E2_City,oad.OA_City),
DState=IIF(e2d.E2_AddressOverride=1 OR e2d.E2_OA_Address IS NULL,e2d.E2_State,oad.OA_State),
DCountry=IIF(e2d.E2_AddressOverride=1 OR e2d.E2_OA_Address IS NULL,e2d.E2_RN_NKCountryCode,oad.OA_RN_NKCountryCode),
DGeoloc=IIF(e2d.E2_AddressOverride=1 OR e2d.E2_OA_Address IS NULL,e2d.E2_GeoLocation,oad.OA_GeoLocation).ToString()
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string ActiveOn => "ALL";

		public override string WhereClause => string.Empty;

		#endregion // SuppressResourceStringsCheckRegion
	}
}
