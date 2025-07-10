CREATE TABLE [RefAirlineProductCodeCommodityCodePivot]
(
	[RPC_PK] UNIQUEIDENTIFIER NOT NULL,
	[RPC_AirlineID] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefAirlineProductCodeCommodityCodePivot_RPC_AirlineID] DEFAULT '',
	[RPC_RAR] UNIQUEIDENTIFIER NOT NULL,
	[RPC_RAC_NKCode] VARCHAR(20) NOT NULL,
	[RPC_RAC_NKAirlineID] VARCHAR(3) NOT NULL,

	CONSTRAINT [PK_RefAirlineProductCodeCommodityCodePivot] PRIMARY KEY NONCLUSTERED ([RPC_PK] ASC),
	CONSTRAINT [FK_RefAirlineProductCodeCommodityCodePivot_RefAirlineProductCode] FOREIGN KEY (RPC_RAR) REFERENCES RefAirlineProductCode (RAR_PK),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefAirlineProductCodeCommodityCodePivot_RPC_RAR] ON RefAirlineProductCodeCommodityCodePivot (RPC_RAR ASC)
GO
