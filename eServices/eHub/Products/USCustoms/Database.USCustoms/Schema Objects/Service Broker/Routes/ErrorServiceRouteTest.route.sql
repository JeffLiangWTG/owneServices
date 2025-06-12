CREATE ROUTE [ErrorServiceRouteTest]
    AUTHORIZATION [dbo]
    WITH SERVICE_NAME = N'//cargowise.com/eServices/ErrorProcessingServiceTest', ADDRESS = N'TCP://$(eHubTransactionsSBTestEndpoint)';

