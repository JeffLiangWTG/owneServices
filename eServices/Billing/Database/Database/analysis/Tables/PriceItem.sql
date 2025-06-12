CREATE TABLE [analysis].[PriceItem] (
    [Category]        VARCHAR (3)   NOT NULL,
    [PriceItemCode]   VARCHAR (3)   NOT NULL,
    [Role]            VARCHAR (60)  NOT NULL,
    [Module]          VARCHAR (60)  NOT NULL,
    [Function]        VARCHAR (60)  NOT NULL,
    [Feature]         VARCHAR (120) NOT NULL,
    [UnitOfMeasure]   VARCHAR (30)  NULL,
    [IsSemiAggregate] BIT           DEFAULT ((0)) NULL,
    [ReportingSource] VARCHAR (3)   NOT NULL
);
GO

CREATE UNIQUE CLUSTERED INDEX [UX_AnalysisPriceItem] ON [analysis].[PriceItem] (
	[Category],
	[PriceItemCode]
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_AnalysisPriceItemFeature] ON [analysis].[PriceItem] (
	[Feature]
);
GO