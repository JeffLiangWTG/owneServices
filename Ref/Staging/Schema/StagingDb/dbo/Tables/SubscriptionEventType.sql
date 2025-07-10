CREATE TABLE SubscriptionEventType (
	[SST_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_SubscriptionEventType_SST_PK] DEFAULT (NEWID()),
	[SST_EventType] VARCHAR(5) NOT NULL,

	CONSTRAINT PK_SubscriptionEventType PRIMARY KEY (SST_PK),
	CONSTRAINT CK_SubscriptionEventType_SST_EventType CHECK ([SST_EventType] <> ''),
)
GO
