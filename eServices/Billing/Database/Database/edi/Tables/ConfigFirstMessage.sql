create table edi.ConfigFirstMessage
(
	FM_Category varchar(3) not null,
	FM_PriceItemCode varchar(3) not null,
	FM_Days int not null default (180),
	[FM_IncludeRef1] BIT NOT NULL DEFAULT 0, 
    [FM_IncludeRef2] BIT NOT NULL DEFAULT 0, 
    [FM_IncludeRef3] BIT NOT NULL DEFAULT 0, 
    [FM_IncludeRef4] BIT NOT NULL DEFAULT 0, 
    [FM_IncludeRef5] BIT NOT NULL DEFAULT 0,
	[FM_IncludeCompanyNumber] BIT NOT NULL DEFAULT 1, 
    constraint PK_ConfigFirstMessage PRIMARY KEY CLUSTERED (FM_Category, FM_PriceItemCode),
	CONSTRAINT [Constraint_FM_Days] CHECK (FM_Days >= 30)
)
