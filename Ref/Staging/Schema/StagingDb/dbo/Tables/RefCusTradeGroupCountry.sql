CREATE TABLE RefCusTradeGroupCountry (
	ZZB_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTradeGroupCountry_ZZB_PK DEFAULT (NEWID()),
	ZZB_ZZA_TradeGroup UNIQUEIDENTIFIER NOT NULL,
	ZZB_RN_NKTradeGroupCountryCode CHAR(2) NOT NULL,
	ZZB_StartDate DATE NOT NULL CONSTRAINT DF_RefCusTradeGroupCountry_ZZB_StartDate DEFAULT GetUtcDate(),
	ZZB_EndDate DATE NOT NULL CONSTRAINT DF_RefCusTradeGroupCountry_ZZB_EndDate DEFAULT '2079-06-06 23:59',
	ZZB_Description NVARCHAR(200) NOT NULL CONSTRAINT DF_RefCusTradeGroupCountry_ZZB_Description DEFAULT '',
	CONSTRAINT PK_RefCusTradeGroupCountry PRIMARY KEY CLUSTERED( ZZB_PK ASC ),
	CONSTRAINT FK_RefCusTradeGroupCountry_RefCusTradeGroup FOREIGN KEY(ZZB_ZZA_TradeGroup) REFERENCES RefCusTradeGroup (ZZA_PK),
	CONSTRAINT CK_RefCusTradeGroupCountry_ZZB_StartDate_ZZB_EndDate CHECK (ZZB_StartDate <= ZZB_EndDate)
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusTradeGroupCountry_ZZB_ZZA_TradeGroup] ON [RefCusTradeGroupCountry] ([ZZB_ZZA_TradeGroup])
