-- Run CW1toCW1Clients.sql and SqlDeployment-1.sql first
use eHubTransactions;
GO
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

DECLARE @CARGOWISE_CCPK uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'CARGOWISE')
DECLARE @CARGOWISE_BK_CCPK uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'CARGOWISE_BK')
DECLARE @CARGOWISE_SI_CCPK uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'CARGOWISE_SI')
DECLARE @CARGOWISE_VGM_CCPK uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'CARGOWISE_VM')
DECLARE @CARGOWISE_AC_CCPK uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'CARGOWISE_AC')
DECLARE @CARGOWISE_BC_CCPK uniqueIdentifier = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'CARGOWISE_BC')

DECLARE @UInterchange2011_DTPK uniqueIdentifier = (SELECT DT_PK FROM eHubMessageType WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange')
DECLARE @CarrierUniversal_DTPK uniqueIdentifier = (SELECT DT_PK FROM eHubMessageType WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2011/11#UniversalShipment')
DECLARE @UInterchangeInclude_DTPK uniqueIdentifier = (SELECT DT_PK FROM eHubMessageType WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2012/11#UniversalInterchangeInclude')
DECLARE @VGM_DTPK uniqueIdentifier = (SELECT DT_PK FROM eHubMessageType WHERE DT_Code = 'http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2#UniversalShipment')

DECLARE @CU2UI_TTPK uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2UInterchange.CarrierUniversal2UInterchange%')
DECLARE @US2UI_TTPK uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Clients.EDI.Universal_2011_11.Transforms.UniversalShipment2UniversalInterchange%')
Declare @UI2US_BK_TTPK uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1.UniversalInterchange2UniversalShipment_BK%')
Declare @US2CU_BK_TTPK uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1.Universal2CarrierUniversal_BK%')
Declare @UI2US_SI_TTPK uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1.UniversalInterchange2UniversalShipment_SI%')
Declare @US2CU_SI_TTPK uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1.Universal2CarrierUniversal_SI%')
DECLARE @UIEnvelop_TTPK uniqueIdentifier = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType like 'CargoWise.eHub.Clients.EDI.Universal_2012_11.Transforms.UniversalInterchangeInclude2UniversalInterchangeEnvelope%')

DECLARE @ACK_DTPK uniqueIdentifier = '925A2D1A-FAAF-48C5-A7FB-1AE5F3409FE2'
DECLARE @BKC_DTPK uniqueIdentifier = 'E4182795-C8B4-4BDA-9D25-66B6425380DF'

DECLARE @UI2US_VGM_TTPK uniqueIdentifier = 'A27F89DA-7C8B-4E72-B98F-9EA6A58D3B4C'
DECLARE @US2CU_VGM_TTPK uniqueIdentifier = '6BC612C1-6B35-4EB7-948B-59374825A7C2'
DECLARE @UI2US_ACK_TTPK uniqueIdentifier = '78D6A87C-8057-4ACA-8072-C80CECA914C1'
DECLARE @UI2US_BKC_TTPK uniqueIdentifier = 'A3C832F8-0A46-4C3B-95D9-8C7FE7AAB129'

DECLARE @CARGOWISE_BK_TTPK uniqueIdentifier = '87BC7DDD-EFF0-4B5A-9928-B9895192EE14'
DECLARE @CARGOWISE_SI_TTPK uniqueIdentifier = '3DAB3BA8-1AAE-46D4-889E-5DF9168CEBE1'
DECLARE @CARGOWISE_VGM_TTPK uniqueIdentifier = '0C22FAB5-E7F6-4191-A6E5-EC81A1475A0D'
DECLARE @CARGOWISE_ACK_TTPK uniqueIdentifier = '7BA05373-EC68-4E0E-8F8F-E3AA3E4E07CE'
DECLARE @CARGOWISE_BKC_TTPK uniqueIdentifier = 'D31B2890-E2AA-499B-9DB2-87C03A379047'

DECLARE @CARGOWISE_ACK_TSPK uniqueIdentifier = '3D432D93-FBA5-450E-AF09-BB71E6ECC054'
DECLARE @CARGOWISE_BKC_TSPK uniqueIdentifier = '7E2D7870-87DC-4F77-958D-5D99E91D390F'

DECLARE @ACK_US_SLPK uniqueidentifier = '549F2B9B-1D4E-4ED2-A59B-A74A74CB9045'
DECLARE @ACK_UE_SLPK uniqueidentifier = 'B705E716-6791-4498-919F-EF47E840D536'
DECLARE @BKC_SLPK uniqueidentifier = 'D1CE6732-B106-4F09-800C-4D834FE57C48'

DECLARE @CARGOWISE_TSPK uniqueIdentifier = 'D42DCD71-C59A-4576-BC46-411B9C43F7CA'
DECLARE @CARGOWISE_BK_TSPK uniqueIdentifier = 'F22294FD-F025-45A7-838D-E9D240551148'
DECLARE @CARGOWISE_SI_TSPK uniqueIdentifier = 'D7A83FCC-2668-4D37-B073-AB8DADC0DF92'
DECLARE @CARGOWISE_VGM_TSPK uniqueIdentifier = 'E29E9A64-D9D6-47BA-BE7B-43C0A0BE69B0'

DECLARE @CW1MSG_STPK uniqueidentifier = 'C96ED9C3-01BF-4505-BCA7-9A34D9E539C6'

INSERT INTO eHubSubscriptionType(ST_PK, ST_ID, ST_Name, ST_ExpiryDays)
VALUES (@CW1MSG_STPK, 'CW1MSG', 'CARGOWISE Message Reference', 180)

INSERT INTO eHubCounter(CN_Name, CN_Value)
VALUES ('CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE', 1)

INSERT eHubMessageType(DT_PK, DT_Code, DT_IsFlatFile, DT_IsEDI)
VALUES
(@ACK_DTPK, 'http://www.cargowise.com/Schemas/Universal/2012/11/BookingConfirmation/1#UniversalShipment', 0, 0),
(@BKC_DTPK, 'http://www.cargowise.com/Schemas/Universal/2012/11/Acknowledgement/1#UniversalShipment', 0, 0)

INSERT INTO eHubTransformationType(TT_PK, TT_DT_Source, TT_DT_Target, TT_TransformationType)
VALUES
(@UI2US_ACK_TTPK, @UInterchange2011_DTPK, @ACK_DTPK, 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UInterchange2UShipment.V1.UInterchange2UShipment_ACK, CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UInterchange2UShipment, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350'),
(@CARGOWISE_ACK_TTPK, @UInterchange2011_DTPK, @UInterchangeInclude_DTPK, 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE.UI2UI_ACK, CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350'),
(@UI2US_BKC_TTPK, @UInterchange2011_DTPK, @BKC_DTPK, 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UInterchange2UShipment.V1.UInterchange2UShipment_BKC, CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UInterchange2UShipment, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350'),
(@CARGOWISE_BKC_TTPK, @BKC_DTPK, @UInterchangeInclude_DTPK, 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE.US2UI_BKC, CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350'),
(@CARGOWISE_BK_TTPK, @CarrierUniversal_DTPK, @UInterchange2011_DTPK, 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE.CU2UI_BK, CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350'),
(@CARGOWISE_SI_TTPK, @CarrierUniversal_DTPK, @UInterchange2011_DTPK, 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE.CU2UI_SI, CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350'),
(@UI2US_VGM_TTPK, @UInterchange2011_DTPK, @VGM_DTPK, 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v2.UniversalInterchange2UniversalShipment_VGM, CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350'),
(@US2CU_VGM_TTPK, @VGM_DTPK, @CarrierUniversal_DTPK, 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v2.Universal2CarrierUniversal_VGM, CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350'),
(@CARGOWISE_VGM_TTPK, @CarrierUniversal_DTPK, @UInterchange2011_DTPK, 'CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE.CU2UI_VGM, CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CARGOWISE2CARGOWISE, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350')

INSERT INTO eHubTransformationSet(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient)
VALUES
(@CARGOWISE_ACK_TSPK,'Acknowledgement from CARGOWISE', null, @CARGOWISE_AC_CCPK, @UInterchange2011_DTPK, 'Acknowledgement to CARGOWISE', 0, 1),
(@CARGOWISE_BKC_TSPK,'Booking Confirmation from CARGOWISE', null, @CARGOWISE_BC_CCPK, @UInterchange2011_DTPK, 'Booking Confirmation to CARGOWISE', 0, 1),
(@CARGOWISE_BK_TSPK,'Booking Request to CARGOWISE', null, @CARGOWISE_BK_CCPK, @UInterchange2011_DTPK, 'Booking Request to CARGOWISE', 1, 0),
(@CARGOWISE_SI_TSPK,'Shipping Instruction to CARGOWISE', null, @CARGOWISE_SI_CCPK, @UInterchange2011_DTPK, 'Shipping Instruction to CARGOWISE', 1, 0),
(@CARGOWISE_VGM_TSPK,'Verified Gross Mass to CARGOWISE', null, @CARGOWISE_VGM_CCPK, @UInterchange2011_DTPK, 'Verified Gross Mass to CARGOWISE', 1, 0)

INSERT INTO eHubTransformationSet(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillingInterfaceName, TS_BillSender, TS_BillRecipient, TS_XPathPredicate) 
VALUES (@CARGOWISE_TSPK, 'CARGOWISE Provider Configuration', @CARGOWISE_CCPK, @CARGOWISE_CCPK, NULL, 'CARGOWISE Provider Configuration', 0, 0, null)

INSERT INTO eHubTransformationMapping(TM_TS_PK, TM_Order, TM_TT_PK)
VALUES
(@CARGOWISE_BK_TSPK, 0, @UI2US_BK_TTPK),
(@CARGOWISE_BK_TSPK, 1, @US2CU_BK_TTPK),
(@CARGOWISE_BK_TSPK, 2, @CARGOWISE_BK_TTPK),

(@CARGOWISE_SI_TSPK, 0, @UI2US_SI_TTPK),
(@CARGOWISE_SI_TSPK, 1, @US2CU_SI_TTPK),
(@CARGOWISE_SI_TSPK, 2, @CARGOWISE_SI_TTPK),

(@CARGOWISE_VGM_TSPK, 0, @UI2US_VGM_TTPK),
(@CARGOWISE_VGM_TSPK, 1, @US2CU_VGM_TTPK),
(@CARGOWISE_VGM_TSPK, 2, @CARGOWISE_VGM_TTPK),

(@CARGOWISE_ACK_TSPK, 0, @CARGOWISE_ACK_TTPK),
(@CARGOWISE_ACK_TSPK, 1, @UIEnvelop_TTPK),

(@CARGOWISE_BKC_TSPK, 0, @UI2US_BKC_TTPK),
(@CARGOWISE_BKC_TSPK, 1, @CARGOWISE_BKC_TTPK),
(@CARGOWISE_BKC_TSPK, 2, @UIEnvelop_TTPK)

INSERT eHubSubscriptionLookup(SL_PK, SL_ST, SL_DT, SL_ValueXpath)
VALUES
(@ACK_UE_SLPK, @CW1MSG_STPK, @ACK_DTPK, '/*[local-name()=''UniversalInterchange'']/*[local-name()=''Body'']/*[local-name()=''UniversalEvent'']/*[local-name()=''Event'']/*[local-name()=''ContextCollection'']/*[local-name()=''Context''][*[local-name()=''Type'']=''eHub Interchange Reference'']/*[local-name()=''Value'']'),
(@ACK_US_SLPK, @CW1MSG_STPK, @ACK_DTPK, '/*[local-name()=''UniversalInterchange'']/*[local-name()=''Body'']/*[local-name()=''UniversalShipment'']/*[local-name()=''Shipment'']/*[local-name()=''AdditionalReferenceCollection'']/*[local-name()=''AdditionalReference''][*[local-name()=''Type'']=''HIR'']/*[local-name()=''ReferenceNumber'']'),
(@BKC_SLPK, @CW1MSG_STPK, @BKC_DTPK, '/*[local-name()=''UniversalShipment'']/*[local-name()=''Shipment'']/*[local-name()=''AdditionalReferenceCollection'']/*[local-name()=''AdditionalReference''][*[local-name()=''Type'']=''HIR'']/*[local-name()=''ReferenceNumber'']')

DECLARE @CodeSet_EventPK uniqueidentifier = '34B227A1-DB50-4611-89FF-D36F9C526C64'
DECLARE @ResultSet_EventTypesPK uniqueidentifier = '0E1D5100-F1EB-4292-9FB3-19B5E8530990'
DECLARE @ResultSet_EventRefrnPK uniqueidentifier = '7D1E8F30-535B-49A4-B95F-8E74AA8DA298'

INSERT eHubCodeSet(CS_PK, CS_Name, CS_TS, CS_CC_Sender, CS_CC_Recipient, CS_Key1Name, CS_Key2Name, CS_Key3Name)
VALUES (@CodeSet_EventPK, 'Event Type', @CARGOWISE_TSPK, @CARGOWISE_CCPK, @CARGOWISE_CCPK, 'Message Type', 'Action Purpose', 'Status Code')

INSERT eHubCodeSetResult(CR_PK, CR_CS, CR_Order, CR_Name)
VALUES
(@ResultSet_EventTypesPK, @CodeSet_EventPK, 1, N'Event Type'),
(@ResultSet_EventRefrnPK, @CodeSet_EventPK, 2, N'Event Reference')

INSERT eHubCodeMapKey(CK_PK, CK_CS, CK_Order, CK_Key1Value, CK_Key2Value, CK_Key3Value)
VALUES
('3E07E03D-2760-4859-894F-A0E7F4443582', @CodeSet_EventPK, 1, 'Booking Confirmation', '%', '%'),
('165C3434-EB77-471A-9E94-2446A253C25E', @CodeSet_EventPK, 2, 'Acknowledgement', '%', '%')

INSERT eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode)
VALUES
('3E07E03D-2760-4859-894F-A0E7F4443582', @ResultSet_EventTypesPK, ''),
('3E07E03D-2760-4859-894F-A0E7F4443582', @ResultSet_EventRefrnPK, ''),
('165C3434-EB77-471A-9E94-2446A253C25E', @ResultSet_EventTypesPK, ''),
('165C3434-EB77-471A-9E94-2446A253C25E', @ResultSet_EventRefrnPK, '')

ROLLBACK
--COMMIT