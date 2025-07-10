CREATE TABLE RefCusApplicability
(
 ZZT_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusApplicability_ZZT_PK DEFAULT NEWID(),
 ZZT_ZZ2_Rate UNIQUEIDENTIFIER,
 ZZT_ZX1_Conditions UNIQUEIDENTIFIER,
 ZZT_ZY2_AdditionalCode UNIQUEIDENTIFIER,
 ZZT_ZZH_TariffRelationship UNIQUEIDENTIFIER,
 ZZT_StartDate SMALLDATETIME NOT NULL,
 ZZT_EndDate SMALLDATETIME NOT NULL,
 ZZT_ZZA_NKTradeGroup VARCHAR(35) NULL,
 ZZT_ZZA_ZZZ_NKDataGrouping VARCHAR(3) NULL,
 ZZT_ZZA_NKSecondTradeGroup VARCHAR(35) NULL,
 ZZT_ZZA_ZZZ_NKSecondDataGrouping VARCHAR(3) NULL,
 ZZT_AdditionalCode NVARCHAR(15) NOT NULL CONSTRAINT DF_RefCusApplicability_ZZT_AdditionalCode DEFAULT '',
 ZZT_OrderNumber NVARCHAR(15) NOT NULL CONSTRAINT DF_RefCusApplicability_ZZT_OrderNumber DEFAULT '',

 CONSTRAINT PK_RefCusApplicability PRIMARY KEY CLUSTERED( ZZT_PK ASC ),
 CONSTRAINT CK_RefCusApplicability_ZZT_StartDate_ZZT_EndDate CHECK (ZZT_StartDate <= ZZT_EndDate),
 CONSTRAINT CK_RefCusApplicability_ZZT_ZZ2_Rate_ZZT_ZX1_Conditions_ZZT_ZY2_AdditionalCode CHECK ((ZZT_ZX1_Conditions IS NOT NULL AND ZZT_ZZ2_Rate IS NULL AND ZZT_ZY2_AdditionalCode IS NULL) OR (ZZT_ZZ2_Rate IS NOT NULL AND ZZT_ZX1_Conditions IS NULL AND ZZT_ZY2_AdditionalCode IS NULL) OR (ZZT_ZY2_AdditionalCode IS NOT NULL AND ZZT_ZX1_Conditions IS NULL AND ZZT_ZZ2_Rate IS NULL)),
 CONSTRAINT CK_RefCusApplicability_ZZT_ZY2_AdditionalCode_ZZT_AdditionalCode_ZZT_OrderNumber CHECK (ZZT_ZY2_AdditionalCode IS NULL OR (ZZT_ZY2_AdditionalCode IS NOT NULL AND ZZT_AdditionalCode = '' AND ZZT_OrderNumber = '')),

 CONSTRAINT FK_RefCusApplicability_RefCusRate FOREIGN KEY(ZZT_ZZ2_Rate) REFERENCES RefCusRate (ZZ2_PK),
 CONSTRAINT FK_RefCusApplicability_RefCusCondition FOREIGN KEY(ZZT_ZX1_Conditions) REFERENCES RefCusCondition (ZX1_PK),
 CONSTRAINT FK_RefCusApplicability_RefCusTariffAdditionalCode FOREIGN KEY(ZZT_ZY2_AdditionalCode) REFERENCES RefCusTariffAdditionalCode (ZY2_PK),
 CONSTRAINT FK_RefCusApplicability_RefCusTariffRelationship FOREIGN KEY(ZZT_ZZH_TariffRelationship) REFERENCES RefCusTariffRelationship (ZZH_PK),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusApplicability_ZZT_ZZ2_Rate] ON [RefCusApplicability] ([ZZT_ZZ2_Rate])
GO
CREATE NONCLUSTERED INDEX [IX_RefCusApplicability_ZZT_ZX1_Conditions] ON [RefCusApplicability] ([ZZT_ZX1_Conditions])
GO
CREATE NONCLUSTERED INDEX [IX_RefCusApplicability_ZZT_ZY2_AdditionalCode] ON [RefCusApplicability] (ZZT_ZY2_AdditionalCode)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusApplicability_ZZT_ZZH_TariffRelationship] ON [RefCusApplicability] (ZZT_ZZH_TariffRelationship)
GO
