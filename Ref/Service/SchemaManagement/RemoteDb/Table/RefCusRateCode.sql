CREATE TABLE RefCusRateCode
(
[ZY1_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusRateCode_ZY1_PK] DEFAULT NEWID(),
[ZY1_RateCode] VARCHAR(5) NOT NULL,
[ZY1_ZZR_RateType] UNIQUEIDENTIFIER NOT NULL,
[ZY1_Description] NVARCHAR(500) NOT NULL,
[ZY1_InternalUse] BIT NOT NULL CONSTRAINT [DF_RefCusRateCode_ZY1_InternalUse] DEFAULT 0,
[ZY1_ZZZ_NKDataGrouping] VARCHAR(3) NULL,

constraint [PK_RefCusRateCode] primary key ([ZY1_PK]),
constraint [CK_RefCusRateCode_ZY1_RateCode] CHECK  ([ZY1_RateCode] <> ''),
constraint [CK_RefCusRateCode_ZY1_Description] CHECK  ([ZY1_Description] <> ''),
constraint [FK_RefCusRateCode_RefCusRateType] foreign key ([ZY1_ZZR_RateType]) references [RefCusRateType] ([ZZR_PK]),
constraint [FK_RefCusRateCode_RefDataGrouping] foreign key ([ZY1_ZZZ_NKDataGrouping]) references [RefDataGrouping] ([ZZZ_DataGrouping])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusRateCode_ZY1_RateCode_ZY1_ZZR_RateType on RefCusRateCode(ZY1_RateCode, ZY1_ZZR_RateType)
GO
CREATE NONCLUSTERED INDEX IX_RefCusRateCode_ZY1_ZZR_RateType on RefCusRateCode(ZY1_ZZR_RateType)
GO
CREATE NONCLUSTERED INDEX IX_RefCusRateCode_ZY1_ZZZ_NKDataGrouping_ZY1_RateCode on RefCusRateCode(ZY1_ZZZ_NKDataGrouping, ZY1_RateCode)
WHERE ZY1_ZZZ_NKDataGrouping <> ''
GO
