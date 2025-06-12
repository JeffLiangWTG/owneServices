use eHubTransactions;
GO
SET XACT_ABORT ON;
GO
BEGIN TRANSACTION;

DECLARE @HAMBURG_SUD_CCPK					uniqueidentifier = 'FDE103FF-BA00-45F7-8F97-289BA875EDCC'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_TSPK					uniqueIdentifier = '5A573C6D-3B5A-48E5-96DF-7564EABDA48F'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_OCMAPERAK_D99B_TSPK	uniqueIdentifier = 'D492BE45-6248-46A6-B6B9-99493A731039'	-- select NEWID()
DECLARE @HAMBURG_SUD_OCMAPERAK_D04A_TSPK	uniqueIdentifier = '43AC4676-62E4-4EAE-A646-0F1802945B8D'	-- select NEWID()
DECLARE @HAMBURG_SUD_GCTAPERAK_D99B_TSPK	uniqueIdentifier = '8EBDD101-3C9B-4AC7-8715-C758CFDBECB7'	-- select NEWID()
DECLARE @HAMBURG_SUD_GCTAPERAK_D04A_TSPK	uniqueIdentifier = '34B7F46B-129F-4A89-96F6-C079A689AB6E'	-- select NEWID()
DECLARE @ContainerTracking_IFTMCS_TSPK		uniqueidentifier = '6EA4DEB3-7D40-4E81-AAEC-AB42C7798D09'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_IFTMCS_TSPK			uniqueIdentifier = 'F154B68D-012D-43CF-9437-21A020D84D06'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_IFTSTA_TSPK			uniqueIdentifier = '1CBBE4CC-94A4-48DB-AD3B-3AE6CC82464F'	-- SELECT NEWID()
DECLARE @HAMBURG_SUD_IFTMBC_TSPK			uniqueIdentifier = 'CC99343C-72E0-490E-B2DD-DDA3875C01B6'	-- SELECT NEWID()
DECLARE @HAMBURG_SUD_OCMCONTRL_TSPK			uniqueIdentifier = '39BD3A6D-C940-4348-8B56-D482EA892830'	-- select NEWID()

DECLARE @HAMBURG_SUD_BK1_CCPK				uniqueIdentifier = 'E6967EFE-A705-4CA1-9AF6-725B8CED0E14'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_SI1_CCPK				uniqueIdentifier = '5CF2C150-DCD3-43BA-B0A7-7397BD8CABE7'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_VM_CCPK				uniqueidentifier = '0A15A49F-833D-48F2-9433-C57C55E81AB1'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_VM2_CCPK				uniqueidentifier = '5FA6F96C-CA26-47CC-97C0-F89E6DFA8026'	--SELECT NEWID()

DECLARE @HAMBURG_SUD_BK1_TSPK				uniqueidentifier = '735A1A78-F14D-415F-90E5-14C51DCA672F'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_SI1_TSPK				uniqueidentifier = '281315BB-866D-4ABA-8B92-3F11E84BC267'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_VM_TSPK				uniqueidentifier = '85D50621-75F1-4717-83D0-56F09422DCC6'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_VM2_TSPK				uniqueidentifier = 'E41CDEB3-886D-4BD3-BD48-B5052BFC424D'	--SELECT NEWID()

DECLARE @UInterchange_2011_DTPK				uniqueIdentifier = (SELECT DT_PK FROM eHubMessageType	WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange')
DECLARE @UShipment_2011_TTPK				uniqueIdentifier = (SELECT DT_PK FROM eHubMessageType	WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2011/11#UniversalShipment')
DECLARE @D99B_APERAK_DTPK					uniqueidentifier = (SELECT DT_PK FROM eHubMessageType	WHERE DT_Code = 'http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D99B_APERAK')
DECLARE @D04A_APERAK_DTPK					uniqueidentifier = (SELECT DT_PK FROM eHubMessageType	WHERE DT_Code = 'http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D04A_APERAK')
DECLARE @D99B_IFTMCS_DTPK					uniqueidentifier = (SELECT DT_PK FROM eHubMessageType	WHERE DT_Code = 'http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D99B_IFTMCS')
DECLARE @D99B_IFTSTA_DTPK					uniqueIdentifier = (SELECT DT_PK from eHubMessageType	WHERE DT_Code = 'http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D99B_IFTSTA')
DECLARE @D99B_IFTMBC_DTPK					uniqueIdentifier = (SELECT DT_PK FROM eHubMessageType	WHERE DT_Code = 'http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D99B_IFTMBC')
DECLARE @CONTRL_DTPK						uniqueidentifier = (SELECT DT_PK FROM eHubMessageType	WHERE DT_Code = 'http://schemas.microsoft.com/Edi/Edifact#Efact_Contrl_Root')

DECLARE @ShippingInstruction_CCPK			uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'SHIPPING_INSTRUCTION')
DECLARE @ShippingIntruction_TSPK			uniqueIdentifier = (SELECT TS_PK FROM eHubTransformationSet WHERE TS_CC_Recipient = @ShippingInstruction_CCPK and TS_Name = 'OCM System Configuration')
DECLARE @ContainerTracking_CCPK				uniqueidentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'CONTAINER_TRACKING')
DECLARE @OCMAPERAK_CCPK						uniqueidentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'OCMAPERAK')
DECLARE @OCMIFTMCS_CCPK	        			uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'OCMIFTMCS')
DECLARE @OCMCONTRL_CCPK						uniqueidentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'OCMCONTRL')

DECLARE @HAMBURG_SUD_BK1_CCID				varchar(50) = 'HAMBURGSUD_BK1'
DECLARE @HAMBURG_SUD_SI1_CCID				varchar(50) = 'HAMBURGSUD_SI1'
DECLARE @HAMBURG_SUD_VM_CCID				varchar(50) = 'HAMBURGSUD_VM'
DECLARE @HAMBURG_SUD_VM2_CCID				varchar(50) = 'HAMBURGSUD_VM2'

DECLARE @CarrierName						varchar(50) = 'HAMBURGSUD'
DECLARE @CarrierID							varchar(50) = 'HSDID'
DECLARE @CarrierMSG							varchar(50) = 'HSDMSG'
DECLARE @CarrierBRS							varchar(50) = 'HSDBRS'
DECLARE @CarrierPrefix						varchar(50) = 'HSD'

DECLARE @Carrier_Config_TSName				varchar(100) = concat(@CarrierName, ' Provider Configuration')

