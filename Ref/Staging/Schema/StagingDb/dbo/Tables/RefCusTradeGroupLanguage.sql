CREATE TABLE [dbo].[RefCusTradeGroupLanguage]
(
	[ZXD_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTradeGroupLanguage_ZXD_PK DEFAULT(NEWID()), 
	[ZXD_ZX6_NKLanguage] VARCHAR(3) NOT NULL, 
	[ZXD_ZZA_TradeGroup] UNIQUEIDENTIFIER NOT NULL, 
	[ZXD_Description] NVARCHAR(MAX) NOT NULL CONSTRAINT DF_RefCusTradeGroupLanguage_ZXD_Description DEFAULT '',

	CONSTRAINT PK_RefCusTradeGroupLanguage PRIMARY KEY CLUSTERED ([ZXD_PK] ASC),
	CONSTRAINT FK_RefCusTradeGroupLanguage_RefCusTradeGroup FOREIGN KEY([ZXD_ZZA_TradeGroup]) REFERENCES RefCusTradeGroup (ZZA_PK)
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusTradeGroupLanguage_ZXD_ZZA_TradeGroup] ON [RefCusTradeGroupLanguage] ([ZXD_ZZA_TradeGroup])
