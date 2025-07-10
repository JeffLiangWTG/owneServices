CREATE TABLE [RefSysConfigType]
(
[ZRT_PK] [UNIQUEIDENTIFIER] NOT NULL,
[ZRT_ConfigCode] [VARCHAR](10) NOT NULL CONSTRAINT [DF_RefSysConfigType_ZRT_ConfigCode] DEFAULT '',
[ZRT_Description] [VARCHAR](100) NOT NULL CONSTRAINT [DF_RefSysConfigType_ZRT_Description] DEFAULT '',
[ZRT_LongDescription] [VARCHAR](500) NOT NULL CONSTRAINT [DF_RefSysConfigType_ZRT_LongDescription] DEFAULT '',

CONSTRAINT [PK_RefSysConfigType] PRIMARY KEY ([ZRT_PK]),
CONSTRAINT [Constraint_ZRT_ConfigCode] CHECK ([ZRT_ConfigCode] <> '' AND datalength([ZRT_ConfigCode]) >= (3)),
CONSTRAINT [Constraint_ZRT_Description] CHECK  ([ZRT_Description] <> ''),
CONSTRAINT [Constraint_ZRT_LongDescription] CHECK  ([ZRT_LongDescription] <> '')
)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefSysConfigType_ZRT_ConfigCode] ON [RefSysConfigType] ([ZRT_ConfigCode] ASC)
GO
