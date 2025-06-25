namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class HVLVItemPlusUsageLegacy : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "HP2";
		public override string RoleName => "Ecommerce Plus";
		public override string ModuleName => "Ecommerce";
		public override string FunctionName => "Ecommerce Standard & Security Functions";
		public override string FeatureName => "Ecommerce Plus Legacy";
		public override string CompanyCode => "UsageTable.GC_Code";
		public override string BranchCode => "UsageTable.GB_Code";
		public override string TransactionDateUtc => "UsageTable.UsageTimeUtc";
		public override string BillingReference1 => "HVI_ItemId";
		public override string BillingReference2 => "HVI_CurrentBarcode";
		public override string BillingReference4 => "HVI_ShipperReference";
		public override string GuidReference => "HVI_PK";
		public override string PreparationScript => @"
/* The UsageTable returns a single transaction billing time of a HVLVItem                                                                                                           */
/* When HVI_DestinationFirstUsageTimeUtc is populated, we bill that GC (GlbCompany) for that item                                                                                   */
/* When HVI_SecurityFilingFirstUsageTimeUtc is populated, we bill that GC (GlbCompany) for that item                                                                                */
/* When a HVLVItem is edited by a company that did not create that HVLVItem. We bill them once for that item regardless of the number of times they edit it and when they edited it */
/* We select a list of HVLVItems that meet the above criteria, union them all. And that return list is the list of transactions that we use to bill                                 */
WITH UsageTable AS
(
	/* Select all 'P' HVLVItems where  HVI_DestinationFirstUsageTimeUtc IS NOT NULL                                 */
	SELECT
		HVI_PK,
		HVI_ItemId,
		HVI_CurrentBarcode,
		HVI_ShipperReference,
		UsageTimeUtc = HVI_DestinationFirstUsageTimeUtc,
		GB_Code = GlbBranch.GB_Code,
		GC_Code = GlbCompany.GC_Code
	FROM
		dbo.HVLVItem
		LEFT JOIN dbo.StmALog ON StmALog.SL_Parent = HVLVItem.HVI_PK
		LEFT JOIN dbo.GlbBranch ON GlbBranch.GB_Code = StmALog.SL_GB_NKBranch
		LEFT JOIN dbo.GlbCompany ON GlbCompany.GC_PK = GlbBranch.GB_GC
	WHERE
		HVI_DestinationFirstUsageTimeUtc IS NOT NULL
		AND HVI_DestinationFirstUsageTimeUtc >= @StartDateTimeInclusive
		AND HVI_DestinationFirstUsageTimeUtc < @EndDateTimeExclusive
		AND SL_SE_NKEvent = 'ADD' /* This filters out EDT so duplicates are not picked twice last SELECT query      */
		AND HVI_UsageType = 'P'

	UNION

	/* Select all 'S' HVLVItems where HVI_SecurityFilingFirstUsageTimeUtc IS NOT NULL                               */
	SELECT
		HVI_PK,
		HVI_ItemId,
		HVI_CurrentBarcode,
		HVI_ShipperReference,
		UsageTimeUtc = HVI_SecurityFilingFirstUsageTimeUtc,
		GB_Code = GlbBranch.GB_Code,
		GC_Code = GlbCompany.GC_Code
	FROM
		dbo.HVLVItem
		LEFT JOIN dbo.StmALog ON StmALog.SL_Parent = HVLVItem.HVI_PK
		LEFT JOIN dbo.GlbBranch ON GlbBranch.GB_Code = StmALog.SL_GB_NKBranch
		LEFT JOIN dbo.GlbCompany ON GlbCompany.GC_PK = GlbBranch.GB_GC
	WHERE
		HVI_SecurityFilingFirstUsageTimeUtc IS NOT NULL
		AND HVI_SecurityFilingFirstUsageTimeUtc >= @StartDateTimeInclusive
		AND HVI_SecurityFilingFirstUsageTimeUtc < @EndDateTimeExclusive
		AND SL_SE_NKEvent = 'ADD' /* This filters out EDT so duplicates are not picked twice last SELECT query      */
		AND HVI_UsageType = 'S'

	UNION

	/*  Select all HVLVItems                                                                                        */
	/*  That have been edited by a company																			*/
	/*  That did not create the shipment, that the HVLVItem is in                    								*/
	SELECT 
		HVLVItem.HVI_PK, 
		HVI_ItemId, 
		HVI_CurrentBarcode, 
		HVI_ShipperReference, 
		UsageTimeUtc = UniqueCompanyHVIEditLogs.SL_PostedTimeUtc,
		UniqueCompanyHVIEditLogs.GB_Code AS GB_Code,
		UniqueCompanyHVIEditLogs.GC_Code AS GC_Code
	FROM 
		dbo.HVLVItem
		JOIN
		(
			SELECT
				HVI_PK,
				ItemEditBranch.GB_Code,
				ItemEditCompany.GC_Code,
				Min(ItemEditLog.SL_PostedTimeUtc) AS SL_PostedTimeUtc
			FROM
				dbo.HVLVItem
				JOIN dbo.StmALog ItemEditLog ON ItemEditLog.SL_Parent = HVLVItem.HVI_PK
				JOIN dbo.StmALog ShipmentAddLog ON HVLVItem.HVI_JS_LoadedOnShipment = ShipmentAddLog.SL_Parent AND ShipmentAddLog.SL_SE_NKEvent = 'ADD'
				JOIN dbo.GlbBranch ItemEditBranch ON ItemEditBranch.GB_Code = ItemEditLog.SL_GB_NKBranch
				JOIN dbo.GlbCompany ItemEditCompany ON ItemEditCompany.GC_PK = ItemEditBranch.GB_GC
				JOIN dbo.GlbBranch ShipmentAddBranch ON ShipmentAddBranch.GB_Code = ShipmentAddLog.SL_GB_NKBranch
			WHERE
				ItemEditLog.SL_Table = 'HVLVItem'
				AND ItemEditLog.SL_SE_NKEvent = 'EDT'
				AND ItemEditBranch.GB_GC <> ShipmentAddBranch.GB_GC
				AND ItemEditLog.SL_PostedTimeUtc >= @StartDateTimeInclusive
				AND ItemEditLog.SL_PostedTimeUtc < @EndDateTimeExclusive
			GROUP BY
				HVI_PK,
				ItemEditBranch.GB_Code,
				ItemEditCompany.GC_Code
		) AS UniqueCompanyHVIEditLogs ON UniqueCompanyHVIEditLogs.HVI_PK = HVLVItem.HVI_PK
	WHERE
		HVI_DestinationFirstUsageTimeUtc IS NOT NULL
		AND HVI_UsageType = 'P'
		AND HVI_SystemLastEditTimeUtc >= @StartDateTimeInclusive
)
";
		public override string ActiveOn => "ALL";
		public override string FromClause => "UsageTable";
		public override string WhereClause => "";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.3.16.344";

		#endregion

	}
}
