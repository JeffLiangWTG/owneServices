CREATE TABLE [RefLanguageText]
(
	RLT_PK uniqueidentifier NOT NULL,
	RLT_ParentId uniqueidentifier NOT NULL,
	RLT_ParentTableCode varchar(4) NOT NULL,
	RLT_Language VARCHAR(7) NOT NULL,
	RLT_ColumnName varchar(50) NOT NULL,
	RLT_Text nvarchar(1000) NOT NULL,
	CONSTRAINT [PK_UX__RLT_PK] PRIMARY KEY CLUSTERED ([RLT_PK] ASC),
	CONSTRAINT CK_RefLanguageText_RLT_ParentTableCode CHECK (RLT_ParentTableCode = 'RN' OR RLT_ParentTableCode = 'RW' OR RLT_ParentTableCode = 'RX')
)
GO
