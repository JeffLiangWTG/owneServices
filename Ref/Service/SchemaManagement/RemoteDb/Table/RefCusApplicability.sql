CREATE TABLE RefCusApplicability
(
	[ZZT_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusApplicability_ZZT_PK DEFAULT NEWID(),
	[ZZT_ZZ2_Rate] UNIQUEIDENTIFIER,
	[ZZT_ZX1_Conditions] UNIQUEIDENTIFIER,
	[ZZT_ZY2_AdditionalCode] UNIQUEIDENTIFIER,
	[ZZT_ZZH_TariffRelationship] UNIQUEIDENTIFIER SPARSE NULL,
	[ZZT_StartDate] SMALLDATETIME NOT NULL,
	[ZZT_EndDate] SMALLDATETIME NOT NULL,
	[ZZT_ZZA_TradeGroup] UNIQUEIDENTIFIER,
	[ZZT_AdditionalCode] NVARCHAR(15) NOT NULL CONSTRAINT DF_RefCusApplicability_ZZT_AdditionalCode DEFAULT '',
	[ZZT_OrderNumber] NVARCHAR(15) NOT NULL CONSTRAINT DF_RefCusApplicability_ZZT_OrderNumber DEFAULT '',
	[ZZT_ZZA_SecondTradeGroup] UNIQUEIDENTIFIER,
	CONSTRAINT PK_RefCusApplicability PRIMARY KEY CLUSTERED( ZZT_PK ASC ),
	CONSTRAINT CK_RefCusApplicability_ZZT_StartDate_ZZT_EndDate CHECK (ZZT_StartDate < ZZT_EndDate),
	CONSTRAINT CK_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_ZX1_Conditions_ZZT_ZY2_AdditionalCode_ZZT_ZZH_TariffRelationship CHECK ((ZZT_ZX1_Conditions is not null and ZZT_ZZ2_Rate is null and ZZT_ZY2_AdditionalCode is null and ZZT_ZZH_TariffRelationship is null) or (ZZT_ZZ2_Rate is not null and ZZT_ZX1_Conditions is null and ZZT_ZY2_AdditionalCode is null and ZZT_ZZH_TariffRelationship is null) or (ZZT_ZY2_AdditionalCode is not null and ZZT_ZX1_Conditions is null and ZZT_ZZ2_Rate is null and ZZT_ZZH_TariffRelationship is null) or (ZZT_ZZH_TariffRelationship is not null and ZZT_ZX1_Conditions is null and ZZT_ZZ2_Rate is null and ZZT_ZY2_AdditionalCode is null)),
	CONSTRAINT CK_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_AdditionalCode_ZZT_OrderNumber CHECK (ZZT_ZY2_AdditionalCode is null or (ZZT_ZY2_AdditionalCode is not null and ZZT_AdditionalCode = '' and ZZT_OrderNumber = '')),

	CONSTRAINT FK_RefCusApplicability_RefCusRate FOREIGN KEY(ZZT_ZZ2_Rate) REFERENCES RefCusRate (ZZ2_PK),
	CONSTRAINT FK_RefCusApplicability_RefCusCondition FOREIGN KEY(ZZT_ZX1_Conditions) REFERENCES RefCusCondition (ZX1_PK),
	CONSTRAINT FK_RefCusApplicability_RefCusTradeGroup FOREIGN KEY(ZZT_ZZA_TradeGroup) REFERENCES RefCusTradeGroup (ZZA_PK),
	CONSTRAINT FK_RefCusApplicability_RefCusTariffAdditionalCode FOREIGN KEY(ZZT_ZY2_AdditionalCode) REFERENCES RefCusTariffAdditionalCode (ZY2_PK),
	CONSTRAINT FK_RefCusApplicability_RefCusTariffRelationship FOREIGN KEY(ZZT_ZZH_TariffRelationship) REFERENCES RefCusTariffRelationship (ZZH_PK),
	CONSTRAINT FK_RefCusApplicability_RefCusTradeGroup2 FOREIGN KEY(ZZT_ZZA_SecondTradeGroup) REFERENCES RefCusTradeGroup (ZZA_PK),
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_Rate_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup ON RefCusApplicability (ZZT_ZZ2_Rate, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZA_SecondTradeGroup) INCLUDE ([ZZT_EndDate]) WHERE ZZT_ZZ2_Rate IS NOT NULL
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_Conditions_StartDate_TradeGroup_AdditionalCode_OrderNumber_SecondTradeGroup ON RefCusApplicability (ZZT_ZX1_Conditions, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZA_SecondTradeGroup) INCLUDE ([ZZT_EndDate])  WHERE ZZT_ZX1_Conditions IS NOT NULL
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_StartDate_ZZT_ZZA_TradeGroup_ZZT_ZZA_SecondTradeGroup ON RefCusApplicability (ZZT_ZY2_AdditionalCode, ZZT_StartDate, ZZT_ZZA_TradeGroup, ZZT_ZZA_SecondTradeGroup) WHERE ZZT_ZY2_AdditionalCode IS NOT NULL
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_ZZH_TariffRelationship_ZZT_StartDate_ZZT_ZZA_TradeGroup ON RefCusApplicability (ZZT_ZZH_TariffRelationship, ZZT_StartDate, ZZT_ZZA_TradeGroup) WHERE ZZT_ZZH_TariffRelationship IS NOT NULL
GO
CREATE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_OrderNumber ON RefCusApplicability(ZZT_OrderNumber) WHERE ZZT_OrderNumber <> ''
GO
CREATE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_ZZA_TradeGroup ON RefCusApplicability(ZZT_ZZA_TradeGroup) WHERE ZZT_ZZA_TradeGroup IS NOT NULL
GO
CREATE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_AdditionalCode ON RefCusApplicability(ZZT_AdditionalCode) WHERE ZZT_AdditionalCode <> ''
GO
CREATE NONCLUSTERED INDEX IX_RefCusApplicability_ZZT_ZZA_SecondTradeGroup ON RefCusApplicability(ZZT_ZZA_SecondTradeGroup) WHERE ZZT_ZZA_SecondTradeGroup IS NOT NULL
GO
ALTER TABLE RefCusApplicability SET (LOCK_ESCALATION = DISABLE);