----------------- New Client For Mapping
INSERT INTO eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory)
SELECT @HAMBURG_SUD_CCPK, @CarrierName, @CarrierName,'00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party' UNION ALL
SELECT @HAMBURG_SUD_BK1_CCPK, @HAMBURG_SUD_BK1_CCID, concat(@CarrierName, ' - Booking Request (v1)'),'00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party' UNION ALL
SELECT @HAMBURG_SUD_SI1_CCPK, @HAMBURG_SUD_SI1_CCID, concat(@CarrierName, ' - Shipping Instruction (v1)'),'00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party'	UNION ALL
SELECT @HAMBURG_SUD_VM_CCPK, @HAMBURG_SUD_VM_CCID, concat(@CarrierName, ' - Verified Gross Mass'),'00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party'				UNION ALL
SELECT @HAMBURG_SUD_VM2_CCPK, @HAMBURG_SUD_VM2_CCID, concat(@CarrierName, ' Verified Gross Mass Per Container'),'00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party'

------------------ eHubTransformationSet / eHubTransformationMapping
--IFTMBF
DECLARE @UI2US_v1_BK_TTPK				uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1.UniversalInterchange2UniversalShipment_BK%')
DECLARE @US2CU_v1_BK_TTPK				uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1.Universal2CarrierUniversal_BK%')
DECLARE @CU2CU_ISO8859_TTPK				uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CU2CUniveralISO8859.CU2CUniveralISO8859%')
DECLARE @CU2IFTMBF_TTPK					uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMBF.CarrierUniversal2IFTMBF%')
--IFTMIN
DECLARE @UI2US_v1_SI_TTPK				uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1.UniversalInterchange2UniversalShipment_SI%')
DECLARE @US2CU_v1_SI_TTPK				uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1.Universal2CarrierUniversal_SI%')
DECLARE @CU2IFTMIN_TTPK					uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMIN.CarrierUniversal2IFTMIN%')
--VERMAS
DECLARE @UI_v1tov2_TTPK					uniqueIdentifier = (Select TT_PK from eHubTransformationType where TT_TransformationType LIKE 'CargoWise.eHub.Clients.EDI.Universal.Transforms.VersionConversion.UniversalInterchangeShpV2_1ToV2%')
DECLARE @UI2US_2012_TTPK				uniqueIdentifier = (Select TT_PK from eHubTransformationType where TT_TransformationType LIKE 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchange2UniversalShipment%')
DECLARE @US2CU_TTPK						uniqueIdentifier = (Select TT_PK from eHubTransformationType where TT_TransformationType LIKE 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.Universal2CarrierUniversal%')
DECLARE @CU2VM_TTPK						uniqueIdentifier = (Select TT_PK from eHubTransformationType where TT_TransformationType LIKE 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2VERMAS_SMDG.CarrierUniversal2VERMAS_SMDG%')
--VERMAS v2
DECLARE @UI2US_v2_TTPK					uniqueIdentifier = (Select TT_PK from eHubTransformationType where TT_TransformationType LIKE 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UInterchange2UShipment.V2.UniversalInterchange2UniversalShipment_V2%')
DECLARE @US2VERMAS_TTPK					uniqueIdentifier = (Select TT_PK from eHubTransformationType where TT_TransformationType LIKE 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2VERMAS.UniversalShipment2VERMAS%')
--APERAK
DECLARE @UII2UIE_TransType				uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchangeInclude2UniversalInterchangeEnvelope%')
DECLARE @OCM_APERAK2UI_D99B_TransType   uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.APERAK2UInterchangeInclude.APERAK2UInterchangeInclude%')
DECLARE @GCT_APERAK2UI_D99B_TransType	uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.GCT.Transforms.APERAK2UInterchangeInclude.APERAK2UInterchangeInclude%')
DECLARE @OCM_APERAK2UI_D04A_TransType	uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.APERAK2UInterchangeInclude_D04A.APERAK2UInterchangeInclude_D04A%')
DECLARE @GCT_APERAK2UI_D04A_TransType	uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.GCT.Transforms.APERAK2UInterchangeInclude_D04A.APERAK2UInterchangeInclude_D04A%')

DECLARE @IFTMCS2UIEnvelop				uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType where TT_TransformationType Like 'CargoWise.eHub.Products.GCT.Transforms.IFTMCS2UInterchangeEnvelope.IFTMCS2UInterchangeEnvelope%')
DECLARE @IFTMCS2UInterchange			uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType where TT_TransformationType Like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.IFTMCS2UniversalInterchange.IFTMCS2UniversalInterchange%')

DECLARE @IFTSTAGeneric_TransType	uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType where TT_TransformationType = 'CargoWise.eHub.Products.GCT.Transforms.IFTSTA2UInterchange_Generic, CargoWise.eHub.Products.GCT.Transforms.IFTSTA2UInterchange_Generic, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')
DECLARE @UII2UI_TransType			uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType where TT_TransformationType = 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchangeInclude2UniversalInterchange, CargoWise.eHub.Clients.EDI.Universal_2012_11, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')

DECLARE @Control2UE_TransType uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CONTRL2UEvent.CONTRL2UniversalEvent%')
DECLARE @UI2UI_2012_TransType uniqueidentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchangeInclude2UniversalInterchange, CargoWise.eHub.Clients.EDI.Universal_2012_11%')

DECLARE @IFTMBC2UII_TransType			uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType LIKE 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.IFTMBC2UShipment.IFTMBC2UniversalShipment%')

INSERT INTO eHubTransformationSet
(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient)
SELECT @HAMBURG_SUD_BK1_TSPK, concat('Booking Request IFTMBF to ', @CarrierName,' (v1)'), null , @HAMBURG_SUD_BK1_CCPK, @UInterchange_2011_DTPK , concat('Booking Request IFTMBF to ', @CarrierName,' (v1)'), 1, 0			UNION ALL
SELECT @HAMBURG_SUD_SI1_TSPK, concat('Shipping Instruction IFTMIN to ', @CarrierName,' (v1)'), null , @HAMBURG_SUD_SI1_CCPK, @UInterchange_2011_DTPK , concat('Shipping Instruction IFTMIN to ', @CarrierName,' (v1)'), 1, 0	UNION ALL
SELECT @HAMBURG_SUD_TSPK, @Carrier_Config_TSName, @HAMBURG_SUD_CCPK, @HAMBURG_SUD_CCPK, NULL, @Carrier_Config_TSName, 0, 0	UNION ALL
SELECT @HAMBURG_SUD_VM_TSPK, concat('Verified Gross Container Wgt VERMAS to ', @CarrierName), NULL, @HAMBURG_SUD_VM_CCPK, @UInterchange_2011_DTPK, concat('Verified Gross Container Weight VERMAS to ', @CarrierName),1, 0 UNION ALL
SELECT @HAMBURG_SUD_VM2_TSPK, concat('VGM Per Container VERMAS to ', @CarrierName), NULL, @HAMBURG_SUD_VM2_CCPK, @UInterchange_2011_DTPK, concat('VGM Per Container VERMAS to ', @CarrierName), 1, 0 UNION ALL
SELECT @HAMBURG_SUD_OCMAPERAK_D99B_TSPK,'OCM APERAK D99B From HAMBURGSUD', @HAMBURG_SUD_CCPK, null, @D99B_APERAK_DTPK, 'OCM APERAK D99B From HAMBURGSUD', 0, 1 UNION ALL
SELECT @HAMBURG_SUD_GCTAPERAK_D99B_TSPK,'GCT APERAK D99B From HAMBURGSUD', @HAMBURG_SUD_CCPK, @ContainerTracking_CCPK, @D99B_APERAK_DTPK,'GCT APERAK D99B From HAMBURGSUD',0, 0 UNION ALL
SELECT @HAMBURG_SUD_OCMAPERAK_D04A_TSPK,'OCM APERAK D04A from HAMBURGSUD', @HAMBURG_SUD_CCPK, null, @D04A_APERAK_DTPK, 'OCM APERAK D04A from HAMBURGSUD', 0, 1 UNION ALL
SELECT @HAMBURG_SUD_GCTAPERAK_D04A_TSPK,'GCT APERAK D04A From HAMBURGSUD', @HAMBURG_SUD_CCPK, @ContainerTracking_CCPK, @D04A_APERAK_DTPK,'GCT APERAK D04A From HAMBURGSUD',0, 0 UNION ALL
SELECT @ContainerTracking_IFTMCS_TSPK, 'Shipping Instruction IFTMCS from HAMBURG_SUD', @HAMBURG_SUD_CCPK, @ContainerTracking_CCPK, @D99B_IFTMCS_DTPK, 'Shipping Instruction IFTMCS from HAMBURG_SUD', 0, 0 UNION ALL
SELECT @HAMBURG_SUD_IFTMCS_TSPK, 'Shipping Instruction IFTMCS from HAMBURG_SUD', @HAMBURG_SUD_CCPK,	null, @D99B_IFTMCS_DTPK, 'Shipping Instruction IFTMCS from HAMBURG_SUD', 0, 1 UNION ALL
SELECT @HAMBURG_SUD_IFTSTA_TSPK, 'HAMBURG_SUD IFTSTA to UniversalInterchange', @HAMBURG_SUD_CCPK, null, @D99B_IFTSTA_DTPK , 'HAMBURG_SUD IFTSTA to UniversalInterchange', 0, 1 UNION ALL
SELECT @HAMBURG_SUD_IFTMBC_TSPK, 'Booking Confirmation IFTMBC from HAMBURGSUD', @HAMBURG_SUD_CCPK, null, @D99B_IFTMBC_DTPK, 'Booking Confirmation IFTMBC from HAMBURGSUD', 0, 1 UNION ALL
SELECT @HAMBURG_SUD_OCMCONTRL_TSPK, 'OCM CONTRL from HAMBURG_SUD', @HAMBURG_SUD_CCPK, null, @CONTRL_DTPK, 'OCM CONTRL from HAMBURG_SUD', 0, 1

INSERT INTO eHubTransformationMapping
(TM_TS_PK, TM_order, TM_TT_PK)
SELECT @HAMBURG_SUD_BK1_TSPK, 0, @UI2US_v1_BK_TTPK							UNION ALL
SELECT @HAMBURG_SUD_BK1_TSPK, 1, @US2CU_v1_BK_TTPK							UNION ALL
SELECT @HAMBURG_SUD_BK1_TSPK, 2, @CU2CU_ISO8859_TTPK 						UNION ALL
SELECT @HAMBURG_SUD_BK1_TSPK, 3, @CU2IFTMBF_TTPK							UNION ALL
SELECT @HAMBURG_SUD_SI1_TSPK, 0, @UI2US_v1_SI_TTPK							UNION ALL
SELECT @HAMBURG_SUD_SI1_TSPK, 1, @US2CU_v1_SI_TTPK							UNION ALL
SELECT @HAMBURG_SUD_SI1_TSPK, 2, @CU2CU_ISO8859_TTPK 						UNION ALL
SELECT @HAMBURG_SUD_SI1_TSPK, 3, @CU2IFTMIN_TTPK							UNION ALL
select @HAMBURG_SUD_VM_TSPK, 0, @UI_v1tov2_TTPK								UNION ALL
select @HAMBURG_SUD_VM_TSPK, 1, @UI2US_2012_TTPK							UNION ALL
select @HAMBURG_SUD_VM_TSPK, 2, @US2CU_TTPK									UNION ALL
select @HAMBURG_SUD_VM_TSPK, 3, @CU2CU_ISO8859_TTPK 						UNION ALL
select @HAMBURG_SUD_VM_TSPK, 4, @CU2VM_TTPK									UNION ALL
select @HAMBURG_SUD_VM2_TSPK, 0, @UI2US_v2_TTPK								UNION ALL
select @HAMBURG_SUD_VM2_TSPK, 1, @US2VERMAS_TTPK							UNION ALL
SELECT @HAMBURG_SUD_OCMAPERAK_D99B_TSPK, 0, @OCM_APERAK2UI_D99B_TransType   UNION ALL
SELECT @HAMBURG_SUD_OCMAPERAK_D99B_TSPK, 1, @UII2UIE_TransType				UNION ALL
SELECT @HAMBURG_SUD_GCTAPERAK_D99B_TSPK, 0, @GCT_APERAK2UI_D99B_TransType	UNION ALL
SELECT @HAMBURG_SUD_GCTAPERAK_D99B_TSPK, 1, @UII2UIE_TransType				UNION ALL
SELECT @HAMBURG_SUD_OCMAPERAK_D04A_TSPK, 0, @OCM_APERAK2UI_D04A_TransType   UNION ALL
SELECT @HAMBURG_SUD_OCMAPERAK_D04A_TSPK, 1, @UII2UIE_TransType				UNION ALL
SELECT @HAMBURG_SUD_GCTAPERAK_D04A_TSPK, 0, @GCT_APERAK2UI_D04A_TransType	UNION ALL
SELECT @HAMBURG_SUD_GCTAPERAK_D04A_TSPK, 1, @UII2UIE_TransType				UNION ALL
SELECT @ContainerTracking_IFTMCS_TSPK, 0, @IFTMCS2UIEnvelop 				UNION ALL
SELECT @HAMBURG_SUD_IFTMCS_TSPK, 0, @IFTMCS2UInterchange					UNION ALL
SELECT @HAMBURG_SUD_IFTMCS_TSPK, 1, @UII2UIE_TransType						UNION ALL
SELECT @HAMBURG_SUD_IFTSTA_TSPK, 0, @IFTSTAGeneric_TransType				UNION ALL
SELECT @HAMBURG_SUD_IFTSTA_TSPK, 1, @UII2UI_TransType						UNION ALL
SELECT @HAMBURG_SUD_OCMCONTRL_TSPK, 0, @Control2UE_TransType 				UNION ALL
SELECT @HAMBURG_SUD_OCMCONTRL_TSPK, 1, @UI2UI_2012_TransType				UNION ALL
SELECT @HAMBURG_SUD_IFTMBC_TSPK, 0, @IFTMBC2UII_TransType					UNION ALL
SELECT @HAMBURG_SUD_IFTMBC_TSPK, 1, @UII2UIE_TransType 

-- Code Set OCMIFTMBF : CarrierSettings
DECLARE @Client_OCMIFTMBF			uniqueidentifier =(SELECT CC_PK FROM eHubClient		WHERE CC_ID = 'OCMIFTMBF')
DECLARE @OCMIFTMBF_CSPK				uniqueIdentifier =(SELECT CS_PK FROM eHubCodeSet	WHERE CS_CC_Recipient = @Client_OCMIFTMBF And CS_Name = 'CarrierSettings')
DECLARE @OCMIFTMBF_TSPK				uniqueIdentifier =(SELECT CS_TS FROM eHubCodeSet	WHERE CS_CC_Recipient = @Client_OCMIFTMBF And CS_Name = 'CarrierSettings')

DECLARE @OCMIFTMBF_Name_CRPK		uniqueidentifier =(SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMBF_CSPK And CR_Name = 'CarrierName'),
		@OCMIFTMBF_ID_CRPK			uniqueidentifier =(SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMBF_CSPK And CR_Name = 'ID'),
		@OCMIFTMBF_BRSID_CRPK		uniqueidentifier =(SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMBF_CSPK And CR_Name = 'BRSID'),
		@OCMIFTMBF_MSGID_CRPK		uniqueidentifier =(SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMBF_CSPK And CR_Name = 'MSGID'),
		@OCMIFTMBF_Prefix_CRPK		uniqueidentifier =(SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMBF_CSPK And CR_Name = 'SubscriptionPrefix')

DECLARE @OCMIFTMBF_New_CKPK			uniqueIdentifier = '89CAFD0A-666C-4EF9-884C-7EC3975933B0' --SELECT NEWID()
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
SELECT @OCMIFTMBF_New_CKPK, @OCMIFTMBF_CSPK, @OCMIFTMBF_MaxCK_Order, concat(@CarrierName, '_%')

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey)
SELECT @OCMIFTMBF_New_CKPK, @OCMIFTMBF_Name_CRPK, @CarrierName	, NULL	UNION ALL
SELECT @OCMIFTMBF_New_CKPK, @OCMIFTMBF_ID_CRPK, @CarrierID		, NULL	UNION ALL
SELECT @OCMIFTMBF_New_CKPK, @OCMIFTMBF_MSGID_CRPK, @CarrierMSG	, NULL	UNION ALL
SELECT @OCMIFTMBF_New_CKPK, @OCMIFTMBF_BRSID_CRPK, @CarrierBRS	, NULL	UNION ALL
SELECT @OCMIFTMBF_New_CKPK, @OCMIFTMBF_Prefix_CRPK, @CarrierPrefix, NULL

-- Code Set OCMIFTMBC : CarrierSettings
DECLARE @Client_OCMIFTMBC			uniqueIdentifier = (SELECT CC_PK FROM eHubClient		WHERE CC_ID = 'OCMIFTMBC')
DECLARE @OCMIFTMBC_CSPK				uniqueidentifier = (SELECT CS_PK FROM eHubCodeSet		WHERE CS_CC_Recipient = @Client_OCMIFTMBC And CS_Name = 'CarrierSettings')
DECLARE @OCMIFTMBC_ID_CRPK			uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMIFTMBC_CSPK And CR_Name = 'ID')
DECLARE @OCMIFTMBC_MSGID_CRPK		uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMIFTMBC_CSPK And CR_Name = 'MSGID')
DECLARE @OCMIFTMBC_BRSID_CRPK		uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMIFTMBC_CSPK And CR_Name = 'BRSID')

