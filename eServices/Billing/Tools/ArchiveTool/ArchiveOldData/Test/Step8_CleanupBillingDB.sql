USE [CargoWise.eServices.Billing]
GO

CREATE TABLE [edi].[ToDrop](
	[CH_ID] [bigint] NOT NULL,
	[CH_Period] [int] NOT NULL,
	[CH_Category] [char](3) NOT NULL,
	[CH_PriceItemCode] [char](3) NOT NULL,
	[CH_BillableCount] [int] NULL,
	[CH_ReportingSource] [varchar](3) NOT NULL,
	[CH_ServiceOccuredUtc] [datetime2](7) NOT NULL,
	[CH_ClientID] [varchar](9) NOT NULL,
	[CH_ClientNumber] [varchar](50) NULL,
	[CH_DatabaseNumber] [int] NOT NULL,
	[CH_CompanyNumber] [smallint] NOT NULL,
	[CH_ClientStaffCode] [varchar](3) NULL,
	[CH_Reference1] [varchar](50) NOT NULL,
	[CH_Reference2] [varchar](50) NULL,
	[CH_Reference3] [varchar](50) NULL,
	[CH_Reference4] [varchar](50) NULL,
	[CH_Reference5] [varchar](50) NULL,
	[CH_Version] [int] NOT NULL,
	[CH_Branch] [varchar](3) NULL,
	[CH_SystemCreateUtc] [datetime2](0) NOT NULL,
	[CH_SystemLastEditUtc] [datetime2](0) NOT NULL,
	[CH_CapturedUtc] [datetime2](0) NOT NULL,
	[CH_MessageTrackingID] [varchar](36) NULL
) ON YearsPre2018
GO

CREATE UNIQUE CLUSTERED INDEX [UX_ChargeableData] ON [edi].[ToDrop]
(
	[CH_Period] ASC,
	[CH_Category] ASC,
	[CH_PriceItemCode] ASC,
	[CH_ClientID] ASC,
	[CH_Reference1] ASC,
	[CH_Reference2] ASC,
	[CH_Reference3] ASC,
	[CH_Reference4] ASC,
	[CH_Reference5] ASC,
	[CH_ServiceOccuredUtc] ASC,
	[CH_ReportingSource] ASC,
	[CH_ClientStaffCode] ASC,
	[CH_MessageTrackingID] ASC
)WITH (DATA_COMPRESSION = PAGE, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = ON, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO

ALTER TABLE [edi].[Chargeable] SWITCH PARTITION 1 TO [edi].[ToDrop]

DROP TABLE [edi].[ToDrop]

CREATE TABLE [edi].[ToDrop](
	[US_ID] [bigint] NOT NULL,
	[US_Period] [int] NOT NULL,
	[US_Category] [char](3) NOT NULL,
	[US_PriceItemCode] [char](3) NOT NULL,
	[US_BillableCount] [int] NULL,
	[US_ReportingSource] [varchar](3) NOT NULL,
	[US_ServiceOccuredUtc] [datetime2](7) NOT NULL,
	[US_ClientID] [varchar](9) NOT NULL,
	[US_ClientNumber] [varchar](50) NULL,
	[US_DatabaseNumber] [int] NOT NULL,
	[US_CompanyNumber] [smallint] NOT NULL,
	[US_ClientStaffCode] [varchar](3) NULL,
	[US_Reference1] [varchar](50) NOT NULL,
	[US_Reference2] [varchar](50) NULL,
	[US_Reference3] [varchar](50) NULL,
	[US_Reference4] [varchar](50) NULL,
	[US_Reference5] [varchar](50) NULL,
	[US_Version] [int] NOT NULL,
	[US_Branch] [varchar](3) NULL,
	[US_SystemCreateUtc] [datetime2](0) NOT NULL,
	[US_SystemLastEditUtc] [datetime2](0) NOT NULL,
	[US_CapturedUtc] [datetime2](0) NOT NULL,
	[US_MessageTrackingID] [varchar](36) NULL
) ON YearsPre2018
GO

CREATE UNIQUE CLUSTERED INDEX [UX_UsageData] ON [edi].[ToDrop]
(
	[US_Period] ASC,
	[US_Category] ASC,
	[US_PriceItemCode] ASC,
	[US_DatabaseNumber] ASC,
	[US_CompanyNumber] ASC,
	[US_Reference1] ASC,
	[US_Reference2] ASC,
	[US_Reference3] ASC,
	[US_Reference4] ASC,
	[US_Reference5] ASC,
	[US_ServiceOccuredUtc] ASC,
	[US_ReportingSource] ASC,
	[US_ClientNumber] ASC,
	[US_ClientID] ASC,
	[US_ClientStaffCode] ASC,
	[US_MessageTrackingID] ASC
)WITH (DATA_COMPRESSION = PAGE, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = ON, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO

ALTER TABLE [edi].[Usage] SWITCH PARTITION 1 TO [edi].[ToDrop]

DROP TABLE [edi].[ToDrop]

DBCC SHRINKFILE (N'YearsPre2018', 0)