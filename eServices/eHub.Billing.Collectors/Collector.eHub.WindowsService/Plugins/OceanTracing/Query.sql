SET NOCOUNT ON;

IF OBJECT_ID(N'TempDB..#Interfaces') IS NOT NULL DROP TABLE #Interfaces;
SELECT [TS_PK]
        ,[TS_Name]
        ,[TS_CC_Sender]
        ,[TS_CC_Recipient]
        ,[TS_BillingXPathSource]
        ,[TS_BillingXPathTarget]
        ,[TS_BillSender]
        ,[TS_BillRecipient]
        ,[TS_CC_BillOther]
    INTO #Interfaces
    FROM [dbo].[eHubTransformationSet]
    WHERE TS_Name LIKE 'Ocean Containers%' AND TS_BillingElement IS NOT NULL;

IF  OBJECT_ID(N'TempDB..#MsgData') IS NOT NULL DROP TABLE #MsgData;
SELECT CASE
            WHEN TS_BillSender = 1 THEN ISNULL(TS_CC_Sender, AM_CC_SenderInbox)
            WHEN TS_BillRecipient = 1 THEN ISNULL(TS_CC_Recipient, AM_CC_RecipientOutbox)
            ELSE TS_CC_BillOther
        END AM_CC_Billed,
        AM_ReceivedFromSenderUTC,
        AM_TS,
		CASE
            WHEN TS_BillSender = 1 THEN AM_InboxMessageTrackingID
            WHEN TS_BillRecipient = 1 THEN AM_OutboxMessageTrackingID
            ELSE AM_PK
        END MessageTrackingID,
        CAST(CASE
                WHEN TS_BillingXPathSource IS NOT NULL THEN AM_SenderMessageXML
                WHEN TS_BillingXPathTarget IS NOT NULL THEN AM_RecipientMessageXML
            END AS xml)
            AM_MessageXml,
		AM_ArchivedUTC
    INTO #MsgData
    FROM eHubArchiveMessage
    JOIN #Interfaces ON AM_TS = TS_PK
    WHERE @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
    AND AM_Status < 255
	OPTION(RECOMPILE)

DECLARE @TS_PK uniqueidentifier, @SQL nvarchar(max) = '';
DECLARE Interfaces CURSOR FOR SELECT TS_PK FROM #Interfaces;
OPEN Interfaces; FETCH NEXT FROM Interfaces INTO @TS_PK;
    
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @SQL = @SQL + 
    'SELECT AM_ReceivedFromSenderUTC,
			CC_ID ClientID,
			MessageTrackingID,' +
            CASE WHEN TS_Name LIKE '%Email' 
				THEN 'Item.value(''string(/*[1]/*[local-name()="BGM"][1]/*[local-name()="C002"][1]/*[local-name()="C00201"][1])'''
                ELSE 'Item.value(''string(*[local-name()="EventType"][1])'''
            END + ', ''nvarchar(200)'') EventType, ' +
			CASE WHEN TS_Name LIKE '%Email' 
				THEN 'Item.value(''string(*[local-name()="DTM_2"][1]/*[local-name()="C507_2"][1]/*[local-name()="C50702"][1])'''
                ELSE 'Item.value(''string(*[local-name()="EventTime"][1])'''
            END + ', ''nvarchar(200)'') EventTime, ' +
			CASE WHEN TS_Name LIKE '%Email' 
				THEN 'Item.value(''string(*[local-name()="EQD"][1]/*[local-name()="C237" or local-name()="C237_2"][1]/*[local-name()="C23701"][1])'''
                ELSE 'Item.value(''string(*[local-name()="ContextCollection"][1]
                            /*[local-name()="Context" and *[local-name()="Type" and text()="ContainerNumber"]][1]
                            /*[local-name()="Value"][1])'''
            END + ', ''nvarchar(200)'') ContainerNumber, AM_ArchivedUTC ' + 
        'FROM #MsgData
        JOIN #Interfaces ON AM_TS = TS_PK
        JOIN dbo.eHubClient ON AM_CC_Billed = CC_PK
        OUTER APPLY AM_MessageXml.nodes(''' + ISNULL(TS_BillingXPathSource, TS_BillingXPathTarget) + ''') Detail(Item)
        WHERE AM_TS = ''' + CAST(#Interfaces.TS_PK AS nvarchar(36)) + ''''
        FROM #Interfaces
        WHERE TS_PK = @TS_PK;

    FETCH NEXT FROM Interfaces INTO @TS_PK;
    IF @@FETCH_STATUS = 0
        SET @SQL = @SQL + '
        UNION ALL
        '
END
    
CLOSE Interfaces; DEALLOCATE Interfaces;

IF @SQL != ''
BEGIN
    EXEC sp_executesql @SQL;
END