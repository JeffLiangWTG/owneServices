CREATE ROUTE [ErrorServiceRoute]
    AUTHORIZATION [dbo]
    WITH SERVICE_NAME = N'//cargowise.com/eServices/ErrorProcessingService', ADDRESS = N'TCP://$(eHubTransactionsSBEndpoint)';