DECLARE @OCMIFTMBC_New_CKPK			uniqueidentifier =  '20BC111B-8823-4EB6-A058-F4552641D6D2' -- SELECT NEWID()
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
SELECT @OCMIFTMBC_New_CKPK, @OCMIFTMBC_CSPK, @OCMIFTMBC_MaxCK_Order, @CarrierName

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey)
SELECT @OCMIFTMBC_New_CKPK, @OCMIFTMBC_ID_CRPK, @CarrierID		, NULL			UNION ALL
SELECT @OCMIFTMBC_New_CKPK, @OCMIFTMBC_BRSID_CRPK, @CarrierBRS	, NULL			UNION ALL
SELECT @OCMIFTMBC_New_CKPK, @OCMIFTMBC_MSGID_CRPK, @CarrierMSG	, NULL 

-- Code Set HAMBURG_SUD : ContainerTypeToISOCode
DECLARE @ISOToContainerType_CSPK		uniqueidentifier = '0732FD93-2757-4A18-AE13-25AC1F292206', 	-- SELECT NEWID()
		@ISOToContainerType_CRPK		uniqueidentifier = 'A4CE7999-7F29-4061-8220-3BC44E2282BD'	-- SELECT NEWID()

 INSERT eHubCodeSet
(CS_PK, CS_Name, CS_TS, CS_CC_Sender, CS_CC_Recipient, CS_Key1Name)
SELECT @ISOToContainerType_CSPK, 'ContainerTypeToISOCode', @HAMBURG_SUD_TSPK, @HAMBURG_SUD_CCPK, @HAMBURG_SUD_CCPK , 'Container Type'

