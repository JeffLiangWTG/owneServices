CREATE TABLE RefAirlineProductCodeCommodityCodePivot
(
	[RPC_PK] UNIQUEIDENTIFIER NOT NULL,
	[RPC_AirlineID] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefAirlineProductCodeCommodityCodePivot_RPC_AirlineID] DEFAULT '',
	[RPC_RAR] UNIQUEIDENTIFIER NOT NULL,
	[RPC_RAC] UNIQUEIDENTIFIER NOT NULL

	CONSTRAINT [PK_RefAirlineProductCodeCommodityCodePivot] PRIMARY KEY NONCLUSTERED ([RPC_PK] ASC),
	CONSTRAINT [FK_RefAirlineProductCodeCommodityCodePivot_RefAirlineProductCode] FOREIGN KEY (RPC_RAR) REFERENCES RefAirlineProductCode (RAR_PK),
	CONSTRAINT [FK_RefAirlineProductCodeCommodityCodePivot_RefAirlineCommodityCode] FOREIGN KEY (RPC_RAC) REFERENCES RefAirlineCommodityCode (RAC_PK)
)
GO
CREATE UNIQUE CLUSTERED INDEX [IX_RefAirlineProductCodeCommodityCodePivot_RPC_AirlineID_RPC_RAR_RPC_RAC] ON RefAirlineProductCodeCommodityCodePivot (RPC_AirlineID ASC, RPC_RAR ASC, RPC_RAC ASC)
GO
CREATE NONCLUSTERED INDEX [IX_RefAirlineProductCodeCommodityCodePivot_RPC_RAR] ON RefAirlineProductCodeCommodityCodePivot (RPC_RAR ASC)
GO
CREATE NONCLUSTERED INDEX [IX_RefAirlineProductCodeCommodityCodePivot_RPC_RAC] ON RefAirlineProductCodeCommodityCodePivot (RPC_RAC ASC)
GO
