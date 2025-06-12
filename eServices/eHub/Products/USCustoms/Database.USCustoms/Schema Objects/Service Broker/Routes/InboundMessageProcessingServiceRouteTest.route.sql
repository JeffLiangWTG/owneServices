CREATE ROUTE [InboundMessageProcessingServiceRouteTest]
    AUTHORIZATION [dbo]
    WITH SERVICE_NAME = N'//cargowise.com/eServices/USCustoms/InboundMessageProcessingServiceTest', ADDRESS = N'TCP://$(eHubTransactionsSBTestEndpoint)';

