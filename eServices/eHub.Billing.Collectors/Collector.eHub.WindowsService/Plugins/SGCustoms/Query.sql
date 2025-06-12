SET NOCOUNT ON;

SELECT
	Sender.CC_ID AS ClientID
	,Shipment.Reference1
	,Shipment.Reference2
	,WayBillNumbers.WayBillNumber.value('.', 'varchar(50)') AS Reference3
	,Shipment.ActionPurpose
	,Shipment.Branch
	,Shipment.ClientStaffCode
	,AM_InboxMessageTrackingID AS MessageTrackingID
	,AM_ReceivedFromSenderUTC
	,AM_ArchivedUTC
FROM eHubArchiveMessage
INNER JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
INNER JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientInbox
CROSS APPLY (SELECT  AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSourceCollection/*:DataSource[*:Type="AsycudaManifest"]/*:Key)[1]', 'varchar(50)') AS Reference1
					,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:AddInfoCollection/*:AddInfo[*:Key="ManifestNumber"]/*:Value)[1]', 'varchar(50)') AS Reference2
					,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:ActionPurpose/*:Code)[1]', 'nvarchar(3)') AS ActionPurpose
					,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:Branch/*:Code)[1]', 'nvarchar(3)') AS Branch
					,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:CustomsBroker/*:Code)[1]', 'nvarchar(3)') AS ClientStaffCode) Shipment
CROSS APPLY AM_SenderMessageXML.nodes('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:SubShipmentCollection)[1]/*:SubShipment/*:WayBillNumber') WayBillNumbers(WayBillNumber)
WHERE @startUTC < AM_ArchivedUTC
	AND AM_ArchivedUTC <= @endUTC
	AND Recipient.CC_ID = 'SGCustoms'
	AND AM_Status < 255
	AND AM_SenderMessageXML.exist('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext)[1]
		[(*:RecipientRoleCollection/*:RecipientRole/*:Code)[1] = "SGA" and ((*:ActionPurpose/*:Code)[1] = "PCM" or (*:ActionPurpose/*:Code)[1] = "AED")]') = 1
OPTION(RECOMPILE)
