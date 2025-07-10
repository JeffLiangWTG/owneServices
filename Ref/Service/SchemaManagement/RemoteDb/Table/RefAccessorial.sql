CREATE TABLE [dbo].[RefAccessorial]
(
	[ASI_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefAccessorial_ASI_PK DEFAULT (NEWID()),
    [ASI_Code] CHAR(3) NOT NULL CONSTRAINT DF_RefAccessorial_ASI_Code DEFAULT (''), 
    [ASI_Description] VARCHAR(50) NOT NULL CONSTRAINT DF_RefAccessorial_ASI_Description DEFAULT (''), 
    CONSTRAINT [PK_RefAccessorial] PRIMARY KEY NONCLUSTERED ([ASI_PK]), 
    CONSTRAINT [CK_RefAccessorial_ASI_Code] CHECK (LEN([ASI_Code])=(3)), 
    CONSTRAINT [CK_RefAccessorial_ASI_Description] CHECK ([ASI_Description]<>'') 
)
GO
CREATE UNIQUE CLUSTERED INDEX [IX_RefAccessorial_ASI_Code] ON [dbo].[RefAccessorial] ([ASI_Code])
