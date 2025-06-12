SET NOCOUNT ON;

IF OBJECT_ID(N'TempDB..#AllData') IS NOT NULL DROP TABLE #AllData;

WITH XMLNAMESPACES ('http://www.cargowise.com/Schemas/Universal/2011/11' AS ns)
SELECT 
	Sender.CC_ID AS ClientID,
	AM_SenderMessageXML AS SenderXML,
	RecipientOutbox.CC_ID AS Provider,
	AM_SenderMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'varchar(50)') AS DocumentName,
	AM_InboxMessageTrackingID AS MessageTrackingID,
	AM_ReceivedFromSenderUTC,
	AM_ArchivedUTC,
	AM_SenderMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource/*:Type)[1]', 'varchar(50)') as DataSource,
	AM_SenderMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="ForwardingConsol"]/*:Key)[1]','varchar(50)') as DataSourceKeyConsol
INTO
	#AllData
FROM
	eHubArchiveMessage
	LEFT JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
	LEFT JOIN [dbo].[eHubClient] RecipientOutbox ON RecipientOutbox.CC_PK = AM_CC_RecipientOutbox
WHERE @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
AND AM_Status < 255
AND AM_CC_RecipientInbox = 'a8f34356-f459-4805-8026-9cb072b7e0a3'
AND AM_SenderMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource/*:Type)[1]', 'varchar(50)') IN ('ForwardingConsol', 'ForwardingShipment')
AND AM_SenderMessageXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'varchar(50)') IN ('Booking Request', 'Shipping Instruction', 'Shipping Order', 'Verified Gross Container Weight', 'eManifest')
AND Sender.CC_ID != RecipientOutbox.CC_ID
OPTION(RECOMPILE);

WITH XMLNAMESPACES ('http://www.cargowise.com/Schemas/Universal/2011/11' AS ns)
SELECT
	ClientID,
	SenderXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="ForwardingConsol"]/*:Key)[1]','varchar(50)') AS Ref3,
	CASE WHEN SenderXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:ShipmentType/*:Code)[1]', 'varchar(50)') IN ('GCL', 'CLD')
		THEN SenderXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="CoLoadWith"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type="CCC"][*:CountryOfIssue="US"]/*:Value)[1]','varchar(50)')
		ELSE SenderXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="ShippingLineAddress"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type="CCC"][*:CountryOfIssue="US"]/*:Value)[1]','varchar(50)')
	END AS Ref4,
	ContainerNodes.ContainerNode.value('(*:ContainerNumber)[1]','varchar(50)') AS Ref5,
	Provider,
	DocumentName,
	MessageTrackingID,
	AM_ReceivedFromSenderUTC,
	AM_ArchivedUTC
FROM
	#AllData
	CROSS APPLY SenderXML.nodes('/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:ContainerCollection/*:Container') AS ContainerNodes(ContainerNode)
WHERE
	DocumentName = 'Verified Gross Container Weight'
AND	DataSource = 'ForwardingConsol'

UNION ALL

SELECT
	ClientID,
	SenderXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="ForwardingShipment"]/*:Key)[1]','varchar(50)') AS Ref3,
	COALESCE(
		SenderXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="CarrierHandlingAgent"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type="C1C"]/*:Value)[1]','varchar(50)'),
		SenderXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="ShippingLineAddress"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type="CCC"][*:CountryOfIssue="US"]/*:Value)[1]','varchar(50)'))
		AS Ref5,
	ShipmentNodes.ShipmentNode.value('(*:DataContext/*:DataSource[*:Type="Booking"]/*:Key)[1]','varchar(50)') AS Ref5,
	Provider,
	DocumentName,
	MessageTrackingID,
	AM_ReceivedFromSenderUTC,
	AM_ArchivedUTC
FROM
	#AllData
	CROSS APPLY SenderXML.nodes('/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:SubShipmentCollection/*:SubShipment') AS ShipmentNodes(ShipmentNode)
WHERE
	DocumentName = 'eManifest'
AND	DataSource = 'ForwardingShipment'

UNION ALL

SELECT
	ClientID,
	SenderXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="ForwardingConsol"]/*:Key)[1]','varchar(50)') AS Ref3,
	CASE WHEN SenderXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:ShipmentType/*:Code)[1]', 'varchar(50)') IN ('GCL', 'CLD') OR SenderXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:ShipmentType)[1]', 'varchar(50)') IN ('GCL', 'CLD')
		THEN
		COALESCE(SenderXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="CoLoadWith"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type="CCC"][*:CountryOfIssue="US"]/*:Value)[1]','varchar(50)'),
		SenderXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="CoLoadWith"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type="C1C"]/*:Value)[1]','varchar(50)'))
		ELSE
		COALESCE(SenderXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="ShippingLineAddress"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type="CCC"][*:CountryOfIssue="US"]/*:Value)[1]','varchar(50)'),
		SenderXML.value('(/ns:UniversalInterchange/ns:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="ShippingLineAddress"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type="C1C"]/*:Value)[1]','varchar(50)'))
	END AS Ref4,
	NULL AS Ref5,
	Provider,
	DocumentName,
	MessageTrackingID,
	AM_ReceivedFromSenderUTC,
	AM_ArchivedUTC
FROM
	#AllData
WHERE
	DocumentName NOT IN ('Verified Gross Container Weight', 'eManifest')
AND	DataSource = 'ForwardingConsol'
;

DROP TABLE #AllData;
