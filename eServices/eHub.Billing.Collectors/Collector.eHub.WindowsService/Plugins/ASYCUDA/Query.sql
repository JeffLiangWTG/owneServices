SET NOCOUNT ON;

WITH XMLNAMESPACES ('http://www.cargowise.com/Schemas/Universal/2011/11' AS ns)
SELECT * FROM
(
	SELECT
		Sender.CC_ID AS ClientID,
		AM_SenderMessageXML.value('(/ns:UniversalInterchange/ns:Body/ns:UniversalShipment/ns:Shipment/ns:DataContext/ns:DataSourceCollection/ns:DataSource[ns:Type="AsycudaManifest"]/ns:Key)[1]','varchar(50)') AS Consol,
		RIGHT(AM_SenderMessageXML.value('(/ns:UniversalInterchange/ns:Body/ns:UniversalShipment/ns:Shipment/ns:DataContext/ns:ActionPurpose/ns:Code)[1]','varchar(3)'), 2) AS CountryCode,
		AM_InboxMessageTrackingID AS MessageTrackingID,
		AM_ReceivedFromSenderUTC,
		AM_ArchivedUTC
	FROM
		eHubArchiveMessage
		JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
		JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientOutbox
	WHERE
		@startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
		AND Recipient.CC_ID LIKE 'ASYCUDA%'
		AND AM_Status < 255
)result
WHERE LEN(CountryCode) > 1
OPTION(RECOMPILE)