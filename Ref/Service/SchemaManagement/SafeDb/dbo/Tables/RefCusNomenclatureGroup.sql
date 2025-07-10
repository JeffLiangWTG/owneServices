CREATE TABLE RefCusNomenclatureGroup(
	[ZZ5_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusNomenclatureGroup_ZZ5_PK] DEFAULT (NEWID()),
	[ZZ5_ZZ9_NKNomenclatureGroupType] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusNomenclatureGroup_ZZ5_ZZ9_NKNomenclatureGroupType] DEFAULT (''),
	[ZZ5_Value] VARCHAR(15) NOT NULL CONSTRAINT [DF_RefCusNomenclatureGroup_ZZ5_Value] DEFAULT (''),	
	[ZZ5_Description] NVARCHAR(MAX) NOT NULL,
	[ZZ5_StartDate] SMALLDATETIME NOT NULL,
	[ZZ5_EndDate] SMALLDATETIME NOT NULL,
	[ZZ5_CompositeKey] VARCHAR(100) NOT NULL,
	[ZZ5_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
	[ZZ5_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZZ5_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZZ5_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZZ5_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZZ5_SysStartTime], [ZZ5_SysEndTime]),
	CONSTRAINT [PK_RefCusNomenclatureGroup] PRIMARY KEY CLUSTERED( [ZZ5_PK] ASC ),
	CONSTRAINT [CK_RefCusNomenclatureGroup_ZZ5_Description] CHECK ([ZZ5_Description] <>''),
	CONSTRAINT [CK_RefCusNomenclatureGroup_ZZ5_CompositeKey] CHECK ([ZZ5_CompositeKey] <>''),
	CONSTRAINT [CK_RefCusNomenclatureGroup_ZZ5_ZZZ_NKDataGrouping] CHECK ([ZZ5_ZZZ_NKDataGrouping] <>''),
	CONSTRAINT [FK_RefCusNomenclatureGroup_RefDataGrouping] FOREIGN KEY([ZZ5_ZZZ_NKDataGrouping]) REFERENCES RefDataGrouping ([ZZZ_DataGrouping])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusNomenclatureGroupHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusNomenclatureGroup_ZZ5_ZZZ_NKDataGrouping_ZZ5_ZZ9_NKNomenclatureGroupType_ZZ5_StartDate_ZZ5_CompositeKey ON RefCusNomenclatureGroup(ZZ5_ZZZ_NKDataGrouping ASC, ZZ5_ZZ9_NKNomenclatureGroupType ASC, ZZ5_StartDate ASC, ZZ5_CompositeKey ASC)
GO
ALTER TABLE RefCusNomenclatureGroup SET (LOCK_ESCALATION = DISABLE);
