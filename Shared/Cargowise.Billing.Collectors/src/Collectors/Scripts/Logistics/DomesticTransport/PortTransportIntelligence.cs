namespace CargoWise.Billing.Collectors.Logistics
{
	public class PortTransportIntelligence : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "PTR";
		public override string FeatureName => "Port Transport Intelligence Report";
		public override string TransactionDateUtc => "JCL.JU_SystemLastEditTimeUtc";
		public override string CreatingUserCode => "JCL.JU_SystemCreateUser";
		public override string TransactionCount => "1";
		public override string GuidReference => "JCL.JU_PK";
		public override string FromClause => @"
					JobContainerLegs AS JCL 
					JOIN JobBookedCtgMove AS JBCM ON JCL.JU_EW = JBCM.EW_PK -- Join JobBookedCtgMove to filter JobContainerLegs records based on JU_EW = EW_PK 
					JOIN JobCartage AS JC ON JBCM.EW_JJ = JC.JJ_PK -- Join JobCartage to link JobBookedCtgMove to JobCartage based on EW_JJ = JJ_PK 
					LEFT JOIN JobDocAddress AS JDAPICKUP ON JCL.JU_E2PickupAddressID = JDAPICKUP.E2_PK -- Left join to JobDocAddress for pickup address using JU_E2PickupAddressID = E2_PK 
					LEFT JOIN OrgAddress AS OAPICKUP ON JDAPICKUP.E2_OA_Address = OAPICKUP.OA_PK -- Left join to OrgAddress for pickup address, using JobDocAddress' E2_OA_Address = OA_PK 
					LEFT JOIN JobDocAddress AS JDADELIVERY ON JCL.JU_E2DeliveryAddressID = JDADELIVERY.E2_PK -- Left join to JobDocAddress for delivery address using JU_E2DeliveryAddressID = E2_PK 
					LEFT JOIN OrgAddress AS OADELIVERY ON JDADELIVERY.E2_OA_Address = OADELIVERY.OA_PK -- Left join to OrgAddress for delivery address, using JobDocAddress' E2_OA_Address = OA_PK 
					LEFT JOIN JobCartageRunSheet AS JCRS ON JCL.JU_EY_RunSheet = JCRS.EY_PK -- Left join to JobCartageRunSheet for transport info using JU_EY_RunSheet = EY_PK 
					LEFT JOIN OrgHeader AS OH ON JCRS.EY_OH_TransportCo = OH.OH_PK -- Left join to OrgHeader for transport company info, using JobCartageRunSheet's EY_OH_TransportCo = OH_PK
					LEFT JOIN dbo.[GlbBranch] GB ON JC.JJ_GB = GB.GB_PK
					LEFT JOIN dbo.[GlbCompany] GC ON GB.GB_GC = GC.GC_PK
";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (SELECT PlannedPickupTime = JCL.JU_PlannedPickupTime,
	PickupTimeIn = JCL.JU_PickupTimeIn,
	EstimatedDeliveryTime = JCL.JU_EstimatedDeliveryTime,
	DeliverTimeIn = JCL.JU_DeliverTimeIn,
	PickupPostCode = ISNULL(NULLIF(JDAPICKUP.E2_PostCode, ''), OAPICKUP.OA_PostCode),
	PickupCity = ISNULL(NULLIF(JDAPICKUP.E2_City, ''), OAPICKUP.OA_City),
	PickupState = ISNULL(NULLIF(JDAPICKUP.E2_State, ''), OAPICKUP.OA_State),
	PickupCountryCode = ISNULL(NULLIF(JDAPICKUP.E2_RN_NKCountryCode, ''), OAPICKUP.OA_RN_NKCountryCode),
	DeliveryPostCode = ISNULL(NULLIF(JDADELIVERY.E2_PostCode, ''), OADELIVERY.OA_PostCode),
	DeliveryCity = ISNULL(NULLIF(JDADELIVERY.E2_City, ''), OADELIVERY.OA_City),
	DeliveryState = ISNULL(NULLIF(JDADELIVERY.E2_State, ''), OADELIVERY.OA_State),
	DeliveryCountryCode = ISNULL(NULLIF(JDADELIVERY.E2_RN_NKCountryCode, ''), OADELIVERY.OA_RN_NKCountryCode)
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Domestic Transport";
		public override string FunctionName => "Port Transport Intelligence Report";
		public override string CompanyCode => "GC.GC_Code";
		public override string BranchCode => "GB.GB_Code";
		public override string BillingReference1 => "JC.JJ_ConsignmentID";
		public override string BillingReference2 => "JC.JJ_ContainerMode";
		public override string BillingReference3 => "JC.JJ_ParentTableCode";
		public override string BillingReference4 => "OH.OH_Code";
		public override string WhereClause => string.Empty;
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string ActiveOn => "ALL";
	}
}
