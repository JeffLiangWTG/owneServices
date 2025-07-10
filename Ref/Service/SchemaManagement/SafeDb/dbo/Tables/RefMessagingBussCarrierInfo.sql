CREATE TABLE [RefMessagingBussCarrierInfo](
	[ZMC_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefMessagingBussCarrierInfo_ZMC_PK] DEFAULT (NEWID()),
	[ZMC_ZMP_PackageInfo] UNIQUEIDENTIFIER NOT NULL,
	[ZMC_CarrierCode] CHAR(5) NOT NULL CONSTRAINT [DF_RefMessagingBussCarrierInfo_ZMC_CarrierCode] DEFAULT (''),
	[ZMC_CarrierName] VARCHAR(200) NOT NULL CONSTRAINT [DF_RefMessagingBussCarrierInfo_ZMC_CarrierName] DEFAULT (''),
	[ZMC_CountryCode] VARCHAR(2) NULL,
	[ZMC_SysStartTime]  DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZMC_SysStartTime] DEFAULT SYSUTCDATETIME(),
	[ZMC_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZMC_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZMC_SysStartTime], [ZMC_SysEndTime]),
	CONSTRAINT [PK_RefMessagingBussCarrierInfo] PRIMARY KEY NONCLUSTERED([ZMC_PK] ASC),
	CONSTRAINT [FK_RefMessagingBussCarrierInfo_RefMessagingBussPackageInfo] FOREIGN KEY([ZMC_ZMP_PackageInfo]) REFERENCES [RefMessagingBussPackageInfo] ([ZMP_PK]),
	CONSTRAINT [CK_RefMessagingBussCarrierInfo_ZMC_CarrierCode] CHECK (LEN([ZMC_CarrierCode])=(5)),
	CONSTRAINT [CK_RefMessagingBussCarrierInfo_ZMC_CarrierName] CHECK ([ZMC_CarrierName]<>''),
	CONSTRAINT [CK_RefMessagingBussCarrierInfo_ZMC_CountryCode] CHECK ([ZMC_CountryCode] IS NULL OR LEN([ZMC_CountryCode])=(2))
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefMessagingBussCarrierInfoHistory))
GO
CREATE UNIQUE CLUSTERED INDEX [IX_RefMessagingBussCarrierInfo_ZMC_CarrierCode] ON [RefMessagingBussCarrierInfo] ([ZMC_CarrierCode] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_RefMessagingBussCarrierInfo_ZMC_ZMP_PackageInfo] ON [RefMessagingBussCarrierInfo] ([ZMC_ZMP_PackageInfo] ASC)
GO
ALTER TABLE RefMessagingBussCarrierInfo SET (LOCK_ESCALATION = DISABLE);
