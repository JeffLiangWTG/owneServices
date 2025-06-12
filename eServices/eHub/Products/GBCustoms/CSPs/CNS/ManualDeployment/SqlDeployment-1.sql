SET NOCOUNT ON;

BEGIN TRANSACTION

DECLARE @EmptyPk UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000'

-- --------------------------------------------------------------------------------------------------------------------
-- 00. Declare parameters.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @ProductionApiUrl VARCHAR(64) = 'https://www.cnsonline.co.uk'
DECLARE @ProductionAuthorizationCode VARCHAR(64) = 'Basic ProductionCallbackAuthorization'
DECLARE @ProductionCallbackUrl VARCHAR(128) = 'https://gbcnsws.wisegrid.net/CNSNotification/ReceiveNotification'

DECLARE @TestApiUrl VARCHAR(64) = 'https://www.uat.cnsonline.co.uk'
DECLARE @TestAuthorizationCode VARCHAR(64) = 'Basic TestCallbackAuthorization'
DECLARE @TestCallbackUrl VARCHAR(128) = 'https://gbcnsws-test.wisegrid.net/CNSNotification/ReceiveNotification'

-- --------------------------------------------------------------------------------------------------------------------
-- 01. Finding existing CNS clients.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @SharedClientPk UNIQUEIDENTIFIER = (SELECT TOP 1 CC_PK FROM eHubClient WHERE CC_ID = 'GBCustoms')
DECLARE @ProductionClientPk UNIQUEIDENTIFIER = (SELECT TOP 1 CC_PK FROM eHubClient WHERE CC_ID = 'GBCustoms-CNS')
DECLARE @TestClientPk UNIQUEIDENTIFIER = (SELECT TOP 1 CC_PK FROM eHubClient WHERE CC_ID = 'GBCustomsTest-CNS')

PRINT '1> Finding CNS Production and Test clients...'

-- --------------------------------------------------------------------------------------------------------------------
-- 02. Create registration types.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @WebServiceRegistrationTypePk UNIQUEIDENTIFIER = '641A708C-A34B-4BDC-BEE6-B967B613A891'
DECLARE @AccountRegistrationTypePk UNIQUEIDENTIFIER = '734EF6D2-4686-4876-A80A-BCBE824192AA'

INSERT INTO eHubTransactions..eHubRegistrationType(RT_PK, RT_ID, RT_Description, RT_RegistrantType)
SELECT @WebServiceRegistrationTypePk, 'GBCustoms-CNS', 'CNS Inbound Mesage Web Service Authorization and URL', 'Client' UNION ALL
SELECT @AccountRegistrationTypePk, 'GBCustoms-CNSAccount', 'GBCustoms CNS Account', 'ClientSystem'

PRINT '2> Created CNS Inbound Message WS and Account registration types...'

-- --------------------------------------------------------------------------------------------------------------------
-- 03. Create Inbound-Message WS callback registrations.
-- --------------------------------------------------------------------------------------------------------------------

INSERT INTO eHubTransactions..eHubClientRegistration(CX_PK, CX_CC, CX_RT, CX_Code, CX_Attr1, CX_Flag1, CX_Flag2)
SELECT '961DDA9C-3F22-42B4-89B1-BFDC3EDE3A24', @ProductionClientPk, @WebServiceRegistrationTypePk, @ProductionAuthorizationCode, @ProductionCallbackUrl, 0, 0 UNION ALL
SELECT '7670973D-D3E5-4B82-82F6-DDDC34B133C8', @TestClientPk, @WebServiceRegistrationTypePk, @TestAuthorizationCode, @TestCallbackUrl, 0, 0

PRINT '3> Created Inbound WS authorization codes and callback URLs...'

-- --------------------------------------------------------------------------------------------------------------------
-- 04. Create destination party API endpoints.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @TransformationSetPk UNIQUEIDENTIFIER = '5D40F819-1769-43EF-AA21-3632B6252BD8'
DECLARE @CodeSetPk UNIQUEIDENTIFIER = '61DBA7FD-82A6-46E3-968A-BAC2701E6DEA'
DECLARE @CodeSetResultPk UNIQUEIDENTIFIER = 'BBE4549D-170E-4E50-B92C-DE2CFCCC803B'

INSERT INTO eHubTransactions..eHubTransformationSet(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient)
SELECT @TransformationSetPk, 'GBCustoms (CNS) System Configuration', @SharedClientPk, @SharedClientPk

INSERT INTO eHubTransactions..eHubCodeSet(CS_PK, CS_Name, CS_TS, CS_CC_Sender, CS_CC_Recipient, CS_Key1Name)
SELECT @CodeSetPk, 'Endpoints', @TransformationSetPk, @SharedClientPk, @SharedClientPk, 'DestinationParty'

INSERT INTO eHubTransactions..eHubCodeSetResult(CR_PK, CR_CS, CR_Order, CR_Name)
SELECT @CodeSetResultPk, @CodeSetPk, 1, 'Endpoint URL'

DECLARE @ProductionMapKeyPk UNIQUEIDENTIFIER = '6D57496E-9EE3-45D6-8179-A7AC8255870F'
DEClARE @TestMapKeyPk UNIQUEIDENTIFIER = 'FEB389AC-B0DF-4F33-AB56-596218213B39'

INSERT INTO eHubTransactions..eHubCodeMapKey(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT @ProductionMapKeyPk, @CodeSetPk, 1, 'GBCustoms-CNS' UNION ALL
SELECT @TestMapKeyPk, @CodeSetPk, 2, 'GBCustomsTest-CNS'

INSERT INTO eHubTransactions..eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode)
SELECT @ProductionMapKeyPk, @CodeSetResultPk, @ProductionApiUrl UNION ALL
SELECT @TestMapKeyPk, @CodeSetResultPk, @TestApiUrl

PRINT '4> Created system configuration enpoint URLs...'

-- COMMIT TRANSACTION
ROLLBACK TRANSACTION