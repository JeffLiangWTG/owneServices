CREATE TABLE RefCusTariffAdditionalCode (
ZY2_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCode_ZY2_PK DEFAULT NEWID(),
ZY2_ZZ1_Tariff UNIQUEIDENTIFIER,
ZY2_ZZW_NationalCode UNIQUEIDENTIFIER,
ZY2_AdditionalCode NVARCHAR(15) NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCode_ZY2_AdditionalCode DEFAULT '',
ZY2_Description NVARCHAR(200) CONSTRAINT DF_RefCusTariffAdditionalCode_ZY2_Description DEFAULT '',
ZY2_ZY3_NKCategory CHAR(3) NOT NULL,
ZY2_ParentAdditionalCode NVARCHAR(15) NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCode_ZY2_ParentAdditionalCode DEFAULT '',
ZY2_ZY3_NKParentCategory CHAR(3) NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCode_ZY2_ZY3_NKParentCategory DEFAULT '',
ZY2_IsMandatory BIT CONSTRAINT DF_RefCusTariffAdditionalCode_ZY2_IsMandatory DEFAULT 0,
ZY2_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL,

CONSTRAINT PK_RefCusTariffAdditionalCode PRIMARY KEY CLUSTERED (ZY2_PK ASC),
CONSTRAINT FK_RefCusTariffAdditionalCode_RefCusTariff FOREIGN KEY(ZY2_ZZ1_Tariff) REFERENCES RefCusTariff (ZZ1_PK),
CONSTRAINT FK_RefCusTariffAdditionalCode_RefCusTariffNationalCode FOREIGN KEY(ZY2_ZZW_NationalCode) REFERENCES RefCusTariffNationalCode (ZZW_PK),

CONSTRAINT CK_RefCusTariffAdditionalCode_ZY2_ZZ1_Tariff_ZY2_ZZW_NationalCode CHECK (ZY2_ZZ1_Tariff IS NULL OR ZY2_ZZW_NationalCode IS NULL)
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusTariffAdditionalCode_ZY2_ZZ1_Tariff] ON RefCusTariffAdditionalCode (ZY2_ZZ1_Tariff)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusTariffAdditionalCode_ZY2_ZZW_NationalCode] ON RefCusTariffAdditionalCode (ZY2_ZZW_NationalCode)
GO
