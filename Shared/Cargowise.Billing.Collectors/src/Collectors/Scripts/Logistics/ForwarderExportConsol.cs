namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class ForwarderExportConsol : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "BKN";
		public override string RoleName => "Forwarding Consolidation";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "NVOCC Booking per Consol";
		public override string FeatureName => "NVOCC Booking per Consol";
		public override string DataGranularity => "MAH";
		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "COALESCE(BranchFromSI.GB_Code, BranchFromSF.GB_Code)";
		public override string GuidReference => "jk.JK_PK";
		public override string TransactionDateUtc => "COALESCE(si.SL_PostedTimeUtc, jw.TransactionDate)";
		public override string BillingReference1 => "jk.JK_UniqueConsignRef";
		public override string BillingReference2 => "IIF(jk.JK_AgentType = 'CLD', jk.JK_CoLoadBookingReference, jk.JK_BookingReference)";
		public override string BillingReference3 => "IIF(jk.JK_AgentType = 'CLD', jk.JK_CoLoadMasterBill, jk.JK_MasterBillNum)";
		public override string BillingReference4 => "'NVOCC'";
		public override string TransactionCount => "1";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (SELECT ConsolType = jk.JK_AgentType,
	ConsolCreateDate = jk.JK_SystemCreateTimeUtc,
	EventType = IIF(si.JK_PK IS NOT NULL, 'MSN', 'DEP'),
	TransportMode = jk.JK_TransportMode,
	ContainerMode = jk.JK_ConsolMode,
	FirstLoadPort = jk.JK_RL_NKLoadPort,
	LastDiscPort = jk.JK_RL_NKDischargePort,
	CarrierBookingReference = jk.JK_BookingReference,
	CoLoadBookingConfirmationReference = jk.JK_CoLoadBookingReference,
	CoLoadMasterBillNumber = jk.JK_CoLoadMasterBill,
	WayBillNumber = jk.JK_MasterBillNum,
	'CoLoadWith.OrgCode' = Creditor.OH_Code,
	'CoLoadWith.CarrierCode' = COALESCE(CreditorCCC.OK_CustomsRegNo, CreditorC1C.OK_CustomsRegNo),
    'ShippingLine.OrgCode' = ShippingLine.OH_Code,
    'ShippingLine.CarrierCode' = COALESCE(ShippingLineCCC.OK_CustomsRegNo, ShippingLineC1C.OK_CustomsRegNo),
	IsNVO = 1
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";

		public override string PreparationScript => @"WITH FirstSeaTransports AS (
	SELECT
		JW_ParentGUID = DEPWithRowNo.JW_ParentGUID,
		TransactionDate = DEPWithRowNo.SL_PostedTimeUtc,
		EventTime = DEPWithRowNo.SL_EventTime
	FROM
	(
		SELECT
			JW_ParentGUID,
			SL_PostedTimeUtc,
			SL_EventTime,
			ROW_NUMBER() OVER (PARTITION BY JW_ParentGUID ORDER BY SL_PostedTimeUtc) AS RowNo
		FROM
			dbo.JobConsolTransport
		JOIN dbo.StmAlog on SL_Parent = JW_PK
		WHERE
			JW_TransportMode = 'SEA'
			AND JW_ParentType = 'CON'
			AND SL_PostedTimeUtc >= DATEADD(MONTH, -3, @StartDateTimeInclusive)
			AND SL_PostedTimeUtc < @EndDateTimeExclusive
			AND SL_SE_NKEvent = 'DEP'
			AND SL_IsEstimate = 'N'
	) AS DEPWithRowNo
	WHERE DEPWithRowNo.RowNo = 1
),
FirstShippingInstructionMSNEvents AS (
	SELECT
		JDD_ParentID AS JK_PK,
		MIN(SL_PostedTimeUtc) AS SL_PostedTimeUtc,
		MIN(SL_GB_NKBranch) AS STL_BranchCode
	FROM
		dbo.StmALog
	JOIN dbo.JobDocumentData ON JDD_PK = SL_Parent
	WHERE
		JDD_ParentTableCode = 'JK'
		AND JDD_Name = 'SeaBookingRequest2'
		AND SL_PostedTimeUtc >= DATEADD(MONTH, -3, @StartDateTimeInclusive)
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND SL_SE_NKEvent = 'MSN'
		AND SL_IsEstimate = 'N'
		AND SL_IsCancelled = 'N'
		AND SL_Reference LIKE '%MST=Shipping Instruction%'
		AND SL_Reference LIKE '%DEP=Carrier%'
		AND SL_Reference NOT LIKE '%RES=%'
		AND SL_Table = 'JobDocumentData'
	GROUP BY JDD_ParentID
),
ExportConsols AS (
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
		JK_OA_SendingForwarderAddress,
        JK_IsForwarding,
		JK_SystemCreateTimeUtc
    FROM
        dbo.JobConsol
    WHERE
        JK_IsForwarding = 1
        AND JK_IsCancelled = 0
)";

		public override string FromClause => @"ExportConsols AS jk
