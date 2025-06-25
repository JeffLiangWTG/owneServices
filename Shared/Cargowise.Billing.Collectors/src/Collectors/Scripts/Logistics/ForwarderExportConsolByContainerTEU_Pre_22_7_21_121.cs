namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class ForwarderExportConsolByContainerTEU_Pre_22_7_21_121 : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "BKS";
		public override string RoleName => "Forwarding Consolidation/Container";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "Booking per TEU";
		public override string FeatureName => "Booking per TEU";
		public override string DataGranularity => "MAH";
		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "COALESCE(BranchFromSI.GB_Code, BranchFromSF.GB_Code)";
		public override string GuidReference => "jk.JK_PK";
		public override string TransactionDateUtc => "IIF(COALESCE(si.SL_PostedTimeUtc, @MaxDateTime) > COALESCE(jw.TransactionDate, @MaxDateTime), jw.TransactionDate, si.SL_PostedTimeUtc)";
		public override string BillingReference1 => "jk.JK_UniqueConsignRef";
		public override string BillingReference2 => "jk.JK_BookingReference";
		public override string BillingReference3 => "jk.JK_MasterBillNum";
		public override string BillingReference4 => "Containers.JC_ContainerNum + '/' + rc.RC_ISOType  + '/' + CONVERT(varchar(36), rc.CalculatedTEU) + 'TEU'";
		public override string TransactionCount => @"FLOOR(rc.CalculatedTEU)";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX),CONVERT(VARCHAR(MAX),(SELECT ConsolType=jk.JK_AgentType,ConsolCreateDate=jk.JK_SystemCreateTimeUtc,EventType=IIF(COALESCE(si.SL_PostedTimeUtc, @MaxDateTime) > COALESCE(jw.TransactionDate, @MaxDateTime), 'DEP', 'MSN'),TransportMode=jk.JK_TransportMode,ContainerMode=jk.JK_ConsolMode,FirstLoadPort=jk.JK_RL_NKLoadPort,LastDiscPort=jk.JK_RL_NKDischargePort,CoLoadBookingReference=jk.JK_CoLoadBookingReference,CoLoadMasterBillNumber=jk.JK_CoLoadMasterBill,'CoLoadWith.OrgCode'=Creditor.OH_Code,'CoLoadWith.CarrierCode'=COALESCE(CreditorCCC.OK_CustomsRegNo,CreditorC1C.OK_CustomsRegNo),'ShippingLine.OrgCode'=ShippingLine.OH_Code,'ShippingLine.CarrierCode'=COALESCE(ShippingLineCCC.OK_CustomsRegNo,ShippingLineC1C.OK_CustomsRegNo),IsNVO=CAST(rsl.RSL_IsNVO AS INT),'Container.ContainerNumber'=Containers.JC_ContainerNum,'Container.ContainerType'=RTRIM(rc.RC_Code),'Container.ISOCode'=rc.RC_ISOType,'Container.TEU'=rc.CalculatedTEU FOR JSON PATH,WITHOUT_ARRAY_WRAPPER)))";

		public override string PreparationScript => @"DECLARE @MaxDateTime datetime = '9999-12-31';
