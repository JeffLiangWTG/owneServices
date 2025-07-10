CREATE TABLE RefCusCodeTypeAttributeName
(
    ZKA_PK                 UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_RefCusCodeTypeAttributeName_ZKA_PK DEFAULT (NEWID()),
    ZKA_Name               VARCHAR(32)      NOT NULL,
    ZKA_Description        VARCHAR(500)     NOT NULL,
    ZKA_ZZK_NKCodeType     VARCHAR(10)       NOT NULL,
    ZKA_ZZZ_NKDataGrouping VARCHAR(3)       NOT NULL,
    CONSTRAINT PK_RefCusCodeTypeAttributeName PRIMARY KEY NONCLUSTERED (ZKA_PK ASC),
    CONSTRAINT FK_RefCusCodeTypeAttributeName_RefDataGrouping FOREIGN KEY (ZKA_ZZZ_NKDataGrouping) REFERENCES RefDataGrouping (ZZZ_DataGrouping),
    CONSTRAINT FK_RefCusCodeTypeAttributeName_RefCusCodeType FOREIGN KEY (ZKA_ZZZ_NKDataGrouping, ZKA_ZZK_NKCodeType) REFERENCES RefCusCodeType (ZZK_ZZZ_NKDataGrouping, ZZK_CodeType),
    CONSTRAINT CK_RefCusCodeTypeAttributeName_ZKA_Description CHECK (ZKA_Description <> ''),
    CONSTRAINT CK_RefCusCodeTypeAttributeName_ZKA_Name CHECK (ZKA_Name <> ''),
    INDEX IX_RefCusCodeTypeAttributeName_ZKA_ZZZ_NKDataGrouping_ZKA_ZZK_NKCodeType_ZKA_Name UNIQUE CLUSTERED
        (ZKA_ZZZ_NKDataGrouping, ZKA_ZZK_NKCodeType, ZKA_Name),
)
GO