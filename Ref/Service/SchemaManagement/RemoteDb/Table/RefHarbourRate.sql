CREATE TABLE RefHarbourRate(
[ZXF_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefHarbourRate_ZXF_PK] DEFAULT (NEWID()),
[ZXF_Type] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefHarbourRate_ZXF_Type] DEFAULT '',
[ZXF_Port] VARCHAR(5) NOT NULL CONSTRAINT [DF_RefHarbourRate_ZXF_Port] DEFAULT '',
[ZXF_Mode] VARCHAR(4) NOT NULL,
[ZXF_Commodity] VARCHAR(4) NOT NULL CONSTRAINT [DF_RefHarbourRate_ZXF_Commodity] DEFAULT '',
[ZXF_PortTaxType] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefHarbourRate_ZXF_PortTaxType] DEFAULT '',
[ZXF_StartDate] DATE NOT NULL CONSTRAINT [DF_RefHarbourRate_ZXF_StartDate] DEFAULT GetUtcDate(),
[ZXF_EndDate] DATE NOT NULL CONSTRAINT [DF_RefHarbourRate_ZXF_EndDate] DEFAULT '2079-06-06 23:59',
[ZXF_RateFormula] VARCHAR(300) NOT NULL CONSTRAINT [DF_RefHarbourRate_ZXF_RateFormula] DEFAULT '',
[ZXF_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,

CONSTRAINT [PK_RefHarbourRate] PRIMARY KEY CLUSTERED( [ZXF_PK] ASC ),
CONSTRAINT [FK_RefHarbourRate_RefDataGrouping] FOREIGN KEY([ZXF_ZZZ_NKDataGrouping]) REFERENCES [RefDataGrouping] ([ZZZ_DataGrouping]),
CONSTRAINT [CK_RefHarbourRate_ZXF_Mode] CHECK ([ZXF_Mode]='CON' OR [ZXF_Mode]='EMP' OR [ZXF_Mode]='BBK' OR [ZXF_Mode]='BLK' OR [ZXF_Mode]='LIQ' OR [ZXF_Mode]='ALL'),
CONSTRAINT [CK_RefHarbourRate_ZXF_PortTaxType] CHECK (LEN([ZXF_PortTaxType])=0 OR LEN([ZXF_PortTaxType])=3)
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefHarbourRate_ZXF_Type_ZXF_Port_ZXF_Mode_ZXF_Commodity_ZXF_StartDate ON RefHarbourRate (ZXF_Type, ZXF_Port, ZXF_Mode, ZXF_Commodity, ZXF_StartDate)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefHarbourRate_ZXF_Type_ZXF_Port_ZXF_Mode_ZXF_Commodity_ZXF_EndDate ON RefHarbourRate (ZXF_Type, ZXF_Port, ZXF_Mode, ZXF_Commodity, ZXF_EndDate)
GO
CREATE NONCLUSTERED INDEX IX_RefHarbourRate_ZXF_ZZZ_NKDataGrouping ON RefHarbourRate (ZXF_ZZZ_NKDataGrouping)
GO
