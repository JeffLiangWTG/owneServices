CREATE ROUTE [OutboxServiceRouteTest]
    AUTHORIZATION [dbo]
    WITH SERVICE_NAME = N'//cargowise.com/eServices/USCustoms/eHubOutboxServiceTest', ADDRESS = N'TCP://$(eHubTransactionsSBTestEndpoint)';

