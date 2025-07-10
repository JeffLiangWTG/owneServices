using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class PopulateDataChangeHistoryForExistingRecordsPart2Fixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZ4'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZ1'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZY5'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZK'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZ5'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZS'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZ6'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZR'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZX'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZY3'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZI'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZX0'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZA'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZZ'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'DOC'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZXF'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'RSL'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'RL'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ADR'"));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new PopulateDataChangeHistoryForExistingRecordsPart2(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"INSERT INTO [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES (N'ed0ce102-2c22-4995-9f99-8d5825517e52', N'AU', N'AU', N'AU')

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES (N'88d5c8ce-6f35-40a6-874b-169c138f4e11', N'LEV', N'Levies', 1, N'AU', N'GBP', N'(CV * 1.15) + 1P1 + 2P1 + 2P2 + 2P3 - 3P1 - 3P2 - 4P1 - 4P2 - 4P3 - 4P4 - 4P5 - 4P6')

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZ9_NKNomenclatureGroupType],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES (N'f0409a45-b2e2-4712-b759-0f1ed9d61ba1', N'COM', N'Controlling Agency Commodity Codes', N'', N'AU', NULL, 0)

INSERT INTO [dbo].[RefCusTariff]([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_PublishedDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (N'5d5f278b-3d2d-4c6d-a83a-00025b069aee', N'f0409a45-b2e2-4712-b759-0f1ed9d61ba1', N'020910', 0, N'OF PIGS', CAST(N'2012-01-01T00:00:00.000' AS DateTime), CAST(N'2079-06-06T23:59:00.000' AS DateTime), CAST(N'2079-06-06' AS Date), N'VAT', N'AU', N'01.02..09.1')

INSERT INTO [dbo].[RefCarrierCode] ([ZZ4_PK],[ZZ4_Code],[ZZ4_Description],[ZZ4_IsSea],[ZZ4_IsRoad],[ZZ4_IsRail],[ZZ4_IsAir],[ZZ4_ZZZ_NKDataGrouping])
VALUES (N'ed15a722-3e98-427c-a4ae-0015a1d1c9c5', N'SCL', N'Sinotrans Chartering Limited', 1, 0, 0, 0, N'AU')

INSERT INTO [dbo].[RefCusNomenclatureGroup] ([ZZ5_PK],[ZZ5_ZZ9_NKNomenclatureGroupType],[ZZ5_Value],[ZZ5_Description],[ZZ5_StartDate],[ZZ5_EndDate],[ZZ5_CompositeKey],[ZZ5_ZZZ_NKDataGrouping])
VALUES (N'489dd3f9-7c7d-479f-89b6-000005a62c8c', N'CDS', N'200960519080', N'Other', CAST(N'1972-01-01T00:00:00' AS SmallDateTime), CAST(N'1994-01-01T00:00:00' AS SmallDateTime), N'200960519080', N'AU')

INSERT INTO RefCusPreference (ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping)
VALUES (N'21e41bf9-3923-46f0-9422-02002216eacf', N'420', N'Claims to first come first served tes', N'AU')

INSERT INTO [dbo].[RefCusProcedure] ([ZZ6_PK],[ZZ6_Category],[ZZ6_ProcedureCode],[ZZ6_PreviousProcedureCode],[ZZ6_Concession],[ZZ6_Description],[ZZ6_ZZZ_NKDataGrouping],[ZZ6_ShipmentType],[ZZ6_CalculateDuty],[ZZ6_Group],[ZZ6_LandedCost],[ZZ6_IntoWarehouse],[ZZ6_OutOfWarehouse],[ZZ6_StartDate],[ZZ6_EndDate],[ZZ6_CalculateVAT])
VALUES (N'0b45164c-ecad-489b-a2a9-000f14bb0ac5', N'01', N'53', N'00', N'008', N'Temporary ms du22', N'AU', N'IMP', 0, N'IFD,ISD', 0, N'N', N'N', CAST(N'1900-01-01T00:00:00' AS SmallDateTime), CAST(N'2079-06-06T23:59:00' AS SmallDateTime), 1)

INSERT INTO [dbo].[RefCusTariffAdditionalCodeCategory] ([ZY3_PK],[ZY3_Category],[ZY3_Description],[ZY3_ZZZ_NKDataGrouping])
VALUES (N'177b97b1-5ff3-47a2-8d8e-02ff8e0fad4e', N'T  ', N'T', N'AU')

INSERT INTO [dbo].[RefCusTradeGroup] (ZZA_PK,ZZA_TradeGroup,ZZA_Description,ZZA_StartDate,ZZA_EndDate,ZZA_ZZZ_NKDataGrouping)
VALUES (N'1ff27e27-462f-4fd4-a296-006c70173819', N'KW', N'Kuwait', CAST(N'1900-01-01T00:00:00' AS SmallDateTime), CAST(N'2079-06-06T00:00:00' AS SmallDateTime), N'AU')

INSERT INTO [dbo].[RefHarbourRate] ([ZXF_PK],[ZXF_Type],[ZXF_Port],[ZXF_Mode],[ZXF_Commodity],[ZXF_StartDate],[ZXF_EndDate],[ZXF_RateFormula],[ZXF_ZZZ_NKDataGrouping])
VALUES (N'741ae00a-97d9-4e5a-b4d2-c836d51b49b1', N't', N't', N'CON', N't', CAST(N'2012-01-01' AS Date), CAST(N'2079-06-06' AS Date), N'1', N'AU')

INSERT INTO [dbo].[RefCusAUNexdocECMCode] ([ZY5_PK],[ZY5_CommodityCode],[ZY5_PreservationCode],[ZY5_ProductTypeCode],[ZY5_PackTypeCode],[ZY5_SupplementaryCode])
VALUES (N'fb1d4fd7-3619-4fa3-acf4-0004f4ce507b', N'D', N'C', N'UHC', N'BI', N'')

INSERT INTO [dbo].[RefCusCodeType] ([ZZK_PK],[ZZK_CodeType],[ZZK_Description],[ZZK_IsReadonly],[ZZK_MaxLength],[ZZK_ZZZ_NKDataGrouping])
VALUES (N'4cdbac72-f70d-4eef-ba23-058b97067f6d', N'NPRCX', N'NEXDOCS PRODUCT CATEGORY OTHER GOODS', 1, 0, N'AU')

INSERT INTO [dbo].[RefCusRuling] ([ZZX_PK],[ZZX_RN_NKCountryCode],[ZZX_RulingNumber],[ZZX_Description],[ZZX_RulingType],[ZZX_StartDate],[ZZX_EndDate])
VALUES (N'11bc4ee1-5f58-40ea-abd3-99e0831c77df', N'AU', N'12', N'TEST', N'TTT', CAST(N'2012-01-01' AS Date), CAST(N'2079-06-06' AS Date))

INSERT [dbo].[RefCusTaxOrFeeType] ([ZX0_PK], [ZX0_TaxOrFeeType], [ZX0_Description])
VALUES (N'd5aab67e-d76d-4a35-95e4-5a8274c6e2a8', 'STH', 'Other')

INSERT INTO [dbo].[RefDocOrgCusCode] ([DOC_PK],[DOC_RN_NKRegulatingCountry],[DOC_RN_NKCodeCountry],[DOC_CodeType],[DOC_DocumentType],[DOC_Priority],[DOC_Notes])
VALUES (N'49e76496-7749-4d49-8798-34367a075306', N'AU', N'AU', N'A  ', N'HAW', 1, N'C')

INSERT INTO [dbo].[RefShippingLine] ([RSL_PK],[RSL_IsActive],[RSL_IsNVO],[RSL_CarrierName],[RSL_StandardCarrierAlphaCode],[RSL_CargoWiseOneCode],[RSL_OceanCarrierMessagingAvailable],[RSL_GlobalSailingScheduleAvailable],[RSL_ContainerAutomationAvailable],[RSL_CargoSphereRatesAvailable],[RSL_InvoiceAvailable])
VALUES (N'75337cb4-a560-4fe7-9cd9-9451cd0e5405', 1, 0, N'Trinity Shipping Line (TSL)', N'TRNH', N'e5df', 0, 0, 0, 0, 0)

INSERT INTO [dbo].[RefUNLOCO] ([RL_PK],[RL_Code],[RL_IsActive],[RL_PortName],[RL_NameWithDiacriticals],[RL_IATA],[RL_CoOrdinates],[RL_RN_NKCountryCode],[RL_IATARegionCode])
VALUES (NEWID(),'TEST1',1,'Test Port','Test','TST','4230N 00131E','AU','AUS')

INSERT INTO [dbo].[UNDGSubstanceADR] ([ADR_PK],[ADR_IsActive],[ADR_UNNO],[ADR_Variant],[ADR_PSN],[ADR_Class],[ADR_ClassificationCode],[ADR_PG],[ADR_Labels],[ADR_SpecialProvisions],[ADR_LQMaxAmt],[ADR_LQMaxAmtUQ],[ADR_LQ2MaxAmt],[ADR_LQ2MaxAmtUQ],[ADR_ExceptedQuantityCode],[ADR_PackIns],[ADR_PackProv],[ADR_MixedPackingProv],[ADR_BulkTankIns],[ADR_BulkTankSpecProv],[ADR_ADRTankCode],[ADR_ADRTankSpecProv],[ADR_TankVehicle],[ADR_TransportCategory],[ADR_PackingSpecialProv],[ADR_BulkSpecialProv],[ADR_LoadingSpecialProv],[ADR_OperationSpecialProv],[ADR_HazardIDNumber])
VALUES (N'e64fc56f-9be1-44a1-a646-00b7f235ff1b', 1, N'1111', N'a', N'TEST', N'1.1', N'asdf', N'', N'', N'', CAST(0.000 AS Decimal(9, 3)), N'', CAST(0.000 AS Decimal(9, 3)), N'', N'', N'', N'', N'', N'', N'', N'', N'', N'', N'', N'', N'', N'', N'', N'')

";

			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
