CREATE TABLE edi.Usage
(
    US_ID                BIGINT           NOT NULL default (next value for edi.ID_Sequence), 
    US_Period            INT              NOT NULL,
    US_Category          CHAR (3)         NOT NULL,
    US_PriceItemCode     CHAR (3)         NOT NULL,
    US_BillableCount     INT              NOT NULL,
    US_ReportingSource   VARCHAR (3)      NOT NULL,
    US_ServiceOccuredUtc DATETIME2 (7)    NOT NULL,
    US_ClientID          VARCHAR (9)      NOT NULL,
    US_ClientNumber      VARCHAR (50)     NULL,
	US_DatabaseNumber    INT              NOT NULL default(0),
	US_CompanyNumber     SMALLINT         NOT NULL default(0),
    US_ClientStaffCode   VARCHAR (3)      NULL,
    US_Reference1        VARCHAR (50)     NOT NULL,
    US_Reference2        VARCHAR (50)     NULL,
    US_Reference3        VARCHAR (50)     NULL,
    US_Reference4        VARCHAR (50)     NULL,
    US_Reference5        VARCHAR (50)     NULL,
    US_Version           INT              DEFAULT ((0)) NOT NULL,
    US_Branch            VARCHAR (3)      NULL,
    US_SystemCreateUtc   DATETIME2 (0)    DEFAULT (sysutcdatetime()) NOT NULL,
    US_SystemLastEditUtc DATETIME2 (0)    DEFAULT (sysutcdatetime()) NOT NULL,
    US_CapturedUtc       DATETIME2 (0)    NOT NULL,
    US_MessageTrackingID VARCHAR (36)     NULL,
);

GO

CREATE UNIQUE CLUSTERED INDEX UX_UsageData ON edi.Usage (
	US_Period,
	US_Category,
	US_PriceItemCode,
	US_DatabaseNumber,
	US_CompanyNumber,
	US_Reference1,
	US_Reference2,
	US_Reference3,
	US_Reference4,
	US_Reference5,
	US_ServiceOccuredUtc,
	US_ReportingSource, 
	US_ClientNumber,
	US_ClientID,
	US_ClientStaffCode,
	US_MessageTrackingID) 
    WITH (DATA_COMPRESSION = PAGE, IGNORE_DUP_KEY = ON)
    ON PS_Period(US_Period);
GO
