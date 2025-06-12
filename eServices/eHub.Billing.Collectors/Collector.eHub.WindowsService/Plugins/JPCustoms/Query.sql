IF OBJECT_ID('tempDB..#clientPivot','U') IS NOT NULL
DROP TABLE #clientPivot
IF OBJECT_ID('tempDB..#rcvTable','U') IS NOT NULL
DROP TABLE #rcvTable
IF OBJECT_ID('tempDB..#sndTable','U') IS NOT NULL
DROP TABLE #sndTable

-- clientPivot
;WITH registry AS
(
	SELECT
		RANK() OVER ( PARTITION BY STUFF(CC_ID, 4, 3, '___') ORDER BY CR_MessageReference DESC) AS fallBackRank,
		CC_ID AS CC_ID,
		CR_MessageReference AS reporterID
	FROM
		[eHubTransactionsServer].[eHubTransactions].dbo.eHubMessageReferenceRegistry
		LEFT JOIN eHubClient ON CR_CC_Client = CC_PK
	WHERE
		CR_ApplicationCode = 'JPC'
),
directMatch AS (
	SELECT
		c.CC_ID AS sender,
		r.CC_ID AS recipient
	FROM eHubClient c
	INNER JOIN registry r ON r.CC_ID = c.CC_ID
),
indirectMatch AS (
	SELECT
		c.CC_ID AS sender,
		r.CC_ID AS recipient 
	FROM eHubClient c
	INNER JOIN registry r ON c.CC_ID LIKE STUFF(r.CC_ID, 4, 3, '___') AND r.fallBackRank = 1
	LEFT JOIN directMatch d ON d.sender = c.CC_ID
	WHERE
		d.sender IS NULL
)

SELECT 
	* 
INTO #clientPivot
FROM (
	SELECT * FROM directMatch
	UNION 
	SELECT * FROM indirectMatch
) clientPivot

-- rcvTable
SELECT
	AM_PK,
	CC_ID_RecipientOutbox AS recipient,
	COALESCE(AM_RecipientMessageXML.value('(//*[local-name() = ''Context''][*[local-name()=''Type'']=''InternalTransactionNumber'']/*[local-name()=''Value''])[1]','NVARCHAR(MAX)'), '')
		AS InternalTransactionNumber,
	COALESCE(AM_RecipientMessageXML.value('(//*[local-name() = ''EventReference''])[1]','NVARCHAR(MAX)'), '')
		AS MessageType,
	COALESCE(AM_RecipientMessageXML.value('(//*[local-name() = ''Context''][*[local-name()=''Type'']=''MBOLNumber'']/*[local-name()=''Value''])[1]','NVARCHAR(MAX)'), '')
		AS MBOL,
	COALESCE(AM_RecipientMessageXML.value('(//*[local-name() = ''Context''][*[local-name()=''Type'']=''HBOLNumber'']/*[local-name()=''Value''])[1]','NVARCHAR(MAX)'), '')
		AS HBOL,
	AM_ArchivedUTC
INTO
	#rcvTable
FROM
	eHubArchiveMessageExp
WHERE
	CC_ID_SenderInbox = 'JPCustoms'
	AND CC_ID_RecipientOutbox != 'TESTSENDER__JPC'
	AND @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
	AND AM_Status  = 3
OPTION(RECOMPILE)

-- sndTable
SELECT
	AM_PK,
	CC_ID_SenderInbox AS sender,
	AM_ReceivedFromSenderUTC,
	AM_InboxMessageTrackingID MessageTrackingID,
	COALESCE(AM_RecipientMessageXML.value('(//*[local-name() = ''InputMessageID''])[1]','NVARCHAR(MAX)'), '')
		AS InternalTransactionNumber,
	COALESCE(AM_RecipientMessageXML.value('(//*[local-name() = ''ProcedureCode''])[1]','NVARCHAR(MAX)'), '') +
	COALESCE(AM_RecipientMessageXML.value('(//*[local-name() = ''FunctionTypeCode''])[1]','NVARCHAR(MAX)'), '')
		AS MessageType,
	COALESCE(AM_RecipientMessageXML.value('(//*[local-name() = ''MasterBillOfLadingNumber''])[1]','NVARCHAR(MAX)'), '')
		AS MBOL,
	COALESCE(AM_RecipientMessageXML.value('(//*[local-name() = ''HouseBillOfLadingNumber''])[1]','NVARCHAR(MAX)'), '')
		AS HBOL,
	AM_ArchivedUTC
INTO
	#sndTable
FROM
	eHubArchiveMessageExp
WHERE
	CC_ID_RecipientOutbox = 'JPCustoms'
	AND CC_ID_SenderInbox != 'TESTSENDER__JPC'
	AND AM_ApplicationCode = 'SUB'
	AND @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
	AND AM_Status  = 3
OPTION(RECOMPILE)

SELECT
	s.sender AS ClientID,
	s.MBOL AS MBOL,
	s.HBOL AS HBOL,
	s.MessageType AS MessageType,
	s.MessageTrackingID AS MessageTrackingID,
	s.AM_ReceivedFromSenderUTC AS AM_ReceivedFromSenderUTC,
	s.AM_ArchivedUTC
FROM
	#sndTable AS s
	JOIN #clientPivot AS c ON s.sender = c.sender
	JOIN #rcvTable AS r ON r.recipient = c.recipient 
WHERE
	s.InternalTransactionNumber = r.InternalTransactionNumber
	AND s.MBOL = r.MBOL
	AND s.HBOL = r.HBOL
	AND s.HBOL != ''
	AND s.MessageType IN ('AHR9', 'CHR2')
	AND substring(r.MessageType,5,8) = 'ACCEPTED'
