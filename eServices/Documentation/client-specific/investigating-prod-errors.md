# Investigating Client Specific Production Errors

When an error occurs on the production ehub biztalk server an email is automatically sent to Kirsten.
Then she creates a WI and attaches the email and other emails for the same client.

You need to use the attached email to locate the message on ehub.
The `Error ID` in the email is supposed to be the MessageTrackingID but sometimes it doesn't work.

Try searching on eHubAdmin:
*   Try setting the sender and receiver
*   Try restricting the time range

If eHubAdmin times out, try this SQL template. Replace the values of and uncomment any filters you want to use.
```sql
select top 10 eHubTransactions.dbo.DecodeAndDecompress(AM_SenderMessageRaw) as Content, AM_PK, S.CC_ID as Sender, S.CC_FriendlyName, R.CC_ID as Recipient, I.CC_ID as OutboxSender, O.CC_ID as OutboxRecipient, R.CC_FriendlyName, AM_ReceivedFromSenderUTC as ReceivedUtcDateTime, CONVERT(datetime, SWITCHOFFSET(CONVERT(datetimeoffset, AM_ReceivedFromSenderUTC), DATENAME(TzOffset, SYSDATETIMEOFFSET()))) as ReceivedLocalDateTime, AM_SentToRecipientUTC as SentUtcDateTime, AM_Status, AM_InboxFileNameOverride, AM_OutboxFileNameOverride, AM_SenderMessageXML, AM_RecipientMessageXML, TS_Name, * FROM eHubArchiveOnline..eHubArchiveMessage with (nolock)
left join eHubTransactions..eHubClient S with (nolock) on AM_CC_SenderInbox = S.CC_PK
left join eHubTransactions..eHubClient R with (nolock) on AM_CC_RecipientInbox = R.CC_PK
left join eHubTransactions..eHubClient I with (nolock) on AM_CC_SenderOutbox = I.CC_PK
left join eHubTransactions..eHubClient O with (nolock) on AM_CC_RecipientOutbox = O.CC_PK
left join eHubTransactions..ehubtransformationSet with (nolock) on AM_TS = TS_PK
where S.CC_ID like 'UNTLAXLAX[_]ASU' and R.CC_ID like 'UNTLAXLAX'
--and AM_RecipientMessageUncompressedLength > 94
--and eHubTransactions.dbo.DecodeAndDecompress(AM_SenderMessageRaw) like '%07422487776%'
--and AM_ErrorMessage != ''
--and AM_InboxFileNameOverride like ('856_SCMBEA00086150_201809271553017030.txt')--AirlineWarehouse_C02875743.txt
--and AM_OutboxFileNameOverride like ('%7RD2316%')--AirlineWarehouse_C02875743.txt
--and AM_InboxMessageTrackingID in ('8942D753-9323-4648-A902-D542443C664D')
--and AM_OutboxMessageTrackingID is not null
--and (convert(varchar(max), AM_RecipientMessageXML) like '%>4741<%' or convert(varchar(max), AM_RecipientMessageXML) like '%>7620<%')
--and AM_ReceivedFromSenderUTC > '2018-11-28'
--and AM_ReceivedFromSenderUTC < '2018-12-01'
--group by convert(varchar, AM_ReceivedFromSenderUTC, 102)
--and (AM_SentToRecipientUTC - AM_ReadyForDeliveryUTC) > '00:01'
--and AM_TS is not null
and AM_Status = 255
order BY AM_ReceivedFromSenderUTC DESC 
```

Once the message is found, figure out if it is our fault or not.

## When it is the clients fault

1.  Add the following information to the WI description and include it in an email to Kirsten. For each affected message include:
	1.  AM_PK of the message
	2.  Explanation of what was wrong with the message
2.  Close your task and create a task for Kirsten (employee code KD) to forward your information to the client

## When it is our fault

Fix the issue and resubmit the message.