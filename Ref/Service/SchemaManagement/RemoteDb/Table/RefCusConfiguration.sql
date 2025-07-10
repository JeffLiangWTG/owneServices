CREATE TABLE [RefCusConfiguration]
(
	[ZZJ_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusConfiguration_ZZJ_PK] DEFAULT (NEWID()),
	[ZZJ_RN_NKCustomsCountry] CHAR(2) NOT NULL,
	[ZZJ_TariffDataSource] CHAR(3) NOT NULL,
	[ZZJ_IsGenericCountry] BIT NOT NULL CONSTRAINT [DF_RefCusConfiguration_ZZJ_IsGenericCountry]  DEFAULT (0),
	[ZZJ_AllowRiskManagement] BIT NOT NULL CONSTRAINT [DF_RefCusConfiguration_ZZJ_AllowRiskManagement]  DEFAULT (0),
	[ZZJ_IsTransitDeclarationCounty] BIT NOT NULL CONSTRAINT [DF_RefCusConfiguration_ZZJ_IsTransitDeclarationCounty]  DEFAULT (0),
	[ZZJ_TurnOnASYDCUDAManifest] BIT NOT NULL CONSTRAINT [DF_RefCusConfiguration_ZZJ_TurnOnASYDCUDAManifest]  DEFAULT (0),
	[ZZJ_TurnOnASYCUDACustoms] BIT NOT NULL CONSTRAINT [DF_RefCusConfiguration_ZZJ_TurnOnASYCUDACustoms]  DEFAULT (0),
	[ZZJ_ZZZ_NKDefaultDataGrouping] VARCHAR(3) NOT NULL,
	[ZZJ_ZZZ_NKAlternateTariffOnlyDataGrouping] VARCHAR(3) NULL,
	[ZZJ_StartDate] DATE NOT NULL CONSTRAINT [DF_RefCusConfiguration_ZZJ_StartDate] DEFAULT GetUtcDate(),
	[ZZJ_EndDate] DATE NULL,
	CONSTRAINT [PK_RefCusConfiguration] PRIMARY KEY NONCLUSTERED ([ZZJ_PK] ASC),
	CONSTRAINT [CK_RefCusConfiguration_ZZJ_RN_NKCustomsCountry] CHECK (LEN([ZZJ_RN_NKCustomsCountry]) = 2),
	CONSTRAINT [CK_RefCusConfiguration_ZZJ_TariffDataSource] CHECK ([ZZJ_TariffDataSource] = 'WTG' OR [ZZJ_TariffDataSource] = 'OWN'),
	CONSTRAINT [CK_RefCusConfiguration_ZZJ_ZZZ_NKAlternateTariffOnlyDataGrouping] CHECK ([ZZJ_TariffDataSource] <> 'OWN' OR [ZZJ_ZZZ_NKAlternateTariffOnlyDataGrouping] IS NOT NULL),
	CONSTRAINT [FK_RefCusConfiguration_RefDataGrouping] FOREIGN KEY ([ZZJ_ZZZ_NKDefaultDataGrouping]) REFERENCES [RefDataGrouping]([ZZZ_DataGrouping]),
	CONSTRAINT [FK_RefCusConfiguration_RefDataGrouping_AlternateTariffOnlyDataGrouping] FOREIGN KEY ([ZZJ_ZZZ_NKAlternateTariffOnlyDataGrouping]) REFERENCES [RefDataGrouping]([ZZZ_DataGrouping]),
	CONSTRAINT [CK_RefCusConfiguration_ZZJ_EndDate] CHECK ([ZZJ_EndDate] IS NULL OR [ZZJ_EndDate]>=[ZZJ_StartDate])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusConfiguration_ZZJ_RN_NKCustomsCountry_ZZJ_StartDate] ON [RefCusConfiguration] ([ZZJ_RN_NKCustomsCountry] ASC, [ZZJ_StartDate] ASC)
GO
