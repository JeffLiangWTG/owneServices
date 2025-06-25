namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class ForwarderNVOCCShippingInstruction : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "NSI";
		public override string RoleName => "Forwarding Consolidation";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "NVOCC Shipping Instruction sent";
		public override string FeatureName => "NVOCC Shipping Instruction sent";
		public override string DataGranularity => "MAH";
		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string TransactionDateUtc => "si.SL_PostedTimeUtc";
		public override string GuidReference => "jk.JK_PK";
		public override string BillingReference1 => "jk.JK_UniqueConsignRef";
		public override string BillingReference2 => "IIF(jk.JK_AgentType = 'CLD', jk.JK_CoLoadBookingReference, jk.JK_BookingReference)";
		public override string BillingReference3 => "IIF(jk.JK_AgentType = 'CLD', jk.JK_CoLoadMasterBill, jk.JK_MasterBillNum)";
		public override string BillingReference4 => "'Shipping Instruction'";
		public override string TransactionCount => "1";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(
	SELECT
		ConsolNumber = jk.JK_UniqueConsignRef,
		ConsolType = jk.JK_AgentType,
		TransportMode = jk.JK_TransportMode,
		ContainerMode = jk.JK_ConsolMode,
		FirstLoadPort = jk.JK_RL_NKLoadPort,
		LastDiscPort = jk.JK_RL_NKDischargePort,
		CarrierBookingReference = jk.JK_BookingReference,
		CoLoadBookingConfirmationReference = jk.JK_CoLoadBookingReference,
		WayBillNumber = jk.JK_MasterBillNum,
		CoLoadMasterBillNumber = jk.JK_CoLoadMasterBill,
		'CoLoadWith.OrgCode' = Creditor.OH_Code,
		'CoLoadWith.C1CCode' = COALESCE(CreditorCCC.OK_CustomsRegNo, CreditorC1C.OK_CustomsRegNo),
		'ShippingLine.OrgCode' = ShippingLine.OH_Code,
		'ShippingLine.C1CCode' = COALESCE(ShippingLineCCC.OK_CustomsRegNo, ShippingLineC1C.OK_CustomsRegNo),
		IsNVO = 1
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)))";

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
ShippingInstructionSent AS
(
	SELECT
		ISNLog.SL_Parent AS JK_PK,
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
				AND SL_Reference LIKE '%MST=Shipping Instruction%'
			ORDER BY SL_PostedTimeUtc DESC
		) STL_BranchCode
	FROM
		dbo.StmALog ISNLog
	WHERE
		SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND SL_SE_NKEvent = 'ISN'
		AND SL_IsEstimate = 'N'
		AND SL_Reference LIKE '%MST=Shipping Instruction%'
)";

		public override string FromClause => @"ExportSeaConsols jk
JOIN ShippingInstructionSent si ON jk.JK_PK = si.JK_PK
JOIN dbo.GlbBranch Branch on si.STL_BranchCode = Branch.GB_Code
JOIN dbo.GlbCompany Company on Branch.GB_GC = Company.GC_PK
LEFT JOIN dbo.OrgAddress ShippingLineAddr ON ShippingLineAddr.OA_PK = jk.JK_OA_ShippingLineAddress
LEFT JOIN dbo.OrgHeader ShippingLine ON ShippingLine.OH_PK = ShippingLineAddr.OA_OH
LEFT JOIN dbo.OrgCusCode ShippingLineCCC ON ShippingLineCCC.OK_OH = ShippingLine.OH_PK AND ShippingLineCCC.OK_CodeType = 'CCC' AND ShippingLineCCC.OK_RN_NKCodeCountry = 'US'
LEFT JOIN dbo.OrgCusCode ShippingLineC1C ON ShippingLineC1C.OK_OH = ShippingLine.OH_PK AND ShippingLineC1C.OK_CodeType = 'C1C'
LEFT JOIN dbo.OrgAddress CreditorAddr ON CreditorAddr.OA_PK = jk.JK_OA_CreditorAddress
LEFT JOIN dbo.OrgHeader Creditor ON  Creditor.OH_PK = CreditorAddr.OA_OH
LEFT JOIN dbo.OrgCusCode CreditorCCC ON CreditorCCC.OK_OH = Creditor.OH_PK AND CreditorCCC.OK_CodeType = 'CCC' AND CreditorCCC.OK_RN_NKCodeCountry = 'US'
LEFT JOIN dbo.OrgCusCode CreditorC1C ON CreditorC1C.OK_OH = Creditor.OH_PK AND CreditorC1C.OK_CodeType = 'C1C'
LEFT JOIN dbo.RefShippingLine rslShippingLine ON ShippingLine.OH_RSL_ShippingLine = rslShippingLine.RSL_PK
LEFT JOIN dbo.RefShippingLine rslCoLoadWith ON Creditor.OH_RSL_ShippingLine = rslCoLoadWith.RSL_PK";

		public override string WhereClause => @"(
	(
		jk.JK_AgentType = 'CLD'
		AND rslCoLoadWith.RSL_IsNVO = 1
		AND rslCoLoadWith.RSL_ShippingInstructionAvailable = 1
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
		AND rslShippingLine.RSL_ShippingInstructionAvailable = 1
	)
)";
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string MinCW1Version => "23.9.29.11";
	}

	#endregion
}
