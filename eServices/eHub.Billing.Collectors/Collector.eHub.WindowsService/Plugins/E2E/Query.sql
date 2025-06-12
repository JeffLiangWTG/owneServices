with UniversalInterchange as
(select
	AM_PK
	,Sender.CC_ID AS SenderID
	,Recipient.CC_ID AS RecipientID
	,AM_SentToRecipientUTC
	,AM_InboxMessageTrackingID MessageTrackingID
	,AM_SenderMessageXML
	,AM_ArchivedUTC,
	UniType = CASE AM_SenderMessageXML.exist('(/*[local-name()="UniversalInterchange"]/*[local-name()="Body"]/*[local-name()="UniversalShipment"])')
		WHEN 1 THEN 'SHIPMENT'
		ELSE 'TRANSACTION'
	END
from [dbo].eHubArchiveMessage
join [dbo].eHubClient Sender on AM_CC_SenderInbox = Sender.CC_PK
join [dbo].eHubClient Recipient on AM_CC_RecipientOutbox = Recipient.CC_PK
WHERE @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
	and	AM_Status < 255
	and	Sender.CC_OwnerCategory = 'Client'
	and	Sender.CC_SystemCategory = 'Enterprise'
	and	Recipient.CC_OwnerCategory = 'Client'
	and	Recipient.CC_SystemCategory = 'Enterprise'
	and AM_ApplicationCode = 'UDM'
	and (AM_SenderMessageXML.exist('(/*[local-name()="UniversalInterchange"]/*[local-name()="Body"]/*[local-name()="UniversalShipment"])') = 1 OR AM_SenderMessageXML.exist('(/*[local-name()="UniversalInterchange"]/*[local-name()="Body"]/*[local-name()="UniversalTransaction"])') = 1)
	and AM_CC_SenderInbox <> AM_CC_RecipientOutbox)
,UniversalTransaction as
(select
	AM_PK
	,AM_SenderMessageXML.value('(/*[local-name()="UniversalInterchange"]/*[local-name()="Body"]/*[local-name()="UniversalTransaction"]/*[local-name()="TransactionInfo"]/*[local-name()="DataContext"]/*[local-name()="DataSourceCollection"]/*[local-name()="DataSource"]/*[local-name()="Type"])[1]','varchar(35)')
	 + AM_SenderMessageXML.value('(/*[local-name()="UniversalInterchange"]/*[local-name()="Body"]/*[local-name()="UniversalTransaction"]/*[local-name()="TransactionInfo"]/*[local-name()="DataContext"]/*[local-name()="DataSourceCollection"]/*[local-name()="DataSource"]/*[local-name()="Key"])[1]','varchar(35)')
	  as TypeKeysCollection
from UniversalInterchange
WHERE UniversalInterchange.UniType = 'TRANSACTION')
,UniversalShipments as
(select
	AM_PK
	,SenderID
	,RecipientID
	,AM_SentToRecipientUTC
	,MessageTrackingID
	,AM_SenderMessageXML
	,AM_ArchivedUTC
from UniversalInterchange
WHERE UniversalInterchange.UniType = 'SHIPMENT')
,Level1SubShipments as
(select
	AM_PK
	,AM_SenderMessageXML.value('(/*[local-name()="UniversalInterchange"]/*[local-name()="Body"]/*[local-name()="UniversalShipment"]/*[local-name()="Shipment"]/*[local-name()="DataContext"]/*[local-name()="DataSourceCollection"]/*[local-name()="DataSource"]/*[local-name()="Type"])[1]','varchar(35)')
	 + AM_SenderMessageXML.value('(/*[local-name()="UniversalInterchange"]/*[local-name()="Body"]/*[local-name()="UniversalShipment"]/*[local-name()="Shipment"]/*[local-name()="DataContext"]/*[local-name()="DataSourceCollection"]/*[local-name()="DataSource"]/*[local-name()="Key"])[1]','varchar(35)')
	  as TopLevelTypeKey
	,Level1SubShipment.SubShipment.value('(./*[local-name()="DataContext"]/*[local-name()="DataSourceCollection"]/*[local-name()="DataSource"]/*[local-name()="Type"])[1]','varchar(35)')
	 + Level1SubShipment.SubShipment.value('(./*[local-name()="DataContext"]/*[local-name()="DataSourceCollection"]/*[local-name()="DataSource"]/*[local-name()="Key"])[1]','varchar(35)')
	  as Level1SubShipmentTypeKey
	,Level1SubShipment.SubShipment.query('.') as Level1SubShipment
from UniversalInterchange
outer apply AM_SenderMessageXML.nodes('/*[local-name()="UniversalInterchange"]/*[local-name()="Body"]/*[local-name()="UniversalShipment"]/*[local-name()="Shipment"]/*[local-name()="SubShipmentCollection"]/*[local-name()="SubShipment"]') as Level1SubShipment(SubShipment)
WHERE UniversalInterchange.UniType = 'SHIPMENT')
,Level2SubShipments as
(select 
	AM_PK
	,TopLevelTypeKey
	,Level1SubShipmentTypeKey
	,Level2SubShipment.SubShipment.value('(./*[local-name()="DataContext"]/*[local-name()="DataSourceCollection"]/*[local-name()="DataSource"]/*[local-name()="Type"])[1]','varchar(35)')
	 + Level2SubShipment.SubShipment.value('(./*[local-name()="DataContext"]/*[local-name()="DataSourceCollection"]/*[local-name()="DataSource"]/*[local-name()="Key"])[1]','varchar(35)')
	  as Level2SubShipmentTypeKey
	,Level2SubShipment.SubShipment.query('.') as Level2SubShipment
	from Level1SubShipments
	outer apply Level1SubShipment.nodes('/*[local-name()="SubShipment"]/*[local-name()="SubShipmentCollection"]/*[local-name()="SubShipment"]') as Level2SubShipment(SubShipment))
