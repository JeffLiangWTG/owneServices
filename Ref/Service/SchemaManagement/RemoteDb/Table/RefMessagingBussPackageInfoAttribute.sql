CREATE TABLE [RefMessagingBussPackageInfoAttribute]
(
	[ZPA_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefMessagingBussPackageInfoAttribute_ZPA_PK] DEFAULT (NEWID()),
	[ZPA_ZMP_PackageInfo] UNIQUEIDENTIFIER NOT NULL,
	[ZPA_ZAI_AttributeInfo] UNIQUEIDENTIFIER NOT NULL,
	[ZPA_AttributeValue] VARCHAR(200) NOT NULL CONSTRAINT [DF_RefMessagingBussPackageInfoAttribute_ZPA_AttributeValue] DEFAULT (''),

	CONSTRAINT [PK_RefMessagingBussPackageInfoAttribute] PRIMARY KEY NONCLUSTERED([ZPA_PK] ASC),
	CONSTRAINT [FK_RefMessagingBussPackageInfoAttribute_RefMessagingBussAttributeInfo] FOREIGN KEY([ZPA_ZAI_AttributeInfo]) REFERENCES [RefMessagingBussAttributeInfo] ([ZAI_PK]),
	CONSTRAINT [FK_RefMessagingBussPackageInfoAttribute_RefMessagingBussPackageInfo] FOREIGN KEY([ZPA_ZMP_PackageInfo]) REFERENCES [RefMessagingBussPackageInfo] ([ZMP_PK]),
	CONSTRAINT [CK_RefMessagingBussPackageInfoAttribute_ZPA_AttributeValue] CHECK ([ZPA_AttributeValue]<>'')
)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefMessagingBussPackageInfoAttribute_ZPA_ZMP_PackageInfo_ZPA_ZAI_AttributeInfo_ZPA_AttributeValue] ON [RefMessagingBussPackageInfoAttribute]
(
	[ZPA_ZMP_PackageInfo],
	[ZPA_ZAI_AttributeInfo],
	[ZPA_AttributeValue]
)
GO
