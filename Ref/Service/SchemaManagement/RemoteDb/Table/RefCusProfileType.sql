CREATE TABLE RefCusProfileType
(
	XXX_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusProfileType_XXX_PK  DEFAULT (NEWID()),
	XXX_ProfileType VARCHAR(10) NOT NULL,
	XXX_ZZI_TariffType UNIQUEIDENTIFIER NULL,
	XXX_Description NVARCHAR(500) NOT NULL,
	XXX_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL,

	CONSTRAINT PK_RefCusProfileType PRIMARY KEY CLUSTERED (XXX_PK ASC),
	CONSTRAINT CK_RefCusProfileType_XXX_ProfileType CHECK (XXX_ProfileType <> ''),
	CONSTRAINT CK_RefCusProfileType_XXX_Description CHECK (XXX_Description <> ''),
	CONSTRAINT FK_RefCusProfileType_RefCusTariffType FOREIGN KEY (XXX_ZZI_TariffType) REFERENCES RefCusTariffType (ZZI_PK),
	CONSTRAINT FK_RefCusProfileType_RefDataGrouping FOREIGN KEY (XXX_ZZZ_NKDataGrouping) REFERENCES RefDataGrouping (ZZZ_DataGrouping)
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProfileType_XXX_ZZZ_NKDataGrouping_XXX_ProfileType_XXX_ZZI_TariffType ON RefCusProfileType (XXX_ZZZ_NKDataGrouping ASC, XXX_ProfileType ASC, XXX_ZZI_TariffType ASC)
GO
ALTER TABLE RefCusProfileType SET (LOCK_ESCALATION = DISABLE);
