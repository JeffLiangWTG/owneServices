CREATE TABLE RefCusConditionType
(
 ZX2_PK UNIQUEiDENTIFIER NOT NULL CONSTRAINT DF_RefCusConditionType_ZX2_PK DEFAULT NEWID(),
 ZX2_ConditionClass VARCHAR(5) NOT NULL,
 ZX2_ConditionType VARCHAR(6) NOT NULL,
 ZX2_Description NVARCHAR(500) NOT NULL,
 ZX2_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL

 CONSTRAINT PK_RefCusConditionType PRIMARY KEY CLUSTERED( ZX2_PK ASC ),
 CONSTRAINT CK_RefCusConditionType_ZX2_ConditionClass CHECK (ZX2_ConditionClass='CLASS' OR ZX2_ConditionClass='RATE' OR ZX2_ConditionClass='CTRL' OR ZX2_ConditionClass='VAT' OR ZX2_ConditionClass='RISK'),
)
GO
