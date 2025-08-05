SET NOCOUNT ON;

SELECT
    AM_ReceivedFromSenderUTC,
    Sender.CC_ID AS ClientID,
    AM_InboxMessageTrackingID AS MessageTrackingID,
    EventNodes.Item.value('string(*[local-name()="EventType"][1])', 'nvarchar(200)') AS EventType,
    EventNodes.Item.value('string(*[local-name()="EventTime"][1])', 'nvarchar(200)') AS EventTime,
    EventNodes.Item.value('string(*[local-name()="ContextCollection"][1]/*[local-name()="Context" and *[local-name()="Type" and text()="ContainerNumber"]][1]/*[local-name()="Value"][1])', 'nvarchar(200)') AS ContainerNumber,
    AM_ArchivedUTC
FROM eHubArchiveMessage
JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientOutbox
OUTER APPLY COALESCE(AM_RecipientMessageXML, AM_SenderMessageXML).nodes('/*[local-name()="UniversalInterchange"]/*[local-name()="Body"]/*[local-name()="UniversalEvent"]') EventNodes(Item)
WHERE @startUTC < AM_ArchivedUTC
    AND AM_ArchivedUTC <= @endUTC
    AND AM_Status < 255
    AND Sender.CC_ID IN ('NZWLG','NZNSN','MET','ICCLAEPOM','OCT_IRELAND','POROFAAKL','PORT_TAURANGA')
    AND AM_CC_RecipientOutbox IS NOT NULL
OPTION(RECOMPILE);

