CREATE TABLE SubscriptionNotificationQueue (
	[SNQ_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_SubscriptionNotificationQueue_SNQ_PK] DEFAULT (NEWID()),
	[SNQ_ClientId] VARCHAR(9) NOT NULL,
	[SNQ_Status] VARCHAR(3) NOT NULL CONSTRAINT [DF_SubscriptionNotificationQueue_SNQ_Status] DEFAULT ('QUE'),
	[SNQ_Message] XML NOT NULL, 
	CONSTRAINT PK_SubscriptionNotificationQueue PRIMARY KEY (SNQ_PK),
	CONSTRAINT CK_SubscriptionNotificationQueue_SNQ_ClientId CHECK ([SNQ_ClientId] <> ''),
	CONSTRAINT CK_SubscriptionNotificationQueue_SNQ_Status CHECK ([SNQ_Status] <> '')
)
GO
