CREATE TABLE RefCusTariffAdditionalCode(
	[ZY2_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusTariffAdditionalCode_ZY2_PK] DEFAULT NEWID(),
	[ZY2_ZZ1_Tariff] UNIQUEIDENTIFIER,
	[ZY2_ZZW_NationalCode] UNIQUEIDENTIFIER,
	[ZY2_AdditionalCode] NVARCHAR(15) NOT NULL CONSTRAINT [DF_RefCusTariffAdditionalCode_ZY2_AdditionalCode] DEFAULT '',
	[ZY2_Description] NVARCHAR(200) CONSTRAINT [DF_RefCusTariffAdditionalCode_ZY2_Description] DEFAULT '',
	[ZY2_ZY3_NKCategory] CHAR(3) NOT NULL,
	[ZY2_ParentAdditionalCode] NVARCHAR(15) NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCode_ZY2_ParentAdditionalCode DEFAULT '',
	[ZY2_ZY3_NKParentCategory] CHAR(3) NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCode_ZY2_ZY3_NKParentCategory DEFAULT '',
	[ZY2_IsMandatory] BIT CONSTRAINT [DF_RefCusTariffAdditionalCode_ZY2_IsMandatory] DEFAULT 0,
	[ZY2_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
	[ZY2_DataSetPK] UNIQUEIDENTIFIER,
	[ZY2_DataSetCode] VARCHAR(3),
	[ZY2_SysStartTime]  DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZY2_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZY2_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZY2_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZY2_SysStartTime], [ZY2_SysEndTime]),
	CONSTRAINT [PK_RefCusTariffAdditionalCode] PRIMARY KEY CLUSTERED ([ZY2_PK] ASC),
	CONSTRAINT [FK_RefCusTariffAdditionalCode_RefCusTariff] FOREIGN KEY([ZY2_ZZ1_Tariff]) REFERENCES RefCusTariff ([ZZ1_PK]),
	CONSTRAINT [FK_RefCusTariffAdditionalCode_RefCusTariffNationalCode] FOREIGN KEY([ZY2_ZZW_NationalCode]) REFERENCES RefCusTariffNationalCode ([ZZW_PK]),
	CONSTRAINT [FK_RefCusTariffAdditionalCode_RefCusTariffAdditionalCodeCategory] FOREIGN KEY([ZY2_ZZZ_NKDataGrouping], [ZY2_ZY3_NKCategory]) REFERENCES RefCusTariffAdditionalCodeCategory ([ZY3_ZZZ_NKDataGrouping], [ZY3_Category]),
	CONSTRAINT [CK_RefCusTariffAdditionalCode_ZY2_ZZ1_Tariff_ZY2_ZZW_NationalCode] CHECK ([ZY2_ZZ1_Tariff] IS NULL OR [ZY2_ZZW_NationalCode] IS NULL)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusTariffAdditionalCodeHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCode_NKDataGrouping_NKCategory_AdditionalCode_Tariff_NationalCode_ParentAdditionalCode_NKParentCategory ON RefCusTariffAdditionalCode (ZY2_ZZZ_NKDataGrouping, ZY2_ZY3_NKCategory, ZY2_AdditionalCode, ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode, ZY2_ParentAdditionalCode, ZY2_ZY3_NKParentCategory)
GO
CREATE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCode_ZY2_ZZZ_NKDataGrouping_ZY2_ZZ1_Tariff ON RefCusTariffAdditionalCode (ZY2_DataSetPK, ZY2_DataSetCode)
GO
ALTER TABLE RefCusTariffAdditionalCode SET (LOCK_ESCALATION = DISABLE);
