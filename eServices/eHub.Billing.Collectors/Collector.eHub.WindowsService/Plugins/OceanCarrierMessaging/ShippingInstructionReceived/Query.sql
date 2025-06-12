SET NOCOUNT ON;

WITH XMLNAMESPACES ('http://www.cargowise.com/Schemas/Universal/2011/11' AS ns)
SELECT
	RecipientOutbox.CC_ID AS ClientID,
	AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:EventType)[1]','varchar(50)') AS EventType,
	CASE WHEN AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]','varchar(50)') = 'eManifest'
		THEN	AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type="ForwardingShipment"]/*:Key)[1]','varchar(50)')
		ELSE	AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type="ForwardingConsol"]/*:Key)[1]','varchar(50)')
	END as JobNumber,
	CASE	WHEN AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]','varchar(50)') IN ('Booking Request', 'Shipping Instruction', 'Shipping Order')
				THEN COALESCE (	AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="CoLoadBookingConfirmationReference"]/*:Value)[1]','varchar(50)'),
								AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:ContextCollection/*:Context[*:Type="CarriersBookingReference"]/*:Value)[1]','varchar(50)'))
			WHEN AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]','varchar(50)') = 'Verified Gross Container Weight'
				THEN AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:EventParameters/*:EquipmentReferenceNumber)[1]','varchar(50)')
			WHEN AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]','varchar(50)') = 'eManifest'
				THEN AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:EventParameters/*:ReferenceNumber)[1]','varchar(50)')
			ELSE NULL
	END as RefNumber,
	Sender.CC_ID AS Provider,
	AM_OutboxMessageTrackingID AS MessageTrackingID,
	AM_ReceivedFromSenderUTC,
	AM_ArchivedUTC
FROM
	eHubArchiveMessage
	JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
	JOIN [dbo].[eHubClient] RecipientOutbox ON RecipientOutbox.CC_PK = AM_CC_RecipientOutbox
	JOIN [eHubTransactionsServer].[eHubTransactions].[dbo].[eHubServiceProvider] ON SP_CC_Provider = Sender.CC_PK
WHERE @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
AND AM_Status < 255
AND SP_CC_Service = 'a8f34356-f459-4805-8026-9cb072b7e0a3'
AND Sender.CC_ID != RecipientOutbox.CC_ID
AND AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:DataContext/*:DataTargetCollection/*:DataTarget/*:Type)[1]', 'varchar(50)') in ('ForwardingConsol', 'ForwardingShipment')
AND AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'varchar(50)') IN ('Booking Request', 'Shipping Instruction', 'Shipping Order', 'Verified Gross Container Weight', 'eManifest')
AND AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalEvent/*:Event/*:EventType)[1]', 'varchar(3)') NOT IN ('IAK', 'IRA', 'IRJ', 'ISN')
OPTION(RECOMPILE)