INSERT eHubCodeSetResult
(CR_PK, CR_CS, CR_Order, CR_Name)
SELECT @ISOToContainerType_CRPK, @ISOToContainerType_CSPK, 1, concat(@CarrierName, ' Code')

INSERT eHubCodeMapKey
(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT '645DAB92-2381-4F20-B154-507CF6A259E4', @ISOToContainerType_CSPK, 1, '%'

INSERT eHubCodeMapValue
(CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey)
SELECT '645DAB92-2381-4F20-B154-507CF6A259E4', @ISOToContainerType_CRPK, '', null

-- Code Set HAMBURG_SUD : ContainerTypeToISOCode
DECLARE @PackageTypeCodeSet			uniqueIdentifier = '6B1A6AD5-6DD8-476C-A9AC-860A29CB24A8'	-- SELECT NEWID()
DECLARE @PackageTypeCodeSetResult	uniqueIdentifier = '6F1542DD-1F86-46BC-9CDA-D8B311B251F7'	-- SELECT NEWID()

INSERT INTO eHubCodeSet
(CS_PK, CS_Name, CS_TS, CS_CC_Sender, CS_CC_Recipient, CS_Key1Name)
SELECT @PackageTypeCodeSet, N'Package Type', @HAMBURG_SUD_TSPK, @HAMBURG_SUD_CCPK, @HAMBURG_SUD_CCPK, N'ediEnterprise Code'

INSERT INTO eHubCodeSetResult
(CR_PK, CR_CS, CR_Order, CR_Name)
SELECT @PackageTypeCodeSetResult, @PackageTypeCodeSet, 1, concat(@CarrierName, ' Code')

INSERT INTO eHubCodeMapKey
(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT 'F32D2156-CAB2-4FA0-B181-8654154DF5DB', @PackageTypeCodeSet, 1, '%'

INSERT INTO eHubCodeMapValue
(CV_CK, CV_CR, CV_OutputCode)
SELECT 'F32D2156-CAB2-4FA0-B181-8654154DF5DB', @PackageTypeCodeSetResult, ''

-- Code Set HAMBURG_SUD : Container Status
DECLARE @ContainerStatusCodeSet			uniqueIdentifier = '23FA8B73-A0EE-40FB-BB55-FAAF5CFF9804'	-- SELECT NEWID()
DECLARE @ResultSet_EventTypesPK			uniqueIdentifier = '78B21F23-8017-4CD3-963C-5D20FFF0B4F9'	-- SELECT NEWID()
DECLARE @ResultSet_EventRefrnPK			uniqueIdentifier = 'D6E64549-F604-4573-B6E3-459742DC64A1'	-- SELECT NEWID()
DECLARE @ResultSet_EventParamPK			uniqueIdentifier = '76EFD425-EB8C-40B4-898E-CD4EEF6D766A'	-- SELECT NEWID()
DECLARE @ResultSet_EventIsEstPK			uniqueIdentifier = 'FB116879-69C5-437B-9551-AA2FA444144E'	-- SELECT NEWID()

INSERT eHubCodeSet 
(CS_PK, CS_Name, CS_TS, CS_CC_Sender, CS_CC_Recipient, CS_Key1Name) 
SELECT @ContainerStatusCodeSet, 'Container Status', @HAMBURG_SUD_IFTSTA_TSPK, @HAMBURG_SUD_CCPK, @HAMBURG_SUD_CCPK, 'Code'

INSERT eHubCodeSetResult 
(CR_PK, CR_CS, CR_Order, CR_Name) 
SELECT @ResultSet_EventTypesPK, @ContainerStatusCodeSet, 1, 'Event Type'			UNION ALL
SELECT @ResultSet_EventRefrnPK, @ContainerStatusCodeSet, 2, 'Event Reference'		UNION ALL
SELECT @ResultSet_EventParamPK, @ContainerStatusCodeSet, 3, 'Event Parameters'		UNION ALL
SELECT @ResultSet_EventIsEstPK, @ContainerStatusCodeSet, 4, 'Is Estimate'

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT 'DE8F2DF3-6B3B-424B-B2BE-AA12C3577BA4', @ContainerStatusCodeSet, 1, '1'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'DE8F2DF3-6B3B-424B-B2BE-AA12C3577BA4', @ResultSet_EventTypesPK, 'FUL', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'DE8F2DF3-6B3B-424B-B2BE-AA12C3577BA4', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'DE8F2DF3-6B3B-424B-B2BE-AA12C3577BA4', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'DE8F2DF3-6B3B-424B-B2BE-AA12C3577BA4', @ResultSet_EventIsEstPK, '', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '247B595E-1987-46B4-8179-5D78C743F961', @ContainerStatusCodeSet, 2, '21'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '247B595E-1987-46B4-8179-5D78C743F961', @ResultSet_EventTypesPK, 'GOU', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '247B595E-1987-46B4-8179-5D78C743F961', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '247B595E-1987-46B4-8179-5D78C743F961', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '247B595E-1987-46B4-8179-5D78C743F961', @ResultSet_EventIsEstPK, '', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT 'AF8D5BFB-27D4-4670-836B-FF9C184D8AD1', @ContainerStatusCodeSet, 3, '24'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'AF8D5BFB-27D4-4670-836B-FF9C184D8AD1', @ResultSet_EventTypesPK, 'DEP', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'AF8D5BFB-27D4-4670-836B-FF9C184D8AD1', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'AF8D5BFB-27D4-4670-836B-FF9C184D8AD1', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'AF8D5BFB-27D4-4670-836B-FF9C184D8AD1', @ResultSet_EventIsEstPK, '', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT 'EF755A16-E021-439F-BB04-67B8C2DB67C9', @ContainerStatusCodeSet, 4, '27'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'EF755A16-E021-439F-BB04-67B8C2DB67C9', @ResultSet_EventTypesPK, 'GOU', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'EF755A16-E021-439F-BB04-67B8C2DB67C9', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'EF755A16-E021-439F-BB04-67B8C2DB67C9', @ResultSet_EventParamPK, '|Facility=CY', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'EF755A16-E021-439F-BB04-67B8C2DB67C9', @ResultSet_EventIsEstPK, '', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '8DBABB14-D633-4C00-9567-B2516FE74C9B', @ContainerStatusCodeSet, 5, '29'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '8DBABB14-D633-4C00-9567-B2516FE74C9B', @ResultSet_EventTypesPK, 'ARV', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '8DBABB14-D633-4C00-9567-B2516FE74C9B', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '8DBABB14-D633-4C00-9567-B2516FE74C9B', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '8DBABB14-D633-4C00-9567-B2516FE74C9B', @ResultSet_EventIsEstPK, '', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '909BE4AA-96C0-4A82-8D27-7650136B8CE5', @ContainerStatusCodeSet, 6, '48'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '909BE4AA-96C0-4A82-8D27-7650136B8CE5', @ResultSet_EventTypesPK, 'FLO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '909BE4AA-96C0-4A82-8D27-7650136B8CE5', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '909BE4AA-96C0-4A82-8D27-7650136B8CE5', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '909BE4AA-96C0-4A82-8D27-7650136B8CE5', @ResultSet_EventIsEstPK, '', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT 'AFC2B910-FE55-4839-8C5F-B40144AD1B7E', @ContainerStatusCodeSet, 7, '74'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'AFC2B910-FE55-4839-8C5F-B40144AD1B7E', @ResultSet_EventTypesPK, 'GIN', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'AFC2B910-FE55-4839-8C5F-B40144AD1B7E', @ResultSet_EventRefrnPK, '', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'AFC2B910-FE55-4839-8C5F-B40144AD1B7E', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT 'AFC2B910-FE55-4839-8C5F-B40144AD1B7E', @ResultSet_EventIsEstPK, '', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '3F3B07A9-DDD1-4008-BD7E-8F78D8E32803', @ContainerStatusCodeSet, 8, '80'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '3F3B07A9-DDD1-4008-BD7E-8F78D8E32803', @ResultSet_EventTypesPK, 'DHR', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '3F3B07A9-DDD1-4008-BD7E-8F78D8E32803', @ResultSet_EventRefrnPK, 'Rail', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '3F3B07A9-DDD1-4008-BD7E-8F78D8E32803', @ResultSet_EventParamPK, '|Facility=CY', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '3F3B07A9-DDD1-4008-BD7E-8F78D8E32803', @ResultSet_EventIsEstPK, '', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '2B4972F1-D8A0-4275-9F6C-61B7AD1E3A88', @ContainerStatusCodeSet, 9, '98'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '2B4972F1-D8A0-4275-9F6C-61B7AD1E3A88', @ResultSet_EventTypesPK, 'FUL', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '2B4972F1-D8A0-4275-9F6C-61B7AD1E3A88', @ResultSet_EventRefrnPK, 'At Port of Transhipment', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '2B4972F1-D8A0-4275-9F6C-61B7AD1E3A88', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '2B4972F1-D8A0-4275-9F6C-61B7AD1E3A88', @ResultSet_EventIsEstPK, '', NULL

INSERT eHubCodeMapKey (CK_PK, CK_CS, CK_Order, CK_Key1Value)             SELECT '6A6EB5CC-84EA-48D6-A845-0442A8F475BC', @ContainerStatusCodeSet, 10, '99'
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '6A6EB5CC-84EA-48D6-A845-0442A8F475BC', @ResultSet_EventTypesPK, 'FLO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '6A6EB5CC-84EA-48D6-A845-0442A8F475BC', @ResultSet_EventRefrnPK, 'At Port of Transhipment', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '6A6EB5CC-84EA-48D6-A845-0442A8F475BC', @ResultSet_EventParamPK, '|Facility=CTO', NULL
INSERT eHubCodeMapValue (CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey) SELECT '6A6EB5CC-84EA-48D6-A845-0442A8F475BC', @ResultSet_EventIsEstPK, '', NULL

------------------eHubRoutingRule
DECLARE @HAMBURG_SUD_GroupRule	uniqueidentifier = '40DFEB66-285B-4F66-9FCC-888B2D70188B'	--HAMBURG_SUD Group Rule
DECLARE @HAMBURG_SUD_BK1_RRPK	uniqueIdentifier = 'EFF744FE-EFE9-4F07-B22E-92522426729B'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_SI1_RRPK	uniqueIdentifier = '7D268FF4-F7CF-4CBC-892D-B8B1D96D3E81'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_VM_RRPK	uniqueIdentifier = '3B5EF761-B3C5-49B4-A2D4-3CA1AEB8BD5E'	--SELECT NEWID()
DECLARE @HAMBURG_SUD_VM2_RRPK	uniqueIdentifier = '777D954A-4CFB-45B8-85F3-B04CB96D42DE'	--SELECT NEWID()

INSERT INTO eHubRoutingRule
(RR_PK, RR_Condition_Expression, RR_Group_RR_GroupRule, RR_Group_MatchMultiple, RR_Success_CC_Recipient, RR_Group_Ordering)
SELECT @HAMBURG_SUD_GroupRule, NULL, NULL, 0, NULL, NULL UNION ALL
SELECT @HAMBURG_SUD_BK1_RRPK, '[@UShipmentNamespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/BookingRequest/1]', @HAMBURG_SUD_GroupRule, null, @HAMBURG_SUD_BK1_CCPK, 1000			UNION ALL
SELECT @HAMBURG_SUD_SI1_RRPK, '[@UShipmentNamespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/ShippingInstruction/1]', @HAMBURG_SUD_GroupRule, null, @HAMBURG_SUD_SI1_CCPK, 2000		UNION ALL
SELECT @HAMBURG_SUD_VM2_RRPK, '[@UShipmentNamespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2]', @HAMBURG_SUD_GroupRule, null, @HAMBURG_SUD_VM2_CCPK, 3000	UNION ALL
SELECT @HAMBURG_SUD_VM_RRPK, '[@DocumentName,Equal,Verified Gross Container Weight]', @HAMBURG_SUD_GroupRule, null, @HAMBURG_SUD_VM_CCPK, 4000

------------------- eHubRegistrationType/eHubServiceProvider/eHubServiceProviderRequiredRegistration
DECLARE @ServiceProviderPk		uniqueIdentifier = 'B519A491-FF24-4642-B20C-E33CF8B24523'	-- SELECT NEWID()
DECLARE @RegistrationTypePk		uniqueIdentifier = '6FD4CB84-22C8-42A0-86F0-8EB04A3E5379'	-- SELECT NEWID()

INSERT INTO eHubRegistrationType
(RT_PK, RT_ID, RT_Description, RT_RegistrantType)
SELECT @RegistrationTypePk, @CarrierName, concat(@CarrierName, ' Client ID'), 'Client'

INSERT INTO eHubServiceProvider(SP_CC_Service, SP_CC_Provider, SP_RR, SP_PK)
SELECT @ShippingInstruction_CCPK, @HAMBURG_SUD_CCPK, @HAMBURG_SUD_GroupRule, @ServiceProviderPk

INSERT INTO eHubServiceProviderRequiredRegistration(SX_RT, SX_LookupFactName, SX_QualifierFactName, SX_SP)
SELECT  @RegistrationTypePk, 'SourceParty', 'EventBranch', @ServiceProviderPk

------------------- eHubSubscriptionType
DECLARE @Subscription_ID		uniqueIdentifier = '3BA9CA85-6274-4E05-A1F8-E72B7EFFE49F' -- SELECT NEWID()
DECLARE @Subscription_MSG		uniqueIdentifier = '75D934F4-430F-4E5A-A013-9ED67DE1C67D' -- SELECT NEWID()
DECLARE @Subscription_BRS		uniqueIdentifier = 'BEE801FE-8810-47B3-87F5-5A1AA4379484' -- SELECT NEWID()

INSERT INTO eHubSubscriptionType
(ST_PK, ST_ID, ST_Name, ST_ExpiryDays)
SELECT @Subscription_ID,	@CarrierID,	 concat(@CarrierName, ' ID'), 180						UNION ALL
SELECT @Subscription_MSG,	@CarrierMSG, concat(@CarrierName, ' Message Reference'), 180		UNION ALL
SELECT @Subscription_BRS,	@CarrierBRS, concat(@CarrierName, ' Booking Request Status'), 180

-------------------Subscription Lookup
DECLARE @APERAK_D99B_SLPK		uniqueidentifier = 'DC71205E-9758-4C76-A5BC-7AAC769ED54F' --SELECT NEWID()
DECLARE @APERAK_D04A_SLPK		uniqueidentifier = '8B20C615-D178-4C07-85CD-F75DFB018072' --SELECT NEWID()
DECLARE @IFTMCS_D99B_SLPK		uniqueidentifier = '7C6849C6-14EF-4178-AF63-D70DC0D06ABE' --SELECT NEWID()
DECLARE @IFTMBC_D99B_SLPK		uniqueidentifier = '28D75F03-8B98-44EE-8EE7-2E93C1252934' --SELECT NEWID()
DECLARE @CONTRL_SLPK			uniqueidentifier = 'E04347F5-85A6-4DBB-86C1-3EE13695F710' --SELECT NEWID()

INSERT eHubSubScriptionLookup(SL_PK, SL_ST, SL_DT, SL_ValueXpath)
SELECT @APERAK_D99B_SLPK, @Subscription_MSG, @D99B_APERAK_DTPK, '/*[local-name()=''EFACT_D99B_APERAK'']/*[local-name()=''BGM'']/*[local-name()=''C106'']/C10601'	UNION ALL
SELECT @APERAK_D04A_SLPK, @Subscription_MSG, @D04A_APERAK_DTPK, '/*[local-name()=''EFACT_D04A_APERAK'']/*[local-name()=''BGM'']/*[local-name()=''C106'']/C10601'	UNION ALL
SELECT @IFTMCS_D99B_SLPK, @Subscription_MSG, @D99B_IFTMCS_DTPK, '/*[local-name()=''EFACT_D99B_IFTMCS'']/*[local-name()=''RFFLoop1'']/*[local-name()=''RFF'']/*[local-name()=''C506'' and C50601=''ZZZ'']/C50602' UNION ALL
SELECT @IFTMBC_D99B_SLPK, @Subscription_MSG, @D99B_IFTMBC_DTPK, '/*[local-name()=''EFACT_D99B_IFTMBC'']/*[local-name()=''BGM'']/*[local-name()=''C106'']/C10601'	UNION ALL
SELECT @CONTRL_SLPK, @Subscription_MSG, @CONTRL_DTPK, 'string(number(/*[local-name()="Efact_Contrl_Root"]/*[local-name()="UCI"]/UCI1))'

-------------------Subscription
DECLARE @CTSCPY_STPK				uniqueidentifier = (SELECT ST_PK FROM eHubSubscriptionType WHERE ST_ID ='CTSCPY')
DECLARE @HAMBURG_SUD_SVPK			uniqueidentifier = '95ED6D91-960A-4AA9-8305-64558CE1C373' --SELECT NEWID()

INSERT INTO eHubSubscriptionValue (SV_PK, SV_ST, SV_CC_Sender, SV_Value, SV_Reference, SV_SubscribedUTC, SV_ExpiryUTC, SV_CC_Recipient, SV_ReferenceType) 
SELECT @HAMBURG_SUD_SVPK, @CTSCPY_STPK, @HAMBURG_SUD_CCPK, '1', NULL, '20200915 12:59:59.000', NULL, @ContainerTracking_CCPK, NULL

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
SET CK_Order =  CK_Order + 3
WHERE CK_PK = @UNB3_MaxCKOrder_CKPK

INSERT eHubCodeMapKey(CK_PK, CK_CS, CK_Order, CK_Key1Value, CK_Key2Value)
SELECT 'A5D5767D-4CF8-490D-B4B6-9970D2218038', @UNB3_CSPK, @UNB3_MaxCKOrder, concat(@CarrierName, '_%'), 'SUDU'

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode)
SELECT 'A5D5767D-4CF8-490D-B4B6-9970D2218038', @UNB3_SenderID_CRPK, 'CARGOWISE' UNION ALL
SELECT 'A5D5767D-4CF8-490D-B4B6-9970D2218038',@UNB3_ReceiverID_CRPK, 'HSD'

-- Code Set  OCMIFTMIN  : CarrierSettings
DECLARE @OCMIFTMIN_CCPK			uniqueidentifier = (SELECT CC_PK FROM eHubClient	WHERE CC_ID = 'OCMIFTMIN')
DECLARE @OCMIFTMIN_CSPK			uniqueIdentifier = (SELECT CS_PK FROM eHubCodeSet	WHERE CS_CC_Recipient = @OCMIFTMIN_CCPK And CS_Name = 'CarrierSettings')
DECLARE @OCMIFTMIN_TSPK			uniqueIdentifier = (SELECT CS_TS FROM eHubCodeSet	WHERE CS_CC_Recipient = @OCMIFTMIN_CCPK And CS_Name = 'CarrierSettings')

DECLARE @OCMIFTMIN_Name_CRPK	uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMIN_CSPK And CR_Name = 'CarrierName'),
		@OCMIFTMIN_ID_CRPK		uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMIN_CSPK And CR_Name = 'ID'),
		@OCMIFTMIN_MSGID_CRPK	uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMIN_CSPK And CR_Name = 'MSGID'),
		@OCMIFTMIN_Prefix_CRPK	uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult		WHERE CR_CS = @OCMIFTMIN_CSPK And CR_Name = 'SubscriptionPrefix')

