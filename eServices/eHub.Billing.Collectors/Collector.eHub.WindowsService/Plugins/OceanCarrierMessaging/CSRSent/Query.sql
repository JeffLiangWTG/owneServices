SET NOCOUNT ON;

IF OBJECT_ID('tempdb.dbo.#RemoteClientRegistration ', 'U') IS NOT NULL
	DROP TABLE  #RemoteClientRegistration 

SELECT * INTO #RemoteClientRegistration FROM OPENQUERY(
			[eHubTransactionsServer],
			'SELECT Distinct CX_CC as SenderPK
			FROM eHubClientRegistration
				 JOIN eHubRegistrationType ON eHubRegistrationType.RT_PK = eHubClientRegistration.CX_RT
			WHERE RT_ID = ''CARGOWISE'' ');

WITH XMLNAMESPACES ('http://www.cargowise.com/Schemas/Universal/2011/11' AS ns, 'http://www.cargowise.com/Schemas/Universal/2012/11' as ns2)
SELECT 
		'CSR' AS PriceItemCode,
		SenderInbox.CC_ID AS ClientID,
		AM_SenderMessageXML.value('(/ns:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:Purpose)[1]', 'varchar(50)') AS Purpose,
		
		CASE WHEN AM_SenderMessageXML.value('(/ns:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:CoLoadBookingConfirmationReference)[1]', 'varchar(50)') != ''
		THEN AM_SenderMessageXML.value('(/ns:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:CoLoadBookingConfirmationReference)[1]', 'varchar(50)')
		ELSE AM_SenderMessageXML.value('(/ns:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:BookingConfirmationReference)[1]', 'varchar(50)') END 
		AS BookingNumber,

		CASE WHEN AM_SenderMessageXML.value('(/ns:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:CoLoadMasterBillNumber)[1]', 'varchar(50)') != ''
		THEN AM_SenderMessageXML.value('(/ns:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:CoLoadMasterBillNumber)[1]', 'varchar(50)')
		ELSE AM_SenderMessageXML.value('(/ns:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:WayBillNumber)[1]', 'varchar(50)') END 
		AS BillNumber,
		
		AM_SenderMessageXML.value('(/ns:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'varchar(50)') AS DocumentName,
		SenderInbox.CC_ID AS Sender,
		AM_InboxMessageTrackingID AS MessageTrackingID,
		AM_ReceivedFromSenderUTC,
		AM_ArchivedUTC
	FROM
		eHubArchiveMessage with (nolock)
		JOIN [dbo].[eHubClient] SenderInbox with (nolock) ON SenderInbox.CC_PK = AM_CC_SenderInbox
		JOIN [dbo].[eHubClient] RecipientOutbox with (nolock) ON RecipientOutbox.CC_PK = AM_CC_RecipientOutbox
		JOIN #RemoteClientRegistration ON #RemoteClientRegistration.SenderPK =  AM_CC_SenderInbox
	WHERE @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
	AND AM_Status < 255
	AND SenderInbox.CC_OwnerCategory = 'Client'
	AND SenderInbox.CC_SystemCategory = 'Enterprise'
	AND RecipientOutbox.CC_OwnerCategory = 'Client'
	AND RecipientOutbox.CC_SystemCategory = 'Enterprise'
	AND AM_CC_RecipientInbox = 'a8f34356-f459-4805-8026-9cb072b7e0a3'
	AND AM_SenderMessageXML.value('(/ns:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'varchar(50)') IN ('Booking Confirmation','BL Data', 'Electronic Bill of Lading')
	AND AM_RecipientMessageXML.value('(/ns:UniversalInterchange/ns:Body/ns2:UniversalShipment)[1]', 'varchar(255)') is not null
OPTION(RECOMPILE)
