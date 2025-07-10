CREATE TABLE RefCarrierCodeLanguage
(
	ZCL_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCarrierCodeLanguage_ZCL_PK  DEFAULT (newid()),
	ZCL_ZX6_NKLanguage VARCHAR(3) NOT NULL CONSTRAINT DF_RefCarrierCodeLanguage_ZCL_ZX6_NKLanguage  DEFAULT (''),
	ZCL_ZZ4_CarrierCode UNIQUEIDENTIFIER NOT NULL,
	ZCL_Description NVARCHAR(MAX) NOT NULL CONSTRAINT DF_RefCarrierCodeLanguage_ZCL_Description  DEFAULT (''),
  CONSTRAINT PK_RefCarrierCodeLanguage PRIMARY KEY CLUSTERED (ZCL_PK ASC),
  CONSTRAINT FK_RefCarrierCodeLanguage_RefCarrierCode FOREIGN KEY(ZCL_ZZ4_CarrierCode) REFERENCES RefCarrierCode (ZZ4_PK) ON DELETE CASCADE,
  CONSTRAINT FK_RefCarrierCodeLanguage_RefLanguageType FOREIGN KEY(ZCL_ZX6_NKLanguage) REFERENCES RefLanguageType (ZX6_Language)
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCarrierCodeLanguage_ZCL_ZX6_NKLanguage_ZCL_ZZ4_CarrierCode ON RefCarrierCodeLanguage (ZCL_ZX6_NKLanguage ASC, ZCL_ZZ4_CarrierCode ASC) 
GO
CREATE NONCLUSTERED INDEX IX_RefCarrierCodeLanguage_ZCL_ZZ4_CarrierCode ON RefCarrierCodeLanguage(ZCL_ZZ4_CarrierCode ASC)
GO
