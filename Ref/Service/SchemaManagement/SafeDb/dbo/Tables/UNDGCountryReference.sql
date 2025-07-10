CREATE TABLE [UNDGCountryReference] (
	[DCR_PK] UNIQUEIDENTIFIER NOT NULL,
	[DCR_Type] VARCHAR(5) NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_Type] DEFAULT '',
	[DCR_RN_NKCountry] VARCHAR(2) NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_RN_NKCountry] DEFAULT '',
	[DCR_Code] VARCHAR(6) NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_Code] DEFAULT '',
	[DCR_Description] VARCHAR(200) NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_Description] DEFAULT '',
	[DCR_HasFlashPointLower] BIT NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_HasFlashPointLower] DEFAULT 0,
	[DCR_FlashPointLowerCentigrade] DECIMAL(8,1) NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_FlashPointLowerCentigrade] DEFAULT 0,
	[DCR_HasFlashPointUpper] BIT NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_HasFlashPointUpper] DEFAULT 0,
	[DCR_FlashPointUpperCentigrade] DECIMAL(8,1) NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_FlashPointUpperCentigrade] DEFAULT 0,
	[DCR_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[DCR_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_UNDGCountryReference_DCR_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([DCR_SysStartTime], [DCR_SysEndTime]),

	CONSTRAINT [PK_UNDGCountryReference] PRIMARY KEY CLUSTERED  ([DCR_PK] ASC),
	CONSTRAINT [CK_DCR_Code] CHECK ([DCR_Code] <> ''),
	CONSTRAINT [CK_DCR_Description] CHECK ([DCR_Description] <> '' OR [DCR_Type] = 'PSA'),
	CONSTRAINT [CK_DCR_FlashPointLowerCentigrade] CHECK ([DCR_FlashPointLowerCentigrade] >= -273.15),
	CONSTRAINT [CK_DCR_FlashPointUpperCentigrade] CHECK ([DCR_FlashPointUpperCentigrade] >= -273.15),
	CONSTRAINT [CK_DCR_Type] CHECK ([DCR_Type] <> '')
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.UNDGCountryReferenceHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UNDGCountryReference_DCR_RN_NKCountry_DCR_Type_DCR_Code_DCR_HasFlashPointLower_DCR_FlashPointLowerCentigrade] ON [UNDGCountryReference] ([DCR_RN_NKCountry] ASC,[DCR_Type] ASC,[DCR_Code] ASC,[DCR_HasFlashPointLower] ASC,[DCR_FlashPointLowerCentigrade] ASC)
GO
ALTER TABLE UNDGCountryReference SET (LOCK_ESCALATION = DISABLE);
