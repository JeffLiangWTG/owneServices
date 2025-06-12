use eHubTransactions;
GO
SET XACT_ABORT ON;
GO
BEGIN TRANSACTION;

DECLARE @ZIM_VM_CCID				varchar(50) = 'ZIM_VM'
DECLARE @CarrierName				varchar(50) = 'ZIM'

DECLARE @ZIM_CCPK					uniqueidentifier = (SELECT CC_PK PK FROM eHubClient WHERE CC_ID = @CarrierName)
DECLARE @ZIM_VM_CCPK				uniqueIdentifier = '5F0C3906-636B-4374-8935-D6A3549F40FD'	--SELECT NEWID() FROM JEFF

DECLARE @Carrier_Config_TSName		varchar(100) = concat(@CarrierName, ' Provider Configuration')

DECLARE @ZIM_TSPK					uniqueIdentifier = (SELECT TS_PK FROM eHubTransformationSet WHERE TS_Name = @Carrier_Config_TSName AND TS_CC_Recipient = @ZIM_CCPK)
DECLARE @ZIM_VM_TSPK				uniqueidentifier = '6D6DF2D7-ADCB-41FE-A4ED-FCEAEEF117B4'	--SELECT NEWID()

DECLARE @ShippingInstruction_CCPK	uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'SHIPPING_INSTRUCTION')
DECLARE @ShippingIntruction_TSPK	uniqueIdentifier = (SELECT TS_PK FROM eHubTransformationSet WHERE TS_CC_Recipient = @ShippingInstruction_CCPK and TS_Name = 'OCM System Configuration')

DECLARE @CarrierID					varchar(50) = 'ZIMID'
DECLARE @CarrierMSG					varchar(50) = 'ZIMMSG'
DECLARE @CarrierBRS					varchar(50) = 'ZIMBRS'
DECLARE @CarrierPrefix				varchar(50) = 'ZIM'


----------------- New Client For Mapping
INSERT INTO eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
VALUES
(@ZIM_VM_CCPK, @ZIM_VM_CCID, concat(@CarrierName, ' - Verified Gross Mass'),'00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party')

------------------ eHubTransformationSet / eHubTransformationMapping
DECLARE @UI_v1tov2_TTPK					uniqueIdentifier = (Select TT_PK from eHubTransformationType where TT_TransformationType LIKE 'CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion.UniversalInterchangeShpV2_1ToV2%')
DECLARE @UI2US_2012_TTPK				uniqueIdentifier = (Select TT_PK from eHubTransformationType where TT_TransformationType LIKE 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchange2UniversalShipment%')
DECLARE @US2CU_TTPK						uniqueIdentifier = (Select TT_PK from eHubTransformationType where TT_TransformationType LIKE 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.Universal2CarrierUniversal%')
DECLARE @CU2CU_ISO8859_TTPK				uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CU2CUniveralISO8859.CU2CUniveralISO8859%')
DECLARE @CU2VM_TTPK						uniqueIdentifier = (Select TT_PK from eHubTransformationType where TT_TransformationType LIKE 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2VERMAS_SMDG.CarrierUniversal2VERMAS_SMDG%')

DECLARE @UInterchange_2011_DTPK			uniqueIdentifier = (SELECT DT_PK FROM eHubMessageType		 WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange')

INSERT INTO eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient)
SELECT @ZIM_VM_TSPK, concat('Verified Gross Container Weight VERMAS to ', @CarrierName), null , @ZIM_VM_CCPK, @UInterchange_2011_DTPK , concat('Verified Gross Container Weight VERMAS to ', @CarrierName), 1, 0

INSERT INTO eHubTransformationMapping
(TM_TS_PK, TM_order, TM_TT_PK)
select @ZIM_VM_TSPK, 0, @UI_v1tov2_TTPK								UNION ALL
select @ZIM_VM_TSPK, 1, @UI2US_2012_TTPK							UNION ALL
select @ZIM_VM_TSPK, 2, @US2CU_TTPK									UNION ALL
select @ZIM_VM_TSPK, 3, @CU2CU_ISO8859_TTPK 						UNION ALL
select @ZIM_VM_TSPK, 4, @CU2VM_TTPK

-- Code Set  OCMVERMAS  : CarrierSettings
DECLARE @OCMVERMAS_CCPK					uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'OCMVERMAS')

DECLARE @OCMVERMAS_CarrierSetting_CSPK	uniqueIdentifier = (SELECT CS_PK FROM eHubCodeSet WHERE CS_CC_Recipient = @OCMVERMAS_CCPK And CS_Name = 'CarrierSettings')
DECLARE @OCMVERMAS_TSPK					uniqueIdentifier = (SELECT TS_PK FROM eHubTransformationSet WHERE TS_CC_Recipient = @OCMVERMAS_CCPK and TS_Name = 'OCM VERMAS Configuration')

DECLARE @OCMVERMAS_Name_CRPK			uniqueIdentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMVERMAS_CarrierSetting_CSPK And CR_Name = 'Name')
DECLARE @OCMVERMAS_ID_CRPK				uniqueIdentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMVERMAS_CarrierSetting_CSPK And CR_Name = 'ID')
DECLARE @OCMVERMAS_MSGID_CRPK			uniqueIdentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMVERMAS_CarrierSetting_CSPK And CR_Name = 'MSGID')
DECLARE @OCMVERMAS_UNB_CRPK				uniqueIdentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMVERMAS_CarrierSetting_CSPK And CR_Name = 'UNB1.1')
DECLARE @OCMVERMAS_Prefix_CRPK			uniqueIdentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMVERMAS_CarrierSetting_CSPK And CR_Name = 'SubscriptionPrefix')

