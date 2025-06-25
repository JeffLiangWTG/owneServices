namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class BillOfLadingData : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "BLN";
		public override string RoleName => "Market Analytics";
		public override string ModuleName => "Market Intelligence and Analytics";
		public override string FunctionName => "Bill of Lading Data";
		public override string FeatureName => "Consols and Shipments BLN Data";

		public override string CompanyCode => "SendingForwarderCompanyCode";
		public override string BranchCode => "SendingForwarderBranchCode";

		public override string TransactionDateUtc => "COALESCE(JK_SystemLastEditTimeUtc, JK_SystemCreateTimeUtc)";
		public override string GuidReference => "b.JK_PK";
		public override string BillingReference1 => "ConsolNumber";
		public override string BillingReference2 => "ShipmentNumber";

		public override string PreparationScript => @"WITH
ConsolEstimatedTimeCTE AS
(
	SELECT
		JW_ParentGUID,
		ConsolETD = MIN(JW_ETD),
		ConsolETA = MAX(JW_ETA)
	FROM dbo.JobConsolTransport INNER JOIN dbo.JobConsol ON JW_ParentGUID = JK_PK
	GROUP BY JW_ParentGUID
),

BillOfLadingDataCTE AS
(
	SELECT
		JK_PK,
		SendingForwarderCode = SendingForwarder.OH_Code,
		SendingForwarderName = SendingForwarder.OH_FullName,
		SendingForwarderBranchCode = SendingForwarderBranch.GB_Code,
		SendingForwarderBranchName = SendingForwarderBranch.GB_BranchName,
		SendingForwarderCompanyCode = SendingForwarderCompany.GC_Code,
		SendingForwarderCompanyName = SendingForwarderCompany.GC_Name,
		ReceivingForwarderCode = ReceivingForwarder.OH_Code,
		ReceivingForwarderName = ReceivingForwarder.OH_FullName,
		ReceivingForwarderBranchCode = ReceivingForwarderBranch.GB_Code,
		ReceivingForwarderBranchName = ReceivingForwarderBranch.GB_BranchName,
		ReceivingForwarderCompanyCode = ReceivingForwarderCompany.GC_Code,
		ReceivingForwarderCompanyName = ReceivingForwarderCompany.GC_Name,
		Carrier = Carrier.OH_FullName,
		CarrierC1C = ShippingLine.RSL_CargoWiseOneCode,
		CarrierSCAC = ShippingLine.RSL_StandardCarrierAlphaCode,
		ConsolNumber = JK_UniqueConsignRef,
		ConsolCreatedUTC = JK_SystemCreateTimeUtc,
		JK_SystemLastEditTimeUtc,
		ConsolType = JK_AgentType,
		ConsolTransportMode = JK_TransportMode,
		ConsolContainerMode = JK_ConsolMode,
		CBR = JK_BookingReference,
		CoLoadCBR = JK_CoLoadBookingReference,
		ConsolLoadPort = JK_RL_NKLoadPort,
		ConsolLoadPortName = LoadPort.RL_PortName,
		ConsolDischargePort = JK_RL_NKDischargePort,
		ConsolDischargePortName = DischargePort.RL_PortName,
		MBL = JK_MasterBillNum,
		CoLoadMBL = JK_CoLoadMasterBill,
		MBLReleaseType = JK_ReleaseType,
		MBLNumberOfOriginalBills = JK_NoOriginalBills,
		MBLNumberOfCopyBills = JK_NoCopyBills,
		JK_SystemCreateTimeUtc,
		ShipmentNumber = JS_UniqueConsignRef,
		ShipmentType = JS_ShipmentType,
		ShipmentTransportMode = JS_TransportMode,
		ShipmentContainerMode = JS_PackingMode,
		ShipmentOriginCode = JS_RL_NKOrigin,
		ShipmentOriginName = Origin.RL_PortName,
		ShipmentDestinationCode = JS_RL_NKDestination,
		ShipmentDestinationName = Destination.RL_PortName,
		ShipmentETD = JS_E_DEP,
		ShipmentETA = JS_E_ARV,
		HBL = JS_HouseBill,
		HBLReleaseType = JS_ReleaseType,
		HBLType = JS_HouseBillOfLadingType,
		HBLNumberOfOriginalBills = JS_NoOriginalBills,
		HBLNumberOfCopyBills = JS_NoCopyBills,
		ShipmentCarrierContractNumber = JS_CarrierContractNumber
	FROM dbo.JobConsol
		LEFT JOIN dbo.JobConShipLink ConShipLink ON JN_JK = JK_PK
		LEFT JOIN dbo.JobShipment Shipment ON JS_PK = JN_JS
		INNER JOIN dbo.OrgAddress SendingForwarderAddress ON SendingForwarderAddress.OA_PK = JK_OA_SendingForwarderAddress
		INNER JOIN dbo.OrgHeader SendingForwarder ON SendingForwarder.OH_PK = SendingForwarderAddress.OA_OH
		LEFT JOIN dbo.GlbBranch SendingForwarderBranch ON SendingForwarderBranch.GB_OH_OrgProxy = SendingForwarder.OH_PK
		LEFT JOIN dbo.GlbCompany SendingForwarderCompany ON SendingForwarderCompany.GC_OH_OrgProxy = SendingForwarder.OH_PK
		INNER JOIN dbo.OrgAddress ReceivingForwarderAddress ON ReceivingForwarderAddress.OA_PK = JK_OA_ReceivingForwarderAddress
		INNER JOIN dbo.OrgHeader ReceivingForwarder ON ReceivingForwarder.OH_PK = ReceivingForwarderAddress.OA_OH
		LEFT JOIN dbo.GlbBranch ReceivingForwarderBranch ON ReceivingForwarderBranch.GB_OH_OrgProxy = ReceivingForwarder.OH_PK
		LEFT JOIN dbo.GlbCompany ReceivingForwarderCompany ON ReceivingForwarderCompany.GC_OH_OrgProxy = ReceivingForwarder.OH_PK
		LEFT JOIN dbo.OrgAddress CarrierAddress ON CarrierAddress.OA_PK = JK_OA_ShippingLineAddress
		LEFT JOIN dbo.OrgHeader Carrier ON Carrier.OH_PK = CarrierAddress.OA_OH
		LEFT JOIN dbo.RefShippingLine ShippingLine ON ShippingLine.RSL_PK = Carrier.OH_RSL_ShippingLine
		LEFT JOIN dbo.RefUNLOCO LoadPort ON LoadPort.RL_Code = JK_RL_NKLoadPort
		LEFT JOIN dbo.RefUNLOCO DischargePort ON DischargePort.RL_Code = JK_RL_NKDischargePort
		LEFT JOIN dbo.RefUNLOCO Origin ON Origin.RL_Code = JS_RL_NKOrigin
		LEFT JOIN dbo.RefUNLOCO Destination ON Destination.RL_Code = JS_RL_NKDestination
	WHERE
		SendingForwarderBranch.GB_IsActive = 1 AND
		SendingForwarderCompany.GC_IsActive = 1 AND
		ReceivingForwarderBranch.GB_IsActive = 1 AND
		ReceivingForwarderCompany.GC_IsActive = 1
)";

		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX),CONVERT(VARCHAR(MAX),
(SELECT
	SendingForwarderCode,SendingForwarderName,SendingForwarderBranchCode,SendingForwarderBranchName,SendingForwarderCompanyCode,SendingForwarderCompanyName,ReceivingForwarderCode,ReceivingForwarderName,ReceivingForwarderBranchCode,ReceivingForwarderBranchName,ReceivingForwarderCompanyCode,ReceivingForwarderCompanyName,ConsolNumber,ConsolCreatedUTC,ConsolType,ConsolTransportMode,ConsolContainerMode,Carrier,CarrierC1C,CarrierSCAC,
	CBR,CoLoadCBR,ConsolLoadPort,ConsolLoadPortName,ConsolDischargePort,ConsolDischargePortName,ConsolETD,ConsolETA,MBL,CoLoadMBL,MBLReleaseType,MBLNumberOfOriginalBills,MBLNumberOfCopyBills,ShipmentETD,ShipmentETA,ShipmentTransportMode,ShipmentContainerMode,ShipmentType,ShipmentNumber,HBL,HBLReleaseType,HBLType,HBLNumberOfOriginalBills,HBLNumberOfCopyBills,ShipmentOriginCode,ShipmentOriginName,ShipmentDestinationCode,ShipmentDestinationName,ShipmentCarrierContractNumber
FOR JSON PATH,WITHOUT_ARRAY_WRAPPER
)))";

		public override string FromClause => @"BillOfLadingDataCTE b
LEFT JOIN ConsolEstimatedTimeCTE c ON c.JW_ParentGUID = b.JK_PK";

		public override string WhereClause => string.Empty;

		public override string DataGranularity => RefStlItemGrain.Daily;

		public override bool UsedInBilling => false;
	}

	#endregion
}
