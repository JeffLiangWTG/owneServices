namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion
	public class LandTransportIntelligence : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "LTR";
		public override string FeatureName => "Land Transport Intelligence Report";
		public override string TransactionDateUtc => "RSI.K1_SystemLastEditTimeUtc";
		public override string CreatingUserCode => "RSI.K1_SystemCreateUser";
		public override string TransactionCount => "1";
		public override string GuidReference => "RSI.K1_PK";
		public override string FromClause => @"
					DtbConsignmentRunSheetInstruction RSI LEFT JOIN  DtbConsignmentRunSheet RUN ON RSI.K1_KG_RunSheet = RUN.KG_PK -- Join with DtbConsignmentRunSheet
					LEFT JOIN OrgHeader OH ON RUN.KG_Oh_TransportCo = OH.OH_PK -- Join with OrgHeader to get Transport Company code if available
					LEFT JOIN DtbConsignmentAction ACT ON ACT.LTA_K1_RunSheetInstruction = RSI.K1_PK -- Join with DtbConsignmentAction on the instruction ID
					LEFT JOIN DtbConsignmentAddress CAD ON ACT.LTA_LTS_ConsignmentAddress = CAD.LTS_PK -- Join with DtbConsignmentAddress using LTA_LTS_ConsignmentAddress foreign key
					LEFT JOIN JobDocAddress JDA ON JDA.E2_ParentId = CAD.LTS_PK AND JDA.E2_ParentTableCode = 'LTS' -- Join with JobDocAddress using E2_ParentId and E2_ParentTableCode ('LTS')
					LEFT JOIN OrgAddress OAD ON JDA.E2_OA_Address = OAD.OA_PK -- Join with OrgAddress using E2_OA_Address foreign key
					LEFT JOIN DtbConsignment CON ON CAD.LTS_LTC_Consignment = CON.LTC_PK -- Join with DtbConsignment using LTS_LTC_Consignment foreign key
					LEFT JOIN DtbConsignmentLeg LEG ON LEG.LTG_LTC_Consignment = CON.LTC_PK -- Join with DtbConsignmentLeg using LTG_LTC_Consignment foreign key to DtbConsignment
					LEFT JOIN DtbConsignmentAction PUA ON LEG.LTG_LTA_Pickup = PUA.LTA_PK -- Join with DtbConsignmentAction for pickup action
					LEFT JOIN DtbConsignmentAction DEA ON LEG.LTG_LTA_Delivery = DEA.LTA_PK -- Join with DtbConsignmentAction for delivery action
					LEFT JOIN DtbBooking DB ON CON.LTC_KM_Booking = DB.KM_PK -- Join with DtbBooking for checking parent entity
					LEFT JOIN GlbBranch GB ON RUN.KG_GB_Branch = GB.GB_PK
					LEFT JOIN GlbCompany GC ON GB.GB_GC = GC.GC_PK
";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (SELECT EstimatedTimeIn = RSI.K1_EstimatedTimeIn,
		TimeIn = RSI.K1_TimeIn,
		EstimatedTimeOut = RSI.K1_EstimatedTimeOut,
		TimeOut = RSI.K1_TimeOut,
		JobType = ACT.LTA_ActionType,
		PickupRequiredTo =  PUA.LTA_RequiredTo,
		PickupRequiredFrom = PUA.LTA_RequiredFrom,
		DeliveryRequiredTo = DEA.LTA_RequiredTo,
		DeliveryRequiredFrom = DEA.LTA_RequiredFrom,
		RunSheetNumber = RUN.KG_RunSheetNumber,
		InstructionPostCode = ISNULL(NULLIF(JDA.E2_PostCode, ''), OAD.OA_PostCode),
		InstructionCity = ISNULL(NULLIF(JDA.E2_City, ''), OAD.OA_City),
		InstructionState = ISNULL(NULLIF(JDA.E2_State, ''), OAD.OA_State),
		InstructionCountryCode = ISNULL(NULLIF(JDA.E2_RN_NKCountryCode, ''), OAD.OA_RN_NKCountryCode),
		ParentEntityTB = CASE WHEN CON.LTC_KM_Booking IS NULL THEN 'N' ELSE 'Y' END
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Domestic Transport";
		public override string FunctionName => "Land Transport Intelligence Report";
		public override string CompanyCode => "GC.GC_Code";
		public override string BranchCode => "GB.GB_Code";
		public override string BillingReference1 => "CON.LTC_JobID";
		public override string BillingReference2 => "RUN.KG_ContainerMode";
		public override string BillingReference3 => "RUN.KG_TransportMode";
		public override string BillingReference4 => "OH.OH_Code";
		public override string WhereClause => string.Empty;
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string ActiveOn => "ALL";
	}
	#endregion
}
