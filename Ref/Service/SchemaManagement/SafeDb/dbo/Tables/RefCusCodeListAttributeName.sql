CREATE TABLE RefCusCodeListAttributeName(
	[ZXE_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_PK DEFAULT (NEWID()),
	[ZXE_Name] VARCHAR(32) NOT NULL,
	[ZXE_Description] VARCHAR(500) NOT NULL,
	[ZXE_ZZK_NKCodeType] VARCHAR(5) NOT NULL,
	[ZXE_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
	[ZXE_IsMandatory] BIT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_IsMandatory DEFAULT (0),
	[ZXE_AllowDuplicates] BIT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_AllowDuplicates DEFAULT (0),
	[ZXE_IsValueMandatory] BIT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_IsValueMandatory DEFAULT (0),
	[ZXE_ZZK_NKCodeTypeForValueList] VARCHAR(5) NULL,
	[ZXE_ValueDataType] VARCHAR (7) NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_ValueDataType DEFAULT(''),
	[ZXE_MinLengthOrValue] SMALLINT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_MinLengthOrValue DEFAULT(0),
	[ZXE_MaxLengthOrValue] DECIMAL (19,5) NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_MaxLengthOrValue DEFAULT(0),
	[ZXE_DecimalPlaces] TINYINT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_DecimalPlaces DEFAULT(0),
	[ZXE_ColumnCaption] VARCHAR(35) NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_ColumnCaption DEFAULT (''),
	[ZXE_IsDateRangeUsed] BIT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_IsDateRangeUsed DEFAULT (0),
	[ZXE_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZXE_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZXE_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZXE_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZXE_SysStartTime], [ZXE_SysEndTime]),
	CONSTRAINT [PK_RefCusCodeListAttributeName] PRIMARY KEY CLUSTERED( [ZXE_PK] ASC ),
	CONSTRAINT [FK_RefCusCodeListAttributeName_RefDataGrouping] FOREIGN KEY([ZXE_ZZZ_NKDataGrouping]) REFERENCES RefDataGrouping (ZZZ_DataGrouping),
	CONSTRAINT [CK_RefCusCodeListAttributeName_ZXE_ValueDataType] CHECK ([ZXE_ValueDataType]='Decimal' OR [ZXE_ValueDataType]='Integer' OR [ZXE_ValueDataType]='Boolean' OR [ZXE_ValueDataType]='String' OR [ZXE_ValueDataType]=''),
	CONSTRAINT [CK_RefCusCodeListAttributeName_ZXE_MinLengthOrValue] CHECK ([ZXE_MinLengthOrValue] >= 0),
	CONSTRAINT [CK_RefCusCodeListAttributeName_ZXE_MaxLengthOrValue] CHECK ([ZXE_MaxLengthOrValue] >= 0),
	CONSTRAINT [CK_RefCusCodeListAttributeName_ZXE_DecimalPlaces] CHECK (([ZXE_DecimalPlaces] = 0 AND [ZXE_ValueDataType] != 'Decimal') OR ([ZXE_DecimalPlaces] >= 0 AND [ZXE_ValueDataType] = 'Decimal')),
	CONSTRAINT [FK_RefCusCodeListAttributeName_RefCusCodeType] FOREIGN KEY ([ZXE_ZZZ_NKDataGrouping], [ZXE_ZZK_NKCodeType]) REFERENCES RefCusCodeType ([ZZK_ZZZ_NKDataGrouping], [ZZK_CodeType]),
	CONSTRAINT [FK_RefCusCodeListAttributeName_RefCusCodeType_ValueList] FOREIGN KEY ([ZXE_ZZZ_NKDataGrouping], [ZXE_ZZK_NKCodeTypeForValueList]) REFERENCES RefCusCodeType ([ZZK_ZZZ_NKDataGrouping], [ZZK_CodeType])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusCodeListAttributeNameHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeListAttributeName_ZXE_Name_ZXE_ZZK_NKCodeType_ZXE_ZZZ_NKDataGrouping ON RefCusCodeListAttributeName(ZXE_Name ASC, ZXE_ZZK_NKCodeType ASC, ZXE_ZZZ_NKDataGrouping ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption] ON [dbo].[RefCusCodeListAttributeName] (ZXE_ZZZ_NKDataGrouping, ZXE_ZZK_NKCodeType, ZXE_ColumnCaption) WHERE ZXE_ColumnCaption <> ''
GO
ALTER TABLE RefCusCodeListAttributeName SET (LOCK_ESCALATION = DISABLE);
