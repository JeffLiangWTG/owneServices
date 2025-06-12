use eHubTransactions;
GO
SET XACT_ABORT ON;
GO
BEGIN TRANSACTION;

DECLARE @CarrierName				varchar(50) = 'ZIM'

DECLARE @ZIM_CCPK					uniqueidentifier = (SELECT CC_PK PK FROM eHubClient WHERE CC_ID = @CarrierName)

DECLARE @ZIM_IFTMBC_TSPK			uniqueidentifier = '12D37AEC-DC99-4DE8-AF2E-7E10048A808D'	--SELECT NEWID()

DECLARE @ShippingInstruction_CCPK	uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'SHIPPING_INSTRUCTION')
DECLARE @ContainerTracking_CCPK		uniqueidentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'CONTAINER_TRACKING')

DECLARE @CarrierID					varchar(50) = 'ZIMID'
DECLARE @CarrierMSG					varchar(50) = 'ZIMMSG'
DECLARE @CarrierBRS					varchar(50) = 'ZIMBRS'

------------------ eHubTransformationSet / eHubTransformationMapping
DECLARE @IFTMBC2UII_TransType			uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType LIKE 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.IFTMBC2UShipment.IFTMBC2UniversalShipment%')
DECLARE @UII2UIE_TransType				uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType LIKE 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchangeInclude2UniversalInterchangeEnvelope%')

DECLARE @D99B_IFTMBC_DTPK					uniqueIdentifier = (SELECT DT_PK FROM eHubMessageType	WHERE DT_Code = 'http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D99B_IFTMBC')

INSERT INTO eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient)
SELECT @ZIM_IFTMBC_TSPK, concat('Booking Confirmation IFTMBC from ', @CarrierName), @ZIM_CCPK , null, @D99B_IFTMBC_DTPK , concat('Booking Confirmation IFTMBC from ', @CarrierName), 0, 1

INSERT INTO eHubTransformationMapping
(TM_TS_PK, TM_order, TM_TT_PK)
SELECT @ZIM_IFTMBC_TSPK, 0, @IFTMBC2UII_TransType					UNION ALL
SELECT @ZIM_IFTMBC_TSPK, 1, @UII2UIE_TransType 

-- Code Set OCMIFTMBC : CarrierSettings
DECLARE @Client_OCMIFTMBC			uniqueIdentifier = (SELECT CC_PK FROM eHubClient		WHERE CC_ID = 'OCMIFTMBC')
DECLARE @OCMIFTMBC_CSPK				uniqueidentifier = (SELECT CS_PK FROM eHubCodeSet		WHERE CS_CC_Recipient = @Client_OCMIFTMBC And CS_Name = 'CarrierSettings')
DECLARE @OCMIFTMBC_ID_CRPK			uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMIFTMBC_CSPK And CR_Name = 'ID')
DECLARE @OCMIFTMBC_MSGID_CRPK		uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMIFTMBC_CSPK And CR_Name = 'MSGID')
DECLARE @OCMIFTMBC_BRSID_CRPK		uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMIFTMBC_CSPK And CR_Name = 'BRSID')

DECLARE @OCMIFTMBC_MaxCK_PK			uniqueidentifier,
		@OCMIFTMBC_MaxCK_Order		int

SELECT	TOP 1  @OCMIFTMBC_MaxCK_PK = CK_PK, @OCMIFTMBC_MaxCK_Order = CK_Order
FROM	eHubCodeMapKey 
WHERE	CK_CS = @OCMIFTMBC_CSPK
ORDER BY CK_Order DESC

UPDATE eHubCodeMapKey
SET CK_Order = @OCMIFTMBC_MaxCK_Order + 1
WHERE CK_PK = @OCMIFTMBC_MaxCK_PK

INSERT eHubCodeMapKey
(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT '5206DE59-7655-4CD7-A7E7-B0781E6DD9C2', @OCMIFTMBC_CSPK, @OCMIFTMBC_MaxCK_Order, @CarrierName

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey)
SELECT '5206DE59-7655-4CD7-A7E7-B0781E6DD9C2', @OCMIFTMBC_ID_CRPK, 		@CarrierID, 	NULL			UNION ALL
SELECT '5206DE59-7655-4CD7-A7E7-B0781E6DD9C2', @OCMIFTMBC_BRSID_CRPK, 	@CarrierBRS, 	NULL			UNION ALL
SELECT '5206DE59-7655-4CD7-A7E7-B0781E6DD9C2', @OCMIFTMBC_MSGID_CRPK, 	@CarrierMSG, 	NULL 

-------------------Subscription Lookup
DECLARE @Subscription_MSG		uniqueIdentifier = (SELECT ST_PK FROM eHubSubscriptionType WHERE ST_ID = @CarrierMSG AND ST_Name = concat(@CarrierName, ' Message Reference'))
DECLARE @IFTMBC_D99B_SLPK		uniqueidentifier = '7534F90E-4528-47CE-91D2-2D335BA73F0F' --SELECT NEWID()

INSERT eHubSubScriptionLookup(SL_PK, SL_ST, SL_DT, SL_ValueXpath)
SELECT @IFTMBC_D99B_SLPK, @Subscription_MSG, @D99B_IFTMBC_DTPK, '/*[local-name()=''EFACT_D99B_IFTMBC'']/*[local-name()=''BGM'']/*[local-name()=''C106'']/C10601'

-------------------Subscription
DECLARE @CTSCPY_STPK		uniqueidentifier = (SELECT ST_PK FROM eHubSubscriptionType WHERE ST_ID ='CTSCPY')
DECLARE @ZIM_SVPK			uniqueidentifier = '5DC89825-56B0-430F-9800-8763328C13CD' --SELECT NEWID()

INSERT INTO eHubSubscriptionValue (SV_PK, SV_ST, SV_CC_Sender, SV_Value, SV_Reference, SV_SubscribedUTC, SV_ExpiryUTC, SV_CC_Recipient, SV_ReferenceType) 
SELECT @ZIM_SVPK, @CTSCPY_STPK, @ZIM_CCPK, '1', NULL, '20201217 10:30:00.000', NULL, @ContainerTracking_CCPK, NULL


ROLLBACK
--COMMIT