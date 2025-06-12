GRANT CONNECT TO [dbo]
    AS [dbo];


GO
GRANT CONNECT TO [eHubReader]
    AS [dbo];


GO
GRANT SELECT TO [eHubReader]
    AS [dbo];


GO
GRANT VIEW DEFINITION TO [eHubReader]
    AS [dbo];


GO
GRANT SEND
    ON SERVICE::[//cargowise.com/eServices/USCustoms/OutboundMessageProcessingService] TO [public]
    AS [dbo];

GO
GRANT SEND
    ON SERVICE::[//cargowise.com/eServices/USCustoms/OutboundMessageProcessingServiceTest] TO [public]
    AS [dbo];

GO