DECLARE @nzcustoms uniqueidentifier;
SELECT @nzcustoms = CC_PK FROM dbo.eHubClient where CC_ID = 'NZCustoms';

SELECT
	CASE WHEN AM_CC_SenderInbox = @nzcustoms THEN R.CC_ID ELSE S.CC_ID END AS ClientID,
	AM_ApplicationCode,
	AM_SenderMessageRaw,
	CASE WHEN AM_CC_SenderInbox = @nzcustoms THEN AM_OutboxMessageTrackingID ELSE AM_InboxMessageTrackingID END AS MessageTrackingID,
	AM_ReceivedFromSenderUTC,
	AM_ArchivedUTC
FROM
	dbo.eHubArchiveMessage
	LEFT JOIN dbo.eHubClient S ON AM_CC_SenderInbox = S.CC_PK
	LEFT JOIN dbo.eHubClient R ON AM_CC_RecipientOutbox = R.CC_PK
WHERE
	AM_ApplicationCode IN ('NZM', 'NZC')
	AND AM_Status = 3
	AND (AM_CC_SenderInbox = @nzcustoms OR AM_CC_RecipientInbox = @nzcustoms)
	AND @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
OPTION(RECOMPILE)
