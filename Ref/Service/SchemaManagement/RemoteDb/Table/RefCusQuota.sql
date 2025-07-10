CREATE TABLE RefCusQuota
(
	[ZXQ_PK] UNIQUEIDENTIFIER NOT NULL,
	[ZXQ_OrderNumber] VARCHAR(6) NOT NULL CONSTRAINT [DF_RefCusQuota_ZXQ_OrderNumber] DEFAULT '',
	[ZXQ_InitialAmount] DECIMAL(19,5) NOT NULL CONSTRAINT [DF_RefCusQuota_ZXQ_InitialAmount] DEFAULT 0,
	[ZXQ_UnitOfMeasure] VARCHAR(6) NOT NULL CONSTRAINT [DF_RefCusQuota_ZXQ_UnitOfMeasure] DEFAULT '',
	[ZXQ_Balance] DECIMAL(19,5) NOT NULL CONSTRAINT [DF_RefCusQuota_ZXQ_Balance] DEFAULT 0,
	[ZXQ_StartDate] DATETIME NOT NULL,
	[ZXQ_EndDate] DATETIME NOT NULL,
	[ZXQ_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusQuota_ZXQ_ZZZ_NKDataGrouping] DEFAULT '',
	CONSTRAINT [PK_RefCusQuota] PRIMARY KEY CLUSTERED ([ZXQ_PK]),
	CONSTRAINT [FK_RefCusQuota_RefDataGrouping] FOREIGN KEY([ZXQ_ZZZ_NKDataGrouping]) REFERENCES [RefDataGrouping] ([ZZZ_DataGrouping]),
	CONSTRAINT [CK_RefCusQuota_ZXQ_OrderNumber] CHECK ([ZXQ_OrderNumber] <> ''),
	CONSTRAINT [CK_RefCusQuota_ZXQ_InitialAmount] CHECK ([ZXQ_InitialAmount] >= 0),
	CONSTRAINT [CK_RefCusQuota_ZXQ_UnitOfMeasure] CHECK ([ZXQ_UnitOfMeasure] <> ''),
	CONSTRAINT [CK_RefCusQuota_ZXQ_Balance] CHECK ([ZXQ_Balance] >= 0),
	CONSTRAINT [CK_RefCusQuota_ZXQ_EndDate] CHECK ([ZXQ_EndDate] >= [ZXQ_StartDate])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusQuota_ZXQ_ZZZ_NKDataGrouping_ZXQ_OrderNumber_ZXQ_StartDate] ON [RefCusQuota] ([ZXQ_ZZZ_NKDataGrouping], [ZXQ_OrderNumber], [ZXQ_StartDate])
GO
