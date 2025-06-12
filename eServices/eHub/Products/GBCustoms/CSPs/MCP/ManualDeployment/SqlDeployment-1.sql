SET NOCOUNT ON;

BEGIN TRANSACTION

DECLARE @EmptyPk UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000'

-- --------------------------------------------------------------------------------------------------------------------
-- 00. Declare parameters.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @ProductionApiUrl VARCHAR(64) = 'https://www.destin8.co.uk'
DECLARE @ProductionAuthorizationCode VARCHAR(64) = 'Basic ProductionCallbackAuthorization'
DECLARE @ProductionCallbackUrl VARCHAR(128) = 'https://gbmcpws.wisegrid.net/MCPNotification/ReceiveNotification'

DECLARE @TestApiUrl VARCHAR(64) = 'https://uat.destin8.co.uk'
DECLARE @TestAuthorizationCode VARCHAR(64) = 'Basic TestCallbackAuthorization'
DECLARE @TestCallbackUrl VARCHAR(128) = 'https://gbmcpws-test.wisegrid.net/MCPNotification/ReceiveNotification'

-- --------------------------------------------------------------------------------------------------------------------
-- 01. Finding existing MCP clients.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @SharedClientPk UNIQUEIDENTIFIER = (SELECT TOP 1 CC_PK FROM eHubClient WHERE CC_ID = 'GBCustoms')
DECLARE @ProductionClientPk UNIQUEIDENTIFIER = (SELECT TOP 1 CC_PK FROM eHubClient WHERE CC_ID = 'GBCustoms-MCP')
DECLARE @TestClientPk UNIQUEIDENTIFIER = (SELECT TOP 1 CC_PK FROM eHubClient WHERE CC_ID = 'GBCustomsTest-MCP')

PRINT '1> Finding MCP Production and Test clients...'

-- --------------------------------------------------------------------------------------------------------------------
-- 02. Create registration types.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @WebServiceRegistrationTypePk UNIQUEIDENTIFIER = '808e013b-6b4e-481b-bca9-fb7d1cfa395a'
DECLARE @AccountRegistrationTypePk UNIQUEIDENTIFIER = 'f325fc79-f74a-489a-b31e-b89e94059000'

INSERT INTO eHubTransactions..eHubRegistrationType(RT_PK, RT_ID, RT_Description, RT_RegistrantType)
SELECT @WebServiceRegistrationTypePk, 'GBCustoms-MCP', 'MCP Inbound Mesage Web Service Authorization and URL', 'Client' UNION ALL
SELECT @AccountRegistrationTypePk, 'GBCustoms-MCPAccount', 'GBCustoms MCP Account', 'ClientSystem'

PRINT '2> Created MCP Inbound Message WS and Account registration types...'

-- --------------------------------------------------------------------------------------------------------------------
-- 03. Create Inbound-Message WS callback registrations.
-- --------------------------------------------------------------------------------------------------------------------

INSERT INTO eHubTransactions..eHubClientRegistration(CX_PK, CX_CC, CX_RT, CX_Code, CX_Attr1, CX_Flag1, CX_Flag2)
SELECT '47543183-37b1-4668-bb3f-68187def18b7', @ProductionClientPk, @WebServiceRegistrationTypePk, @ProductionAuthorizationCode, @ProductionCallbackUrl, 0, 0 UNION ALL
SELECT '5a293f25-151f-4d67-8b74-b2349aafdddb', @TestClientPk, @WebServiceRegistrationTypePk, @TestAuthorizationCode, @TestCallbackUrl, 0, 0

PRINT '3> Created Inbound WS authorization codes and callback URLs...'

-- --------------------------------------------------------------------------------------------------------------------
-- 04. Create destination party API endpoints.
-- --------------------------------------------------------------------------------------------------------------------

DECLARE @TransformationSetPk UNIQUEIDENTIFIER = '5ad0883a-13eb-4142-8d2e-e299a195875b'
DECLARE @CodeSetPk UNIQUEIDENTIFIER = '0534c6fa-4fd4-47d3-b00f-30164bd000d7'
DECLARE @CodeSetResultPk UNIQUEIDENTIFIER = 'af7f13d8-eb12-45fd-8a04-439e8984c7ca'

INSERT INTO eHubTransactions..eHubTransformationSet(TS_PK, TS_Name, TS_CC_Sender, TS_CC_Recipient)
SELECT @TransformationSetPk, 'GBCustoms (MCP) System Configuration', @SharedClientPk, @SharedClientPk

INSERT INTO eHubTransactions..eHubCodeSet(CS_PK, CS_Name, CS_TS, CS_CC_Sender, CS_CC_Recipient, CS_Key1Name)
SELECT @CodeSetPk, 'Endpoints', @TransformationSetPk, @SharedClientPk, @SharedClientPk, 'DestinationParty'

INSERT INTO eHubTransactions..eHubCodeSetResult(CR_PK, CR_CS, CR_Order, CR_Name)
SELECT @CodeSetResultPk, @CodeSetPk, 1, 'Endpoint URL'

DECLARE @ProductionMapKeyPk UNIQUEIDENTIFIER = '23a276a9-7d8b-4473-ab2c-20c5a496e2d9'
DEClARE @TestMapKeyPk UNIQUEIDENTIFIER = '5b999241-be12-45f9-ada7-b2786b66d51b'

INSERT INTO eHubTransactions..eHubCodeMapKey(CK_PK, CK_CS, CK_Order, CK_Key1Value)
SELECT @ProductionMapKeyPk, @CodeSetPk, 1, 'GBCustoms-MCP' UNION ALL
SELECT @TestMapKeyPk, @CodeSetPk, 2, 'GBCustomsTest-MCP'

INSERT INTO eHubTransactions..eHubCodeMapValue(CV_CK, CV_CR, CV_OutputCode)
SELECT @ProductionMapKeyPk, @CodeSetResultPk, @ProductionApiUrl UNION ALL
SELECT @TestMapKeyPk, @CodeSetResultPk, @TestApiUrl

PRINT '4> Created system configuration enpoint URLs...'

-- COMMIT TRANSACTION
ROLLBACK TRANSACTION