with cte as
	(select 
		SenderInbox.CC_ID as ClientID,
		AM_ReceivedFromSenderUTC,
		AM_InboxMessageTrackingID as MessageTrackingID, 
		left(RecipientOutbox.CC_FriendlyName, 50) as MessageRecipient,
		AM_SenderMessageXML.value('(/*[local-name()="UniversalInterchange"]/*[local-name()="Body"]/*[local-name()="UniversalShipment"]/*[local-name()="Shipment"]/*[local-name()="DataContext"]/*[local-name()="Workflow"]/*[local-name()="RecipientRoleCollection"]/*[local-name()="RecipientRole"])[1]','varchar(3)') as RecipientRoleCode,
		AM_SenderMessageXML.value('(/*[local-name()="UniversalInterchange"]/*[local-name()="Body"]/*[local-name()="UniversalShipment"]/*[local-name()="Shipment"]/*[local-name()="DataContext"]/*[local-name()="DataSource"]/*[local-name()="Key"])[1]','varchar(35)') as DataSourceKey,
		ContainerNodes.ContainerNode.value('*[local-name()="ReleaseNum"][1]', 'varchar(20)') as ContainerReleaseNumber,
		ContainerNodes.ContainerNode.value('*[local-name()="ContainerCount"][1]', 'varchar(10)') as ContainerCount,
		ContainerNodes.ContainerNode.value('*[local-name()="ContainerNumber"][1]', 'varchar(20)') as ContainerNumber,
		AM_ArchivedUTC
	from dbo.eHubArchiveMessage
	join dbo.eHubClient as SenderInbox on SenderInbox.CC_PK = AM_CC_SenderInbox
	join dbo.eHubClient as RecipientInbox on RecipientInbox.CC_PK = AM_CC_RecipientInbox
	join dbo.eHubClient as RecipientOutbox on RecipientOutbox.CC_PK = AM_CC_RecipientOutbox
	outer apply AM_SenderMessageXML.nodes('/*[local-name()="UniversalInterchange"]/*[local-name()="Body"]/*[local-name()="UniversalShipment"]/*[local-name()="Shipment"]/*[local-name()="ContainerCollection"]/*[local-name()="Container"]') as ContainerNodes(ContainerNode)
	where RecipientInbox.CC_ID = 'ShippingPortMessaging'
		and @startUTC < AM_ArchivedUTC and AM_ArchivedUTC <= @endUTC
		and	AM_Status < 255) 
select distinct 
	ClientID,
	AM_ReceivedFromSenderUTC,
	MessageTrackingID,
	MessageRecipient,
	RecipientRoleCode,
	DataSourceKey,
	case when RecipientRoleCode in ('PEM', 'PIM') then null else ContainerReleaseNumber end as ContainerReleaseNumber, --To prepare for removing duplicates for 'PEM' and 'PIM'. They(COPRAR) are billed on shipment with no need of container info.
	case when RecipientRoleCode in ('PEM', 'PIM') then null else ContainerCount end as ContainerCount,
	case when RecipientRoleCode in ('PEM', 'PIM') then null else ContainerNumber end as ContainerNumber,
	AM_ArchivedUTC
from cte
where (RecipientRoleCode = 'PER' and ContainerReleaseNumber is not NULL and ContainerReleaseNumber != '')
	  or RecipientRoleCode in ('PIR', 'PEM', 'PIM')
OPTION(RECOMPILE)

