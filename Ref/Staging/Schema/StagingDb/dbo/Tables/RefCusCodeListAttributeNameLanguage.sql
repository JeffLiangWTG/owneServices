CREATE TABLE RefCusCodeListAttributeNameLanguage(
	ZXH_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusCodeListAttributeNameLanguage_ZXH_PK DEFAULT (NEWID()),
	ZXH_ZX6_NKLanguage VARCHAR(3) NOT NULL,
	ZXH_ZXE_CodeListAttributeName UNIQUEIDENTIFIER NOT NULL,
	ZXH_Description NVARCHAR(MAX) NOT NULL CONSTRAINT DF_RefCusCodeListAttributeNameLanguage_ZXH_Description DEFAULT (''),
	ZXH_Name NVARCHAR(MAX) NULL,
	ZXH_ColumnCaption NVARCHAR(35) NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXH_ColumnCaption DEFAULT (''),
	CONSTRAINT PK_RefCusCodeListAttributeNameLanguage PRIMARY KEY CLUSTERED (ZXH_PK ASC),
	CONSTRAINT FK_RefCusCodeListAttributeNameLanguage_ZXH_ZXE_CodeListAttributeName FOREIGN KEY(ZXH_ZXE_CodeListAttributeName) REFERENCES RefCusCodeListAttributeName (ZXE_PK),
)
GO
CREATE NONCLUSTERED INDEX IX_RefCusCodeListAttributeNameLanguage_ZXH_ZXE_CodeListAttributeName ON RefCusCodeListAttributeNameLanguage (ZXH_ZXE_CodeListAttributeName)
