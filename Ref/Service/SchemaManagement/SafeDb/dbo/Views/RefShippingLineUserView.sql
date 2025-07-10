CREATE VIEW [dbo].[RefShippingLineUserView]
WITH SCHEMABINDING
AS

SELECT
	RSL_PK AS RSL_PK,
	RSL_IsActive AS RSL_IsActive,
	RSL_IsNVO AS RSL_IsNVO,
	RSL_CarrierName AS RSL_CarrierName,
	RSL_StandardCarrierAlphaCode AS RSL_StandardCarrierAlphaCode,
	RSL_CargoWiseOneCode AS RSL_CargoWiseOneCode,
	RSL_OceanCarrierMessagingAvailable AS RSL_OceanCarrierMessagingAvailable,
	RSL_GlobalSailingScheduleAvailable AS RSL_GlobalSailingScheduleAvailable,
	RSL_ContainerAutomationAvailable AS RSL_ContainerAutomationAvailable,
	RSL_CargoSphereRatesAvailable AS RSL_CargoSphereRatesAvailable,
	RSL_IsShippingLine AS RSL_IsShippingLine,
	RSL_InvoiceAvailable AS RSL_InvoiceAvailable,
	RSL_IsCW1User AS RSL_IsCW1User,
	RSL_EHubIds AS RSL_EHubIds,
	RSL_BookingRequestAvailable AS RSL_BookingRequestAvailable,
	RSL_ShippingInstructionAvailable AS RSL_ShippingInstructionAvailable,
	RSL_VerifiedGrossContainerWeightAvailable AS RSL_VerifiedGrossContainerWeightAvailable,
	RSL_ShippingOrderAvailable AS RSL_ShippingOrderAvailable,
	RSL_EManifestAvailable AS RSL_EManifestAvailable,
	RSL_ShippingLineLogo AS RSL_ShippingLineLogo,
	RSL_SysStartTime AS RSL_SysStartTime,
	RSL_SysEndTime AS RSL_SysEndTime,
	CAST (1 AS BIT) AS RSL_IsSystem,
	~RVC_Deleted AS RSL_IsPublished

FROM [dbo].RefShippingLine
JOIN [dbo].RefDbVersionControl ON RVC_ParentPK = RSL_PK AND RVC_ParentCode = 'RSL'
GO

CREATE PROCEDURE [dbo].[RefShippingLineUserView_Ins]
	@RSL_PK UNIQUEIDENTIFIER,
	@RSL_IsActive BIT,
	@RSL_IsNVO BIT,
	@RSL_CarrierName NVARCHAR(75),
	@RSL_StandardCarrierAlphaCode VARCHAR(4),
	@RSL_CargoWiseOneCode VARCHAR(4),
	@RSL_OceanCarrierMessagingAvailable BIT,
	@RSL_GlobalSailingScheduleAvailable BIT,
	@RSL_ContainerAutomationAvailable BIT,
	@RSL_CargoSphereRatesAvailable BIT,
	@RSL_IsShippingLine BIT,
	@RSL_InvoiceAvailable BIT,
	@RSL_IsCW1User BIT,
	@RSL_EHubIds NVARCHAR(250),
	@RSL_IsPublished BIT,
	@RSL_BookingRequestAvailable BIT,
	@RSL_ShippingInstructionAvailable BIT,
	@RSL_VerifiedGrossContainerWeightAvailable BIT,
	@RSL_ShippingOrderAvailable BIT,
	@RSL_EManifestAvailable BIT,
	@RSL_ShippingLineLogo VARBINARY(MAX)
AS
BEGIN

