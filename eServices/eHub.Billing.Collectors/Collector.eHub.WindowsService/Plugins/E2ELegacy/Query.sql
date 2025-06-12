SELECT
	Sender.CC_ID as SenderID, 
	Receiver.CC_ID as ReceiverID, 
	AM_InboxMessageTrackingID as MessageTrackingID, 
	AM_SentToRecipientUTC, 
	AM_ArchivedUTC,
	TL.HighLevelEntity.value('fn:concat(fn:local-name(.), (./*/*[local-name()="AgentReference"])[1], (./*[local-name()="JobNumber"])[1], (./*[local-name()="ReferenceKeys"]/*[local-name()="ReferenceKey" and @ReferenceKeyName="UniqueIdentifier"]/@ReferenceKeyType)[1], 
		(./*[local-name()="OrderIdentifier"]/*[local-name()="OrderNumber"])[1], fn:substring("~", 1, count(*[local-name(..) = "Order"])), (./*[local-name()="OrderIdentifier"]/*[local-name()="OrderNumberSplit"])[1], fn:substring("~", 1, count(*[local-name(..) = "Order"])), (./*[local-name()="OrderDetail"]/*[local-name()="Buyer"]/@EDICode)[1], 
		(./*[local-name()="Ledger"])[1], fn:substring("~", 1, count(*[local-name(..) = "TxnHeader"])), (./*[local-name()="DebtorOrCreditor"]/@EDICode)[1], fn:substring("~", 1, count(*[local-name(..) = "TxnHeader"])), fn:substring(./*[local-name()="PostDate"][1], 1, 10), fn:substring("~", 1, count(*[local-name(..) = "TxnHeader"])), (./*[local-name()="OsInvoiceAmtInclTax"])[1] )', 'varchar(100)') as JobNumber
from eHubArchiveMessage
	cross apply eHubArchiveMessage.AM_SenderMessageXML.nodes('/*[local-name()="XmlInterchange"]/*[local-name()="Payload"]/*[local-name()!="NettingClearingJournals"]/*, 
		/*[local-name()="XmlInterchange"]/*[local-name()="Payload"]/*/*//*[local-name()="Shipment"], 
		/*[local-name()="XmlInterchange"]/*[local-name()="Payload"]/*[local-name()="NettingClearingJournals"]/*/*') as TL(HighLevelEntity)
	inner join eHubClient Sender on Sender.CC_PK = AM_CC_SenderInbox
	inner join eHubClient Receiver on Receiver.CC_PK = AM_CC_RecipientOutbox
where @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
	and AM_SenderMessageXML.exist('/*[local-name()="XmlInterchange"]/*[local-name()="Payload"]/*/*') = 1
	and AM_Status < 255
	and	Sender.CC_OwnerCategory = 'Client'
	and	Sender.CC_SystemCategory = 'Enterprise'
	and	Receiver.CC_OwnerCategory = 'Client'
	and	Receiver.CC_SystemCategory = 'Enterprise'
	and AM_ApplicationCode = 'XMS'
