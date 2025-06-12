CREATE SERVICE [//cargowise.com/eServices/USCustoms/OutboundMessageProcessingService]
    AUTHORIZATION [dbo]
    ON QUEUE [dbo].[USCustomsOutboundMessageProcessingQueue]
    ([//cargowise.com/eServices/USCustoms/OutboundMessageProcessingContract]);

