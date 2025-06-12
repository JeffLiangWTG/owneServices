CREATE ROUTE [OutboxServiceRoute]
    AUTHORIZATION [dbo]
    WITH SERVICE_NAME = N'//cargowise.com/eServices/USCustoms/eHubOutboxService', ADDRESS = N'TCP://$(eHubTransactionsSBEndpoint)';

