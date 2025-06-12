CREATE TABLE [dbo].[eHubAuditRequest] (
    [B0_PK]                            UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL,
    [B0_LicenceCode]                   VARCHAR (20)     NOT NULL,
    [B0_RequestUTC]                    DATETIME         NOT NULL,
    [B0_ClientSpecifiedIdentifier]     UNIQUEIDENTIFIER NULL,
    [B0_UserName]                      VARCHAR (100)    NOT NULL,
    [B0_TransactionType]               VARCHAR (3)      NOT NULL,
    [B0_RequestIP]                     VARCHAR (39)     NOT NULL,
    [B0_TransactionSubType]            VARCHAR (3)      NOT NULL,
    [B0_TransactionIdentifier]         UNIQUEIDENTIFIER NULL,
    [B0_ClientSpecifiedIdentifierType] VARCHAR (5)      NOT NULL
);

