SET NOCOUNT ON;

DECLARE @crlf NCHAR(2) = CHAR(13) + CHAR(10);
DECLARE @backdateUTC DATETIME = DATEADD(DAY, -30, @startUTC)

IF  OBJECT_ID(N'TempDB..#ToProvider') IS NOT NULL DROP TABLE #ToProvider;
IF  OBJECT_ID(N'TempDB..#FromProvider') IS NOT NULL DROP TABLE #FromProvider;

SELECT  CASE WHEN m.AM_CC_SenderInbox = '25582C3A-A669-4E8A-8792-FF231D26971A'
                 THEN m.AM_EmailSubjectOverride
                 ELSE Sender.CC_ID END ClientID,
        Recipient.CC_ID Network,
        AM_ReceivedFromSenderUTC,
        CAST(dbo.DecodeAndDecompress(AM_SenderMessageRaw) AS XML) MessageXml,
		AM_InboxMessageTrackingID MessageTrackingID,
		AM_EI_PK MessageEIPK,
		AM_ArchivedUTC
INTO #ToProvider
FROM (
	SELECT *   
		FROM eHubArchiveMessage
		WHERE (@startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC)
		AND  (AM_ReceivedFromSenderUTC > @backdateUTC AND AM_ReceivedFromSenderUTC <= @endUTC)
		AND     (AM_CC_RecipientInbox = '9819EFF9-9CD8-4622-B58E-32A251115722' -- ri.CC_ID = 'eHubAirService'
                        OR AM_CC_SenderInbox = '25582C3A-A669-4E8A-8792-FF231D26971A')
                AND     AM_Status < 255
) m
JOIN [dbo].eHubClient Sender ON m.AM_CC_SenderInbox = Sender.CC_PK
JOIN [dbo].eHubClient Recipient ON m.AM_CC_RecipientOutbox = Recipient.CC_PK
WHERE LEN(Sender.CC_ID) <= 9
OPTION(RECOMPILE)

