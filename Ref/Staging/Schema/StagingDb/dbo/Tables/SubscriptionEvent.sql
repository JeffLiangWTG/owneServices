CREATE TABLE SubscriptionEvent (
	[SSV_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_SubscriptionEvent_SSV_PK] DEFAULT (NEWID()),
	[SSV_SST_NKEventType] VARCHAR(5) NOT NULL,
	[SSV_DataSetCode] VARCHAR(3) NOT NULL,
	[SSV_Country] VARCHAR(2) NOT NULL,
	[SSV_ClientGroupCountry] VARCHAR(3) NOT NULL,
	[SSV_IsActive] BIT NOT NULL,
	[SSV_LastUpdatedTime] DATETIME2(7) NULL,

	CONSTRAINT PK_SubscriptionEvent PRIMARY KEY (SSV_PK),
	CONSTRAINT CK_SubscriptionEvent_SSV_SST_NKEventType CHECK ([SSV_SST_NKEventType] <> ''),
	CONSTRAINT CK_SubscriptionEvent_SSV_DataSetCode CHECK ([SSV_DataSetCode] <> ''),
	CONSTRAINT CK_SubscriptionEvent_SSV_Country CHECK ([SSV_Country] <> ''),
	CONSTRAINT CK_SubscriptionEvent_SSV_ClientGroupCountry CHECK ([SSV_ClientGroupCountry] <> ''),
)
GO
