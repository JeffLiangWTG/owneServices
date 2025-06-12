use eHubTransactions;
GO  
SET XACT_ABORT ON;  
GO  
BEGIN TRANSACTION;

DECLARE @CarrierName				varchar(50) = 'ZIM'

DECLARE @ZIM_CCPK					uniqueidentifier = (SELECT CC_PK PK FROM eHubClient WHERE CC_ID = @CarrierName)
DECLARE @OCMCONTRL_CCPK				uniqueidentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'OCMCONTRL')

DECLARE @ZIM_OCMCONTRL_TSPK			uniqueIdentifier = 'F715FEC1-D6B0-4FCC-A586-3B1875B1342C'	-- select NEWID()

DECLARE @Control2UE_TransType uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CONTRL2UEvent.CONTRL2UniversalEvent%')
DECLARE @UI2UI_2012_TransType uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchangeInclude2UniversalInterchange, CargoWise.eHub.Clients.EDI.Universal_2012_11%')

DECLARE @CONTRL_DTPK				uniqueidentifier = (SELECT DT_PK FROM eHubMessageType	WHERE DT_Code = 'http://schemas.microsoft.com/Edi/Edifact#Efact_Contrl_Root')

DECLARE @CarrierMSG					varchar(50) = 'ZIMMSG'

------------------ eHubTransformationSet / eHubTransformationMapping
INSERT INTO eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient)
SELECT @ZIM_OCMCONTRL_TSPK, 'OCM CONTRL from ZIM', @ZIM_CCPK, null, @CONTRL_DTPK, 'OCM CONTRL from ZIM', 0, 1

INSERT INTO eHubTransformationMapping
(TM_TS_PK, TM_order, TM_TT_PK)
SELECT @ZIM_OCMCONTRL_TSPK, 0, @Control2UE_TransType 				UNION ALL
SELECT @ZIM_OCMCONTRL_TSPK, 1, @UI2UI_2012_TransType

-------------------Subscription Lookup
DECLARE @Subscription_MSG		uniqueIdentifier = (SELECT ST_PK FROM eHubSubscriptionType WHERE ST_ID = @CarrierMSG AND ST_Name = concat(@CarrierName, ' Message Reference'))
DECLARE @CONTRL_SLPK			uniqueidentifier = '4AE6C42B-2E03-41C0-B758-8B42E9189976' --SELECT NEWID()

INSERT eHubSubScriptionLookup(SL_PK, SL_ST, SL_DT, SL_ValueXpath)
SELECT @CONTRL_SLPK, @Subscription_MSG, @CONTRL_DTPK, 'string(number(/*[local-name()="Efact_Contrl_Root"]/*[local-name()="UCI"]/UCI1))'

--OCMCONTRL - Subscription Type ID
DECLARE @OCMCONTRL_SubscriptionTypeID_CSPK			uniqueidentifier = (SELECT CS_PK FROM eHubCodeSet		WHERE CS_CC_Recipient = @OCMCONTRL_CCPK and CS_Name = 'Subscription Type ID')
DECLARE @OCMCONTRL_STID_CRPK						uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMCONTRL_SubscriptionTypeID_CSPK and CR_Name = 'ST ID')

DECLARE @OCMCONTRL_SubscriptionTypeID_MaxCKOrder 	INT,
		@OCMCONTRL_SubscriptionTypeID_MaxCKPK 		uniqueidentifier

SELECT TOP 1 @OCMCONTRL_SubscriptionTypeID_MaxCKOrder = CK_Order, @OCMCONTRL_SubscriptionTypeID_MaxCKPK = CK_PK
FROM eHubCodeMapKey
WHERE CK_CS = @OCMCONTRL_SubscriptionTypeID_CSPK
ORDER BY CK_Order DESC

UPDATE eHubCodeMapKey
SET CK_Order =  CK_Order + 1
WHERE CK_PK = @OCMCONTRL_SubscriptionTypeID_MaxCKPK

INSERT eHubCodeMapKey(CK_PK, CK_CS, CK_Order, CK_Key1Value) 
SELECT '7010F6AC-91C7-4053-94AF-865E2734BE41', @OCMCONTRL_SubscriptionTypeID_CSPK, @OCMCONTRL_SubscriptionTypeID_MaxCKOrder, @CarrierName

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode) 
SELECT '7010F6AC-91C7-4053-94AF-865E2734BE41', @OCMCONTRL_STID_CRPK, @CarrierMSG


ROLLBACK
--COMMIT

