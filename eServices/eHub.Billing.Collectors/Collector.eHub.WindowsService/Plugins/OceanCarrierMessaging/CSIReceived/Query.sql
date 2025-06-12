IF OBJECT_ID('tempdb.dbo.#RemoteClientRegistration ', 'U') IS NOT NULL
	DROP TABLE  #RemoteClientRegistration 

SELECT * INTO #RemoteClientRegistration FROM OPENQUERY(
			[eHubTransactionsServer],
			'SELECT Distinct CX_CC as OutboxRecipientPK
			FROM eHubClientRegistration
				 JOIN eHubRegistrationType ON eHubRegistrationType.RT_PK = eHubClientRegistration.CX_RT
			WHERE RT_ID = ''CARGOWISE'' ')
SELECT
		'CSI' AS PriceItemCode,
		RecipientOutbox.CC_ID AS ClientID,
		AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:AgentsReference)[1]', 'varchar(50)') AS AgentsReference,
		
		CASE WHEN AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:RecipientRoleCollection/*:RecipientRole/*:Code)[1]', 'varchar(50)') = 'NVO'
		THEN AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:CoLoadBookingConfirmationReference)[1]', 'varchar(50)')
		ELSE AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:BookingConfirmationReference)[1]', 'varchar(50)')
		END 
		AS BookingNumber,
		
		CASE WHEN AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:RecipientRoleCollection/*:RecipientRole/*:Code)[1]', 'varchar(50)') = 'NVO'
		THEN AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:CoLoadMasterBillNumber)[1]', 'varchar(50)')
		ELSE AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:WayBillNumber)[1]', 'varchar(50)')
		END
		AS BillNumber,

		SenderInbox.CC_ID AS Sender,
		AM_OutboxMessageTrackingID AS MessageTrackingID,
		AM_ReceivedFromSenderUTC,
		AM_ArchivedUTC
FROM
		eHubArchiveMessage with (nolock)
		JOIN [dbo].[eHubClient] SenderInbox with (nolock) ON SenderInbox.CC_PK = AM_CC_SenderInbox
		JOIN [dbo].[eHubClient] RecipientOutbox with (nolock) ON RecipientOutbox.CC_PK = AM_CC_RecipientOutbox
		JOIN #RemoteClientRegistration ON #RemoteClientRegistration.OutboxRecipientPK =  AM_CC_RecipientOutbox
WHERE @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
	AND AM_Status < 255
	AND SenderInbox.CC_OwnerCategory = 'Client'
	AND SenderInbox.CC_SystemCategory = 'Enterprise'
	AND RecipientOutbox.CC_OwnerCategory = 'Client'
	AND RecipientOutbox.CC_SystemCategory = 'Enterprise'
    AND AM_CC_RecipientInbox = 'a8f34356-f459-4805-8026-9cb072b7e0a3'
	AND AM_RecipientMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:RecipientRoleCollection/*:RecipientRole/*:ServiceCode)[1]', 'varchar(50)') = 'SIN'

OPTION(RECOMPILE)
