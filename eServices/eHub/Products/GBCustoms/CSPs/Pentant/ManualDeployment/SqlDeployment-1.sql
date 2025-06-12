SET NOCOUNT ON;

BEGIN TRANSACTION

-- --------------------------------------------------------------------------------------------------------------------
-- 01. Create Pentant clients.
-- --------------------------------------------------------------------------------------------------------------------
DECLARE @SharedClientPk UNIQUEIDENTIFIER = (SELECT CC_PK FROM eHubClient WHERE CC_ID = 'GBCustoms')
DECLARE @ProductionClientPk				uniqueidentifier = 'CB2FB018-FF6C-4C85-B4C9-3BE3169FF703'	--SELECT NEWID()
DECLARE @TestClientPk			uniqueidentifier = 'C76E58B2-5F8E-4BE2-BE15-8E53A3C23376'	--SELECT NEWID()

INSERT INTO eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory, CC_PermitInboxRecipient, CC_PermitInboxSender)
SELECT @ProductionClientPk, 'GBCustoms-Pentant', 'Great Britain Customs - Pentant','00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party', 1, 1

INSERT INTO eHubClient
(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_DistributionZone, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory, CC_PermitInboxRecipient, CC_PermitInboxSender)
SELECT @TestClientPk, 'GBCustomsTest-Pentant', 'Great Britain Customs Test - Pentant ','00000000-0000-0000-0000-000000000000','75419F4C-C522-4890-BD5D-BCA5E12268F6','','','Service Provider','Third Party', 1, 1


PRINT '1> Created Pentant Production and Test clients...'

-- --------------------------------------------------------------------------------------------------------------------
-- 02. Create registration types.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @WebServiceRegistrationTypePk UNIQUEIDENTIFIER = 'A9B25B92-C646-4875-BA8F-C644F7BF4385'
DECLARE @AccountRegistrationTypePk UNIQUEIDENTIFIER = 'D23CB3E1-BC11-4BE0-960C-B9DCD202D114'

INSERT INTO eHubTransactions..eHubRegistrationType(RT_PK, RT_ID, RT_Description, RT_RegistrantType)
SELECT @AccountRegistrationTypePk, 'GBCustoms-PentantAccount', 'GBCustoms Pentant Account', 'ClientSystem'

PRINT '2> Created Pentant Inbound Message WS and Account registration types...'

-- --------------------------------------------------------------------------------------------------------------------
-- 03. Create destination party API endpoints.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @TransformationSetPk UNIQUEIDENTIFIER = 'CABDC171-19B6-4C4C-8500-D90488C03AB8'
DECLARE @CodeSetPk UNIQUEIDENTIFIER = '0131ABE4-45AC-4CD0-9CA5-26FF4D51DBF2'
DECLARE @CodeSetResultPk UNIQUEIDENTIFIER = 'A0F9EBCF-364D-480C-91A6-4D15E5DBB690'

INSERT INTO eHubTransactions..eHubTransformationSet(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient)
SELECT @TransformationSetPk, 'GBCustoms (Pentant) System Configuration', @SharedClientPk, @SharedClientPk

INSERT INTO eHubTransactions..eHubCodeSet(CS_PK, CS_Name, CS_TS, CS_CC_Sender, CS_CC_Recipient, CS_Key1Name)
SELECT @CodeSetPk, 'Endpoints', @TransformationSetPk, @SharedClientPk, @SharedClientPk, 'DestinationParty'

INSERT INTO eHubTransactions..eHubCodeSetResult(CR_PK, CR_CS, CR_Order, CR_Name)
SELECT @CodeSetResultPk, @CodeSetPk, 1, 'Endpoint URL'

DECLARE @ProductionMapKeyPk UNIQUEIDENTIFIER = 'A59862AA-E701-42E1-943D-F51AD2BF89B3'
DEClARE @TestMapKeyPk UNIQUEIDENTIFIER = '0164F102-6A34-43F4-A25F-EAF10FB4CEFE'

INSERT INTO eHubTransactions..eHubCodeMapKey(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT @ProductionMapKeyPk, @CodeSetPk, 1, 'GBCustoms-Pentant' UNION ALL
SELECT @TestMapKeyPk, @CodeSetPk, 2, 'GBCustomsTest-Pentant'

PRINT '3> Created system configuration enpoint URLs...'

-- --------------------------------------------------------------------------------------------------------------------
-- 04. Create TrasformationSet.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @GBCustomsMessageTypePK UNIQUEIDENTIFIER = (SELECT DT_PK FROM eHubMessageType WHERE DT_Code = 'http://cargowise.com/ehub/products/GBCustoms#GBCustoms')
DECLARE @GB2GB_TTPK UNIQUEIDENTIFIER = (SELECT TT_PK FROM eHubTransformationType WHERE TT_TransformationType LIKE 'CargoWise.eHub.Products.GBCustoms.Core.BT.Transforms.GBCustoms2GBCustoms%')
         
-- eHubTransformationSet For Prod
DECLARE @GBCustomsSendToPentant_TSPK uniqueidentifier = '001C4DA2-EA55-473B-9D70-F145538258EC'	--SELECT NEWID()
INSERT INTO eHubTransactions..eHubTransformationSet(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillSender, TS_BillRecipient)
SELECT @GBCustomsSendToPentant_TSPK, 'GB Customs Send to Descartes Pentant', null, @ProductionClientPk, @GBCustomsMessageTypePK , 0, 0

INSERT INTO eHubTransactions..eHubTransformationMapping(TM_TS_PK, TM_Order, TM_TT_PK)
VALUES
(@GBCustomsSendToPentant_TSPK, 0, @GB2GB_TTPK)

-- eHubTransformationSet For Test
DECLARE @GBCustomsSendToPentantTest_TSPK uniqueidentifier = '49785F20-3839-499E-80D7-CF5379FC551B'	--SELECT NEWID()
INSERT INTO eHubTransactions..eHubTransformationSet(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient, TS_DT_Source, TS_BillSender, TS_BillRecipient)
SELECT @GBCustomsSendToPentantTest_TSPK, 'GB Customs Send to Descartes Pentant Test', null, @TestClientPk, @GBCustomsMessageTypePK , 0, 0

INSERT INTO eHubTransactions..eHubTransformationMapping(TM_TS_PK, TM_Order, TM_TT_PK)
VALUES
(@GBCustomsSendToPentantTest_TSPK, 0, @GB2GB_TTPK)

PRINT '4> Created TansformationSets for prod and test...'

--COMMIT TRANSACTION
ROLLBACK TRANSACTION
