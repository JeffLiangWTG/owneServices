CREATE TABLE edi.Chargeable
(
    CH_ID                BIGINT           NOT NULL default (next value for edi.ID_Sequence), 
    CH_Period            INT              NOT NULL,
    CH_Category          CHAR (3)         NOT NULL,
    CH_PriceItemCode     CHAR (3)         NOT NULL,
    CH_BillableCount     INT              NOT NULL,
    CH_ReportingSource   VARCHAR (3)      NOT NULL,
    CH_ServiceOccuredUtc DATETIME2 (7)    NOT NULL,
    CH_ClientID          VARCHAR (9)      NOT NULL,
    CH_ClientNumber      VARCHAR (50)     NULL,
	CH_DatabaseNumber    INT              NOT NULL,
	CH_CompanyNumber     SMALLINT         NOT NULL,
    CH_ClientStaffCode   VARCHAR (3)      NULL,
    CH_Reference1        VARCHAR (50)     NOT NULL,
    CH_Reference2        VARCHAR (50)     NULL,
    CH_Reference3        VARCHAR (50)     NULL,
    CH_Reference4        VARCHAR (50)     NULL,
    CH_Reference5        VARCHAR (50)     NULL,
    CH_Version           INT              DEFAULT ((0)) NOT NULL,
    CH_Branch            VARCHAR (3)      NULL,
    CH_SystemCreateUtc   DATETIME2 (0)    DEFAULT (sysutcdatetime()) NOT NULL,
    CH_SystemLastEditUtc DATETIME2 (0)    DEFAULT (sysutcdatetime()) NOT NULL,
    CH_CapturedUtc       DATETIME2 (0)    NOT NULL,
    CH_MessageTrackingID VARCHAR (36)     NULL,
);

GO

-- Note: indexing on Database and Company number, not ClientNumber or ClientID
-- since Database and Company are mandatory for it to be Chargeable
CREATE UNIQUE CLUSTERED INDEX UX_ChargeableData ON edi.Chargeable (
	CH_Period,
	CH_Category,
	CH_PriceItemCode,
	CH_DatabaseNumber,
	CH_CompanyNumber,
	CH_Reference1,
	CH_Reference2,
	CH_Reference3,
	CH_Reference4,
	CH_Reference5,
	CH_ServiceOccuredUtc,
	CH_ReportingSource,
	CH_ClientStaffCode,
	CH_MessageTrackingID) 
    WITH (DATA_COMPRESSION = PAGE, IGNORE_DUP_KEY = ON)
    ON PS_Period(CH_Period);
GO

CREATE TRIGGER edi.ChargeableSystemLastEditUTC
    ON edi.Chargeable
    FOR UPDATE
    AS
    BEGIN
		SET NOCOUNT ON
        IF NOT(UPDATE(CH_SystemLastEditUtc))
        BEGIN
			-- Since there is no index on the ID it's not practical to join the
			-- inserted table to the real table to automatically update the edit time.
			-- Make the update do it.
			RAISERROR('UPDATE on ediChargeable must set CH_SystemLastEditUtc.', 16, 1)
			ROLLBACK TRANSACTION
        END
    END

GO
