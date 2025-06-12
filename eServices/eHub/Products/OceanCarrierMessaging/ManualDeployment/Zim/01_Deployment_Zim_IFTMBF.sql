use eHubTransactions;
GO
SET XACT_ABORT ON;
GO
BEGIN TRANSACTION;

DECLARE @ZIM_CCPK					uniqueidentifier = '7C83E1D9-84CA-4D85-BAEC-2703EEF9BF5A'	--SELECT NEWID() FROM JEFF
DECLARE @ZIM_BK1_CCPK				uniqueIdentifier = '3C532783-48E8-4B51-81EC-B13ABE6EA86B'	--SELECT NEWID() FROM JEFF

DECLARE @ZIM_TSPK					uniqueIdentifier = '919C80A5-713C-47F4-B092-3D00B85EEEF4'	--SELECT NEWID()
DECLARE @ZIM_BK1_TSPK				uniqueidentifier = 'FE10750E-E059-46F5-89C5-F3D8AE475E68'	--SELECT NEWID()

DECLARE @ShippingInstruction_CCPK	uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'SHIPPING_INSTRUCTION')
DECLARE @ShippingIntruction_TSPK	uniqueIdentifier = (SELECT TS_PK FROM eHubTransformationSet WHERE TS_CC_Recipient = @ShippingInstruction_CCPK and TS_Name = 'OCM System Configuration')

DECLARE @ZIM_BK1_CCID				varchar(50) = 'ZIM_BK1'
DECLARE @CarrierName				varchar(50) = 'ZIM'
DECLARE @CarrierID					varchar(50) = 'ZIMID'
DECLARE @CarrierMSG					varchar(50) = 'ZIMMSG'
DECLARE @CarrierBRS					varchar(50) = 'ZIMBRS'
DECLARE @CarrierPrefix				varchar(50) = 'ZIM'

DECLARE @Carrier_Config_TSName		varchar(100) = concat(@CarrierName, ' Provider Configuration')

----------------- New Client For Mapping
INSERT INTO eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
VALUES
(@ZIM_CCPK, @CarrierName, @CarrierName,'00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party')

INSERT INTO eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
VALUES
(@ZIM_BK1_CCPK, @ZIM_BK1_CCID, concat(@CarrierName, ' - Booking Request (v1)'),'00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party')

------------------ eHubTransformationSet / eHubTransformationMapping
DECLARE @UI2US_v1_BK_TTPK				uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1.UniversalInterchange2UniversalShipment_BK%')
DECLARE @US2CU_v1_BK_TTPK				uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1.Universal2CarrierUniversal_BK%')
DECLARE @CU2CU_ISO8859_TTPK				uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CU2CUniveralISO8859.CU2CUniveralISO8859%')
DECLARE @CU2IFTMBF_TTPK					uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMBF.CarrierUniversal2IFTMBF%')

DECLARE @UInterchange_2011_DTPK			uniqueIdentifier = (SELECT DT_PK FROM eHubMessageType		 WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange')

INSERT INTO eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient)
SELECT @ZIM_TSPK, @Carrier_Config_TSName, @ZIM_CCPK, @ZIM_CCPK, NULL, @Carrier_Config_TSName, 0, 0	UNION ALL
SELECT @ZIM_BK1_TSPK, concat('Booking Request IFTMBF to ', @CarrierName,' (v1)'), null , @ZIM_BK1_CCPK, @UInterchange_2011_DTPK , concat('Booking Request IFTMBF to ', @CarrierName,' (v1)'), 1, 0

INSERT INTO eHubTransformationMapping
(TM_TS_PK, TM_order, TM_TT_PK)
SELECT @ZIM_BK1_TSPK, 0, @UI2US_v1_BK_TTPK							UNION ALL
SELECT @ZIM_BK1_TSPK, 1, @US2CU_v1_BK_TTPK							UNION ALL
SELECT @ZIM_BK1_TSPK, 2, @CU2CU_ISO8859_TTPK 						UNION ALL
SELECT @ZIM_BK1_TSPK, 3, @CU2IFTMBF_TTPK

