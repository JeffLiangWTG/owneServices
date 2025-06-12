CREATE TABLE [dbo].[ELKResubmitTransaction]
(
	[RT_PK] UNIQUEIDENTIFIER NOT NULL, 
    [RT_JsonData] VARCHAR(MAX) NOT NULL, 
    [RT_SystemCreateTimeUtc] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_UX__RT_PK] PRIMARY KEY NONCLUSTERED ([RT_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF),
)

GO 
CREATE CLUSTERED INDEX IX_ELKResubmitTransaction_SystemCreateTimeUtc ON [dbo].[ELKResubmitTransaction] ([RT_SystemCreateTimeUtc]);  