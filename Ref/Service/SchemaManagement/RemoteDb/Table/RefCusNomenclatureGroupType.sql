CREATE TABLE RefCusNomenclatureGroupType
(
[ZZ9_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusNomenclatureGroupType_ZZ9_PK] DEFAULT (NEWID()),
[ZZ9_GroupType] VARCHAR(3) NOT NULL,
[ZZ9_Description] NVARCHAR(MAX) NOT NULL,

CONSTRAINT [PK_RefCusNomenclatureGroupType] PRIMARY KEY CLUSTERED ([ZZ9_PK] ASC),
CONSTRAINT [CK_RefCusNomenclatureGroupType_ZZ9_GroupType] CHECK ([ZZ9_GroupType] <> ''),
CONSTRAINT [CK_RefCusNomenclatureGroupType_ZZ9_Description] CHECK ([ZZ9_Description] <> ''),
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusNomenclatureGroupType_ZZ9_GroupType ON RefCusNomenclatureGroupType(ZZ9_GroupType)
GO
