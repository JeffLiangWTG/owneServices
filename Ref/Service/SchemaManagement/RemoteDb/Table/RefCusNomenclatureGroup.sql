CREATE TABLE RefCusNomenclatureGroup
(
	[ZZ5_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusNomenclatureGroup_ZZ5_PK] DEFAULT (NEWID()),
	[ZZ5_ZZ9_NKNomenclatureGroupType] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusNomenclatureGroup_ZZ5_ZZ9_NKNomenclatureGroupType] DEFAULT (''),
	[ZZ5_Value] VARCHAR(15) NOT NULL CONSTRAINT [DF_RefCusNomenclatureGroup_ZZ5_Value] DEFAULT (''),
	[ZZ5_Description] NVARCHAR(MAX) NOT NULL,
	[ZZ5_StartDate] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefCusNomenclatureGroup_ZZ5_StartDate] DEFAULT GetUtcDate(),
	[ZZ5_EndDate] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefCusNomenclatureGroup_ZZ5_EndDate] DEFAULT '2079-06-06 23:59',
	[ZZ5_CompositeKey] VARCHAR(100) NOT NULL,
	[ZZ5_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
	CONSTRAINT [PK_RefCusNomenclatureGroup] PRIMARY KEY CLUSTERED( [ZZ5_PK] ASC ),
	CONSTRAINT [CK_RefCusNomenclatureGroup_ZZ5_Description] CHECK ([ZZ5_Description] <>''),
	CONSTRAINT [CK_RefCusNomenclatureGroup_ZZ5_CompositeKey] CHECK ([ZZ5_CompositeKey] <>''),
	CONSTRAINT [CK_RefCusNomenclatureGroup_ZZ5_ZZZ_NKDataGrouping] CHECK ([ZZ5_ZZZ_NKDataGrouping] <>''),
	CONSTRAINT [FK_RefCusNomenclatureGroup_RefDataGrouping] FOREIGN KEY ([ZZ5_ZZZ_NKDataGrouping]) REFERENCES [RefDataGrouping] ([ZZZ_DataGrouping])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusNomenclatureGroup_ZZ5_ZZZ_NKDataGrouping_ZZ5_ZZ9_NKNomenclatureGroupType_ZZ5_StartDate_ZZ5_CompositeKey ON RefCusNomenclatureGroup(ZZ5_ZZZ_NKDataGrouping ASC, ZZ5_ZZ9_NKNomenclatureGroupType ASC, ZZ5_StartDate ASC, ZZ5_CompositeKey ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusNomenclatureGroup_ZZ5_ZZZ_NKDataGrouping_ZZ5_ZZ9_NKNomenclatureGroupType_ZZ5_StartDate_ZZ5_CompositeKey_ZZ5_Value ON RefCusNomenclatureGroup (ZZ5_ZZZ_NKDataGrouping ASC, ZZ5_ZZ9_NKNomenclatureGroupType ASC, ZZ5_StartDate ASC, ZZ5_CompositeKey ASC, ZZ5_Value ASC)
GO