DECLARE @OCMVERMAS_MaxCKOrder_PK		uniqueidentifier
DECLARE @OCMVERMAS_MaxCKOrder			Int

SELECT	TOP 1
		@OCMVERMAS_MaxCKOrder_PK = CK_PK,
		@OCMVERMAS_MaxCKOrder = CK_Order
FROM	eHubCodeMapKey
WHERE	CK_CS = @OCMVERMAS_CarrierSetting_CSPK
ORDER BY CK_Order DESC

UPDATE	eHubCodeMapKey
SET		CK_Order = @OCMVERMAS_MaxCKOrder + 1
WHERE	CK_PK = @OCMVERMAS_MaxCKOrder_PK

INSERT eHubCodeMapKey(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT 'F876266B-7B64-4EC4-8938-192F43DC6A4D', @OCMVERMAS_CarrierSetting_CSPK, @OCMVERMAS_MaxCKOrder, concat(@CarrierName, '_%')

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey)
SELECT 'F876266B-7B64-4EC4-8938-192F43DC6A4D', @OCMVERMAS_Name_CRPK, @CarrierName, NULL		UNION ALL
SELECT 'F876266B-7B64-4EC4-8938-192F43DC6A4D', @OCMVERMAS_ID_CRPK, @CarrierID, NULL			UNION ALL
SELECT 'F876266B-7B64-4EC4-8938-192F43DC6A4D', @OCMVERMAS_MSGID_CRPK, @CarrierMSG, NULL		UNION ALL
SELECT 'F876266B-7B64-4EC4-8938-192F43DC6A4D', @OCMVERMAS_UNB_CRPK, 'UNOC', NULL			UNION ALL
SELECT 'F876266B-7B64-4EC4-8938-192F43DC6A4D', @OCMVERMAS_Prefix_CRPK, @CarrierPrefix, NULL

-- Code Set  SHIPPING_INSTRUCTION  : Default Interface Name
DECLARE @Default_Interface_CSPK			uniqueIdentifier = (SELECT CS_PK FROM eHubCodeSet		WHERE CS_CC_Recipient = @ShippingInstruction_CCPK And CS_Name = 'Default Interface Name')
DECLARE @Default_InterfaceName_CRPK		uniqueIdentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @Default_Interface_CSPK And CR_Name = 'Interface Name')

DECLARE @Default_Interface_Max_CKPK		uniqueidentifier
DECLARE @Default_Interface_Max_CKOrder	Int

SELECT	TOP 1
		@Default_Interface_Max_CKPK = CK_PK,
		@Default_Interface_Max_CKOrder = CK_Order
FROM	eHubCodeMapKey
WHERE	CK_CS = @Default_Interface_CSPK
ORDER BY CK_Order DESC


UPDATE	eHubCodeMapKey
SET		CK_Order = @Default_Interface_Max_CKOrder + 1
WHERE	CK_PK = @Default_Interface_Max_CKPK


INSERT eHubCodeMapKey(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT '350EBA79-EF56-4937-82BC-79C5D25C870B', @Default_Interface_CSPK, @Default_Interface_Max_CKOrder, concat(@CarrierName, '_%')

INSERT INTO eHubCodeMapValue
(CV_CK, CV_CR, CV_OutputCode)
SELECT '350EBA79-EF56-4937-82BC-79C5D25C870B', @Default_InterfaceName_CRPK, @Carrier_Config_TSName


------------------eHubRoutingRule
DECLARE @ZIM_GroupRule	uniqueidentifier = 'B726FABA-D568-4957-92DC-923232BAF92E'  --ZIM Group Rule FROM JEFF
DECLARE @ZIM_VM_RRPK	uniqueIdentifier = '3E51E845-91E0-422A-97FC-D7936B76492C'	--SELECT NEWID() FROM JEFF

INSERT INTO eHubRoutingRule
(RR_PK, RR_Condition_Expression, RR_Group_RR_GroupRule, RR_Group_MatchMultiple, RR_Success_CC_Recipient, RR_Group_Ordering)
VALUES (@ZIM_VM_RRPK, '[@DocumentName,Equal,Verified Gross Container Weight]', @ZIM_GroupRule, null, @ZIM_VM_CCPK, 5000)  --RR_Group_Ordering 5000 is from JEFF

ROLLBACK
--COMMIT