DECLARE @OCMIFTMIN_New_CKPK		uniqueIdentifier = '8174CF2A-97D6-46B1-9039-C3E99CB217A2' --SELECT NEWID()
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
SELECT '8174CF2A-97D6-46B1-9039-C3E99CB217A2', @OCMIFTMIN_CSPK, @MaxCK_Order, concat(@CarrierName, '_%')

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey)
SELECT '8174CF2A-97D6-46B1-9039-C3E99CB217A2', @OCMIFTMIN_Name_CRPK, @CarrierName, NULL				UNION ALL
SELECT '8174CF2A-97D6-46B1-9039-C3E99CB217A2', @OCMIFTMIN_ID_CRPK, @CarrierID, NULL					UNION ALL
SELECT '8174CF2A-97D6-46B1-9039-C3E99CB217A2', @OCMIFTMIN_MSGID_CRPK, @CarrierMSG, NULL				UNION ALL
SELECT '8174CF2A-97D6-46B1-9039-C3E99CB217A2', @OCMIFTMIN_Prefix_CRPK, @CarrierPrefix, NULL

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
SELECT '0D30422B-538B-4596-9208-8B4575E15DB3', @OCMVERMAS_CarrierSetting_CSPK, @OCMVERMAS_MaxCKOrder, concat(@CarrierName, '_%')

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode, CV_PassThroughKey)
SELECT '0D30422B-538B-4596-9208-8B4575E15DB3', @OCMVERMAS_Name_CRPK, @CarrierName, NULL		UNION ALL
SELECT '0D30422B-538B-4596-9208-8B4575E15DB3', @OCMVERMAS_ID_CRPK, @CarrierID, NULL			UNION ALL
SELECT '0D30422B-538B-4596-9208-8B4575E15DB3', @OCMVERMAS_MSGID_CRPK, @CarrierMSG, NULL		UNION ALL
SELECT '0D30422B-538B-4596-9208-8B4575E15DB3', @OCMVERMAS_UNB_CRPK, 'UNOC', NULL			UNION ALL
SELECT '0D30422B-538B-4596-9208-8B4575E15DB3', @OCMVERMAS_Prefix_CRPK, @CarrierPrefix, NULL

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
SELECT '5642287A-226A-4C86-9BB5-44F1A59063C9', @Default_Interface_CSPK, @Default_Interface_Max_CKOrder, concat(@CarrierName, '_%')

