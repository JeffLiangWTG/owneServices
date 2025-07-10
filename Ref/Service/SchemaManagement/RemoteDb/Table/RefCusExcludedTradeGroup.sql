CREATE TABLE RefCusExcludedTradeGroup
(
	[ZZC_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusExcludedTradeGroup_ZZC_PK] DEFAULT NEWID(),
	[ZZC_ZZT_Applicability] UNIQUEIDENTIFIER NOT NULL,
	[ZZC_ZZA_TradeGroup] UNIQUEIDENTIFIER NOT NULL,
	CONSTRAINT [PK_RefCusExcludedTradeGroup] PRIMARY KEY CLUSTERED( [ZZC_PK] ASC ),
	CONSTRAINT [FK_RefCusExcludedTradeGroup_RefCusApplicability] FOREIGN KEY([ZZC_ZZT_Applicability]) REFERENCES [RefCusApplicability] (ZZT_PK),
	CONSTRAINT [FK_RefCusExcludedTradeGroup_RefCusTradeGroup] FOREIGN KEY(ZZC_ZZA_TradeGroup ) REFERENCES [RefCusTradeGroup] ([ZZA_PK])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusExcludedTradeGroup_ZZC_ZZT_Applicability_ZZC_ZZA_TradeGroup ON RefCusExcludedTradeGroup (ZZC_ZZT_Applicability, ZZC_ZZA_TradeGroup)
GO
CREATE NONCLUSTERED INDEX IX_RefCusExcludedTradeGroup_ZZC_ZZA_TradeGroup ON RefCusExcludedTradeGroup (ZZC_ZZA_TradeGroup)
GO
ALTER TABLE RefCusExcludedTradeGroup SET (LOCK_ESCALATION = DISABLE);
