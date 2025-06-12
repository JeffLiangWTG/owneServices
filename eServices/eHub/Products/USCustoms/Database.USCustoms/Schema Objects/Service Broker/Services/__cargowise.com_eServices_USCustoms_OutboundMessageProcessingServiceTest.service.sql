CREATE SERVICE [//cargowise.com/eServices/USCustoms/OutboundMessageProcessingServiceTest]
    AUTHORIZATION [dbo]
    ON QUEUE [dbo].[USCustomsOutboundMessageProcessingQueueTest]
    ([//cargowise.com/eServices/USCustoms/OutboundMessageProcessingContract]);

