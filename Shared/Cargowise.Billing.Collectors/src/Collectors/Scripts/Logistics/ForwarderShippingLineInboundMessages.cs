namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class ForwarderShippingLineInboundMessages : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "SVS";
		public override string RoleName => "Forwarding Consolidation";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "Shipping Line Responses";
		public override string FeatureName => "Shipping Line Responses";
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "cc.JK_PK";
		public override string TransactionDateUtc => "im.SL_PostedTimeUtc";
		public override string BillingReference1 => "cc.JK_UniqueConsignRef";
		public override string BillingReference2 => "cc.JK_BookingReference";
		public override string BillingReference3 => "cc.JK_MasterBillNum";
		public override string BillingReference4 => "im.EventType + '/' + im.SL_SE_NKEvent";
		public override string TransactionCount => "1";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (SELECT ConsolNumber = cc.JK_UniqueConsignRef,
	ConsolType = cc.JK_AgentType,
	TransportMode = cc.JK_TransportMode,
	ContainerMode = cc.JK_ConsolMode,
	FirstLoadPort = cc.JK_RL_NKLoadPort,
	LastDiscPort = cc.JK_RL_NKDischargePort,
	CarrierBookingReference = cc.JK_BookingReference,
	CoLoadBookingConfirmationReference = cc.JK_CoLoadBookingReference,
	WayBillNumber = cc.JK_MasterBillNum,
	'ShippingLine.OrgCode' = ShippingLine.OH_Code,
	'ShippingLine.C1CCode' = COALESCE(ShippingLineCCC.OK_CustomsRegNo, ShippingLineC1C.OK_CustomsRegNo),
	IsNVO = 0
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";

		public override string PreparationScript => @"WITH ExportSeaConsols AS
(
	SELECT
        JK_PK,
		JK_UniqueConsignRef,
        JK_BookingReference,
        JK_MasterBillNum,
        JK_AgentType,
        JK_TransportMode,
        JK_ConsolMode,
        JK_RL_NKLoadPort,
        JK_RL_NKDischargePort,
        JK_CoLoadBookingReference,
        JK_CoLoadMasterBill,
        JK_OA_ShippingLineAddress,
        JK_OA_CreditorAddress
	FROM
		dbo.JobConsol
    WHERE
        JK_IsForwarding = 1
        AND JK_IsCancelled = 0
		AND JK_TransportMode = 'SEA'
),
InboundMessages AS
(
	SELECT
		InboundLog.SL_Parent AS JK_PK,
		InboundLog.SL_PostedTimeUtc,
		InboundLog.SL_SE_NKEvent,
		(
			SELECT CASE
				WHEN SL_Reference LIKE '%MST=Booking Request%' THEN 'Booking Request'
				WHEN SL_Reference LIKE '%MST=Shipping Instruction%' THEN 'Shipping Instruction'
				WHEN SL_Reference LIKE '%MST=Shipping Order%' THEN 'Shipping Order'
			END
		) EventType,
		(
			SELECT TOP 1
				MSNLog.SL_GB_NKBranch
			FROM
				dbo.StmALog MSNLog
			WHERE
				SL_Parent = InboundLog.SL_Parent
				AND SL_PostedTimeUtc >= DATEADD(MONTH, -1, InboundLog.SL_PostedTimeUtc)
				AND SL_PostedTimeUtc < InboundLog.SL_PostedTimeUtc
				AND SL_SE_NKEvent = 'MSN'
				AND SL_IsEstimate = 'N'
				AND (SL_Reference LIKE '%MST=Booking Request%' OR SL_Reference LIKE '%MST=Shipping Instruction%' OR SL_Reference LIKE '%MST=Shipping Order%')
			ORDER BY SL_PostedTimeUtc DESC
		) STL_BranchCode
	FROM
		dbo.StmALog InboundLog
	WHERE
		SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND SL_SE_NKEvent IN ('MAA', 'MRJ', 'MWA')
		AND SL_IsEstimate = 'N'
		AND (SL_Reference LIKE '%MST=Booking Request%' OR SL_Reference LIKE '%MST=Shipping Instruction%' OR SL_Reference LIKE '%MST=Shipping Order%')
		AND SL_Table = 'JobConsol'
)";

		public override string FromClause => @"ExportSeaConsols cc
JOIN InboundMessages im ON cc.JK_PK = im.JK_PK
JOIN dbo.GlbBranch Branch on im.STL_BranchCode = Branch.GB_Code
JOIN dbo.GlbCompany Company on Branch.GB_GC = Company.GC_PK
LEFT JOIN dbo.OrgAddress AS ShippingLineAddr ON ShippingLineAddr.OA_PK = cc.JK_OA_ShippingLineAddress
LEFT JOIN dbo.OrgHeader  AS ShippingLine     ON ShippingLine.OH_PK = ShippingLineAddr.OA_OH
LEFT JOIN dbo.OrgCusCode ShippingLineCCC  ON ShippingLineCCC.OK_OH = ShippingLine.OH_PK AND ShippingLineCCC.OK_CodeType = 'CCC' AND ShippingLineCCC.OK_RN_NKCodeCountry = 'US'
LEFT JOIN dbo.OrgCusCode ShippingLineC1C  ON ShippingLineC1C.OK_OH = ShippingLine.OH_PK AND ShippingLineC1C.OK_CodeType = 'C1C'
LEFT JOIN dbo.RefShippingLine AS rsl ON ShippingLine.OH_RSL_ShippingLine = RSL_PK";

		public override string WhereClause => @"cc.JK_AgentType != 'CLD'
AND rsl.RSL_IsShippingLine = 1
AND
(
	rsl.RSL_IsNVO = 0
	OR cc.JK_ConsolMode != 'LCL'
)
AND ((im.EventType = 'Shipping Instruction' AND rsl.RSL_ShippingInstructionAvailable = 1) OR (im.EventType = 'Booking Request' AND rsl.RSL_BookingRequestAvailable = 1) OR (im.EventType = 'Shipping Order' AND rsl.RSL_ShippingOrderAvailable = 1))";
		public override bool UsedInBilling => false;
		public override string ActiveOn => "ALL";
		public override string MinCW1Version => "23.9.29.11";
	}

	#endregion
}
