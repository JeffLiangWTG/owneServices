CREATE TABLE [dbo].[eHubQuarantine] (
    [EQ_PK]              UNIQUEIDENTIFIER NOT NULL,
    [EQ_InsertUTC]       DATETIME         NOT NULL,
    [EQ_TrackingId]      UNIQUEIDENTIFIER NULL,
    [EQ_ClientId]        VARCHAR (36)     NULL,
    [EQ_IsProduction]    BIT              NULL,
    [EQ_ApplicationCode] VARCHAR (3)      NULL,
    [EQ_MessageType]     VARCHAR (200)    NULL,
    [EQ_MessageText]     NVARCHAR (MAX)   NULL
);

