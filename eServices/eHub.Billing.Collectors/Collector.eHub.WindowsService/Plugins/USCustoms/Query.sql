SELECT
	CC_ID,
	AM_ApplicationCode,
	AM_RecipientMessageRaw,
	AM_OutboxMessageTrackingID AS MessageTrackingID,
	AM_ReceivedFromSenderUTC,
	AM_ArchivedUTC
FROM
	dbo.eHubArchiveMessage
	LEFT JOIN dbo.eHubClient ON AM_CC_RecipientOutbox = CC_PK
	LEFT JOIN [eHubTransactionsServer].[eHubTransactions].dbo.eHubUSCustomsRegistry ON (ER_CC_Client = AM_CC_RecipientOutbox AND AM_ApplicationCode = ER_ApplicationCode)
WHERE
	AM_CC_SenderInbox = '146e2f62-f80e-49bb-b8fa-c46fd057af4b'
	AND CC_ID <> 'TESTSENDER__1'
	AND @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
	AND AM_Status < 255
	AND ER_IsProduction = 1
	AND ER_ApplicationCode <> 'AMA'
OPTION(RECOMPILE)