INSERT INTO eHubCodeMapValue
(CV_CK, CV_CR, CV_OutputCode)
SELECT '5642287A-226A-4C86-9BB5-44F1A59063C9', @Default_InterfaceName_CRPK, @Carrier_Config_TSName

-- Code Set OCMAPERAK : Subscription Type ID
DECLARE @OCMAPERAK_SubscriptionTypeID_CSPK			uniqueidentifier = (SELECT CS_PK FROM eHubCodeSet		WHERE CS_CC_Recipient = @OCMAPERAK_CCPK and CS_Name = 'Subscription Type ID')

DECLARE @OCMAPERAK_ID_CRPK							uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMAPERAK_SubscriptionTypeID_CSPK and CR_Name = 'ID'),
		@OCMAPERAK_BRSID_CRPK						uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMAPERAK_SubscriptionTypeID_CSPK and CR_Name = 'BRS ST ID'),
		@OCMAPERAK_Reference_CRPK					uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMAPERAK_SubscriptionTypeID_CSPK and CR_Name = 'Message Reference ST ID')

DECLARE @OCMAPERAK_SubscriptionTypeID_CodeMapPK 	uniqueidentifier = '08C80425-CA14-4D42-89B3-BA13A90E0D8D'		-- select NEWID()
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
SELECT @OCMAPERAK_SubscriptionTypeID_CodeMapPK, @OCMAPERAK_SubscriptionTypeID_CSPK, @OCMAPERAK_SubscriptionTypeID_MaxCKOrder, @CarrierName

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode)
SELECT @OCMAPERAK_SubscriptionTypeID_CodeMapPK, @OCMAPERAK_ID_CRPK, @CarrierID			UNION ALL
SELECT @OCMAPERAK_SubscriptionTypeID_CodeMapPK, @OCMAPERAK_BRSID_CRPK, @CarrierBRS		UNION ALL
SELECT @OCMAPERAK_SubscriptionTypeID_CodeMapPK, @OCMAPERAK_Reference_CRPK, @CarrierMSG

