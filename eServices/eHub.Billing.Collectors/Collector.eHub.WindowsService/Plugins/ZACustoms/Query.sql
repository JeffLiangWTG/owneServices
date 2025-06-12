DECLARE @zacustoms uniqueidentifier;
SELECT @zacustoms = CC_PK FROM dbo.eHubClient where CC_ID = 'ZACustoms';

WITH DecodedMessage AS
(
	SELECT AM_CC_SenderInbox,
		   AM_ReceivedFromSenderUTC,
		   AM_ArchivedUTC,
		   AM_InboxMessageTrackingID MessageTrackingID,
		   dbo.DecodeAndDecompress(AM_SenderMessageRaw) DecodedMessageRaw
	FROM dbo.eHubArchiveMessage m
	WHERE
		  AM_CC_RecipientInbox = @zacustoms AND
		  AM_Status < 255 AND
		  AM_ArchivedUTC > @startUTC AND AM_ArchivedUTC <= @endUTC
)
SELECT * FROM
(
	SELECT Sender.CC_ID ClientID,
		   AM_ReceivedFromSenderUTC,
		   AM_ArchivedUTC,
		   MessageTrackingID,
		   RawAsXml.value('/P1[P2[1]/P3[1]/@Value = "BGM"][1]/P2[2]/P3[4]/@Value', 'nvarchar(3)') MessageName,
		   RawAsXml.value('/P1[P2[1]/P3[1]/@Value = "RFF" and P2[2]/P3[1]/@Value = "LO"][1]/P2[2]/P3[2]/@Value', 'nvarchar(20)') JobNumber,
		   RawAsXml.value('/P1[P2[1]/P3[1]/@Value = "NAD" and P2[2]/P3[1]/@Value = "RL"][1]/P2[3]/P3[1]/@Value', 'nvarchar(20)') CarrierCode,
		   RawAsXml.value('/P1[P2[1]/P3[1]/@Value = "TDT" and P2[2]/P3[1]/@Value = "20"][1]/P2[3]/P3[1]/@Value', 'nvarchar(20)') FlightVoyage,
		   RawAsXml.value('/P1[P2[1]/P3[1]/@Value = "CNI" and P2[2]/P3[1]/@Value = "1"][1]/P2[3]/P3[1]/@Value', 'nvarchar(20)') MasterBillNumber,
		   RawAsXml.value('/P1[P2[1]/P3[1]/@Value = "CNI" and P2[2]/P3[1]/@Value = "1"][1]/P2[3]/P3[5]/@Value', 'nvarchar(8)') BillDate
	FROM DecodedMessage
	JOIN [dbo].eHubClient Sender ON Sender.CC_PK = AM_CC_SenderInbox
	OUTER APPLY (SELECT CASE WHEN DecodedMessage.DecodedMessageRaw  LIKE 'UNA:+.? ''%'
						THEN SUBSTRING(DecodedMessage.DecodedMessageRaw , 10, LEN(DecodedMessage.DecodedMessageRaw))
						ELSE DecodedMessage.DecodedMessageRaw  
						END) Msg(RawBody)
	OUTER APPLY (SELECT REPLACE(REPLACE(REPLACE(RawBody, CHAR(10), ''), CHAR(13), ''), '?''', '')) RefinedMsg(RefinedRawBody)
	OUTER APPLY (SELECT CAST(RawXmlTxt AS XML)
				 FROM (SELECT *
					   FROM (SELECT 1,NULL,Pos,Value,NULL,NULL,NULL,NULL
							 FROM dbo.SplitText(RefinedRawBody, '''') P1
							 UNION ALL

							 SELECT 2,1,P1.Pos,NULL,P2.Pos,NULL,NULL,NULL
							 FROM dbo.SplitText(RefinedRawBody, '''') P1
							 OUTER APPLY dbo.SplitText(P1.Value, '+') P2
							 WHERE CHARINDEX('+',P1.Value) > 0
							 UNION ALL

							 SELECT 3,2,P1.Pos,NULL,P2.Pos,NULL,P3.Pos,P3.Value
							 FROM dbo.SplitText(RefinedRawBody, '''') P1
							 OUTER APPLY dbo.SplitText(P1.Value, '+') P2
							 OUTER APPLY dbo.SplitText(P2.Value, ':') P3
							 WHERE CHARINDEX('+',P1.Value) > 0 OR CHARINDEX(':',P1.Value) > 0
							) msgs(Tag,Parent,[P1!1!Pos],[P1!1!Value],[P2!2!Pos],[P2!2!Value],[P3!3!Pos],[P3!3!Value])
						ORDER BY 3,5,7
						FOR XML EXPLICIT
						) RawMsgs(RawXmlTxt)
				) RawXmlMsgs(RawAsXml)
	WHERE RawAsXml.value('/P1[P2[1]/P3[1]/@Value = "BGM"][1]/P2[2]/P3[4]/@Value', 'nvarchar(3)') IN ('COH', 'HAB', 'FFM', 'ECL', 'RFM', 'BBB', 'RMA', 'COM', 'FWB', 'AQM', 'ALM', 'ALH') 
			 AND RawAsXml.value('/P1[P2[1]/P3[1]/@Value = "BGM"][1]/P2[4]/P3[1]/@Value', 'nvarchar(1)') = 9
			 AND CHARINDEX('NAD+RL', DecodedMessage.DecodedMessageRaw) > 0
			 AND (CHARINDEX(':HAB+', DecodedMessage.DecodedMessageRaw) > 0 
			  OR CHARINDEX(':COH+', DecodedMessage.DecodedMessageRaw) > 0 
			  OR CHARINDEX(':ECL+', DecodedMessage.DecodedMessageRaw) > 0 
			  OR CHARINDEX(':RFM+', DecodedMessage.DecodedMessageRaw) > 0 
			  OR CHARINDEX(':BBB+', DecodedMessage.DecodedMessageRaw) > 0 
			  OR CHARINDEX(':RMA+', DecodedMessage.DecodedMessageRaw) > 0 
			  OR CHARINDEX(':COM+', DecodedMessage.DecodedMessageRaw) > 0 
			  OR CHARINDEX(':FWB+', DecodedMessage.DecodedMessageRaw) > 0 
			  OR CHARINDEX(':FFM+', DecodedMessage.DecodedMessageRaw) > 0
			  OR CHARINDEX(':AQM+', DecodedMessage.DecodedMessageRaw) > 0
			  OR CHARINDEX(':ALM+', DecodedMessage.DecodedMessageRaw) > 0
			  OR CHARINDEX(':ALH+', DecodedMessage.DecodedMessageRaw) > 0)
) AS Result
WHERE Result.MessageName NOT IN ('COH', 'HAB', 'AQM', 'ALM', 'ALH') OR (Result.BillDate IS NOT NULL)
OPTION(RECOMPILE)
