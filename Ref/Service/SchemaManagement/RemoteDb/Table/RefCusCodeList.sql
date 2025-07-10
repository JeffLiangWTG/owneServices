CREATE TABLE RefCusCodeList
(
[ZZD_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusCodeList_ZZD_PK] DEFAULT (NEWID()),
[ZZD_ZZK_NKCodeType] VARCHAR(10) NOT NULL,
[ZZD_Code] VARCHAR(35) NOT NULL,
[ZZD_Description] NVARCHAR(2000) NOT NULL,
[ZZD_StartDate] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefCusCodeList_ZZD_StartDate] DEFAULT GetUtcDate(),
[ZZD_EndDate] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefCusCodeList_ZZD_EndDate] DEFAULT '2079-06-06 23:59',
[ZZD_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
[ZZD_ZZK_NKCodeTypeComputed] AS ISNULL(CASE WHEN LEN([ZZD_ZZK_NKCodeType]) <= 5 THEN SUBSTRING([ZZD_ZZK_NKCodeType], 1, 5) ELSE '' END, ''),
CONSTRAINT [PK_RefCusCodeList] PRIMARY KEY CLUSTERED( [ZZD_PK] ASC ),
CONSTRAINT [CK_RefCusCodeList_ZZD_ZZK_NKCodeType] CHECK ([ZZD_ZZK_NKCodeType] <> ''),
CONSTRAINT [CK_RefCusCodeList_ZZD_Code] CHECK ([ZZD_Code] <> ''),
CONSTRAINT [CK_RefCusCodeList_ZZD_Description] CHECK ([ZZD_Description] <> ''),
CONSTRAINT [CK_RefCusCodeList_ZZD_ZZZ_NKDataGrouping] CHECK ([ZZD_ZZZ_NKDataGrouping] <> ''),
CONSTRAINT [CK_RefCusCodeList_ZZD_StartDate_ZZD_EndDate] CHECK ([ZZD_StartDate] <= [ZZD_EndDate]),
CONSTRAINT [FK_RefCusCodeList_RefDataGrouping] FOREIGN KEY ([ZZD_ZZZ_NKDataGrouping]) REFERENCES [RefDataGrouping] ([ZZZ_DataGrouping])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeType_ZZD_Code ON RefCusCodeList ( ZZD_ZZZ_NKDataGrouping, ZZD_ZZK_NKCodeType, ZZD_Code )
GO
CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZK_NKCodeType_ZZD_StartDate_ZZD_EndDate ON RefCusCodeList(ZZD_ZZK_NKCodeType,ZZD_StartDate,ZZD_EndDate) INCLUDE (ZZD_PK,ZZD_Code,ZZD_Description,ZZD_ZZZ_NKDataGrouping)
GO
CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_Code ON RefCusCodeList(ZZD_Code)
GO
CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeTypeComputed_ZZD_Code ON RefCusCodeList (ZZD_ZZZ_NKDataGrouping, ZZD_ZZK_NKCodeTypeComputed, ZZD_Code)
GO
CREATE NONCLUSTERED INDEX IX_RefCusCodeList_ZZD_ZZK_NKCodeTypeComputed_ZZD_StartDate_ZZD_EndDate ON RefCusCodeList(ZZD_ZZK_NKCodeTypeComputed,ZZD_StartDate,ZZD_EndDate) INCLUDE (ZZD_PK,ZZD_Code,ZZD_Description,ZZD_ZZZ_NKDataGrouping)
GO
