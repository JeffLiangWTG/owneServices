use eHubTransactions;
GO  
SET XACT_ABORT ON;  
GO  
BEGIN TRANSACTION;

DECLARE @CarrierName							varchar(50) = 'ZIM'

DECLARE @ZIM_CCPK								uniqueidentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = @CarrierName)
DECLARE @ContainerTracking_CCPK					uniqueidentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'CONTAINER_TRACKING')
DECLARE @OCMIFTMCS_CCPK	        				uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'OCMIFTMCS')

DECLARE @ZIM_IFTMCS_TSPK						uniqueIdentifier = 'CEB5A063-5CA3-4098-BB8C-440D60DED62B'	--SELECT NEWID()
DECLARE @ZIM_ContainerTracking_IFTMCS_TSPK		uniqueidentifier = '83D6F05A-58F3-4ECE-9730-510F382CCFCE'	--SELECT NEWID()

DECLARE @UII2UIE_TransType						uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchangeInclude2UniversalInterchangeEnvelope%')
DECLARE @IFTMCS2UIEnvelop						uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType where TT_TransformationType Like 'CargoWise.eHub.Products.GCT.Transforms.IFTMCS2UInterchangeEnvelope.IFTMCS2UInterchangeEnvelope%')
DECLARE @IFTMCS2UInterchange					uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType where TT_TransformationType Like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.IFTMCS2UniversalInterchange.IFTMCS2UniversalInterchange%')

DECLARE @D99B_IFTMCS_DTPK						uniqueidentifier = (SELECT DT_PK FROM eHubMessageType	WHERE DT_Code = 'http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D99B_IFTMCS')

DECLARE @CarrierMSG								varchar(50) = 'ZIMMSG'

------------------ eHubTransformationSet / eHubTransformationMapping
INSERT INTO eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient)
SELECT @ZIM_ContainerTracking_IFTMCS_TSPK, 'Shipping Instruction IFTMCS from ZIM', @ZIM_CCPK, @ContainerTracking_CCPK, @D99B_IFTMCS_DTPK, 'Shipping Instruction IFTMCS from ZIM', 0, 0 UNION ALL
SELECT @ZIM_IFTMCS_TSPK, 'Shipping Instruction IFTMCS from ZIM', @ZIM_CCPK,	null, @D99B_IFTMCS_DTPK, 'Shipping Instruction IFTMCS from ZIM', 0, 1

INSERT INTO eHubTransformationMapping
(TM_TS_PK, TM_order, TM_TT_PK)
SELECT @ZIM_ContainerTracking_IFTMCS_TSPK, 0, @IFTMCS2UIEnvelop 				UNION ALL
SELECT @ZIM_IFTMCS_TSPK, 0, @IFTMCS2UInterchange								UNION ALL
SELECT @ZIM_IFTMCS_TSPK, 1, @UII2UIE_TransType

--OCMIFTMCS - Subscription Type ID
DECLARE @OCMIFTMCS_SubscriptionTypeID_CSPK			uniqueidentifier = (SELECT CS_PK FROM eHubCodeSet		WHERE CS_CC_Recipient = @OCMIFTMCS_CCPK and CS_Name = 'Subscription Type ID')
DECLARE @OCMIFTMCS_STID_CRPK						uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMIFTMCS_SubscriptionTypeID_CSPK and CR_Name = 'ST ID')

DECLARE @OCMIFTMCS_SubscriptionTypeID_MaxCKOrder 	INT,
		@OCMIFTMCS_SubscriptionTypeID_MaxCKPK 		uniqueidentifier

SELECT TOP 1 @OCMIFTMCS_SubscriptionTypeID_MaxCKOrder = CK_Order, @OCMIFTMCS_SubscriptionTypeID_MaxCKPK = CK_PK
FROM eHubCodeMapKey
WHERE CK_CS = @OCMIFTMCS_SubscriptionTypeID_CSPK
ORDER BY CK_Order DESC

UPDATE eHubCodeMapKey
SET CK_Order =  CK_Order + 1
WHERE CK_PK = @OCMIFTMCS_SubscriptionTypeID_MaxCKPK

INSERT eHubCodeMapKey(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT 'CE446A5C-908E-4FFD-9557-1C24EE9A59DD', @OCMIFTMCS_SubscriptionTypeID_CSPK, @OCMIFTMCS_SubscriptionTypeID_MaxCKOrder, @CarrierName

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode)
SELECT 'CE446A5C-908E-4FFD-9557-1C24EE9A59DD', @OCMIFTMCS_STID_CRPK, @CarrierMSG

--OCMIFTMCS - Reference No
DECLARE @OCMIFTMCS_ReferenceNo_CSPK			uniqueidentifier = (SELECT CS_PK FROM eHubCodeSet		WHERE CS_CC_Recipient = @OCMIFTMCS_CCPK and CS_Name = 'Reference No')
DECLARE @OCMIFTMCS_Value_CRPK				uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMIFTMCS_ReferenceNo_CSPK and CR_Name = 'Value')

DECLARE @OCMIFTMCS_ReferenceNo_MaxCKOrder 	INT,
		@OCMIFTMCS_ReferenceNo_MaxCKPK 		uniqueidentifier

SELECT TOP 1 @OCMIFTMCS_ReferenceNo_MaxCKOrder = CK_Order, @OCMIFTMCS_ReferenceNo_MaxCKPK = CK_PK
FROM eHubCodeMapKey
WHERE CK_CS = @OCMIFTMCS_ReferenceNo_CSPK
ORDER BY CK_Order DESC

UPDATE eHubCodeMapKey
SET CK_Order =  CK_Order + 1
WHERE CK_PK = @OCMIFTMCS_ReferenceNo_MaxCKPK

INSERT eHubCodeMapKey(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT 'B0943978-762B-4ECF-862B-365CC2213357', @OCMIFTMCS_ReferenceNo_CSPK, @OCMIFTMCS_ReferenceNo_MaxCKOrder, @CarrierName

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode)
SELECT 'B0943978-762B-4ECF-862B-365CC2213357', @OCMIFTMCS_Value_CRPK, 'ZZZ'

ROLLBACK
--COMMIT

