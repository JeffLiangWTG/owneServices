CREATE TABLE RefCusTariffNationalCode
(
	 [ZZW_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusTariffNationalCode_ZZW_PK] DEFAULT NEWID(),
	 [ZZW_ZZ1_Tariff] UNIQUEIDENTIFIER NOT NULL,
	 [ZZW_NationalCode] VARCHAR(10) NOT NULL CONSTRAINT [DF_RefCusTariffNationalCode_ZZW_NationalCode] DEFAULT '',
	 [ZZW_Description] NVARCHAR(4000) NOT NULL CONSTRAINT DF_RefCusTariffNationalCode_ZZW_Description DEFAULT '',
	 [ZZW_ZZF_NKTaxOrFeeCode] VARCHAR(4) NOT NULL,
	 [ZZW_StartDate] DATETIME NOT NULL CONSTRAINT [DF_RefCusTariffNationalCode_ZZW_StartDate] DEFAULT GetUtcDate(),
	 [ZZW_EndDate] DATETIME NOT NULL CONSTRAINT [DF_RefCusTariffNationalCode_ZZW_EndDate] DEFAULT '2079-06-06 23:59',
	 [ZZW_PublishedDate] DATE NULL,
	 [ZZW_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,

	 CONSTRAINT [PK_RefCusTariffNationalCode] PRIMARY KEY NONCLUSTERED( [ZZW_PK] ASC ),
	 CONSTRAINT [FK_RefCusTariffNationalCode_RefCusTariff] FOREIGN KEY([ZZW_ZZ1_Tariff]) REFERENCES [RefCusTariff] ([ZZ1_PK]),
	 CONSTRAINT [FK_RefCusTariffNationalCode_RefDataGrouping] FOREIGN KEY([ZZW_ZZZ_NKDataGrouping]) REFERENCES [RefDataGrouping] ([ZZZ_DataGrouping]),
	 CONSTRAINT [CK_RefCusTariffNationalCode_ZZW_StartDate_ZZW_EndDate] CHECK ([ZZW_StartDate]<=[ZZW_EndDate]),
	 CONSTRAINT [CK_RefCusTariffNationalCode_ZZW_NationalCode] CHECK ([ZZW_NationalCode] <> '')
)
GO
CREATE CLUSTERED INDEX IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_PK ON RefCusTariffNationalCode (ZZW_ZZZ_NKDataGrouping ASC, ZZW_PK ASC)
GO
CREATE NONCLUSTERED INDEX IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_PublishedDate ON RefCusTariffNationalCode (ZZW_ZZZ_NKDataGrouping, ZZW_ZZ1_Tariff, ZZW_PublishedDate ASC)
GO
CREATE NONCLUSTERED INDEX IX_RefCusTariffNationalCode_ZZW_ZZ1_Tariff ON RefCusTariffNationalCode (ZZW_ZZ1_Tariff)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffNationalCode_ZZW_ZZZ_NKDataGrouping_ZZW_ZZ1_Tariff_ZZW_StartDate_ZZW_ZZF_NKTaxOrFeeCode_ZZW_NationalCode ON RefCusTariffNationalCode (ZZW_ZZZ_NKDataGrouping ASC, ZZW_ZZ1_Tariff ASC, ZZW_StartDate ASC, ZZW_ZZF_NKTaxOrFeeCode ASC, ZZW_NationalCode ASC)
GO
ALTER TABLE RefCusTariffNationalCode SET (LOCK_ESCALATION = DISABLE);