-- Code Set OCMIFTMBF : CarrierSettings
DECLARE @Client_OCMIFTMBF			uniqueidentifier =(SELECT CC_PK FROM eHubClient		WHERE CC_ID = 'OCMIFTMBF')
DECLARE @OCMIFTMBF_CSPK				uniqueIdentifier =(SELECT CS_PK FROM eHubCodeSet	WHERE CS_CC_Recipient = @Client_OCMIFTMBF And CS_Name = 'CarrierSettings')
DECLARE @OCMIFTMBF_TSPK				uniqueIdentifier =(SELECT CS_TS FROM eHubCodeSet	WHERE CS_CC_Recipient = @Client_OCMIFTMBF And CS_Name = 'CarrierSettings')

DECLARE @OCMIFTMBF_Name_CRPK		uniqueidentifier =(SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMBF_CSPK And CR_Name = 'CarrierName'),
		@OCMIFTMBF_ID_CRPK			uniqueidentifier =(SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMBF_CSPK And CR_Name = 'ID'),
		@OCMIFTMBF_BRSID_CRPK		uniqueidentifier =(SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMBF_CSPK And CR_Name = 'BRSID'),
		@OCMIFTMBF_MSGID_CRPK		uniqueidentifier =(SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMBF_CSPK And CR_Name = 'MSGID'),
		@OCMIFTMBF_Prefix_CRPK		uniqueidentifier =(SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMBF_CSPK And CR_Name = 'SubscriptionPrefix')

DECLARE @OCMIFTMBF_MaxCK_Order		INT,
		@OCMIFTMBF_MaxCK_PK			uniqueidentifier

SELECT	TOP 1 @OCMIFTMBF_MaxCK_Order = CK_Order, @OCMIFTMBF_MaxCK_PK = CK_PK
FROM	eHubCodeMapKey
WHERE	CK_CS = @OCMIFTMBF_CSPK
ORDER BY CK_Order DESC

UPDATE eHubCodeMapKey
SET CK_Order = CK_Order + 1
WHERE CK_PK = @OCMIFTMBF_MaxCK_PK

INSERT eHubCodeMapKey
(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT '9019BD0E-06DC-4BB2-A0FB-9F2A2112FA52', @OCMIFTMBF_CSPK, @OCMIFTMBF_MaxCK_Order, concat(@CarrierName, '_%')

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey)
SELECT '9019BD0E-06DC-4BB2-A0FB-9F2A2112FA52', @OCMIFTMBF_Name_CRPK, 	@CarrierName, 	NULL	UNION ALL
SELECT '9019BD0E-06DC-4BB2-A0FB-9F2A2112FA52', @OCMIFTMBF_ID_CRPK, 		@CarrierID, 	NULL	UNION ALL
SELECT '9019BD0E-06DC-4BB2-A0FB-9F2A2112FA52', @OCMIFTMBF_MSGID_CRPK, 	@CarrierMSG, 	NULL	UNION ALL
SELECT '9019BD0E-06DC-4BB2-A0FB-9F2A2112FA52', @OCMIFTMBF_BRSID_CRPK, 	@CarrierBRS, 	NULL	UNION ALL
SELECT '9019BD0E-06DC-4BB2-A0FB-9F2A2112FA52', @OCMIFTMBF_Prefix_CRPK, 	@CarrierPrefix, NULL


-- Code Set ZIM : ContainerTypeToISOCode
DECLARE @ISOToContainerType_CSPK		uniqueidentifier = 'FD4FD21C-CFD0-40A3-8249-9CC9DFC6ACD5', 	-- SELECT NEWID()
		@ISOToContainerType_CRPK		uniqueidentifier = 'DEA4D1DB-03B7-4B01-89B8-0DF3DDBE8CEA'	-- SELECT NEWID()

