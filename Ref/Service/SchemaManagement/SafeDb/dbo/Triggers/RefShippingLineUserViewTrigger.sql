CREATE TRIGGER RefShippingLineUserView_Version_Create ON RefShippingLineUserView
INSTEAD OF INSERT
AS
BEGIN
	INSERT dbo.RefShippingLine (RSL_PK, RSL_IsActive, RSL_IsNVO, RSL_CarrierName, RSL_StandardCarrierAlphaCode, RSL_CargoWiseOneCode, RSL_OceanCarrierMessagingAvailable, RSL_GlobalSailingScheduleAvailable, RSL_ContainerAutomationAvailable, RSL_CargoSphereRatesAvailable, RSL_IsShippingLine, RSL_InvoiceAvailable, RSL_IsCW1User, RSL_EHubIds, RSL_BookingRequestAvailable, RSL_ShippingInstructionAvailable, RSL_VerifiedGrossContainerWeightAvailable, RSL_ShippingOrderAvailable, RSL_EManifestAvailable, RSL_ShippingLineLogo)
	SELECT RSL_PK, RSL_IsActive, RSL_IsNVO, RSL_CarrierName, RSL_StandardCarrierAlphaCode, RSL_CargoWiseOneCode, RSL_OceanCarrierMessagingAvailable, RSL_GlobalSailingScheduleAvailable, RSL_ContainerAutomationAvailable, RSL_CargoSphereRatesAvailable, RSL_IsShippingLine, RSL_InvoiceAvailable, RSL_IsCW1User, RSL_EHubIds, RSL_BookingRequestAvailable, RSL_ShippingInstructionAvailable, RSL_VerifiedGrossContainerWeightAvailable, RSL_ShippingOrderAvailable, RSL_EManifestAvailable, RSL_ShippingLineLogo
	FROM inserted

	UPDATE r SET RVC_Deleted = ~RSL_IsPublished, r.RVC_LastEditedUser = dbo.GetUserId()
	FROM RefDbVersionControl r
	JOIN inserted i ON RVC_ParentPK = RSL_PK AND RVC_ParentCode = 'RSL'
END
GO

CREATE TRIGGER RefShippingLineUserView_Version_Update ON RefShippingLineUserView
INSTEAD OF UPDATE
AS
BEGIN
	UPDATE r SET
		RSL_IsActive = i.RSL_IsActive,
		RSL_IsNVO = i.RSL_IsNVO,
		RSL_CarrierName = i.RSL_CarrierName,
		RSL_StandardCarrierAlphaCode = i.RSL_StandardCarrierAlphaCode,
		RSL_CargoWiseOneCode = i.RSL_CargoWiseOneCode,
		RSL_OceanCarrierMessagingAvailable = i.RSL_OceanCarrierMessagingAvailable,
		RSL_GlobalSailingScheduleAvailable = i.RSL_GlobalSailingScheduleAvailable,
		RSL_ContainerAutomationAvailable = i.RSL_ContainerAutomationAvailable,
		RSL_CargoSphereRatesAvailable = i.RSL_CargoSphereRatesAvailable,
		RSL_IsShippingLine = i.RSL_IsShippingLine,
		RSL_InvoiceAvailable = i.RSL_InvoiceAvailable,
		RSL_IsCW1User = i.RSL_IsCW1User,
		RSL_EHubIds = i.RSL_EHubIds,
		RSL_BookingRequestAvailable = i.RSL_BookingRequestAvailable,
		RSL_ShippingInstructionAvailable = i.RSL_ShippingInstructionAvailable,
		RSL_VerifiedGrossContainerWeightAvailable = i.RSL_VerifiedGrossContainerWeightAvailable,
		RSL_ShippingOrderAvailable = i.RSL_ShippingOrderAvailable,
		RSL_EManifestAvailable = i.RSL_EManifestAvailable,
		RSL_ShippingLineLogo = i.RSL_ShippingLineLogo
	FROM RefShippingLine r
	JOIN inserted i ON r.RSL_PK = i.RSL_PK

	UPDATE r
	SET RVC_Deleted = ~RSL_IsPublished, r.RVC_LastEditedUser = dbo.GetUserId()
	FROM RefDbVersionControl r
	JOIN inserted ON RVC_ParentPK = RSL_PK and RVC_ParentCode = 'RSL'
END
GO
