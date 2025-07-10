CREATE TABLE [RefMessagingBussAttributeInfo]
(
	[ZAI_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefMessagingBussAttributeInfo_ZAI_PK] DEFAULT (NEWID()),
	[ZAI_AttributeName] VARCHAR(200) NOT NULL CONSTRAINT [DF_RefMessagingBussAttributeInfo_ZAI_AttributeName] DEFAULT (''),
	
	CONSTRAINT [PK_RefMessagingBussAttributeInfo] PRIMARY KEY NONCLUSTERED([ZAI_PK] ASC),
	CONSTRAINT [CK_RefMessagingBussAttributeInfo_ZAI_AttributeName] CHECK ([ZAI_AttributeName]<>'')
)
GO
CREATE UNIQUE CLUSTERED INDEX [IX_RefMessagingBussAttributeInfo_ZAI_AttributeName] ON [RefMessagingBussAttributeInfo]
(
	[ZAI_AttributeName] ASC
)
GO
