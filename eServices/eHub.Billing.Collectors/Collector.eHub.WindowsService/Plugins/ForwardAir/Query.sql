SET NOCOUNT ON;

WITH XMLNAMESPACES ('http://www.cargowise.com/Schemas/Universal/2011/11' AS ns)
SELECT * FROM
(
	SELECT
		CASE WHEN Recipient.CC_ID = 'FORAIRCMH' THEN Sender.CC_ID ELSE Recipient.CC_ID END AS ClientID,
		CASE WHEN Recipient.CC_ID = 'FORAIRCMH' THEN AM_InboxMessageTrackingID ELSE AM_OutboxMessageTrackingID END AS MessageTrackingID,
		CASE 
		WHEN Recipient.CC_ID = 'FORAIRCMH' THEN
		AM_SenderMessageXML.value('(/ns:UniversalInterchange/ns:Body/ns:UniversalShipment/ns:Shipment/ns:DataContext/ns:DataSourceCollection/ns:DataSource[ns:Type="ForwardingConsol"]/ns:Key)[1]','varchar(50)')
		ELSE
		AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/ns:UniversalEvent/ns:Event/ns:DataContext/ns:DataTargetCollection/ns:DataTarget[ns:Type="ForwardingConsol"]/ns:Key)[1]','varchar(50)')
		END AS Consol,
		AM_ReceivedFromSenderUTC,
		AM_ArchivedUTC
	FROM
		eHubArchiveMessage
		JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
		JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientOutbox
	WHERE
		@startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
		AND (Recipient.CC_ID = 'FORAIRCMH' OR Sender.CC_ID = 'FORAIRCMH')
		AND AM_Status < 255
)result
WHERE LEN(Consol) > 0 AND LEN(Consol) < 21
OPTION(RECOMPILE)
