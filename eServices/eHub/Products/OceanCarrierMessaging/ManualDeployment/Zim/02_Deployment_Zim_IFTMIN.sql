use eHubTransactions;
GO
SET XACT_ABORT ON;
GO
BEGIN TRANSACTION;

DECLARE @ZIM_SI1_CCID				varchar(50) = 'ZIM_SI1'
DECLARE @CarrierName				varchar(50) = 'ZIM'

DECLARE @ZIM_CCPK					uniqueidentifier = (SELECT CC_PK PK FROM eHubClient WHERE CC_ID = @CarrierName)
DECLARE @ZIM_SI1_CCPK				uniqueIdentifier = '83FC0FAF-B40D-4253-8482-86EEE57957F4'	--SELECT NEWID() FROM JEFF

DECLARE @Carrier_Config_TSName		varchar(100) = concat(@CarrierName, ' Provider Configuration')

DECLARE @ZIM_TSPK					uniqueIdentifier = (SELECT TS_PK FROM eHubTransformationSet WHERE TS_Name = @Carrier_Config_TSName AND TS_CC_Recipient = @ZIM_CCPK)
DECLARE @ZIM_SI1_TSPK				uniqueidentifier = '4DBC444E-A285-48CB-8F09-EDE51A949C37'	--SELECT NEWID()

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
(@ZIM_SI1_CCPK, @ZIM_SI1_CCID, concat(@CarrierName, ' - Shipping Instruction (v1)'),'00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party')

------------------ eHubTransformationSet / eHubTransformationMapping
DECLARE @UI2US_v1_SI_TTPK				uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1.UniversalInterchange2UniversalShipment_SI%')
DECLARE @US2CU_v1_SI_TTPK				uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1.Universal2CarrierUniversal_SI%')
DECLARE @CU2CU_ISO8859_TTPK				uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CU2CUniveralISO8859.CU2CUniveralISO8859%')
DECLARE @CU2IFTMIN_TTPK					uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMIN.CarrierUniversal2IFTMIN%')

DECLARE @UInterchange_2011_DTPK			uniqueIdentifier = (SELECT DT_PK FROM eHubMessageType		 WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange')

INSERT INTO eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient)
SELECT @ZIM_SI1_TSPK, concat('Shipping Instruction IFTMIN to ', @CarrierName,' (v1)'), null , @ZIM_SI1_CCPK, @UInterchange_2011_DTPK , concat('Shipping Instruction IFTMIN to ', @CarrierName,' (v1)'), 1, 0

INSERT INTO eHubTransformationMapping
(TM_TS_PK, TM_order, TM_TT_PK)
SELECT @ZIM_SI1_TSPK, 0, @UI2US_v1_SI_TTPK							UNION ALL
SELECT @ZIM_SI1_TSPK, 1, @US2CU_v1_SI_TTPK							UNION ALL
SELECT @ZIM_SI1_TSPK, 2, @CU2CU_ISO8859_TTPK 						UNION ALL
SELECT @ZIM_SI1_TSPK, 3, @CU2IFTMIN_TTPK

-- Code Set  OCMIFTMIN  : CarrierSettings
DECLARE @OCMIFTMIN_CCPK			uniqueidentifier = (SELECT CC_PK FROM eHubClient	WHERE CC_ID = 'OCMIFTMIN')
DECLARE @OCMIFTMIN_CSPK			uniqueIdentifier = (SELECT CS_PK FROM eHubCodeSet	WHERE CS_CC_Recipient = @OCMIFTMIN_CCPK And CS_Name = 'CarrierSettings')
DECLARE @OCMIFTMIN_TSPK			uniqueIdentifier = (SELECT CS_TS FROM eHubCodeSet	WHERE CS_CC_Recipient = @OCMIFTMIN_CCPK And CS_Name = 'CarrierSettings')

DECLARE @OCMIFTMIN_Name_CRPK	uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMIN_CSPK And CR_Name = 'CarrierName'),
		@OCMIFTMIN_ID_CRPK		uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMIN_CSPK And CR_Name = 'ID'),
		@OCMIFTMIN_MSGID_CRPK	uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMIN_CSPK And CR_Name = 'MSGID'),
		@OCMIFTMIN_Prefix_CRPK	uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMIN_CSPK And CR_Name = 'SubscriptionPrefix')

DECLARE @MaxCK_Order				INT,
		@MaxCK_PK					uniqueidentifier

SELECT	TOP 1 @MaxCK_Order = CK_Order, @MaxCK_PK = CK_PK
FROM	eHubCodeMapKey
WHERE	CK_CS = @OCMIFTMIN_CSPK
ORDER BY CK_Order DESC

UPDATE eHubCodeMapKey
SET CK_Order = CK_Order + 1
WHERE CK_PK = @MaxCK_PK

INSERT eHubCodeMapKey
(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT '2E3E1502-2788-442D-8B7F-5E331A1E8382', @OCMIFTMIN_CSPK, @MaxCK_Order, concat(@CarrierName, '_%')

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey)
SELECT '2E3E1502-2788-442D-8B7F-5E331A1E8382', @OCMIFTMIN_Name_CRPK, @CarrierName, NULL				UNION ALL
SELECT '2E3E1502-2788-442D-8B7F-5E331A1E8382', @OCMIFTMIN_ID_CRPK, @CarrierID, NULL					UNION ALL
SELECT '2E3E1502-2788-442D-8B7F-5E331A1E8382', @OCMIFTMIN_MSGID_CRPK, @CarrierMSG, NULL				UNION ALL
SELECT '2E3E1502-2788-442D-8B7F-5E331A1E8382', @OCMIFTMIN_Prefix_CRPK, @CarrierPrefix, NULL


------------------eHubRoutingRule
DECLARE @ZIM_GroupRule	uniqueidentifier = 'B726FABA-D568-4957-92DC-923232BAF92E'  --ZIM Group Rule FROM JEFF
DECLARE @ZIM_SI1_RRPK	uniqueIdentifier = 'D51D3520-E801-47A5-A835-CAA0288F7E87'	--SELECT NEWID() FROM JEFF

INSERT INTO eHubRoutingRule
(RR_PK, RR_Condition_Expression, RR_Group_RR_GroupRule, RR_Group_MatchMultiple, RR_Success_CC_Recipient, RR_Group_Ordering)
VALUES (@ZIM_SI1_RRPK, '[@UShipmentNamespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/ShippingInstruction/1]', @ZIM_GroupRule, null, @ZIM_SI1_CCPK, 2000)

ROLLBACK
--COMMIT