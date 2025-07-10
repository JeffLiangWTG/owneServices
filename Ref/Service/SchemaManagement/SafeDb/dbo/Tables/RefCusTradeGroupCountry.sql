CREATE TABLE RefCusTradeGroupCountry (
	[ZZB_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusTradeGroupCountry_ZZB_PK] DEFAULT (NEWID()),
	[ZZB_ZZA_TradeGroup] UNIQUEIDENTIFIER NOT NULL,
	[ZZB_RN_NKTradeGroupCountryCode] CHAR(2) NOT NULL,
	[ZZB_StartDate] DATE NOT NULL CONSTRAINT [DF_RefCusTradeGroupCountry_ZZB_StartDate] DEFAULT GetUtcDate(),
	[ZZB_EndDate] DATE NOT NULL CONSTRAINT [DF_RefCusTradeGroupCountry_ZZB_EndDate] DEFAULT '2079-06-06 23:59',
	[ZZB_Description] NVARCHAR(200) NOT NULL CONSTRAINT [DF_RefCusTradeGroupCountry_ZZB_Description] DEFAULT '',
	[ZZB_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZZB_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZZB_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZZB_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZZB_SysStartTime], [ZZB_SysEndTime]),
	CONSTRAINT [PK_RefCusTradeGroupCountry] PRIMARY KEY CLUSTERED( [ZZB_PK] ASC ),
	CONSTRAINT [FK_RefCusTradeGroupCountry_RefCusTradeGroup] FOREIGN KEY([ZZB_ZZA_TradeGroup]) REFERENCES RefCusTradeGroup ([ZZA_PK]),
	CONSTRAINT [CK_RefCusTradeGroupCountry_ZZB_RN_NKTradeGroupCountryCode] CHECK ([ZZB_RN_NKTradeGroupCountryCode] <> ''),
	CONSTRAINT [CK_RefCusTradeGroupCountry_ZZB_StartDate_ZZB_EndDate] CHECK ([ZZB_StartDate] <= [ZZB_EndDate])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusTradeGroupCountryHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTradeGroupCountry_ZZB_ZZA_TradeGroup_ZZB_RN_NKTradeGroupCountryCode_ZZB_StartDate ON RefCusTradeGroupCountry ( ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate )
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTradeGroupCountry_ZZB_ZZA_TradeGroup_ZZB_RN_NKTradeGroupCountryCode_ZZB_EndDate ON RefCusTradeGroupCountry ( ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_EndDate )
GO
ALTER TABLE RefCusTradeGroupCountry SET (LOCK_ESCALATION = DISABLE);