LEFT JOIN FirstShippingInstructionMSNEvents si ON si.JK_PK = jk.JK_PK
LEFT JOIN FirstSeaTransports jw ON jw.JW_ParentGUID = jk.JK_PK AND si.JK_PK IS NULL
LEFT JOIN dbo.OrgAddress SendingForwarderAddr ON SendingForwarderAddr.OA_PK = JK_OA_SendingForwarderAddress
LEFT JOIN dbo.OrgHeader SendingForwarder ON SendingForwarder.OH_PK = SendingForwarderAddr.OA_OH
LEFT JOIN dbo.OrgAddress ShippingLineAddr ON ShippingLineAddr.OA_PK = jk.JK_OA_ShippingLineAddress
LEFT JOIN dbo.OrgHeader ShippingLine ON ShippingLine.OH_PK = ShippingLineAddr.OA_OH
LEFT JOIN dbo.OrgCusCode ShippingLineCCC ON ShippingLineCCC.OK_OH = ShippingLine.OH_PK AND ShippingLineCCC.OK_CodeType = 'CCC' AND ShippingLineCCC.OK_RN_NKCodeCountry = 'US'
LEFT JOIN dbo.OrgCusCode ShippingLineC1C ON ShippingLineC1C.OK_OH = ShippingLine.OH_PK AND ShippingLineC1C.OK_CodeType = 'C1C'
LEFT JOIN dbo.OrgAddress CreditorAddr ON CreditorAddr.OA_PK = jk.JK_OA_CreditorAddress
LEFT JOIN dbo.OrgHeader Creditor ON  Creditor.OH_PK = CreditorAddr.OA_OH
LEFT JOIN dbo.OrgCusCode CreditorCCC ON CreditorCCC.OK_OH = Creditor.OH_PK AND CreditorCCC.OK_CodeType = 'CCC' AND CreditorCCC.OK_RN_NKCodeCountry = 'US'
LEFT JOIN dbo.OrgCusCode CreditorC1C ON CreditorC1C.OK_OH = Creditor.OH_PK AND CreditorC1C.OK_CodeType = 'C1C'
LEFT JOIN dbo.RefShippingLine rslShippingLine ON rslShippingLine.RSL_PK = ShippingLine.OH_RSL_ShippingLine OR (ShippingLine.OH_RSL_ShippingLine IS NULL AND rslShippingLine.RSL_StandardCarrierAlphaCode = ShippingLineCCC.OK_CustomsRegNo)
LEFT JOIN dbo.RefShippingLine rslCoLoadWith ON rslCoLoadWith.RSL_PK = Creditor.OH_RSL_ShippingLine OR (Creditor.OH_RSL_ShippingLine IS NULL AND rslCoLoadWith.RSL_StandardCarrierAlphaCode = CreditorCCC.OK_CustomsRegNo)
LEFT JOIN dbo.GlbBranch BranchFromSI ON BranchFromSI.GB_Code = si.STL_BranchCode
LEFT JOIN dbo.GlbBranch BranchFromSF ON BranchFromSF.GB_PK =
(
	SELECT TOP 1 GB_PK
	FROM dbo.GlbBranch
	WHERE
		GB_IsActive = 1
		AND	GB_OH_OrgProxy = SendingForwarder.OH_PK
)
JOIN dbo.GlbCompany Company ON Company.GC_PK = COALESCE(BranchFromSI.GB_GC, BranchFromSF.GB_GC,
(
	SELECT TOP 1 GC_PK
	FROM dbo.GlbCompany
	WHERE
		GC_IsActive = 1
		AND GC_OH_OrgProxy = SendingForwarder.OH_PK
))";

		public override string WhereClause => @"(
	si.JK_PK IS NOT NULL OR
	(
		jw.EventTime >= DATEADD(HOUR, -24, DATEADD(MONTH, -1, @StartDateTimeInclusive))
		AND jw.EventTime < DATEADD(HOUR, 24, @EndDateTimeExclusive)
	)
)
AND Company.GC_IsActive = 1
AND COALESCE(BranchFromSI.GB_IsActive, BranchFromSF.GB_IsActive) = 1
AND jk.JK_IsForwarding = 1
AND
(
	(
		jk.JK_AgentType = 'CLD'
		AND rslCoLoadWith.RSL_IsNVO = 1
		AND (rslCoLoadWith.RSL_BookingRequestAvailable = 1 OR rslCoLoadWith.RSL_ShippingInstructionAvailable = 1)
	)
	OR
	(
		jk.JK_AgentType != 'CLD'
		AND rslShippingLine.RSL_IsNVO = 1
		AND
		(
			rslShippingLine.RSL_IsShippingLine = 0
			OR jk.JK_ConsolMode = 'LCL'
		)
		AND (rslShippingLine.RSL_BookingRequestAvailable = 1 OR rslShippingLine.RSL_ShippingInstructionAvailable = 1)
	)
)";
		public override bool UsedInBilling => false;
		public override string ActiveOn => "ALL";
		public override string MinCW1Version => "23.9.29.11";
	}

	#endregion
}
