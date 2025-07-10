CREATE TABLE RefComplianceCommodityAlert (
  [RCR_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RCR_PK] DEFAULT NEWID(),
  [RCR_IsActive] BIT NOT NULL CONSTRAINT [DF_RCR_IsActive] DEFAULT 1,
  [RCR_AlertCode] VARCHAR(200) NOT NULL CONSTRAINT [DF_RCR_AlertCode] DEFAULT '',
  [RCR_AlertDescription] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RCR_AlertDescription] DEFAULT '',
  [RCR_AlertName] NVARCHAR(200) NOT NULL CONSTRAINT [DF_RCR_AlertName] DEFAULT '',
  [RCR_AlertType] CHAR(3) NOT NULL CONSTRAINT [DF_RCR_AlertType] DEFAULT '',
  [RCR_CountryRegion] CHAR(2) NOT NULL CONSTRAINT [DF_RCR_CountryRegion] DEFAULT '',
  [RCR_PublishYear] SMALLINT NOT NULL CONSTRAINT [DF_RCR_PublishYear] DEFAULT 0,
  [RCR_SourceURL] VARCHAR(2048) NOT NULL CONSTRAINT [DF_RCR_SourceURL] DEFAULT '',
  [RCR_TradeDirection] CHAR(3) NOT NULL CONSTRAINT [DF_RCR_TradeDirection] DEFAULT '',
  [RCR_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_RCR_SysStartTime] DEFAULT CONVERT(DATETIME2, SYSUTCDATETIME()),
  [RCR_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_RCR_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
  PERIOD FOR SYSTEM_TIME ([RCR_SysStartTime], [RCR_SysEndTime]),
  CONSTRAINT [PK_RefComplianceCommodityAlert] PRIMARY KEY CLUSTERED ([RCR_PK] ASC),
  CONSTRAINT [CK_RefComplianceCommodityAlert_RCR_AlertCode] CHECK ([RCR_AlertCode] <> ''),
  CONSTRAINT [CK_RefComplianceCommodityAlert_RCR_AlertName] CHECK (DATALENGTH([RCR_AlertName]) > 0),
  CONSTRAINT [CK_RefComplianceCommodityAlert_RCR_AlertType] CHECK ([RCR_AlertType] = 'NOM' OR [RCR_AlertType] =  'COM' OR [RCR_AlertType] =  'LOC'),
  CONSTRAINT [CK_RefComplianceCommodityAlert_RCR_CountryRegion] CHECK (LEN([RCR_CountryRegion]) = 2),
  CONSTRAINT [CK_RefComplianceCommodityAlert_RCR_PublishYear] CHECK ([RCR_PublishYear] >= 0),
  CONSTRAINT [CK_RefComplianceCommodityAlert_RCR_TradeDirection] CHECK ([RCR_TradeDirection] = 'EXP' OR [RCR_TradeDirection] =  'IMP'),
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefComplianceCommodityAlertHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefComplianceCommodityAlert_RCR_CountryRegion_RCR_AlertCode_RCR_TradeDirection] ON [RefComplianceCommodityAlert] ([RCR_CountryRegion] ASC, [RCR_AlertCode] ASC, [RCR_TradeDirection] ASC)
GO
ALTER TABLE RefComplianceCommodityAlert SET (LOCK_ESCALATION = DISABLE);