INSERT dbo.RefShippingLine (RSL_PK, RSL_IsActive, RSL_IsNVO, RSL_CarrierName, RSL_StandardCarrierAlphaCode, RSL_CargoWiseOneCode, RSL_OceanCarrierMessagingAvailable, RSL_GlobalSailingScheduleAvailable, RSL_ContainerAutomationAvailable, RSL_CargoSphereRatesAvailable, RSL_IsShippingLine, RSL_InvoiceAvailable, RSL_IsCW1User, RSL_EHubIds, RSL_BookingRequestAvailable, RSL_ShippingInstructionAvailable, RSL_VerifiedGrossContainerWeightAvailable, RSL_ShippingOrderAvailable, RSL_EManifestAvailable, RSL_ShippingLineLogo)
VALUES (@RSL_PK, @RSL_IsActive, @RSL_IsNVO, @RSL_CarrierName, @RSL_StandardCarrierAlphaCode, @RSL_CargoWiseOneCode, @RSL_OceanCarrierMessagingAvailable, @RSL_GlobalSailingScheduleAvailable, @RSL_ContainerAutomationAvailable, @RSL_CargoSphereRatesAvailable, @RSL_IsShippingLine, @RSL_InvoiceAvailable, @RSL_IsCW1User, @RSL_EHubIds, @RSL_BookingRequestAvailable, @RSL_ShippingInstructionAvailable, @RSL_VerifiedGrossContainerWeightAvailable, @RSL_ShippingOrderAvailable, @RSL_EManifestAvailable, @RSL_ShippingLineLogo)

UPDATE RefDbVersionControl SET RVC_Deleted = ~@RSL_IsPublished WHERE RVC_ParentPK = @RSL_PK AND RVC_ParentCode = 'RSL'
END
GO

CREATE PROCEDURE [dbo].[RefShippingLineUserView_Del]
	@RSL_PK UNIQUEIDENTIFIER
AS
DELETE RefShippingLine WHERE RSL_PK = @RSL_PK
GO

CREATE PROCEDURE [dbo].[RefShippingLineUserView_Ups]
	@RSL_PK UNIQUEIDENTIFIER,
	@RSL_IsActive BIT,
	@RSL_IsNVO BIT,
	@RSL_CarrierName NVARCHAR(75),
	@RSL_StandardCarrierAlphaCode VARCHAR(4),
	@RSL_CargoWiseOneCode VARCHAR(4),
	@RSL_OceanCarrierMessagingAvailable BIT,
	@RSL_GlobalSailingScheduleAvailable BIT,
	@RSL_ContainerAutomationAvailable BIT,
	@RSL_CargoSphereRatesAvailable BIT,
	@RSL_IsShippingLine BIT,
	@RSL_InvoiceAvailable BIT,
	@RSL_IsCW1User BIT,
	@RSL_EHubIds NVARCHAR(250),
	@RSL_IsPublished BIT,
	@RSL_BookingRequestAvailable BIT,
	@RSL_ShippingInstructionAvailable BIT,
	@RSL_VerifiedGrossContainerWeightAvailable BIT,
	@RSL_ShippingOrderAvailable BIT,
	@RSL_EManifestAvailable BIT,
	@RSL_ShippingLineLogo VARBINARY(MAX)
AS
BEGIN

UPDATE RefShippingLine SET
	RSL_IsActive = @RSL_IsActive,
	RSL_IsNVO = @RSL_IsNVO,
	RSL_CarrierName = @RSL_CarrierName,
	RSL_StandardCarrierAlphaCode = @RSL_StandardCarrierAlphaCode,
	RSL_CargoWiseOneCode = @RSL_CargoWiseOneCode,
	RSL_OceanCarrierMessagingAvailable = @RSL_OceanCarrierMessagingAvailable,
	RSL_GlobalSailingScheduleAvailable = @RSL_GlobalSailingScheduleAvailable,
	RSL_ContainerAutomationAvailable = @RSL_ContainerAutomationAvailable,
	RSL_CargoSphereRatesAvailable = @RSL_CargoSphereRatesAvailable,
	RSL_IsShippingLine = @RSL_IsShippingLine,
	RSL_InvoiceAvailable = @RSL_InvoiceAvailable,
	RSL_IsCW1User = @RSL_IsCW1User,
	RSL_EHubIds = @RSL_EHubIds,
	RSL_BookingRequestAvailable = @RSL_BookingRequestAvailable,
	RSL_ShippingInstructionAvailable = @RSL_ShippingInstructionAvailable,
	RSL_VerifiedGrossContainerWeightAvailable = @RSL_VerifiedGrossContainerWeightAvailable,
	RSL_ShippingOrderAvailable = @RSL_ShippingOrderAvailable,
	RSL_EManifestAvailable = @RSL_EManifestAvailable,
	RSL_ShippingLineLogo = @RSL_ShippingLineLogo
WHERE RSL_PK = @RSL_PK

UPDATE RefDbVersionControl SET RVC_Deleted = ~@RSL_IsPublished WHERE RVC_ParentPK = @RSL_PK and RVC_ParentCode = 'RSL'
END
