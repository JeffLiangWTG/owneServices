CREATE TABLE [RefMessagingBussPackageVersion]
(
	[ZMV_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefMessagingBussPackageVersion_ZMV_PK] DEFAULT (NEWID()),
	[ZMV_ZMP_PackageInfo] UNIQUEIDENTIFIER NOT NULL,
	[ZMV_Version] VARCHAR(50) NOT NULL CONSTRAINT [DF_RefMessagingBussPackageVersion_ZMV_Version] DEFAULT (''),
	CONSTRAINT [PK_RefMessagingBussPackageVersion] PRIMARY KEY NONCLUSTERED([ZMV_PK] ASC),
	CONSTRAINT [FK_RefMessagingBussPackageVersion_RefMessagingBussPackageInfo] FOREIGN KEY([ZMV_ZMP_PackageInfo]) REFERENCES [RefMessagingBussPackageInfo] ([ZMP_PK])
)
GO
CREATE NONCLUSTERED INDEX [IX_RefMessagingBussPackageVersion_ZMV_ZMP_PackageInfo] ON [RefMessagingBussPackageVersion] ([ZMV_ZMP_PackageInfo] ASC)
GO