UPDATE #ToProvider
SET MessageXml = 
(SELECT CAST(dbo.DecodeAndDecompress(AM_SenderMessageRaw) AS XML) FROM eHubArchiveMessage WHERE AM_EI_PK = #ToProvider.MessageEIPK AND AM_SenderMessageRaw IS NOT NULL AND AM_ReceivedFromSenderUTC = #ToProvider.AM_ReceivedFromSenderUTC)
WHERE MessageXml is null
OPTION(RECOMPILE) 

SELECT	Recipient.CC_ID ClientID,
        Sender.CC_ID Network,
        AM_ReceivedFromSenderUTC,
        AM_SenderMessageXml MessageXml,
        CASE WHEN AM_SenderMessageXml IS NULL THEN dbo.DecodeAndDecompress(AM_SenderMessageRaw) END MessageRaw,
		AM_OutboxMessageTrackingID MessageTrackingID,
		AM_ArchivedUTC
  INTO	#FromProvider
  FROM	eHubArchiveMessage
  JOIN	[dbo].eHubClient Sender ON AM_CC_SenderInbox = Sender.CC_PK
  JOIN	[dbo].eHubClient Recipient ON AM_CC_RecipientOutbox = Recipient.CC_PK
 WHERE	(@startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC)
   AND  (AM_ReceivedFromSenderUTC > @backdateUTC AND AM_ReceivedFromSenderUTC <= @endUTC)
   AND	Sender.CC_IsAirServiceProvider = 1
   AND	AM_Status < 255
   AND  LEN(Recipient.CC_ID) <= 9
   AND  AM_DT_RecipientMessageType <> '15A42A38-8111-40CC-8A89-D6C5981C236A' -- DT_Code = 'HKCustoms'
   OPTION(RECOMPILE)

;WITH ToProviderData AS
(
	SELECT ClientID,
		   MessageXml.value('(/*[local-name()="CargoIMP"]/*[local-name()="MessageType"])[1]','varchar(3)') MsgType,
		   MessageXml.value('(/*[local-name()="CargoIMP"]/*[local-name()="Carrier"])[1]','varchar(3)') AirlineCodeOrPrefix,
		   Network,
		   MessageXml.value('(/*[local-name()="CargoIMP"]/*[local-name()="MAWB"])[1]','varchar(30)') MAWB,
		   MessageXml.value('(/*[local-name()="CargoIMP"]/*[local-name()="HAWB"])[1]','varchar(30)') HAWB,
		   NULL StatusCode,
		   AM_ReceivedFromSenderUTC,
		   MessageTrackingID,
		   AM_ArchivedUTC
	  FROM #ToProvider
),
FromProviderData1 AS
(
	SELECT ClientID,
		   MessageXml.value('(/*[local-name()="FSU_FSA12"]/MessageIdentification/MessageIdentifier)[1]','varchar(3)') MsgType,
		   MessageXml.value('(/*[local-name()="FSU_FSA12"]/ConsignmentRecordGroup/ConsignmentDetail/AWBDetails/AirlinePrefix)[1]','varchar(3)') AirlineCodeOrPrefix,
		   Network,
		   MessageXml.value('(/*[local-name()="FSU_FSA12"]/ConsignmentRecordGroup/ConsignmentDetail/AWBDetails/AWBSerialNumber)[1]','varchar(30)') MAWB,
		   NULL HAWB,
		   MessageXml.query('local-name((/*[local-name()="FSU_FSA12"]/ConsignmentRecordGroup/StatusRecordGroup/OptionalRecords/*[1])[1])') StatusCode,
		   AM_ReceivedFromSenderUTC,
		   MessageTrackingID,
		   AM_ArchivedUTC
	  FROM #FromProvider
	  WHERE MessageXml.exist('/*[local-name()="FSU_FSA12"]') = 1 
),
FromProviderData2 AS
(
	SELECT ClientID,
		   CASE WHEN MessageXml.value('(/*[local-name()="FMA_FNA"]/MessageType/Type)[1]','varchar(3)') = 'FNA'
				THEN '-' + RIGHT(MessageXml.value('local-name((/*[local-name()="FMA_FNA"]/OriginalMessage/*)[1])','varchar(3)'), 2)
				ELSE MessageXml.value('(/*[local-name()="FMA_FNA"]/MessageType/Type)[1]','varchar(3)')
		   END MsgType,
		   MessageXml.value('(/*[local-name()="FMA_FNA"]/OriginalMessage/*/MBI/Details/AirlinePrefix)[1]','varchar(3)') AirlineCodeOrPrefix,
		   Network,
		   MessageXml.value('(/*[local-name()="FMA_FNA"]/OriginalMessage/*/MBI/Details/Details/MAWB)[1]','varchar(30)') MAWB,
		   NULL HAWB,
		   NULL StatusCode,
		   AM_ReceivedFromSenderUTC,
		   MessageTrackingID,
		   AM_ArchivedUTC
	  FROM #FromProvider  WHERE MessageXml.exist('/*[local-name()="FMA_FNA"]/OriginalMessage/*/MBI/Details/Details/MAWB') = 1
),
FromProviderData3 AS
(
	SELECT ClientID,
		   CASE WHEN RawAsXml.value('/P1[3]/P2[1]/P3[1]/@Value','varchar(3)') = 'FNA'
				THEN '-' + CASE WHEN RawAsXml.exist('/P1[3]/P2/P3[1][@Value="FHL"]') = 1 THEN 'HL' ELSE 'WB' END
				ELSE RawAsXml.value('/P1[3]/P2[1]/P3[1]/@Value','varchar(3)')
		   END MsgType,
		   CASE WHEN RawAsXml.exist('/P1[3]/P2/P3[1][@Value="FHL"]') = 1
				THEN SUBSTRING(RawAsXml.value('(/P1[3]/P2[position()=(/P1[3]/P2[P3[1][@Value="FHL"]]/@Pos)[1]+1]/P3[2]/@Value)[1]','nvarchar(50)'),1,3)
				ELSE SUBSTRING(RawAsXml.value('(/P1[3]/P2[position()=(/P1[3]/P2[P3[1][@Value="FWB"]]/@Pos)[1]+1]/P3[1]/@Value)[1]','nvarchar(50)'),1,3)
		   END AirlineCodeOrPrefix,
		   Network,
		   CASE WHEN RawAsXml.exist('/P1[3]/P2/P3[1][@Value="FHL"]') = 1
				THEN SUBSTRING(RawAsXml.value('(/P1[3]/P2[position()=(/P1[3]/P2[P3[1][@Value="FHL"]]/@Pos)[1]+1]/P3[2]/@Value)[1]','nvarchar(50)'),5,8)
				ELSE SUBSTRING(RawAsXml.value('(/P1[3]/P2[position()=(/P1[3]/P2[P3[1][@Value="FWB"]]/@Pos)[1]+1]/P3[1]/@Value)[1]','nvarchar(50)'),5,8)
		   END MAWB,
		   NULL HAWB,
		   NULL StatusCode,
		   AM_ReceivedFromSenderUTC,
		   MessageTrackingID,
		   AM_ArchivedUTC
	  FROM #FromProvider 
		   OUTER APPLY (SELECT CASE WHEN MessageRaw LIKE 'UNA:+.? ''%' THEN SUBSTRING(MessageRaw,10,LEN(MessageRaw)) ELSE MessageRaw END) Msg(RawBody)
		   OUTER APPLY (SELECT CAST(RawXmlTxt AS XML) FROM 
			  (SELECT * FROM 
				  (SELECT 1,NULL,Pos,CASE WHEN CHARINDEX(@crlf,Value) = 0 THEN Value END,NULL,NULL,NULL,NULL
					 FROM dbo.SplitText(RawBody, '''') P1
					UNION ALL
				   SELECT 2,1,P1.Pos,NULL,P2.Pos,NULL,NULL,NULL
					 FROM dbo.SplitText(RawBody, '''') P1
						  OUTER APPLY dbo.SplitText(P1.Value, @crlf) P2
					WHERE CHARINDEX(@crlf,P1.Value) > 0
					UNION ALL
					SELECT 3,2,P1.Pos,NULL,P2.Pos,NULL,P3.Pos,P3.Value
					  FROM dbo.SplitText(RawBody, '''') P1
						   OUTER APPLY dbo.SplitText(P1.Value, @crlf) P2
						   OUTER APPLY dbo.SplitText(P2.Value, '/') P3
					 WHERE CHARINDEX(@crlf,P1.Value) > 0
				  ) msgs(Tag,Parent,[P1!1!Pos],[P1!1!Value],[P2!2!Pos],[P2!2!Value],[P3!3!Pos],[P3!3!Value])
				  ORDER BY 3,5,7
				  FOR XML EXPLICIT
			  ) RawMsgs(RawXmlTxt)
		   ) RawXmlMsgs(RawAsXml)
	 WHERE MessageXml IS NULL AND (MessageRaw LIKE '%CIMFMA%' OR MessageRaw LIKE '%CIMFNA%') AND Network !='BT'
)

SELECT *
FROM ToProviderData
WHERE MAWB IS NOT NULL AND MAWB <> ''
UNION ALL
SELECT *
FROM FromProviderData1
WHERE MAWB IS NOT NULL AND MAWB <> ''
UNION ALL
SELECT *
FROM FromProviderData2
WHERE MAWB IS NOT NULL AND MAWB <> ''
UNION ALL
SELECT *
FROM FromProviderData3
WHERE MAWB IS NOT NULL AND MAWB <> ''
ORDER BY ClientID, AM_ReceivedFromSenderUTC;

IF  OBJECT_ID(N'TempDB..#ToProvider') IS NOT NULL DROP TABLE #ToProvider;
IF  OBJECT_ID(N'TempDB..#FromProvider') IS NOT NULL DROP TABLE #FromProvider;
