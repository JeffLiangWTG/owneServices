CREATE TABLE RefDataGrouping
(
	ZZZ_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefDataGrouping_ZZZ_PK DEFAULT (NEWID()),
	ZZZ_DataGrouping VARCHAR(3) NOT NULL,
	ZZZ_Description VARCHAR(500) NOT NULL,
	ZZZ_ZZZ_Grouping UNIQUEIDENTIFIER NULL,
	CONSTRAINT PK_RefDataGrouping PRIMARY KEY CLUSTERED( ZZZ_PK ASC ),
	CONSTRAINT CK_RefDataGrouping_ZZZ_DataGrouping CHECK (ZZZ_DataGrouping <>''),
	CONSTRAINT CK_RefDataGrouping_ZZZ_Description CHECK (ZZZ_Description <>''),
	CONSTRAINT FK_RefDataGrouping_RefDataGrouping FOREIGN KEY (ZZZ_ZZZ_Grouping) REFERENCES RefDataGrouping (ZZZ_PK)
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefDataGrouping_ZZZ_DataGrouping ON RefDataGrouping (ZZZ_DataGrouping)
GO
CREATE NONCLUSTERED INDEX IX_RefDataGrouping_ZZZ_ZZZ_Grouping ON RefDataGrouping (ZZZ_ZZZ_Grouping)
GO
