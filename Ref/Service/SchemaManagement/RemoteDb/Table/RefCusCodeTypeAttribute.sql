CREATE TABLE RefCusCodeTypeAttribute
(
    ZKE_PK           UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_RefCusCodeTypeAttribute_ZKE_PK DEFAULT (NEWID()),
    ZKE_ZZK_CodeType UNIQUEIDENTIFIER NOT NULL,
    ZKE_ZKA_NKName   VARCHAR(32)      NOT NULL,
    ZKE_Value        NVARCHAR(255)    NOT NULL,
    ZKE_StartDate    SMALLDATETIME    NOT NULL
        CONSTRAINT DF_RefCusCodeTypeAttribute_ZKE_StartDate DEFAULT GetUtcDate(),
    ZKE_EndDate      SMALLDATETIME    NOT NULL
        CONSTRAINT DF_RefCusCodeTypeAttribute_ZKE_EndDate DEFAULT ('2079-06-06 23:59'),
    CONSTRAINT PK_RefCusCodeTypeAttribute PRIMARY KEY CLUSTERED (ZKE_PK ASC),
    CONSTRAINT FK_RefCusCodeTypeAttribute_RefCusCodeType FOREIGN KEY (ZKE_ZZK_CodeType) REFERENCES RefCusCodeType (ZZK_PK),
    CONSTRAINT CK_RefCusCodeTypeAttribute_ZKE_ZKA_NKName CHECK (ZKE_ZKA_NKName <> ''),
    CONSTRAINT CK_RefCusCodeTypeAttribute_ZKE_StartDate_ZKE_EndDate CHECK (ZKE_StartDate <= ZKE_EndDate),
    INDEX IX_RefCusCodeTypeAttribute_ZKE_ZZK_CodeType_ZKE_ZKA_NKName_ZKE_StartDate_ZKE_Value UNIQUE NONCLUSTERED (ZKE_ZZK_CodeType, ZKE_ZKA_NKName, ZKE_StartDate, ZKE_Value)
)
GO
CREATE NONCLUSTERED INDEX IX_RefCusCodeTypeAttribute_ZKE_ZKA_NKName ON RefCusCodeTypeAttribute (ZKE_ZKA_NKName) INCLUDE (ZKE_ZZK_CodeType)
GO