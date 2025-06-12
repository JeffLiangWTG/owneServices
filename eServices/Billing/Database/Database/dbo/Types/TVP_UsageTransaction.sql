CREATE TYPE [dbo].[TVP_UsageTransaction] AS TABLE
(
    [RT_PK]       UNIQUEIDENTIFIER NOT NULL,
    [RT_JsonData] VARCHAR(MAX)     NOT NULL
)
