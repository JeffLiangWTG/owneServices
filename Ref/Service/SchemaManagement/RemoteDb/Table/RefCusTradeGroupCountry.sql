CREATE TABLE RefCusTradeGroupCountry
(
[ZZB_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusTradeGroupCountry_ZZB_PK] DEFAULT (NEWID()),
[ZZB_ZZA_TradeGroup] UNIQUEIDENTIFIER NOT NULL,
[ZZB_RN_NKTradeGroupCountryCode] CHAR(2) NOT NULL,
[ZZB_StartDate] DATE NOT NULL CONSTRAINT [DF_RefCusTradeGroupCountry_ZZB_StartDate] DEFAULT GetUtcDate(),
[ZZB_EndDate] DATE NOT NULL CONSTRAINT [DF_RefCusTradeGroupCountry_ZZB_EndDate] DEFAULT '2079-06-06',
[ZZB_Description] NVARCHAR(200) NOT NULL CONSTRAINT [DF_RefCusTradeGroupCountry_ZZB_Description] DEFAULT '',
CONSTRAINT [PK_RefCusTradeGroupCountry] PRIMARY KEY CLUSTERED( [ZZB_PK] ASC ),
CONSTRAINT [FK_RefCusTradeGroupCountry_RefCusTradeGroup] FOREIGN KEY([ZZB_ZZA_TradeGroup]) REFERENCES [RefCusTradeGroup] ([ZZA_PK]) ON DELETE CASCADE,
CONSTRAINT [CK_RefCusTradeGroupCountry_ZZB_RN_NKTradeGroupCountryCode] CHECK ([ZZB_RN_NKTradeGroupCountryCode] <> ''),
CONSTRAINT [CK_RefCusTradeGroupCountry_ZZB_StartDate_ZZB_EndDate] CHECK ([ZZB_StartDate] <= [ZZB_EndDate])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTradeGroupCountry_ZZB_ZZA_TradeGroup_ZZB_RN_NKTradeGroupCountryCode_ZZB_StartDate ON RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTradeGroupCountry_ZZB_ZZA_TradeGroup_ZZB_RN_NKTradeGroupCountryCode_ZZB_EndDate ON RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup ASC, ZZB_RN_NKTradeGroupCountryCode ASC, ZZB_EndDate ASC)
GO
