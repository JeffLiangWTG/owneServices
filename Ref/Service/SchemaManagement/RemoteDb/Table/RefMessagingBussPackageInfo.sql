CREATE TABLE [RefMessagingBussPackageInfo]
(
	[ZMP_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefMessagingBussPackageInfo_ZMP_PK] DEFAULT (NEWID()),
	[ZMP_PackageName] VARCHAR(200) NOT NULL CONSTRAINT [DF_RefMessagingBussPackageInfo_ZMP_PackageName] DEFAULT (''),

	CONSTRAINT [PK_RefMessagingBussPackageInfo] PRIMARY KEY NONCLUSTERED([ZMP_PK] ASC),
	CONSTRAINT [CK_RefMessagingBussPackageInfo_ZMP_PackageName] CHECK ([ZMP_PackageName]<>'')
)
GO
CREATE UNIQUE CLUSTERED INDEX [IX_RefMessagingBussPackageInfo_ZMP_PackageName] ON [RefMessagingBussPackageInfo]
(
	[ZMP_PackageName] ASC
)
GO
