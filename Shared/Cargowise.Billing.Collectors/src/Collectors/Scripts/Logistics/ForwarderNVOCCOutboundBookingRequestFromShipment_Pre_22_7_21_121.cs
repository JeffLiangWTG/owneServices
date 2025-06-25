namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class ForwarderNVOCCOutboundBookingRequestFromShipment_Pre_22_7_21_121 : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "NBS";
		public override string RoleName => "Forwarding Consolidation";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "Shipment NVOCC Booking Request sent";
		public override string FeatureName => "Shipment NVOCC Booking Request sent";
		public override string DataGranularity => "MAH";
		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string TransactionDateUtc => "bk.SL_PostedTimeUtc";
		public override string GuidReference => "js.JS_PK";
		public override string BillingReference1 => "js.JS_UniqueConsignRef";
		public override string BillingReference2 => "ce.CE_EntryNum";
		public override string BillingReference3 => "js.JS_HouseBill";
		public override string BillingReference4 => "'Booking Request'";
		public override string TransactionCount => "1";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(
	SELECT
		ShipmentNumber = js.JS_UniqueConsignRef,
		ShipmentType = js.JS_ShipmentType,
		TransportMode = js.JS_TransportMode,
		ContainerMode = js.JS_PackingMode,
		Origin = js.JS_RL_NKOrigin,
		Destination = js.JS_RL_NKDestination,
		ReferenceNumber = ISNULL(ce.CE_EntryNum, ''),
		HouseBillNumber = js.JS_HouseBill,
		'Carrier.OrgCode' = PlannedCarrier.OH_Code,
		'Carrier.C1CCode' = COALESCE(PlannedCarrierCCC.OK_CustomsRegNo, PlannedCarrierC1C.OK_CustomsRegNo),
		IsNVO = 1
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)))";

		public override string PreparationScript => @"WITH ExportSeaShipments AS
(
	SELECT
        JS_PK,
		JS_UniqueConsignRef,
        JS_HouseBill,
        JS_ShipmentType,
        JS_TransportMode,
        JS_PackingMode,
        JS_RL_NKOrigin,
        JS_RL_NKDestination,
        JS_OA_BookedShippingLineAddress
	FROM
		dbo.JobShipment
    WHERE
        JS_IsForwardRegistered = 1
        AND JS_IsCancelled = 0
		AND JS_TransportMode = 'SEA'
),
BookingRequestSent AS
(
	SELECT
		ISNLog.SL_Parent AS JS_PK,
		ISNLog.SL_PostedTimeUtc AS SL_PostedTimeUtc,
		(
			SELECT TOP 1
				MSNLog.SL_GB_NKBranch
			FROM
				dbo.StmALog MSNLog
			WHERE
				SL_Parent = ISNLog.SL_Parent
				AND SL_PostedTimeUtc >= DATEADD(MONTH, -1, ISNLog.SL_PostedTimeUtc)
				AND SL_PostedTimeUtc < ISNLog.SL_PostedTimeUtc
				AND SL_SE_NKEvent = 'MSN'
				AND SL_IsEstimate = 'N'
				AND SL_Reference LIKE '%MST=Booking Request%'
			ORDER BY SL_PostedTimeUtc DESC
		) BranchCode
	FROM
		dbo.StmALog ISNLog
	WHERE
		SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND SL_SE_NKEvent = 'ISN'
		AND SL_IsEstimate = 'N'
		AND SL_Reference LIKE '%MST=Booking Request%'
		AND SL_Table = 'JobShipment'
)";

		public override string FromClause => @"ExportSeaShipments js
JOIN BookingRequestSent bk ON js.JS_PK = bk.JS_PK
JOIN dbo.GlbBranch Branch on bk.BranchCode = Branch.GB_Code
JOIN dbo.GlbCompany Company on Branch.GB_GC = Company.GC_PK
LEFT JOIN dbo.CusEntryNum ce ON ce.CE_PK =
(
	SELECT TOP 1 CE_PK
	FROM dbo.CusEntryNum
	WHERE
		CE_ParentID = js.JS_PK
		AND CE_ParentTable = 'JobShipment'
		AND CE_EntryType = 'BKG'
		AND CE_IsValid = 1
)
LEFT JOIN dbo.OrgAddress PlannedCarrierAddr ON PlannedCarrierAddr.OA_PK = js.JS_OA_BookedShippingLineAddress
LEFT JOIN dbo.OrgHeader PlannedCarrier ON PlannedCarrier.OH_PK = PlannedCarrierAddr.OA_OH
LEFT JOIN dbo.OrgCusCode PlannedCarrierCCC ON PlannedCarrierCCC.OK_OH = PlannedCarrier.OH_PK AND PlannedCarrierCCC.OK_CodeType = 'CCC' AND PlannedCarrierCCC.OK_RN_NKCodeCountry = 'US'
LEFT JOIN dbo.OrgCusCode PlannedCarrierC1C ON PlannedCarrierC1C.OK_OH = PlannedCarrier.OH_PK AND PlannedCarrierC1C.OK_CodeType = 'C1C'
LEFT JOIN dbo.RefShippingLine rslShippingLine ON PlannedCarrier.OH_RSL_ShippingLine = rslShippingLine.RSL_PK";

		public override string WhereClause => @"PlannedCarrier.OH_IsSeaWholesaler = 1  AND rslShippingLine.RSL_IsNVO = 1  AND rslShippingLine.RSL_OceanCarrierMessagingAvailable = 1";
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string MaxCW1Version => "22.7.21.120";
	}

	#endregion
}
