SET NOCOUNT ON;

SELECT * FROM (
	SELECT 
		RecipientOutbox.CC_ID AS ClientID,
		ContainerNodes.ContainerNode.value('(*:ContainerNumber)[1]','varchar(50)') AS ContainerNumber,
		AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:BookingConfirmationReference)[1]', 'varchar(50)') AS BookingNumber,
		AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:WayBillNumber)[1]', 'varchar(50)') AS BillNumber,
		Sender.CC_ID AS Sender,
		AM_OutboxMessageTrackingID AS MessageTrackingID,
		AM_ReceivedFromSenderUTC,
		AM_ArchivedUTC
	FROM
		eHubArchiveMessage
		JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
		JOIN [dbo].[eHubClient] RecipientOutbox ON RecipientOutbox.CC_PK = AM_CC_RecipientOutbox
		JOIN [eHubTransactionsServer].[eHubTransactions].[dbo].[eHubServiceProvider] ON SP_CC_Provider = AM_CC_RecipientOutbox
		OUTER APPLY AM_RecipientMessageXML.nodes('/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:ContainerCollection/*:Container') AS ContainerNodes(ContainerNode)
	WHERE @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
	AND AM_Status < 255
	AND RecipientOutbox.CC_OwnerCategory = 'Client'
	AND RecipientOutbox.CC_SystemCategory = 'Enterprise'
	AND SP_CC_Service = 'a8f34356-f459-4805-8026-9cb072b7e0a3'
	AND AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:RecipientRoleCollection/*:RecipientRole/*:ServiceCode)[1]', 'varchar(3)') = 'VGM'
)result
WHERE BookingNumber IS NOT NULL OR BillNumber IS NOT NULL
OPTION(RECOMPILE)
