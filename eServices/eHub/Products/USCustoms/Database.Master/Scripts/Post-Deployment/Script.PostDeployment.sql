/* 
--------------------------------------------------------------------------------------
Begin Post-Deployment Script
--------------------------------------------------------------------------------------
*/

IF NOT EXISTS (SELECT * FROM sys.certificates WHERE [name] = 'ServiceBrokerTransportSecurity')
BEGIN
    PRINT N'Creating [ServiceBrokerTransportSecurity]...';

    :r "..\..\Schema Objects\Security\Certificates\ServiceBrokerTransportSecurity.certificate.sql"
END


GO
IF NOT EXISTS (SELECT * FROM sys.certificates WHERE [name] = 'ServiceBrokerTransportSecuritySYD6A')
BEGIN
    PRINT N'Creating [ServiceBrokerTransportSecuritySYD6A]...';
    :r "..\..\Schema Objects\Security\Certificates\ServiceBrokerTransportSecuritySYD6A.certificate.sql"
END


GO
IF NOT EXISTS (SELECT * FROM sys.certificates WHERE [name] = 'ServiceBrokerTransportSecuritySYD6B')
BEGIN
    PRINT N'Creating [ServiceBrokerTransportSecuritySYD6B]...';
    :r "..\..\Schema Objects\Security\Certificates\ServiceBrokerTransportSecuritySYD6B.certificate.sql"
END


GO
IF NOT EXISTS (SELECT * FROM sys.endpoints WHERE type_desc = N'SERVICE_BROKER')
BEGIN
    PRINT N'Creating [ServiceBrokerEndpoint]...';
    :r "..\..\Schema Objects\Service Broker\ServiceBrokerEndpoint.endpoint.sql"

    GRANT CONNECT ON ENDPOINT::ServiceBrokerEndpoint TO public
END


GO
/* 
--------------------------------------------------------------------------------------
End Post-Deployment Script
--------------------------------------------------------------------------------------
*/
