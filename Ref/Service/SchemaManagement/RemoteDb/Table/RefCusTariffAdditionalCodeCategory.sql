CREATE TABLE RefCusTariffAdditionalCodeCategory
(
ZY3_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCodeCategory_ZY3_PK DEFAULT NEWID(),
ZY3_Category CHAR(3) NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCodeCategory_ZY3_Category DEFAULT '',
ZY3_Description NVARCHAR(200) NOT NULL CONSTRAINT DF_RefCusTariffAdditionalCodeCategory_ZY3_Description DEFAULT '',
ZY3_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL,

CONSTRAINT PK_RefCusTariffAdditionalCodeCategory PRIMARY KEY CLUSTERED (ZY3_PK ASC),
CONSTRAINT FK_RefCusTariffAdditionalCodeCategory_RefDataGrouping FOREIGN KEY(ZY3_ZZZ_NKDataGrouping) REFERENCES RefDataGrouping (ZZZ_DataGrouping)
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffAdditionalCodeCategory_ZY3_ZZZ_NKDataGrouping_ZY3_Category ON RefCusTariffAdditionalCodeCategory(ZY3_ZZZ_NKDataGrouping, ZY3_Category)
GO
