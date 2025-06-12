SET NOCOUNT ON;

WITH XMLNAMESPACES ('http://www.cargowise.com/Schemas/Universal/2011/11' AS ns)
SELECT
	Sender.CC_ID AS ClientID,
	AM_SenderMessageXML.value('(/ns:UniversalInterchange/ns:Body/ns:UniversalShipment/ns:Shipment/ns:PortMessaging/ns:MessagePurpose)[1]','varchar(3)') AS MessagePurpose,
	AM_SenderMessageXML.value('(/ns:UniversalInterchange/ns:Body/ns:UniversalShipment/ns:Shipment/ns:DataContext/ns:DataSourceCollection/ns:DataSource[ns:Type="ForwardingConsol"]/ns:Key)[1]','varchar(50)') AS Consol,
	AM_SenderMessageXML.value('(/ns:UniversalInterchange/ns:Body/ns:UniversalShipment/ns:Shipment/ns:DataContext/ns:DataSourceCollection/ns:DataSource[ns:Type="ForwardingShipment"]/ns:Key)[1]','varchar(50)') AS Shipment,
	AM_InboxMessageTrackingID AS MessageTrackingID,
	AM_ReceivedFromSenderUTC,
	AM_ArchivedUTC
FROM
	eHubArchiveMessage
	JOIN [eHubTransactions].[dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
	JOIN [eHubTransactions].[dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientOutbox
WHERE
	@startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
	AND Recipient.CC_ID = 'DAKOSYHAM'
	AND AM_Status < 255