CREATE TABLE [RefAirlineProductCodeCommodityCodePivot]
(
	[RPC_PK] UNIQUEIDENTIFIER NOT NULL,
	[RPC_AirlineID] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefAirlineProductCodeCommodityCodePivot_RPC_AirlineID] DEFAULT '',
	[RPC_RAR] UNIQUEIDENTIFIER NOT NULL,
	[RPC_RAC] UNIQUEIDENTIFIER NOT NULL,
	[RPC_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_RPC_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[RPC_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_RPC_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([RPC_SysStartTime], [RPC_SysEndTime]), 

	CONSTRAINT [PK_RefAirlineProductCodeCommodityCodePivot] PRIMARY KEY NONCLUSTERED ([RPC_PK] ASC),
	CONSTRAINT [FK_RefAirlineProductCodeCommodityCodePivot_RefAirlineProductCode] FOREIGN KEY (RPC_RAR) REFERENCES RefAirlineProductCode (RAR_PK),
	CONSTRAINT [FK_RefAirlineProductCodeCommodityCodePivot_RefAirlineCommodityCode] FOREIGN KEY (RPC_RAC) REFERENCES RefAirlineCommodityCode (RAC_PK)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefAirlineProductCodeCommodityCodePivotHistory))
GO
CREATE UNIQUE CLUSTERED INDEX [IX_RefAirlineProductCodeCommodityCodePivot_RPC_AirlineID_RPC_RAR_RPC_RAC] ON RefAirlineProductCodeCommodityCodePivot (RPC_AirlineID ASC, RPC_RAR ASC, RPC_RAC ASC)
GO
CREATE NONCLUSTERED INDEX [IX_RefAirlineProductCodeCommodityCodePivot_RPC_RAR] ON RefAirlineProductCodeCommodityCodePivot (RPC_RAR ASC)
GO
CREATE NONCLUSTERED INDEX [IX_RefAirlineProductCodeCommodityCodePivot_RPC_RAC] ON RefAirlineProductCodeCommodityCodePivot (RPC_RAC ASC)
GO
ALTER TABLE RefAirlineProductCodeCommodityCodePivot SET (LOCK_ESCALATION = DISABLE);