,Level3SubShipments as
(select
	AM_PK
	,TopLevelTypeKey
	,Level1SubShipmentTypeKey
	,Level2SubShipmentTypeKey
	,Level3SubShipment.SubShipment.value('(./*[local-name()="DataContext"]/*[local-name()="DataSourceCollection"]/*[local-name()="DataSource"]/*[local-name()="Type"])[1]','varchar(35)')
	 + Level3SubShipment.SubShipment.value('(./*[local-name()="DataContext"]/*[local-name()="DataSourceCollection"]/*[local-name()="DataSource"]/*[local-name()="Key"])[1]','varchar(35)')
	  as Level3SubShipmentTypeKey
	from Level2SubShipments
	outer apply Level2SubShipment.nodes('/*[local-name()="SubShipment"]/*[local-name()="SubShipmentCollection"]/*[local-name()="SubShipment"]') as Level3SubShipment(SubShipment))
,AllSubShipments AS
(
    SELECT
        AM_PK,
        STRING_AGG(
            CAST((COALESCE(TopLevelTypeKey, '') + ':' + COALESCE(Level1SubShipmentTypeKey, '') + ':' + COALESCE(Level2SubShipmentTypeKey, '') + ':' + COALESCE(Level3SubShipmentTypeKey, '')) as VARCHAR(MAX)),
            ';'
        ) AS TypeKeysCollection
    FROM Level3SubShipments
    GROUP BY AM_PK
)
select 	
	SenderID
	,RecipientID
	,AM_SentToRecipientUTC
	,MessageTrackingID
	,TypeKeysCollection
	,AM_ArchivedUTC
	,UniType
from AllSubShipments
join UniversalInterchange on UniversalInterchange.AM_PK = AllSubShipments.AM_PK
UNION ALL
select 	
	SenderID
	,RecipientID
	,AM_SentToRecipientUTC
	,MessageTrackingID
	,TypeKeysCollection
	,AM_ArchivedUTC
	,UniType
from UniversalTransaction
join UniversalInterchange on UniversalInterchange.AM_PK = UniversalTransaction.AM_PK

OPTION(RECOMPILE)
