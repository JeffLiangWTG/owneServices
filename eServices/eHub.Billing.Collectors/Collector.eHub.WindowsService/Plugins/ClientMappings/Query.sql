SET NOCOUNT ON;

CREATE TABLE #ToCalculateBillableCount
	(
		ID INT IDENTITY(1,1),
		ClientID VARCHAR(36), 
		SenderID VARCHAR(36), 
		RecipientID VARCHAR(36), 
		AM_SenderMessageXML XML,
		TS_BillingXPathSource NVARCHAR(2048),
		AM_RecipientMessageXML XML,
		TS_BillingXPathTarget NVARCHAR(2048),
		BillableCount INT,
		AM_ReceivedFromSenderUTC DATETIME,
		MessageTrackingID UNIQUEIDENTIFIER,
		[FileName] VARCHAR(50),
		TS_BillingElement NVARCHAR (35),
		TS_Name NVARCHAR (50),
		AM_ArchivedUTC DATETIME
	);

WITH CTE AS
(SELECT
	CASE
		WHEN TSSender.CC_SystemCategory = 'Enterprise' AND TSRecipient.CC_SystemCategory = 'Enterprise' THEN RecipientOutbox.CC_ID
		WHEN Sender.CC_OwnerCategory = 'Client' AND Sender.CC_SystemCategory = 'Enterprise' THEN Sender.CC_ID
		WHEN RecipientOutbox.CC_OwnerCategory = 'Client' AND RecipientOutbox.CC_SystemCategory = 'Enterprise' THEN RecipientOutbox.CC_ID
		ELSE Other.CC_ID
	END ClientID,
	Sender.CC_ID SenderID,
	RecipientOutbox.CC_ID RecipientID,
	AM_SenderMessageXML,
	TS_BillingXPathSource,
	AM_RecipientMessageXML,
	TS_BillingXPathTarget,
	CASE
		WHEN AM_SenderMessageXML IS NOT NULL AND TS_BillingXPathSource IS NOT NULL AND TS_BillingXPathSource <> '' THEN -1
		WHEN AM_RecipientMessageXML IS NOT NULL AND TS_BillingXPathTarget IS NOT NULL AND TS_BillingXPathTarget <> '' THEN -2
		WHEN AM_SenderMessageXML IS NULL AND AM_RecipientMessageXML IS NULL AND AM_DT_SenderMessageType IS NULL AND AM_DT_RecipientMessageType IS NULL THEN 1
		WHEN AM_TS IS NULL AND AM_ApplicationCode in ('XMS', 'UDM') THEN
			CASE
				WHEN AM_RecipientMessageXML.exist('data(//*:UniversalShipment)') = 1 AND cast(AM_RecipientMessageXML.query('data(//*:DataSource/*:Type)[1]') AS varchar(35)) = 'ForwardingConsol' THEN AM_RecipientMessageXML.value('count(//*:SubShipment[//*:DataSource/*:Type = "ForwardingShipment"])', 'int')
				WHEN AM_RecipientMessageXML.exist('data(//*:UniversalEvent)') = 1 THEN AM_RecipientMessageXML.value('count(//*:Event)', 'int')
				WHEN AM_RecipientMessageXML.exist('data(//*:Consols)') = 1 THEN AM_RecipientMessageXML.value('count(//*:Shipment)', 'int')
				WHEN AM_RecipientMessageXML.exist ('data(//*:CartageJobs)') = 1 THEN AM_RecipientMessageXML.value('count(//*:CartageJob)', 'int')
				WHEN AM_RecipientMessageXML.exist ('data(//*:AgencyBillsOfLading)') = 1 THEN AM_RecipientMessageXML.value('count(//*:AgencyBillOfLading)', 'int')
				ELSE 0
			END
		ELSE 0
	END BillableCount,
	AM_ReceivedFromSenderUTC,
	CASE
		WHEN TSSender.CC_SystemCategory = 'Enterprise' AND TSRecipient.CC_SystemCategory = 'Enterprise' THEN AM_OutboxMessageTrackingID
		WHEN Sender.CC_OwnerCategory = 'Client' AND Sender.CC_SystemCategory = 'Enterprise' THEN AM_InboxMessageTrackingID
		WHEN RecipientOutbox.CC_OwnerCategory = 'Client' AND RecipientOutbox.CC_SystemCategory = 'Enterprise' THEN AM_OutboxMessageTrackingID
		WHEN Sender.CC_OwnerCategory = 'Client' THEN AM_InboxMessageTrackingID
		WHEN RecipientOutbox.CC_OwnerCategory = 'Client' THEN AM_OutboxMessageTrackingID
	END MessageTrackingID,
	CASE
		WHEN RecipientOutbox.CC_OwnerCategory = 'Client' AND RecipientOutbox.CC_SystemCategory = 'Enterprise' THEN RIGHT(AM_OutboxFileNameOverride, 50)
		ELSE RIGHT(AM_InboxFileNameOverride, 50)
	END [FileName],
	TS_BillingElement,
	TS_Name,
	AM_ArchivedUTC
FROM
	eHubArchiveMessage
	LEFT JOIN [dbo].eHubTransformationSet ON AM_TS = TS_PK
	LEFT JOIN [dbo].eHubClient Other ON TS_CC_BillOther = Other.CC_PK
	LEFT JOIN [dbo].eHubClient Sender ON AM_CC_SenderInbox = Sender.CC_PK
	LEFT JOIN [dbo].eHubClient RecipientOutbox ON AM_CC_RecipientOutbox = RecipientOutbox.CC_PK
	LEFT JOIN [dbo].eHubClient RecipientInbox ON AM_CC_RecipientInbox = RecipientInbox.CC_PK
	LEFT JOIN [dbo].eHubClient TSSender ON TS_CC_Sender = TSSender.CC_PK
	LEFT JOIN [dbo].eHubClient TSRecipient ON TS_CC_Recipient = TSRecipient.CC_PK
WHERE
	@startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC AND
	AM_Status < 255 AND
	(
		(TSSender.CC_OwnerCategory = 'Client' AND TSSender.CC_SystemCategory = 'Enterprise' AND TSRecipient.CC_OwnerCategory = 'Client' AND TSRecipient.CC_SystemCategory = 'Enterprise')
		OR
		(Sender.CC_OwnerCategory = 'Client' AND Sender.CC_SystemCategory = 'Third Party')
		OR 
		(RecipientOutbox.CC_OwnerCategory = 'Client' AND RecipientOutbox.CC_SystemCategory = 'Third Party')
		OR
		(RecipientInbox.CC_OwnerCategory = 'Client' AND RecipientInbox.CC_SystemCategory = 'Third Party')
		OR
		(Sender.CC_OwnerCategory = 'Client' AND Sender.CC_SystemCategory = 'XH')
		OR 
		(RecipientOutbox.CC_OwnerCategory = 'Client' AND RecipientOutbox.CC_SystemCategory = 'XH')
		OR
		(RecipientInbox.CC_OwnerCategory = 'Client' AND RecipientInbox.CC_SystemCategory = 'XH')
	)
	AND	(Other.CC_ID IS NULL OR Other.CC_ID <> 'ESERVICE')
)
INSERT #ToCalculateBillableCount
SELECT * FROM CTE WHERE BillableCount <> 0 AND ClientID Is Not Null OPTION(RECOMPILE)

