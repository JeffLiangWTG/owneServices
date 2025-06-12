# eHubMessageFlow

1.  file is dropped into the receive ports folder
    1.  message goes through Rcv_ResolveInboundMessage pipeline
        1.  IncomingMessageLogger inserts the message into eHubInboxMessage table
        2.  StaticValuePromotionComponent sets the property NextStep=DeliverToOutbox
or
1.  gateway inserts to eHubInboxMessage table
    1.  message is picked up by send port Gateway_SelectInboxMessageByStatus_WCF-SQL (by reading ehubInboxMessage table)
        2.  StaticValuePromotionComponent sets the property NextStep=DeliverToOutbox
2.  a copy of the message from 1 is picked up by the send port Core_dbo_InsertInboxXmlContent (by checking for NextStep=DeliverToOutbox)
    1.  message goes through Send_AssembleAndTransform
    2.  The send port inserts the resulting message into eHubInboxXmlContent table
3.  a copy of the message from 1 is picked up by the send port Core_InsertFinalisedOutboxMessage_WCF-SQL (by checking for NextStep=DeliverToOutbox)
    1.  message goes through Snd_FormatRequestForOutbox
        1.  The MultipleTransformationComponent runs all transformations in the matching transformation set
    2.  The send port inserts the resulting message into eHubOutboxMessage table
4.  the outbox message is picked up by receive port Core_dbo_SelectOutboxEnvelopesBySchedule_AU_WCF-SQL (by reading eHubOutboxMessage table)
    1.  message goes through Rcv_MapXmlDisassemblePromote
        1.  StaticValuePromotionComponent sets the property NextStep=GenerateRecipientMessage
5.  a copy of the message from 4 is picked up by the send port Core_dbo_SelectOutboxMessageByEnvelopeID_WCF-SQL (by checking for NextStep=GenerateRecipientMessage)
    1.  message goes through Rcv_MapDynamicDebatchPromote
        1.  StaticValuePromotionComponent sets the property NextStep=DistributeToRecipient
6.  The debatched messages from 5 are picked up by the relevant send port (by checking for NextStep=DistributeToRecipient)


TODO: Combine eHub + CW1 sections explaining how gateway works


# CW1 Message Flow

1.  EHI Service Task
    1.  Poll gateway for new messages.
    2.  Process mesages and save to the DB tables StmBillingHeader, EDIMessage, EDIInterchange
2.  UMI Service Task
    1.  Read messages from EDIMessage, EDIInterchange
    2.  Process messages and save to JobShipment, StmBillingLine
3.  User can now open Forwarding -> Forwarding -> Shipments to view the imported shipments