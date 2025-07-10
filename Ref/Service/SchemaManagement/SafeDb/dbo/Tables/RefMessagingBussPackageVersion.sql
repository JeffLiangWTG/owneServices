CREATE TABLE [RefMessagingBussPackageVersion]
(
	[ZMV_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefMessagingBussPackageVersion_ZMV_PK] DEFAULT (NEWID()),
	[ZMV_ZMP_PackageInfo] UNIQUEIDENTIFIER NOT NULL,
	[ZMV_Version] VARCHAR(50) NOT NULL CONSTRAINT [DF_RefMessagingBussPackageVersion_ZMV_Version] DEFAULT (''),
	[ZMV_SysStartTime]  DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZMV_SysStartTime] DEFAULT SYSUTCDATETIME(),
	[ZMV_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZMV_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZMV_SysStartTime], [ZMV_SysEndTime]),
	CONSTRAINT [PK_RefMessagingBussPackageVersion] PRIMARY KEY NONCLUSTERED([ZMV_PK] ASC),
	CONSTRAINT [FK_RefMessagingBussPackageVersion_RefMessagingBussPackageInfo] FOREIGN KEY([ZMV_ZMP_PackageInfo]) REFERENCES [RefMessagingBussPackageInfo] ([ZMP_PK]),
	CONSTRAINT [CK_RefMessagingBussPackageVersion_ZMV_Version] CHECK ([ZMV_Version]<>'')
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefMessagingBussPackageVersionHistory))
GO
CREATE UNIQUE CLUSTERED INDEX [IX_RefMessagingBussPackageVersion_ZMV_ZMP_PackageInfo_ZMV_Version] ON [RefMessagingBussPackageVersion] ([ZMV_ZMP_PackageInfo] ASC,[ZMV_Version] ASC)
GO
ALTER TABLE RefMessagingBussPackageVersion SET (LOCK_ESCALATION = DISABLE);
