CREATE TABLE RefCusCondition
(
	[ZX1_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusCondition_ZX1_PK] DEFAULT NEWID(),
	[ZX1_ZX2_NKConditionType] VARCHAR(6) NOT NULL,
	[ZX1_ZX2_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusCondition_ZX1_ZX2_ZZZ_NKDataGrouping] DEFAULT (''),
	[ZX1_ZZ1_Tariff] UNIQUEIDENTIFIER,
	[ZX1_ZZ5_Nomenclature] UNIQUEIDENTIFIER,
	[ZX1_StartDate] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefCusCondition_ZX1_StartDate] DEFAULT '1900-01-01',
	[ZX1_EndDate] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefCusCondition_ZX1_EndDate] DEFAULT '2079-06-06 23:59',
	[ZX1_Source] NVARCHAR(4000) NOT NULL CONSTRAINT [DF_RefCusCondition_ZX1_Source] DEFAULT '',
	[ZX1_Comment] NVARCHAR(4000) NOT NULL CONSTRAINT [DF_RefCusCondition_ZX1_Comment] DEFAULT '',
	[ZX1_IsImport] BIT NOT NULL CONSTRAINT [DF_RefCusCondition_ZX1_IsImport] DEFAULT 0,
	[ZX1_IsExport] BIT NOT NULL CONSTRAINT [DF_RefCusCondition_ZX1_IsExport] DEFAULT 0,
	[ZX1_ConditionValueTrueMeansStop] BIT NOT NULL CONSTRAINT [DF_RefCusCondition_ZX1_ConditionValueTrueMeansStop] DEFAULT 0,
	[ZX1_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
	[ZX1_ZZS_NKPreference] VARCHAR(10),
	[ZX1_ZZS_ZZZ_NKDataGrouping] VARCHAR(3),
	[ZX1_LogicalANDWithinGroup] TINYINT NOT NULL CONSTRAINT [DF_RefCusCondition_ZX1_LogicalANDWithinGroup] DEFAULT 0,
	[ZX1_ZY7_NKConditionCode] VARCHAR(3) NULL,
	[ZX1_AdditionalComment] VARCHAR(4000) NOT NULL CONSTRAINT [DF_RefCusCondition_ZX1_AdditionalComment] DEFAULT '',
	[ZX1_Severity] CHAR(3) SPARSE NULL,
	CONSTRAINT [PK_RefCusCondition] PRIMARY KEY CLUSTERED( [ZX1_PK] ASC ),
	CONSTRAINT [CK_RefCusCondition_ZX1_ZZ1_Tariff_ZX1_ZZ5_Nomenclature_ZX1_ZZS_Preference] CHECK (([ZX1_ZZ1_Tariff] IS NULL AND [ZX1_ZZ5_Nomenclature] is NOT NULL) OR ([ZX1_ZZ1_Tariff] IS NOT NULL AND [ZX1_ZZ5_Nomenclature] IS NULL) OR ([ZX1_ZZ1_Tariff] IS NULL AND [ZX1_ZZ5_Nomenclature] IS NULL AND [ZX1_ZZS_NKPreference] IS NOT NULL)),
	CONSTRAINT [CK_RefCusCondition_ZX1_StartDate_ZX1_EndDate] CHECK ([ZX1_StartDate]<=[ZX1_EndDate]),
	CONSTRAINT [CK_RefCusCondition_ZX1_Severity] CHECK ([ZX1_Severity] = 'MSG' OR [ZX1_Severity] = 'WAR'),
	CONSTRAINT [FK_RefCusCondition_RefCusTariff] FOREIGN KEY([ZX1_ZZ1_Tariff]) REFERENCES [RefCusTariff] ([ZZ1_PK]),
	CONSTRAINT [FK_RefCusCondition_RefCusNomenclatureGroup] FOREIGN KEY([ZX1_ZZ5_Nomenclature]) REFERENCES [RefCusNomenclatureGroup] ([ZZ5_PK]),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusCondition_ZX1_ZZ1_Tariff] ON [RefCusCondition] ([ZX1_ZZ1_Tariff])
GO
CREATE NONCLUSTERED INDEX [IX_RefCusCondition_ZX1_ZZ5_Nomenclature] ON [RefCusCondition] ([ZX1_ZZ5_Nomenclature])
