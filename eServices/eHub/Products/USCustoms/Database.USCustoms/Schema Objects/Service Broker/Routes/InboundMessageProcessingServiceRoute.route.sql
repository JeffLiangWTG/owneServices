CREATE ROUTE [InboundMessageProcessingServiceRoute]
    AUTHORIZATION [dbo]
    WITH SERVICE_NAME = N'//cargowise.com/eServices/USCustoms/InboundMessageProcessingService', ADDRESS = N'TCP://$(eHubTransactionsSBEndpoint)';

