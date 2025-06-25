namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class ForwarderShippingLineVGMOutboundMessages : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "VGS";
		public override string RoleName => "Forwarding Consolidation";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "Verified Gross Container Weight Sent";
		public override string FeatureName => "Verified Gross Container Weight Sent";
		public override string DataGranularity => "MAH";
		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "cc.JK_PK";
		public override string TransactionDateUtc => "om.SL_PostedTimeUtc";
		public override string BillingReference1 => "cc.JK_UniqueConsignRef";
		public override string BillingReference2 => "cc.JK_BookingReference";
		public override string BillingReference3 => "cc.ContainerNum";
		public override string BillingReference4 => "'Verified Gross Container Weight'";
		public override string TransactionCount => "1";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (SELECT ConsolNumber = cc.JK_UniqueConsignRef,
	ContainerNumber = cc.ContainerNum,
	ConsolType = cc.JK_AgentType,
	TransportMode = cc.JK_TransportMode,
	ContainerMode = cc.JK_ConsolMode,
	FirstLoadPort = cc.JK_RL_NKLoadPort,
	LastDiscPort = cc.JK_RL_NKDischargePort,
	CarrierBookingReference = cc.JK_BookingReference,
	CoLoadBookingConfirmationReference = cc.JK_CoLoadBookingReference,
	WayBillNumber = cc.JK_MasterBillNum,
	CoLoadMasterBillNumber = cc.JK_CoLoadMasterBill,
	'CoLoadWith.OrgCode' = Creditor.OH_Code,
	'CoLoadWith.C1CCode' = COALESCE(CreditorCCC.OK_CustomsRegNo, CreditorC1C.OK_CustomsRegNo),
	'ShippingLine.OrgCode' = ShippingLine.OH_Code,
	'ShippingLine.C1CCode' = COALESCE(ShippingLineCCC.OK_CustomsRegNo, ShippingLineC1C.OK_CustomsRegNo),
	IsNVO = 0
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";

		public override string PreparationScript => @"WITH ExportSeaConsolContainers AS
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
        JK_OA_CreditorAddress,
		ContainerGuid = JC_PK,
		ContainerNum = JC_ContainerNum
	FROM
		dbo.JobConsol
	JOIN dbo.JobContainer ON JK_PK = JC_JK
    WHERE
        JK_IsForwarding = 1
        AND JK_IsCancelled = 0
		AND JK_TransportMode = 'SEA'
),
OutboundMessages AS
(
	SELECT
		OutboundLog.SL_Parent AS JC_PK,
		OutboundLog.SL_PostedTimeUtc,
		OutboundLog.SL_SE_NKEvent,
		(
			SELECT TOP 1
				MSNLog.SL_GB_NKBranch
			FROM
				dbo.StmALog MSNLog
			WHERE
				SL_Parent = OutboundLog.SL_Parent
				AND SL_PostedTimeUtc >= DATEADD(MONTH, -1, OutboundLog.SL_PostedTimeUtc)
				AND SL_PostedTimeUtc < OutboundLog.SL_PostedTimeUtc
				AND SL_SE_NKEvent = 'MSN'
				AND SL_IsEstimate = 'N'
				AND SL_Reference LIKE '%MST=Verified Gross Container Weight%'
			ORDER BY SL_PostedTimeUtc DESC
		) STL_BranchCode
	FROM
		dbo.StmALog OutboundLog
	WHERE
		SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND SL_SE_NKEvent = 'ISN'
		AND SL_IsEstimate = 'N'
		AND SL_Reference LIKE '%MST=Verified Gross Container Weight%'
		AND SL_Table = 'JobContainer'
)";

		public override string FromClause => @"ExportSeaConsolContainers cc
JOIN OutboundMessages om ON cc.ContainerGuid = om.JC_PK
JOIN dbo.GlbBranch Branch on om.STL_BranchCode = Branch.GB_Code
JOIN dbo.GlbCompany Company on Branch.GB_GC = Company.GC_PK
LEFT JOIN dbo.OrgAddress AS ShippingLineAddr ON ShippingLineAddr.OA_PK = cc.JK_OA_ShippingLineAddress
LEFT JOIN dbo.OrgHeader  AS ShippingLine     ON ShippingLine.OH_PK = ShippingLineAddr.OA_OH
LEFT JOIN dbo.OrgCusCode ShippingLineCCC  ON ShippingLineCCC.OK_OH = ShippingLine.OH_PK AND ShippingLineCCC.OK_CodeType = 'CCC' AND ShippingLineCCC.OK_RN_NKCodeCountry = 'US'
LEFT JOIN dbo.OrgCusCode ShippingLineC1C  ON ShippingLineC1C.OK_OH = ShippingLine.OH_PK AND ShippingLineC1C.OK_CodeType = 'C1C'
LEFT JOIN dbo.OrgAddress AS CreditorAddr ON CreditorAddr.OA_PK = cc.JK_OA_CreditorAddress
LEFT JOIN dbo.OrgHeader AS Creditor ON  Creditor.OH_PK = CreditorAddr.OA_OH
LEFT JOIN dbo.OrgCusCode CreditorCCC  ON CreditorCCC.OK_OH = Creditor.OH_PK AND CreditorCCC.OK_CodeType = 'CCC' AND CreditorCCC.OK_RN_NKCodeCountry = 'US'
LEFT JOIN dbo.OrgCusCode CreditorC1C  ON CreditorC1C.OK_OH = Creditor.OH_PK AND CreditorC1C.OK_CodeType = 'C1C'
LEFT JOIN dbo.RefShippingLine AS rsl ON ShippingLine.OH_RSL_ShippingLine = RSL_PK";

		public override string WhereClause => @"cc.JK_AgentType != 'CLD'
AND rsl.RSL_IsShippingLine = 1
AND
(
	rsl.RSL_IsNVO = 0
	OR cc.JK_ConsolMode != 'LCL'
)
AND rsl.RSL_VerifiedGrossContainerWeightAvailable = 1";
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string MinCW1Version => "23.9.29.11";
	}

	#endregion
}
