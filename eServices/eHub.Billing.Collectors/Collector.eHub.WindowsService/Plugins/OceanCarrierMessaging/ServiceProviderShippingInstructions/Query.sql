SET NOCOUNT ON

SELECT 
		'SHN' AS PriceItemCode,
		SenderInbox.CC_ID AS ClientID,
		RecipientOutbox.CC_ID AS ServiceProvider,
		AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="ForwardingConsol"]/*:Key)[1]','varchar(50)') AS Consol,
		AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:CoLoadBookingConfirmationReference)[1]','varchar(50)') AS CoLoadBookingConfirmationReference,
		AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:BookingConfirmationReference)[1]','varchar(50)') AS BookingConfirmationReference,
		AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="ShippingLineAddress"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type="CCC"][*:CountryOfIssue="US"]/*:Value)[1]','varchar(50)') AS CarrierSCAC,
		AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="CoLoadWith"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type="CCC"][*:CountryOfIssue="US"]/*:Value)[1]','varchar(50)') AS NVOCCSCAC,
		AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="CoLoadWith"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type="C1C"]/*:Value)[1]','varchar(50)') AS NVOCCCW1,
		AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:OrganizationAddressCollection/*:OrganizationAddress[*:AddressType="ShippingLineAddress"]/*:RegistrationNumberCollection/*:RegistrationNumber[*:Type="C1C"]/*:Value)[1]','varchar(50)') AS CarrierCW1,
		AM_InboxMessageTrackingID AS MessageTrackingID,
		AM_ReceivedFromSenderUTC,
		AM_ArchivedUTC
	FROM
		eHubArchiveMessage
		JOIN [dbo].[eHubClient] SenderInbox ON SenderInbox.CC_PK = AM_CC_SenderInbox
		JOIN [dbo].[eHubClient] RecipientOutbox ON RecipientOutbox.CC_PK = AM_CC_RecipientOutbox
	WHERE @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
	AND AM_Status < 255
	AND AM_CC_RecipientInbox = 'a8f34356-f459-4805-8026-9cb072b7e0a3'
	AND SenderInbox.CC_ID != RecipientOutbox.CC_ID
	AND AM_SenderMessageXML.exist('/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext[*:DataSource/*:Type[.="ForwardingConsol"] and *:DocumentaryOverride/*:DocumentName[.="Shipping Instruction"]]') = 1

OPTION(RECOMPILE)