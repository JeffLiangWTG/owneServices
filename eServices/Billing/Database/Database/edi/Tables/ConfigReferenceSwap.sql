CREATE TABLE [edi].[ConfigReferenceSwap]
(
	Category VARCHAR (3) NOT NULL,
	PriceItemCode VARCHAR (3) NOT NULL,
	FirstVersion int not null default(0),
	LastVersion int null,
	RefIndex1 tinyint not null, 
	RefIndex2 tinyint not null, 
	RefIndex3 tinyint not null, 
	RefIndex4 tinyint not null, 
	RefIndex5 tinyint not null,
	CONSTRAINT [Constraint_RefIndex1] CHECK (RefIndex1 in (1, 2, 3, 4, 5)),
	CONSTRAINT [Constraint_RefIndex2] CHECK (RefIndex2 in (1, 2, 3, 4, 5)),
	CONSTRAINT [Constraint_RefIndex3] CHECK (RefIndex3 in (1, 2, 3, 4, 5)),
	CONSTRAINT [Constraint_RefIndex4] CHECK (RefIndex4 in (1, 2, 3, 4, 5)),
	CONSTRAINT [Constraint_RefIndex5] CHECK (RefIndex5 in (1, 2, 3, 4, 5)),
	CONSTRAINT [Constraint_FirstVersion_LastVersion] CHECK (FirstVersion <= LastVersion),
	CONSTRAINT [Constraint_RefIndexUnique] CHECK (
			RefIndex1 != RefIndex2
		and RefIndex1 != RefIndex3
		and RefIndex1 != RefIndex4
		and RefIndex1 != RefIndex5
		and RefIndex2 != RefIndex3
		and RefIndex2 != RefIndex4
		and RefIndex2 != RefIndex5
		and RefIndex3 != RefIndex4
		and RefIndex3 != RefIndex5
		and RefIndex4 != RefIndex5
	)
)

GO

create unique clustered index IX_ConfigReferenceSwap_FirstVersion on edi.ConfigReferenceSwap (Category, PriceItemCode, FirstVersion);
go
create unique index IX_ConfigReferenceSwap_LastVersion on edi.ConfigReferenceSwap (Category, PriceItemCode, LastVersion);