DECLARE @i INT = 1;
DECLARE @numBillable INT = (SELECT COUNT(*) FROM #ToCalculateBillableCount)

WHILE @i <= @numBillable BEGIN
	-- we need to run this query via SP_EXECUTESQL because xml.value only accepts a constant string literal. It is a compiler error to give it a concatenated string.
	DECLARE @sqlCmd NVARCHAR(MAX)
	SELECT @sqlCmd = 'SELECT @out = ' +
		CASE
			WHEN BillableCount = -1 THEN 'AM_SenderMessageXML.value(''count(' + REPLACE(TS_BillingXPathSource, '''', '"') + ')'', ''int'')'
			WHEN BillableCount = -2 THEN 'AM_RecipientMessageXML.value(''count(' + REPLACE(TS_BillingXPathTarget, '''', '"') + ')'', ''int'')'
			ELSE 'BillableCount'
		END +
		' FROM #ToCalculateBillableCount WHERE ID = @i'
		FROM #ToCalculateBillableCount WHERE ID = @i

	DECLARE @billableCountResult INT
	EXECUTE SP_EXECUTESQL @sqlCmd, N'@i INT, @out int OUTPUT', @i=@i, @out=@billableCountResult OUTPUT -- Using @i as a param allows sql server to better optimize the query

	UPDATE #ToCalculateBillableCount SET BillableCount = @billableCountResult WHERE ID = @i

   SET @i = @i + 1;
END;

SELECT ClientID,SenderID, RecipientID, BillableCount,AM_ReceivedFromSenderUTC,MessageTrackingID,[FileName],TS_BillingElement,TS_Name,AM_ArchivedUTC FROM #ToCalculateBillableCount

IF OBJECT_ID('tempdb..#ToCalculateBillableCount') IS NOT NULL DROP TABLE #ToCalculateBillableCount
