namespace CargoWise.RefDbRepo.STLBillingCollector.TestDummyCollectors.Logistics
{
	public class LandTransportCompletedConsignment : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion
		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyCurrentDataOnly;
		public override string FeatureCode => "LCC";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Domestic Transport";
		public override string FunctionName => "Land Transport";
		public override string FeatureName => "Land Transport Completed Consignment";
		public override string MinCW1Version => "22.4.30.0";
		public override string TransactionDateUtc => "SystemCreateTimeUtc";
		public override string GuidReference => "LTC_PK";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string BillingReference1 => "JobID";
		public override string BillingReference2 => "ConnoteNumber";
		public override string BillingReference3 => "JobType";
		public override string CreatingUserCode => "SystemCreateUser";
		public override string PreparationScript => @"WITH CompletedConsignmentCTE
AS
(
SELECT 
SystemCreateTimeUtc = dc.LTC_SystemCreateTimeUtc,
LTC_PK = dc.LTC_PK,
GC_Code = c.GC_Code,
GB_Code = b.GB_Code,
JobID = dc.LTC_JobID,
ConnoteNumber = dc.LTC_ConnoteNumber,
SystemCreateUser = dc.LTC_SystemCreateUser,
JobType = dc.LTC_JobType,
HasTB = CASE WHEN dc.LTC_KM_Booking IS NOT NULL THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END,
StaffCode = dc.LTC_SystemCreateUser,
BillingLinesAttached = CASE WHEN (SELECT COUNT(*) FROM dbo.JobCharge jr LEFT JOIN dbo.JobHeader jh ON jh.JH_PK = jr.JR_JH WHERE jh.JH_ParentID = dc.LTC_PK AND jh.JH_ParentTableCode = 'LTC') > 0 THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END,
CountOfPICInstructions = (SELECT COUNT(*) FROM dbo.DtbConsignmentAddress dca WHERE dca.LTS_InstructionType = 'PIC' AND dca.LTS_LTC_Consignment = dc.LTC_PK),
CountOfPICInstructionsLinkedToMiscellaneousAddress = (SELECT COUNT(*) FROM dbo.DtbConsignmentAddress dca WHERE dca.LTS_InstructionType = 'PIC' AND dca.LTS_LTC_Consignment = dc.LTC_PK AND (SELECT COUNT(*) FROM dbo.JobDocAddress jda WHERE jda.E2_ParentID = dca.LTS_PK AND jda.E2_ParentTableCode = 'LTS' AND jda.E2_AddressOverride = 1) > 0),
CountOfDLVInstructions = (SELECT COUNT(*) FROM dbo.DtbConsignmentAddress dca WHERE dca.LTS_InstructionType = 'DLV' AND dca.LTS_LTC_Consignment = dc.LTC_PK),
CountOfDLVInstructionsLinkedToMiscellaneousAddress = (SELECT COUNT(*) FROM dbo.DtbConsignmentAddress dca WHERE dca.LTS_InstructionType = 'DLV' AND dca.LTS_LTC_Consignment = dc.LTC_PK AND (SELECT COUNT(*) FROM dbo.JobDocAddress jda WHERE jda.E2_ParentID = dca.LTS_PK AND jda.E2_ParentTableCode = 'LTS' AND jda.E2_AddressOverride = 1) > 0),
CountOfMLTInstructions = (SELECT COUNT(*) FROM dbo.DtbConsignmentAddress dca WHERE dca.LTS_InstructionType = 'MLT' AND dca.LTS_LTC_Consignment = dc.LTC_PK),
PCity=IIF(e2p.E2_AddressOverride=1 OR e2p.E2_OA_Address IS NULL,e2p.E2_City,oap.OA_City),
PState=IIF(e2p.E2_AddressOverride=1 OR e2p.E2_OA_Address IS NULL,e2p.E2_State,oap.OA_State),
PPostcode=IIF(e2p.E2_AddressOverride=1 OR e2p.E2_OA_Address IS NULL,e2p.E2_Postcode,oap.OA_PostCode),
PCountry=IIF(e2p.E2_AddressOverride=1 OR e2p.E2_OA_Address IS NULL,e2p.E2_RN_NKCountryCode,oap.OA_RN_NKCountryCode),
PGeoloc=IIF(e2p.E2_AddressOverride=1 OR e2p.E2_OA_Address IS NULL,e2p.E2_GeoLocation,oap.OA_GeoLocation).ToString(),
AnyPicAddressIsDepot = CASE WHEN (SELECT COUNT(*) FROM dbo.DtbConsignmentAddress dca WHERE dca.LTS_InstructionType = 'PIC' AND dca.LTS_LTC_Consignment = dc.LTC_PK AND (SELECT COUNT(*) FROM dbo.JobDocAddress jda INNER JOIN dbo.OrgAddress oa ON oa.OA_PK = jda.E2_OA_Address INNER JOIN dbo.OrgHeader oh ON oh.OH_PK = oa.OA_OH WHERE jda.E2_ParentID = dca.LTS_PK AND jda.E2_ParentTableCode = 'LTS' AND oh.OH_IsMiscFreightServices = 1 AND (oh.OH_IsUnpackDepot = 1 OR oh.OH_IsPackDepot = 1 OR oh.OH_IsRoadFreightDepot = 1 OR oh.OH_IsRailHead = 1)) > 0) > 0 THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END,
AnyDlvAddressIsDepot = CASE WHEN (SELECT COUNT(*) FROM dbo.DtbConsignmentAddress dca WHERE dca.LTS_InstructionType = 'DLV' AND dca.LTS_LTC_Consignment = dc.LTC_PK AND (SELECT COUNT(*) FROM dbo.JobDocAddress jda INNER JOIN dbo.OrgAddress oa ON oa.OA_PK = jda.E2_OA_Address INNER JOIN dbo.OrgHeader oh ON oh.OH_PK = oa.OA_OH WHERE jda.E2_ParentID = dca.LTS_PK AND jda.E2_ParentTableCode = 'LTS' AND oh.OH_IsMiscFreightServices = 1 AND (oh.OH_IsUnpackDepot = 1 OR oh.OH_IsPackDepot = 1 OR oh.OH_IsRoadFreightDepot = 1 OR oh.OH_IsRailHead = 1)) > 0) > 0 THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END,
DCity=IIF(e2d.E2_AddressOverride=1 OR e2d.E2_OA_Address IS NULL,e2d.E2_City,oad.OA_City),
DState=IIF(e2d.E2_AddressOverride=1 OR e2d.E2_OA_Address IS NULL,e2d.E2_State,oad.OA_State),
DPostcode=IIF(e2d.E2_AddressOverride=1 OR e2d.E2_OA_Address IS NULL,e2d.E2_Postcode,oad.OA_Postcode),
DCountry=IIF(e2d.E2_AddressOverride=1 OR e2d.E2_OA_Address IS NULL,e2d.E2_RN_NKCountryCode,oad.OA_RN_NKCountryCode),
DGeoloc=IIF(e2d.E2_AddressOverride=1 OR e2d.E2_OA_Address IS NULL,e2d.E2_GeoLocation,oad.OA_GeoLocation).ToString(),
CountOfServices = (SELECT COUNT(*) FROM dbo.JobService js WHERE js.es_ParentID = dc.LTC_PK AND js.ES_ParentTableCode = 'LTC'),
CountOfAdditionalAddresses = (SELECT COUNT(*) FROM dbo.JobDocAddress jda WHERE jda.E2_ParentID = dc.LTC_PK AND jda.E2_ParentTableCode = 'LTC' AND jda.E2_AddressType NOT IN ('CRB','BKD')),
HasHazardousPackage = (CASE WHEN (SELECT COUNT(*) FROM dbo.UNDGDataItem ui 
						INNER JOIN dbo.PkgPackage pp ON pp.KP_PK = ui.DI_ParentID AND ui.DI_ParentTableCode = 'KP'
						INNER JOIN dbo.PkgPackageJob ppj ON ppj.KJ_PK = pp.KP_KJ_ParentPackageJob
						WHERE ppj.KJ_ParentID = dc.LTC_PK AND ppj.KJ_ParentTableCode = 'LTC') > 0 THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END),
HasRefrigeratedPackage = (CASE WHEN (SELECT COUNT(*) FROM dbo.PkgPackage pp
						INNER JOIN dbo.PkgPackageJob ppj ON ppj.KJ_PK = pp.KP_KJ_ParentPackageJob
						WHERE ppj.KJ_ParentID = dc.LTC_PK AND ppj.KJ_ParentTableCode = 'LTC' AND pp.KP_RequiresTemperatureControl = 1) > 0 THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END),
TotalPacksQty = (SELECT SUM(pp.KP_PackageQty) FROM dbo.PkgPackage pp
						INNER JOIN dbo.PkgPackageJob ppj ON ppj.KJ_PK = pp.KP_KJ_ParentPackageJob
						WHERE ppj.KJ_ParentID = dc.LTC_PK AND ppj.KJ_ParentTableCode = 'LTC'),
ContainerCount = (SELECT COUNT(*) FROM dbo.PkgPackage pp
						INNER JOIN dbo.PkgPackageJob ppj ON ppj.KJ_PK = pp.KP_KJ_ParentPackageJob
						WHERE ppj.KJ_ParentID = dc.LTC_PK AND ppj.KJ_ParentTableCode = 'LTC' AND pp.KP_F3_NKPackType = 'CNT'),
PackLineCount = (SELECT COUNT(*) FROM dbo.PkgPackage pp
						INNER JOIN dbo.PkgPackageJob ppj ON ppj.KJ_PK = pp.KP_KJ_ParentPackageJob
						WHERE ppj.KJ_ParentID = dc.LTC_PK AND ppj.KJ_ParentTableCode = 'LTC'),
TotalPacksHavingPackageID = (SELECT COUNT(*) FROM dbo.PkgPackage pp
						INNER JOIN dbo.PkgPackageJob ppj ON ppj.KJ_PK = pp.KP_KJ_ParentPackageJob
						WHERE ppj.KJ_ParentID = dc.LTC_PK AND ppj.KJ_ParentTableCode = 'LTC' AND pp.KP_KPH_PackageHeader IS NOT NULL),
TotalPacksWeightKG = (SELECT SUM(ConvertedWeight.Value) FROM dbo.PkgPackage pp
						INNER JOIN dbo.PkgPackageJob ppj ON ppj.KJ_PK = pp.KP_KJ_ParentPackageJob
						CROSS APPLY dbo.ConvertWeight(pp.KP_Weight, pp.KP_WeightUQ, 'KG') ConvertedWeight
						WHERE ppj.KJ_ParentID = dc.LTC_PK AND ppj.KJ_ParentTableCode = 'LTC'),
TotalPacksVolumeM3 = (SELECT SUM(ConvertedVolume.Value) FROM dbo.PkgPackage pp
						INNER JOIN dbo.PkgPackageJob ppj ON ppj.KJ_PK = pp.KP_KJ_ParentPackageJob
						CROSS APPLY dbo.ConvertVolume(pp.KP_Volume, pp.KP_VolumeUQ, 'M3') ConvertedVolume
						WHERE ppj.KJ_ParentID = dc.LTC_PK AND ppj.KJ_ParentTableCode = 'LTC'),
TotalRejectedPIC = (SELECT COUNT(*) FROM dbo.DtbConsignmentAction ca 
					LEFT JOIN dbo.DtbConsignmentAddress cd on ca.LTA_LTS_ConsignmentAddress = cd.LTS_PK
					LEFT JOIN dbo.DtbConsignmentRunSheetInstruction k1 on k1.K1_PK = ca.LTA_K1_RunSheetInstruction
					WHERE cd.LTS_LTC_Consignment = dc.LTC_PK AND ca.LTA_ActionType = 'PIC' AND (ca.LTA_FailureReason > '' OR k1.K1_FailureReason > '')),
TotalRejectedDLV = (SELECT COUNT(*) FROM dbo.DtbConsignmentAction ca 
					LEFT JOIN dbo.DtbConsignmentAddress cd on ca.LTA_LTS_ConsignmentAddress = cd.LTS_PK
					LEFT JOIN dbo.DtbConsignmentRunSheetInstruction k1 on k1.K1_PK = ca.LTA_K1_RunSheetInstruction
					WHERE cd.LTS_LTC_Consignment = dc.LTC_PK AND ca.LTA_ActionType = 'DLV' AND (ca.LTA_FailureReason > '' OR k1.K1_FailureReason > '')),
IfAnyActionPackageDivot = CASE WHEN ISNULL((SELECT COUNT(*) FROM dbo.DtbConsignmentActionPackageDivot ltp
										INNER JOIN dbo.DtbConsignmentAction lta on lta.LTA_PK = ltp.LTP_LTA_ConsignmentAction
										INNER JOIN dbo.DtbConsignmentAddress lts on lts.LTS_PK = lta.LTA_LTS_ConsignmentAddress
										INNER JOIN dbo.DtbConsignment ltc on ltc.LTC_PK =  lts.LTS_LTC_Consignment
										WHERE ltc.LTC_PK = dc.LTC_PK),0) > 0 THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END,
IfRoutingOverridden = CASE WHEN dc.LTC_IsRouteOverridden = 1 THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END
FROM dbo.DtbConsignment dc 
LEFT JOIN dbo.GlbBranch AS b ON b.GB_PK = dc.LTC_GB_Branch
LEFT JOIN dbo.GlbCompany AS c ON b.GB_GC = c.GC_PK 
CROSS APPLY (SELECT TOP 1 LTS_LTC_Consignment, LTS_PK FROM dbo.DtbConsignmentAddress WHERE LTS_InstructionType = 'PIC' AND dc.LTC_PK = LTS_LTC_Consignment ORDER BY LTS_SystemCreateTimeUtc ASC) dc_pic
CROSS APPLY (SELECT TOP 1 LTS_LTC_Consignment, LTS_PK FROM dbo.DtbConsignmentAddress WHERE LTS_InstructionType = 'DLV' AND dc.LTC_PK = LTS_LTC_Consignment ORDER BY LTS_SystemCreateTimeUtc DESC) dc_dlv
LEFT JOIN dbo.JobDocAddress e2p on e2p.E2_ParentID = dc_pic.LTS_PK AND e2p.E2_ParentTableCode = 'LTS'
LEFT JOIN dbo.JobDocAddress e2d on e2d.E2_ParentID = dc_dlv.LTS_PK AND e2d.E2_ParentTableCode = 'LTS'
LEFT JOIN dbo.OrgAddress oap on oap.OA_PK = e2p.E2_OA_Address
LEFT JOIN dbo.OrgAddress oad on oad.OA_PK = e2d.E2_OA_Address
WHERE dc.LTC_Status = 'CMP'
)";
		public override string FromClause => @"CompletedConsignmentCTE";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(SELECT
JobType,HasTB,StaffCode,BillingLinesAttached,CountOfPICInstructions,CountOfPICInstructionsLinkedToMiscellaneousAddress,CountOfDLVInstructions,CountOfDLVInstructionsLinkedToMiscellaneousAddress,CountOfMLTInstructions,
PCity,PState,PPostcode,PCountry,PGeoloc,AnyPicAddressIsDepot,AnyDlvAddressIsDepot,DCity,DState,DPostcode,DCountry,DGeoloc,CountOfServices,CountOfAdditionalAddresses,HasHazardousPackage,HasRefrigeratedPackage,TotalPacksQty,
ContainerCount,PackLineCount,TotalPacksHavingPackageID,TotalPacksWeightKG,TotalPacksVolumeM3,TotalRejectedPIC,TotalRejectedDLV,IfAnyActionPackageDivot,IfRoutingOverridden
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string ActiveOn => "ALL";

		public override string WhereClause => string.Empty;

		#endregion // SuppressResourceStringsCheckRegion
	}
}