INSERT eHubCodeSet
(CS_PK, CS_Name, CS_TS, CS_CC_Sender, CS_CC_Recipient, CS_Key1Name)
SELECT @ISOToContainerType_CSPK, 'ContainerTypeToISOCode', @ZIM_TSPK, @ZIM_CCPK, @ZIM_CCPK , 'Container Type'

INSERT eHubCodeSetResult
(CR_PK, CR_CS, CR_Order, CR_Name)
SELECT @ISOToContainerType_CRPK, @ISOToContainerType_CSPK, 1, concat(@CarrierName, ' Code')

INSERT eHubCodeMapKey
(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT '89BE1F97-9A27-4C8A-9E7E-EC1B87D64B3C', @ISOToContainerType_CSPK, 1, '%'

INSERT eHubCodeMapValue
(CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey)
SELECT '89BE1F97-9A27-4C8A-9E7E-EC1B87D64B3C', @ISOToContainerType_CRPK, '', null

-- Code Set ZIM : Package Type
DECLARE @PackageTypeCodeSet			uniqueIdentifier = '675C1BD4-83DC-4B1F-86D2-A9E96D8E1B9D'	-- SELECT NEWID()
DECLARE @PackageTypeCodeSetResult	uniqueIdentifier = 'D5A573B5-8C18-411A-856B-8FBD2056800F'	-- SELECT NEWID()

INSERT INTO eHubCodeSet
(CS_PK, CS_Name, CS_TS, CS_CC_Sender, CS_CC_Recipient, CS_Key1Name)
SELECT @PackageTypeCodeSet, N'Package Type', @ZIM_TSPK, @ZIM_CCPK, @ZIM_CCPK, N'ediEnterprise Code'

INSERT INTO eHubCodeSetResult
(CR_PK, CR_CS, CR_Order, CR_Name)
SELECT @PackageTypeCodeSetResult, @PackageTypeCodeSet, 1, concat(@CarrierName, ' Code')

INSERT INTO eHubCodeMapKey
(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT 'B6DF0322-96F8-436C-BD7D-C1157877EDA7', @PackageTypeCodeSet, 1, '%'

INSERT INTO eHubCodeMapValue
(CV_CK, CV_CR, CV_OutputCode)
SELECT 'B6DF0322-96F8-436C-BD7D-C1157877EDA7', @PackageTypeCodeSetResult, ''


------------------eHubRoutingRule
DECLARE @ZIM_GroupRule	uniqueidentifier = 'B726FABA-D568-4957-92DC-923232BAF92E'  --ZIM Group Rule FROM JEFF
DECLARE @ZIM_BK1_RRPK	uniqueIdentifier = '4E21AEFE-E8C2-45B0-9E24-2121ABFA593C'	--SELECT NEWID() FROM JEFF

INSERT INTO eHubRoutingRule
(RR_PK, RR_Condition_Expression, RR_Group_RR_GroupRule, RR_Group_MatchMultiple, RR_Success_CC_Recipient, RR_Group_Ordering)
VALUES (@ZIM_GroupRule, NULL, NULL, 0, NULL, NULL)

INSERT INTO eHubRoutingRule
(RR_PK, RR_Condition_Expression, RR_Group_RR_GroupRule, RR_Group_MatchMultiple, RR_Success_CC_Recipient, RR_Group_Ordering)
VALUES (@ZIM_BK1_RRPK, '[@UShipmentNamespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/BookingRequest/1]', @ZIM_GroupRule, null, @ZIM_BK1_CCPK, 1000)


------------------- eHubRegistrationType/eHubServiceProvider/eHubServiceProviderRequiredRegistration
DECLARE @ServiceProviderPk		uniqueIdentifier = '2A448B1B-0149-405A-9F5A-3AEBFFB7E280'	-- SELECT NEWID() FROM JEFF
DECLARE @RegistrationTypePk		uniqueIdentifier = 'C6C73DF8-4256-4220-BAB7-7068CFB80643'	-- SELECT NEWID()

INSERT INTO eHubRegistrationType
(RT_PK, RT_ID, RT_Description, RT_RegistrantType)
SELECT @RegistrationTypePk, @CarrierName, concat(@CarrierName, ' Client ID'), 'Client'

INSERT INTO eHubServiceProvider(SP_CC_Service, SP_CC_Provider, SP_RR, SP_PK)
SELECT @ShippingInstruction_CCPK, @ZIM_CCPK, @ZIM_GroupRule, @ServiceProviderPk

INSERT INTO eHubServiceProviderRequiredRegistration(SX_RT, SX_LookupFactName, SX_QualifierFactName, SX_SP)
SELECT  @RegistrationTypePk, 'SourceParty', 'EventBranch', @ServiceProviderPk

------------------- eHubSubscriptionType
DECLARE @Subscription_ID		uniqueIdentifier = '4D7FBBA3-52C0-4C84-B827-4E3EEAE40DE7' -- SELECT NEWID()
DECLARE @Subscription_MSG		uniqueIdentifier = '8E8C57F2-7EA8-4A13-A282-124C8407D60A' -- SELECT NEWID()
DECLARE @Subscription_BRS		uniqueIdentifier = '46104B5A-A314-46CB-AC13-AD0EA8035E7E' -- SELECT NEWID()

INSERT INTO eHubSubscriptionType
(ST_PK, ST_ID, ST_Name, ST_ExpiryDays)
SELECT @Subscription_ID,	@CarrierID,	 concat(@CarrierName, ' ID'), 180						UNION ALL
SELECT @Subscription_MSG,	@CarrierMSG, concat(@CarrierName, ' Message Reference'), 180		UNION ALL
SELECT @Subscription_BRS,	@CarrierBRS, concat(@CarrierName, ' Booking Request Status'), 180

------------------ eHubCounter
INSERT INTO eHubCounter(CN_Name, CN_Value)
SELECT concat('CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.',@CarrierName, '.UNH1'), 1 UNION ALL
SELECT concat('CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.',@CarrierName, '.BGM'), 1

-- Code Set SHIPPING INSTRUCTION : UNB3
DECLARE @UNB3_CSPK				uniqueidentifier = (SELECT CS_PK FROM eHubCodeSet		WHERE CS_CC_Recipient = @ShippingInstruction_CCPK And CS_Name = 'UNB3')
DECLARE @UNB3_SenderID_CRPK		uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @UNB3_CSPK And CR_Name = 'PartySenderID')
DECLARE @UNB3_ReceiverID_CRPK	uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @UNB3_CSPK And CR_Name = 'PartyReceiverID')

DECLARE @UNB3_MaxCKOrder INT,
		@UNB3_MaxCKOrder_CKPK uniqueidentifier

SELECT TOP 1 @UNB3_MaxCKOrder = CK_Order, @UNB3_MaxCKOrder_CKPK = CK_PK
FROM eHubCodeMapKey
WHERE CK_CS = @UNB3_CSPK
ORDER BY CK_Order DESC

UPDATE eHubCodeMapKey
SET CK_Order =  CK_Order + 1
WHERE CK_PK = @UNB3_MaxCKOrder_CKPK

INSERT eHubCodeMapKey(CK_PK, CK_CS, CK_Order, CK_Key1Value, CK_Key2Value)
SELECT '3A43DF27-DE68-4835-92A6-DEE8D2D98DC7', @UNB3_CSPK, @UNB3_MaxCKOrder, 	 concat(@CarrierName, '_%'), '%'

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode)
SELECT '3A43DF27-DE68-4835-92A6-DEE8D2D98DC7', @UNB3_SenderID_CRPK, 'CARGOWISE' UNION ALL
SELECT '3A43DF27-DE68-4835-92A6-DEE8D2D98DC7', @UNB3_ReceiverID_CRPK, 'ZIMU'

ROLLBACK
--COMMIT