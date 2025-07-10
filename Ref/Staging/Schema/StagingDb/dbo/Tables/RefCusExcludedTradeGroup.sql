CREATE TABLE RefCusExcludedTradeGroup
(
 ZZC_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusExcludedTradeGroup_ZZC_PK default newid(),
 ZZC_ZZT_Applicability UNIQUEIDENTIFIER NOT NULL,
 ZZC_ZZA_NKTradeGroup VARCHAR(35) NOT NULL,
 ZZC_ZZA_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL,
 CONSTRAINT PK_RefCusExcludedTradeGroup PRIMARY KEY CLUSTERED( ZZC_PK ASC ),
 CONSTRAINT FK_RefCusExcludedTradeGroup_RefCusApplicability FOREIGN KEY(ZZC_ZZT_Applicability) REFERENCES  RefCusApplicability (ZZT_PK),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusExcludedTradeGroup_ZZC_ZZT_Applicability] ON [RefCusExcludedTradeGroup] ([ZZC_ZZT_Applicability])
