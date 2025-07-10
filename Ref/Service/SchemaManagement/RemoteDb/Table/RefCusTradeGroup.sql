CREATE TABLE RefCusTradeGroup
(
[ZZA_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusTradeGroup_ZZA_PK] DEFAULT (NEWID()),
[ZZA_TradeGroup] VARCHAR(35) NOT NULL,
[ZZA_Description] NVARCHAR(4000) NOT NULL,
[ZZA_StartDate] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefCusTradeGroup_ZZA_StartDate] DEFAULT GetUtcDate(),
[ZZA_EndDate] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefCusTradeGroup_ZZA_EndDate] DEFAULT '2079-06-06 23:59',
[ZZA_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
CONSTRAINT [PK_RefCusTradeGroup] PRIMARY KEY CLUSTERED( [ZZA_PK] ASC ),
CONSTRAINT [CK_RefCusTradeGroup_ZZA_TradeGroup] CHECK ([ZZA_TradeGroup] <> ''),
CONSTRAINT [CK_RefCusTradeGroup_ZZA_Description] CHECK ([ZZA_Description] <> ''),
CONSTRAINT [CK_RefCusTradeGroup_ZZA_StartDate_ZZA_EndDate] CHECK ([ZZA_StartDate] <= [ZZA_EndDate]),
CONSTRAINT [CK_RefCusTradeGroup_ZZA_ZZZ_NKDataGrouping] CHECK ([ZZA_ZZZ_NKDataGrouping] <> ''),
CONSTRAINT [FK_RefCusTradeGroup_RefDataGrouping] FOREIGN KEY ([ZZA_ZZZ_NKDataGrouping]) REFERENCES [RefDataGrouping] ([ZZZ_DataGrouping])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTradeGroup_ZZA_ZZZ_NKDataGrouping_ZZA_TradeGroup ON RefCusTradeGroup (ZZA_ZZZ_NKDataGrouping, ZZA_TradeGroup)
GO