WITH ExportConsols AS (
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
),
FirstShippingInstructionMSNEvents AS (
	SELECT
		JDD_ParentID AS JK_PK,
		MIN(SL_PostedTimeUtc) AS SL_PostedTimeUtc,
		MIN(SL_GB_NKBranch) AS BranchCode
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
FirstSeaTransports AS (
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
RefContainersWithTEU AS
(
	SELECT
		RC_PK,
		RC_Code,
		RC_ISOType,
		CalculatedTEU = CASE
	WHEN RC_TEU IS NOT NULL AND RC_TEU > 0 THEN RC_TEU
	ELSE CONVERT(DECIMAL(5,2),
		CASE LEFT(RC_ISOType, 1)
		WHEN '1' THEN 10
		WHEN '2' THEN 20
		WHEN '3' THEN 30
		WHEN '4' THEN 40
		WHEN 'A' THEN 23.5
		WHEN 'B' THEN 24
		WHEN 'C' THEN 24.5
		WHEN 'D' THEN 24.5
		WHEN 'E' THEN 25.7
		WHEN 'F' THEN 26.6
		WHEN 'G' THEN 41
		WHEN 'H' THEN 43
		WHEN 'K' THEN 44.6
		WHEN 'L' THEN 45
		WHEN 'M' THEN 48
		WHEN 'N' THEN 49
		WHEN 'P' THEN 53
		ELSE 40 END
	/ 20) END
	FROM dbo.RefContainer
)";

		public override string FromClause => @"ExportConsols AS jk
LEFT JOIN FirstShippingInstructionMSNEvents si ON si.JK_PK = jk.JK_PK
LEFT JOIN FirstSeaTransports jw ON jw.JW_ParentGUID = jk.JK_PK
LEFT JOIN dbo.JobContainer Containers ON Containers.JC_JK = jk.JK_PK AND (si.SL_PostedTimeUtc IS NOT NULL OR jw.TransactionDate IS NOT NULL)
LEFT JOIN RefContainersWithTEU rc ON rc.RC_PK = Containers.JC_RC
LEFT JOIN dbo.OrgAddress SendingForwarderAddr ON SendingForwarderAddr.OA_PK = jk.JK_OA_SendingForwarderAddress
LEFT JOIN dbo.OrgHeader SendingForwarder ON SendingForwarder.OH_PK = SendingForwarderAddr.OA_OH
LEFT JOIN dbo.OrgAddress AS ShippingLineAddr ON ShippingLineAddr.OA_PK = jk.JK_OA_ShippingLineAddress
LEFT JOIN dbo.OrgHeader  AS ShippingLine ON ShippingLine.OH_PK = ShippingLineAddr.OA_OH
LEFT JOIN dbo.OrgCusCode ShippingLineCCC ON ShippingLineCCC.OK_OH = ShippingLine.OH_PK AND ShippingLineCCC.OK_CodeType = 'CCC' AND ShippingLineCCC.OK_RN_NKCodeCountry = 'US'
LEFT JOIN dbo.OrgCusCode ShippingLineC1C ON ShippingLineC1C.OK_OH = ShippingLine.OH_PK AND ShippingLineC1C.OK_CodeType = 'C1C'
LEFT JOIN dbo.OrgAddress AS CreditorAddr ON CreditorAddr.OA_PK = jk.JK_OA_CreditorAddress
LEFT JOIN dbo.OrgHeader AS Creditor ON  Creditor.OH_PK = CreditorAddr.OA_OH
LEFT JOIN dbo.OrgCusCode CreditorCCC ON CreditorCCC.OK_OH = Creditor.OH_PK AND CreditorCCC.OK_CodeType = 'CCC' AND CreditorCCC.OK_RN_NKCodeCountry = 'US'
LEFT JOIN dbo.OrgCusCode CreditorC1C ON CreditorC1C.OK_OH = Creditor.OH_PK AND CreditorC1C.OK_CodeType = 'C1C'
LEFT JOIN dbo.RefShippingLine AS rsl ON rsl.RSL_PK = ShippingLine.OH_RSL_ShippingLine OR (ShippingLine.OH_RSL_ShippingLine IS NULL AND rsl.RSL_StandardCarrierAlphaCode = ShippingLineCCC.OK_CustomsRegNo)
LEFT JOIN dbo.GlbBranch BranchFromSI ON BranchFromSI.GB_Code = si.BranchCode
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

		public override string WhereClause => @"(   COALESCE(si.SL_PostedTimeUtc, @MaxDateTime) < COALESCE(jw.TransactionDate, @MaxDateTime) OR -- we're taking the DEP event we need to check that the event time of the DEP is not too old   (    jw.EventTime >= DATEADD(HOUR, -24, DATEADD(MONTH, -1, @StartDateTimeInclusive))    AND jw.EventTime < DATEADD(HOUR, 24, @EndDateTimeExclusive)   )  )  AND rc.RC_PK IS NOT NULL  AND Containers.JC_ContainerNum != ''  AND Company.GC_IsActive = 1  AND COALESCE(BranchFromSI.GB_IsActive, BranchFromSF.GB_IsActive) = 1  AND jk.JK_AgentType != 'CLD'  AND rsl.RSL_IsNVO = 0  AND rsl.RSL_OceanCarrierMessagingAvailable = 1";
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string MaxCW1Version => "22.7.21.120";
	}

	#endregion
}
