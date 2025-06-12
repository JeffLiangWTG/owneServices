    CREATE ENDPOINT [ServiceBrokerEndpoint]
        AUTHORIZATION [sa] 
        STATE = STARTED
        AS TCP (
                LISTENER_PORT = 4741,
                LISTENER_IP = ALL
               )
        FOR SERVICE_BROKER (
                AUTHENTICATION = CERTIFICATE ServiceBrokerTransportSecurity,
                ENCRYPTION = DISABLED
                           );
