CREATE TABLE RefCusCodeListAttributeName(
	ZXE_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_PK DEFAULT (NEWID()),
	ZXE_Name VARCHAR(32) NOT NULL,
	ZXE_Description VARCHAR(500) NOT NULL,
	ZXE_ZZK_NKCodeType VARCHAR(5) NOT NULL,
	ZXE_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL,
	ZXE_IsMandatory BIT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_IsMandatory DEFAULT (0),
	ZXE_AllowDuplicates BIT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_AllowDuplicates DEFAULT (0),
	ZXE_IsValueMandatory BIT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_IsValueMandatory DEFAULT (0),
	ZXE_ZZK_NKCodeTypeForValueList VARCHAR(5) NULL,
	ZXE_ValueDataType VARCHAR (7) NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_ValueDataType DEFAULT(''),
	ZXE_MinLengthOrValue SMALLINT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_MinLengthOrValue DEFAULT(0),
	ZXE_MaxLengthOrValue DECIMAL (19,5) NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_MaxLengthOrValue DEFAULT(0),
	ZXE_DecimalPlaces TINYINT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_DecimalPlaces DEFAULT(0),
	ZXE_ColumnCaption VARCHAR(35) NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_ColumnCaption DEFAULT (''),
	ZXE_IsDateRangeUsed BIT NOT NULL CONSTRAINT DF_RefCusCodeListAttributeName_ZXE_IsDateRangeUsed DEFAULT (0),
	CONSTRAINT PK_RefCusCodeListAttributeName PRIMARY KEY CLUSTERED( ZXE_PK ASC ),
	CONSTRAINT CK_RefCusCodeListAttributeName_ZXE_ValueDataType CHECK (ZXE_ValueDataType='Decimal' OR ZXE_ValueDataType='Integer' OR ZXE_ValueDataType='Boolean' OR ZXE_ValueDataType='String' OR ZXE_ValueDataType=''),
	CONSTRAINT CK_RefCusCodeListAttributeName_ZXE_MinLengthOrValue CHECK (ZXE_MinLengthOrValue >= 0),
	CONSTRAINT CK_RefCusCodeListAttributeName_ZXE_MaxLengthOrValue CHECK (ZXE_MaxLengthOrValue >= 0),
	CONSTRAINT CK_RefCusCodeListAttributeName_ZXE_DecimalPlaces CHECK ((ZXE_DecimalPlaces = 0 AND ZXE_ValueDataType != 'Decimal') OR (ZXE_DecimalPlaces >= 0 AND ZXE_ValueDataType = 'Decimal'))
)
GO
