CREATE TABLE RefCusRuling
(
[ZZX_PK] UNIQUEIDENTIFIER NOT NULL,
[ZZX_RN_NKCountryCode] CHAR(2) NOT NULL,
[ZZX_RulingNumber] VARCHAR(35) NOT NULL,
[ZZX_Description] VARCHAR(500) NOT NULL,
[ZZX_RulingType] VARCHAR(3) NOT NULL,
[ZZX_StartDate] DATE NOT NULL CONSTRAINT [DF_RefCusRuling_ZZX_StartDate] DEFAULT GetUtcDate(),
[ZZX_EndDate] DATE NOT NULL CONSTRAINT [DF_RefCusRuling_ZZX_EndDate] DEFAULT '2079-06-06 23:59',

CONSTRAINT [PK_RefCusRuling] PRIMARY KEY CLUSTERED( [ZZX_PK] ASC ),
CONSTRAINT [CK_RefCusRuling_ZZX_RulingNumber] CHECK ([ZZX_RulingNumber] <>''), 
CONSTRAINT [CK_RefCusRuling_ZZX_Description] CHECK ([ZZX_Description] <>''), 
CONSTRAINT [CK_RefCusRuling_ZZX_RulingType] CHECK ([ZZX_RulingType] <>''), 
CONSTRAINT [CK_RefCusRuling_ZZX_EndDate] CHECK ([ZZX_EndDate] >= [ZZX_StartDate]),
CONSTRAINT [CK_RefCusRuling_ZZX_RN_NKCountryCode] CHECK (LEN([ZZX_RN_NKCountryCode]) = 2),
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusRuling_ZZX_RN_NKCountryCode_ZZX_RulingNumber_ZZX_RulingType_ZZX_StartDate ON RefCusRuling (ZZX_RN_NKCountryCode,ZZX_RulingNumber,ZZX_RulingType, ZZX_StartDate)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusRuling_ZZX_RN_NKCountryCode_ZZX_RulingNumber_ZZX_RulingType_ZZX_EndDate ON RefCusRuling (ZZX_RN_NKCountryCode ASC, ZZX_RulingNumber ASC, ZZX_RulingType ASC, ZZX_EndDate ASC)
GO