--OCMIFTMCS - Subscription Type ID
DECLARE @OCMIFTMCS_SubscriptionTypeID_CSPK			uniqueidentifier = (SELECT CS_PK FROM eHubCodeSet		WHERE CS_CC_Recipient = @OCMIFTMCS_CCPK and CS_Name = 'Subscription Type ID')
DECLARE @OCMIFTMCS_STID_CRPK						uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMIFTMCS_SubscriptionTypeID_CSPK and CR_Name = 'ST ID')

DECLARE @OCMIFTMCS_SubscriptionTypeID_CodeMapPK 	uniqueidentifier = '375AD8E4-FF02-4237-AC2D-39B98E2415A4'		-- select NEWID()
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
SELECT @OCMIFTMCS_SubscriptionTypeID_CodeMapPK, @OCMIFTMCS_SubscriptionTypeID_CSPK, @OCMIFTMCS_SubscriptionTypeID_MaxCKOrder, @CarrierName

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode)
SELECT @OCMIFTMCS_SubscriptionTypeID_CodeMapPK, @OCMIFTMCS_STID_CRPK, @CarrierMSG

--OCMIFTMCS - Reference No
DECLARE @OCMIFTMCS_ReferenceNo_CSPK			uniqueidentifier = (SELECT CS_PK FROM eHubCodeSet		WHERE CS_CC_Recipient = @OCMIFTMCS_CCPK and CS_Name = 'Reference No')
DECLARE @OCMIFTMCS_Value_CRPK				uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMIFTMCS_ReferenceNo_CSPK and CR_Name = 'Value')

