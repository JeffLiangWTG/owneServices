use eHubTransactions;
GO  
SET XACT_ABORT ON;  
GO  
BEGIN TRANSACTION;

DECLARE @CarrierName				varchar(50) = 'ZIM'

DECLARE @ZIM_CCPK					uniqueidentifier = (SELECT CC_PK PK FROM eHubClient WHERE CC_ID = @CarrierName)
DECLARE @ContainerTracking_CCPK		uniqueidentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'CONTAINER_TRACKING')
DECLARE @OCMAPERAK_CCPK				uniqueidentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'OCMAPERAK')

DECLARE @ZIM_OCMAPERAK_D99B_TSPK	uniqueIdentifier = 'DCA837E3-B75B-4823-ACB7-D91F288CB851'	-- select NEWID()
DECLARE @ZIM_OCMAPERAK_D04A_TSPK	uniqueIdentifier = 'DA5667DD-4E34-4D76-872D-AA62D42D7CCD'	-- select NEWID()
DECLARE @ZIM_GCTAPERAK_D99B_TSPK	uniqueIdentifier = '3A796868-14E3-4770-B1FE-0D8DD705495D'	-- select NEWID()
DECLARE @ZIM_GCTAPERAK_D04A_TSPK	uniqueIdentifier = '15D759A0-1EDE-47C6-806B-5A7E907A55DB'	-- select NEWID()

DECLARE @UII2UIE_TransType				uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchangeInclude2UniversalInterchangeEnvelope%')
DECLARE @OCM_APERAK2UI_D99B_TransType   uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.APERAK2UInterchangeInclude.APERAK2UInterchangeInclude%')
DECLARE @GCT_APERAK2UI_D99B_TransType	uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.GCT.Transforms.APERAK2UInterchangeInclude.APERAK2UInterchangeInclude%')
DECLARE @OCM_APERAK2UI_D04A_TransType	uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.APERAK2UInterchangeInclude_D04A.APERAK2UInterchangeInclude_D04A%')
DECLARE @GCT_APERAK2UI_D04A_TransType	uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.GCT.Transforms.APERAK2UInterchangeInclude_D04A.APERAK2UInterchangeInclude_D04A%')

DECLARE @D99B_APERAK_DTPK				uniqueidentifier = (SELECT DT_PK FROM eHubMessageType	WHERE DT_Code = 'http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D99B_APERAK')
DECLARE @D04A_APERAK_DTPK				uniqueidentifier = (SELECT DT_PK FROM eHubMessageType	WHERE DT_Code = 'http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D04A_APERAK')

DECLARE @CarrierID					varchar(50) = 'ZIMID'
DECLARE @CarrierMSG					varchar(50) = 'ZIMMSG'
DECLARE @CarrierBRS					varchar(50) = 'ZIMBRS'

------------------ eHubTransformationSet / eHubTransformationMapping
INSERT INTO eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient)
SELECT @ZIM_OCMAPERAK_D99B_TSPK,'OCM APERAK D99B From ZIM', @ZIM_CCPK, null, @D99B_APERAK_DTPK, 'OCM APERAK D99B From ZIM', 0, 1 UNION ALL
SELECT @ZIM_GCTAPERAK_D99B_TSPK,'GCT APERAK D99B From ZIM', @ZIM_CCPK, @ContainerTracking_CCPK, @D99B_APERAK_DTPK,'GCT APERAK D99B From ZIM',0, 0 UNION ALL
SELECT @ZIM_OCMAPERAK_D04A_TSPK,'OCM APERAK D04A from ZIM', @ZIM_CCPK, null, @D04A_APERAK_DTPK, 'OCM APERAK D04A from ZIM', 0, 1 UNION ALL
SELECT @ZIM_GCTAPERAK_D04A_TSPK,'GCT APERAK D04A From ZIM', @ZIM_CCPK, @ContainerTracking_CCPK, @D04A_APERAK_DTPK,'GCT APERAK D04A From ZIM',0, 0

