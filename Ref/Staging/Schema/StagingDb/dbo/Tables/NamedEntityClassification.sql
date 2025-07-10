CREATE TABLE NamedEntityClassification
(
	NEC_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_NamedEntityClassification_NEC_PK DEFAULT NEWID(),
	NEC_Name NVARCHAR(max) NOT NULL,
	NEC_Class VARCHAR(50) NOT NULL,
	NEC_Language VARCHAR(7) NOT NULL,
	NEC_Code VARCHAR(20) NOT NULL,
	CONSTRAINT PK_NamedEntityClassification PRIMARY KEY CLUSTERED (NEC_PK),
	CONSTRAINT CK_NamedEntityClassification_NEC_Name CHECK (NEC_Name <> ''),
	CONSTRAINT CK_NamedEntityClassification_NEC_Class CHECK (NEC_Class <> ''),
	CONSTRAINT CK_NamedEntityClassification_NEC_Language CHECK (NEC_Language <> ''),
	CONSTRAINT CK_NamedEntityClassification_NEC_Code CHECK (NEC_Code <> ''),
)
GO
CREATE NONCLUSTERED INDEX [IX_NamedEntityClassification_NEC_Language_NEC_Class] ON NamedEntityClassification (NEC_Language, NEC_Class)