DECLARE @OCMIFTMCS_ReferenceNo_CodeMapPK 	uniqueidentifier = '9E2993E1-9353-46F7-99E8-83CA439622AC'		-- select NEWID()
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
SELECT @OCMIFTMCS_ReferenceNo_CodeMapPK, @OCMIFTMCS_ReferenceNo_CSPK, @OCMIFTMCS_ReferenceNo_MaxCKOrder, @CarrierName

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode)
SELECT @OCMIFTMCS_ReferenceNo_CodeMapPK, @OCMIFTMCS_Value_CRPK, 'ZZZ'

--OCMCONTRL - Subscription Type ID
DECLARE @OCMCONTRL_SubscriptionTypeID_CSPK			uniqueidentifier = (SELECT CS_PK FROM eHubCodeSet		WHERE CS_CC_Recipient = @OCMCONTRL_CCPK and CS_Name = 'Subscription Type ID')
DECLARE @OCMCONTRL_STID_CRPK						uniqueidentifier = (SELECT CR_PK FROM eHubCodeSetResult WHERE CR_CS = @OCMCONTRL_SubscriptionTypeID_CSPK and CR_Name = 'ST ID')

DECLARE @OCMCONTRL_SubscriptionTypeID_CodeMapPK 	uniqueidentifier = '76581B87-B275-4D5C-9707-B0FEE17593F7'		-- select NEWID()		
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
SELECT @OCMCONTRL_SubscriptionTypeID_CodeMapPK, @OCMCONTRL_SubscriptionTypeID_CSPK, @OCMCONTRL_SubscriptionTypeID_MaxCKOrder, @CarrierName

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode) 
SELECT @OCMCONTRL_SubscriptionTypeID_CodeMapPK, @OCMCONTRL_STID_CRPK, @CarrierMSG

ROLLBACK
--COMMIT