INSERT INTO eHubTransformationMapping
(TM_TS_PK, TM_order, TM_TT_PK)
SELECT @ZIM_OCMAPERAK_D99B_TSPK, 0, @OCM_APERAK2UI_D99B_TransType   UNION ALL
SELECT @ZIM_OCMAPERAK_D99B_TSPK, 1, @UII2UIE_TransType				UNION ALL
SELECT @ZIM_GCTAPERAK_D99B_TSPK, 0, @GCT_APERAK2UI_D99B_TransType	UNION ALL
SELECT @ZIM_GCTAPERAK_D99B_TSPK, 1, @UII2UIE_TransType				UNION ALL
SELECT @ZIM_OCMAPERAK_D04A_TSPK, 0, @OCM_APERAK2UI_D04A_TransType   UNION ALL
SELECT @ZIM_OCMAPERAK_D04A_TSPK, 1, @UII2UIE_TransType				UNION ALL
SELECT @ZIM_GCTAPERAK_D04A_TSPK, 0, @GCT_APERAK2UI_D04A_TransType	UNION ALL
SELECT @ZIM_GCTAPERAK_D04A_TSPK, 1, @UII2UIE_TransType

-------------------Subscription Lookup
DECLARE @Subscription_MSG		uniqueIdentifier = (SELECT ST_PK FROM eHubSubscriptionType WHERE ST_ID = @CarrierMSG AND ST_Name = concat(@CarrierName, ' Message Reference'))
DECLARE @APERAK_D99B_SLPK		uniqueidentifier = '60F8EA3A-B68C-426B-9060-5239AF9CCC41' --SELECT NEWID()
DECLARE @APERAK_D04A_SLPK		uniqueidentifier = '81D3CC2A-F16B-46D3-B696-793E95632C36' --SELECT NEWID()

INSERT eHubSubScriptionLookup(SL_PK, SL_ST, SL_DT, SL_ValueXpath)
SELECT @APERAK_D99B_SLPK, @Subscription_MSG, @D99B_APERAK_DTPK, '/*[local-name()=''EFACT_D99B_APERAK'']/*[local-name()=''BGM'']/*[local-name()=''C106'']/C10601'	UNION ALL
SELECT @APERAK_D04A_SLPK, @Subscription_MSG, @D04A_APERAK_DTPK, '/*[local-name()=''EFACT_D04A_APERAK'']/*[local-name()=''BGM'']/*[local-name()=''C106'']/C10601'

-- Code Set OCMAPERAK : Subscription Type ID
DECLARE @OCMAPERAK_SubscriptionTypeID_CSPK			uniqueidentifier = (SELECT CS_PK FROM eHubCodeSet		WHERE CS_CC_Recipient = @OCMAPERAK_CCPK and CS_Name = 'Subscription Type ID')

DECLARE @OCMAPERAK_ID_CRPK							uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMAPERAK_SubscriptionTypeID_CSPK and CR_Name = 'ID'),
		@OCMAPERAK_BRSID_CRPK						uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMAPERAK_SubscriptionTypeID_CSPK and CR_Name = 'BRS ST ID'),
		@OCMAPERAK_Reference_CRPK					uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMAPERAK_SubscriptionTypeID_CSPK and CR_Name = 'Message Reference ST ID')

DECLARE @OCMAPERAK_SubscriptionTypeID_MaxCKOrder 	INT,
		@OCMAPERAK_SubscriptionTypeID_MaxCKPK 		uniqueidentifier

SELECT TOP 1 @OCMAPERAK_SubscriptionTypeID_MaxCKOrder = CK_Order, @OCMAPERAK_SubscriptionTypeID_MaxCKPK = CK_PK
FROM eHubCodeMapKey
WHERE CK_CS = @OCMAPERAK_SubscriptionTypeID_CSPK
ORDER BY CK_Order DESC

UPDATE eHubCodeMapKey
SET CK_Order =  CK_Order + 1
WHERE CK_PK = @OCMAPERAK_SubscriptionTypeID_MaxCKPK

INSERT eHubCodeMapKey(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT '83FA4407-3FCB-4DC8-9E5D-1FA12EF3266E', @OCMAPERAK_SubscriptionTypeID_CSPK, @OCMAPERAK_SubscriptionTypeID_MaxCKOrder, @CarrierName

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode)
SELECT '83FA4407-3FCB-4DC8-9E5D-1FA12EF3266E', @OCMAPERAK_ID_CRPK, @CarrierID			UNION ALL
SELECT '83FA4407-3FCB-4DC8-9E5D-1FA12EF3266E', @OCMAPERAK_BRSID_CRPK, @CarrierBRS		UNION ALL
SELECT '83FA4407-3FCB-4DC8-9E5D-1FA12EF3266E', @OCMAPERAK_Reference_CRPK, @CarrierMSG



ROLLBACK
--COMMIT

