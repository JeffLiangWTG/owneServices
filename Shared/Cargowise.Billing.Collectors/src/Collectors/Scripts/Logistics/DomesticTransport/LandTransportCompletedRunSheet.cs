namespace CargoWise.Billing.Collectors.Logistics
{
	public class LandTransportCompletedRunSheet : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion
		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string FeatureCode => "LCR";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Domestic Transport";
		public override string FunctionName => "Land Transport";
		public override string FeatureName => "Land Transport Completed Run Sheet";
		public override string MinCW1Version => "22.2.17.0";
		public override string TransactionDateUtc => "TransactionDateUtc";
		public override string GuidReference => "KG_PK";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string BillingReference1 => "RunSheetNumber";
		public override string BillingReference2 => "TransportMode";
		public override string BillingReference3 => "ContainerMode";
		public override string BillingReference4 => "CarrierServiceLevel";
		public override string CreatingUserCode => "SystemCreateUser";
		public override string PreparationScript => @"WITH CompletedRunSheetCTE
AS
(
SELECT 
KG_PK = kg.KG_PK,
SystemCreateTimeUtc = kg.KG_SystemCreateTimeUtc,
SystemCreateUser = kg.KG_SystemCreateUser,
TransactionDateUtc = DATEADD(DAY, 10, kg.KG_SystemCreateTimeUtc),
GC_Code = c.GC_Code,
GB_Code = b.GB_Code,
RunSheetNumber = kg.KG_RunSheetNumber,
TransportMode = kg.KG_TransportMode,
ContainerMode = kg.KG_ContainerMode,
CarrierServiceLevel = kg.KG_PL_NKCarrierServiceLevel,
DriverAllocated = kg.KG_GS_NKTruckDriver,
TransportCompanyAllocated = oh.OH_Code,
PrimeMoverAllocated = truck.RQ_ShortCode,
StartTime = kg.KG_StartTime,
EndTime = kg.KG_EndTime,
TransitTime = kg.KG_TransitTime,
Duration = DATEDIFF(MINUTE, '1901-01-01 00:00:00', COALESCE(KG_Duration, '1901-01-01 00:00:00')),
AdhocDriversLicence = kg.KG_AdHocDriversLicence,
AdhocDriversName = kg.KG_AdHocDriversName,
AdhocTransportCoName = kg.KG_AdHocTransportCoName,
AdhocTruckRegistration = kg.KG_AdHocTruckRegistration,
CountOfDepotInstructions = (SELECT COUNT(*) FROM dbo.DtbConsignmentRunSheetInstruction k1
									WHERE k1.K1_KG_RunSheet = kg.KG_PK AND (SELECT COUNT(*) FROM dbo.DtbConsignmentAction lta
										   LEFT JOIN dbo.JobDocAddress jda ON E2_ParentID = lta.LTA_LTS_ConsignmentAddress AND jda.E2_ParentTableCode = 'LTS'
										   INNER JOIN dbo.OrgAddress oa ON oa.OA_PK = jda.E2_OA_Address 
										   INNER JOIN dbo.OrgHeader oh ON oh.OH_PK = oa.OA_OH
										   WHERE lta.LTA_K1_RunSheetInstruction = k1.K1_PK AND oh.OH_IsMiscFreightServices = 1 AND (oh.OH_IsUnpackDepot = 1 OR oh.OH_IsPackDepot = 1 OR oh.OH_IsRoadFreightDepot = 1 OR oh.OH_IsRailHead = 1)) > 0),
HasORGInstruction = CASE WHEN (SELECT COUNT(*) FROM dbo.DtbConsignmentRunSheetInstruction k1 WHERE k1.K1_KG_RunSheet = kg.KG_PK AND k1.K1_InstructionType = 'ORG') > 0 THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END,
HasDSTInstruction = CASE WHEN (SELECT COUNT(*) FROM dbo.DtbConsignmentRunSheetInstruction k1 WHERE k1.K1_KG_RunSheet = kg.KG_PK AND k1.K1_InstructionType = 'DST') > 0 THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END,
CountOfInstructions = (SELECT COUNT(*) FROM dbo.DtbConsignmentRunSheetInstruction k1 WHERE k1.K1_KG_RunSheet = kg.KG_PK),
CountOfDLVInstructions = (SELECT COUNT(*) FROM dbo.DtbConsignmentRunSheetInstruction k1 WHERE k1.K1_KG_RunSheet = kg.KG_PK AND (SELECT COUNT(*) FROM dbo.DtbConsignmentAction lta WHERE lta.LTA_K1_RunSheetInstruction = k1.K1_PK AND lta.LTA_ActionType != 'DLV') = 0),
CountOfPICInstructions = (SELECT COUNT(*) FROM dbo.DtbConsignmentRunSheetInstruction k1 WHERE k1.K1_KG_RunSheet = kg.KG_PK AND (SELECT COUNT(*) FROM dbo.DtbConsignmentAction lta WHERE lta.LTA_K1_RunSheetInstruction = k1.K1_PK AND lta.LTA_ActionType != 'PIC') = 0),
CountOfNotRejectedOrCompletedInstructions = (SELECT COUNT(*) FROM dbo.DtbConsignmentRunSheetInstruction k1 WHERE k1.K1_KG_RunSheet = kg.KG_PK AND k1.K1_FailureReason = '' AND k1.K1_TimeOut IS NULL),
CountOfRejectedInstructions = (SELECT COUNT(*) FROM dbo.DtbConsignmentRunSheetInstruction k1 WHERE k1.K1_KG_RunSheet = kg.KG_PK AND k1.K1_FailureReason > ''),
CountOfAllocatedPICActions = (SELECT COUNT(*) FROM dbo.DtbConsignmentAction lta 
							  LEFT JOIN dbo.DtbConsignmentRunSheetInstruction k1 on k1.K1_PK = lta.LTA_K1_RunSheetInstruction 
							  WHERE k1.K1_KG_RunSheet = kg.KG_PK AND lta.LTA_ActionType = 'PIC'),
HasBillingLinesAttached = CASE WHEN (SELECT COUNT(*) FROM dbo.JobConsolCost e6 WHERE e6.E6_ParentID = kg.KG_PK AND e6.E6_ParentTableCode= 'KG') > 0 THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END,
HasAdditionalService = (SELECT COUNT(*) FROM dbo.JobService js WHERE js.es_ParentID = kg.KG_PK AND js.ES_ParentTableCode = 'KG'),
HasAdditionalReference = (SELECT COUNT(*) FROM dbo.CusEntryNum ce WHERE ce.CE_ParentID = kg.KG_PK AND ce.CE_ParentTable = 'DtbConsignmentRunSheet'),
CountOfAdditionalAssets = (SELECT COUNT(*) FROM dbo.DtbEquipmentItem lte LEFT JOIN dbo.RefEquipment rq ON lte.LTE_RQ_Equipment = rq.RQ_PK WHERE lte.LTE_ParentID = kg.KG_PK AND lte.LTE_ParentTableCode = 'KG' AND rq.RQ_IsVehicle = 0),
CountOfAncillaryAssets = (SELECT COUNT(*) FROM dbo.DtbEquipmentItem lte 
						  WHERE lte.LTE_ParentID = kg.KG_PK AND lte.LTE_ParentTableCode = 'KG' AND lte.LTE_EquipmentTypeQuantity > 0)
FROM dbo.DtbConsignmentRunSheet kg
LEFT JOIN dbo.GlbBranch AS b ON b.GB_PK = kg.KG_GB_Branch
LEFT JOIN dbo.GlbCompany AS c ON b.GB_GC = c.GC_PK 
LEFT JOIN dbo.OrgHeader AS oh on oh.OH_PK = kg.KG_OH_TransportCo
LEFT JOIN dbo.RefEquipment truck on truck.RQ_PK = kg.KG_RQ_Truck
WHERE kg.KG_SystemCreateTimeUtc >= CONVERT(SMALLDATETIME, DATEADD(DAY, -10, @StartDateTimeInclusive))
AND kg.KG_SystemCreateTimeUtc < CONVERT(SMALLDATETIME, DATEADD(DAY, -10, @EndDateTimeExclusive))
)";
		public override string FromClause => @"CompletedRunSheetCTE";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(SELECT
SystemCreateTimeUtc, DriverAllocated, TransportCompanyAllocated, PrimeMoverAllocated, StartTime, EndTime, TransitTime, Duration, AdhocDriversLicence, AdhocDriversName, AdhocTransportCoName, AdhocTruckRegistration, CountOfDepotInstructions, CountOfDLVInstructions, CountOfPICInstructions,
CountOfInstructions - CountOfPICInstructions - CountOfDLVInstructions AS CountOfMLTInstructions, CountOfAllocatedPICActions, CountOfNotRejectedOrCompletedInstructions, CountOfRejectedInstructions, CountOfInstructions, HasORGInstruction, HasDSTInstruction, HasBillingLinesAttached, HasAdditionalService, HasAdditionalReference, CountOfAdditionalAssets, CountOfAncillaryAssets
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string ActiveOn => "ALL";

		public override string WhereClause => string.Empty;

		#endregion // SuppressResourceStringsCheckRegion
	}
}
