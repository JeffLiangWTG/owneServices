using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class PopulateDataChangeHistoryForExistingRecordsPart1Fixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'RN'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'RX'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'RW'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZM'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZP'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZX2'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZX4'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZ9'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZN'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZX6'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'R3'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'ZZO'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'DC'"));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, "SELECT COUNT(1) FROM DataSetChangeHistory WHERE DCH_ParentCode = 'DG'"));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new PopulateDataChangeHistoryForExistingRecordsPart1(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','AU','AU','AU');

INSERT [dbo].[RefCountry] ([RN_PK], [RN_Code], [RN_IsActive], [RN_Desc], [RN_EconomicGrouping], [RN_CountryDialingCode], [RN_AddressFormattingRule], [RN_PostcodeValidationRule], [RN_StateProvinceValidationRule], [RN_RX_NKLocalCurrency], [RN_RX_NKAirWaybillCurrency], [RN_IsoAlpha3Code], [RN_IsoNumericUNM49Code], [RN_ValidationStatus])
VALUES (N'ede3e194-4040-4359-957e-02b285036b95', N'GS', 1, N'South Georgia and the South Sandwic', N'', N'', N'DEF', N'NVR', N'NVR', N'BSD', N'BSD', N'SGS', N'239', N'NAV')

INSERT [dbo].[RefCountryStates] ([RW_PK], [RW_Description], [RW_RegionName], [RW_IsActive], [RW_Code], [RW_RN_NKCountryCode])
VALUES (N'd5488910-0190-498e-ad5e-906e5b99acae', N'Kikuube', N'', 1, N'432', N'GS')

INSERT [dbo].[RefCurrency] ([RX_PK], [RX_Code], [RX_IsActive], [RX_Symbol], [RX_Desc], [RX_UnitName], [RX_SubUnitName], [RX_SubUnitRatio], [RX_ISOSubUnitRatio])
VALUES (N'60b4fee8-52ed-4c30-b519-4c41ef784c9f', N'BSD', 1, N'$', N'Bahamian Dollar', N'dollar', N'cents', 100, 2)

INSERT [dbo].[RefCusMapType] ([ZZP_PK], [ZZP_MapType], [ZZP_Direction], [ZZP_Description], [ZZP_IsReadonly])
VALUES (N'da161ba2-d7fb-45e1-bb46-f0a3ef75d459', N'STATE', N'OUT', N'State Mapping', 0)

INSERT [dbo].[RefCusMap] ([ZZM_PK], [ZZM_ZZP_NKMapType], [ZZM_CW1orCommercialValue], [ZZM_CustomsValue], [ZZM_StartDate], [ZZM_EndDate], [ZZM_ZZZ_NKDataGrouping])
VALUES (N'ef4b2f90-3cd0-4d18-8476-fc1cd80e3d91', N'STATE', N'4407101033', N'4407191017', CAST(N'2017-01-01T00:00:00' AS SmallDateTime), CAST(N'2079-06-06T23:59:00' AS SmallDateTime), N'AU')

INSERT [dbo].[RefCusConditionType] ([ZX2_PK],[ZX2_ConditionClass],[ZX2_ConditionType],[ZX2_Description],[ZX2_ZZZ_NKDataGrouping])
VALUES ('7F91FBC1-3E8D-479E-ACE0-0100F66E87F0','CTRL','740','Export control on cat and dog fur','AU')

INSERT [dbo].[RefCusConditionValueType] ([ZX4_PK],[ZX4_ValueType],[ZX4_Description],[ZX4_IsFormula],[ZX4_ZZZ_NKDataGrouping])
VALUES ('B4FECB11-5312-4A61-87F7-00AB6301B048','Q','Presentation of an endorsed certificate/licence',0,'AU')

INSERT [dbo].[RefCusNomenclatureGroupType] ([ZZ9_PK],[ZZ9_GroupType],[ZZ9_Description])
VALUES ('E12CB88C-2B59-4BE5-8588-285B7FF434B8','CDS','CDS GB Goods Nomenclature')

INSERT [dbo].[RefExchangeRateZZ] ( [ZZN_PK],[ZZN_ExRateType],[ZZN_StartDate],[ZZN_EndDate],[ZZN_Rate],[ZZN_RX_NKExCurrency],[ZZN_RN_NKCountry])
VALUES ('2085C381-CF73-45BF-A635-00002DFCD908','CUS','2016-12-09 00:00:00','2016-12-09 00:00:00',5.917800000,'SBD','AU')

INSERT [dbo].[RefLanguageType] ([ZX6_PK],[ZX6_Language],[ZX6_Description])
VALUES ('246D2EBA-DE17-4CB5-8A1A-0393152D76D1','ZHS','ChineseSimplified')

INSERT [dbo].[RefTimeZoneSet] ([R3_PK],[R3_TimeZoneSetName],[R3_IsActive])
VALUES ('158F7F31-3770-45E5-9C0B-0176A38D6D05','Australia/West',1)

INSERT [dbo].[RefVesselZZ] ([ZZO_PK],[ZZO_Code],[ZZO_RadioCallSign],[ZZO_VesselType],[ZZO_RN_NKCountryOfReg],[ZZO_LloydsNumber],[ZZO_ZZZ_NKDataGrouping])
VALUES ('585BA86C-412F-4155-A5C5-000E996073D8','Welle','C4EW2','CV','','','AU')

INSERT [dbo].[UNDGCommonData] ([DC_PK],[DC_Language],[DC_Type],[DC_Index],[DC_Descriptor])
VALUES ('041C315B-D55B-4E35-B0C4-E9DB501723B2','ENG','STS','3','On deck only.')

INSERT [dbo].[UNDGSubstance] ([DG_PK], [DG_UNNO], [DG_Variant], [DG_Variation], [DG_Class], [DG_SubLabel1], [DG_SubLabel2], [DG_PSN], [DG_PG], [DG_EMS], [DG_MP], [DG_FlashPoint], [DG_LQMaxAmt], [DG_LQMaxAmtUQ], [DG_LQSpecProvIndex], [DG_TechName], [DG_TreatAs], [DG_DglPhrase], [DG_PackIns], [DG_PackProv], [DG_IBCIns], [DG_IBCProv], [DG_IMOTankIns], [DG_UNTankIns], [DG_TankProv], [DG_Markers], [DG_Pointers], [DG_EXVector], [DG_StowCat], [DG_CodedStow], [DG_State], [DG_ExpLim], [DG_UlineEMS], [DG_UsrUSDOTShippingName], [DG_ExceptedQuantityCode], [DG_IsActive], [DG_Standard], [DG_IsNotOtherwiseSpecified], [DG_LQMaxAmtType], [DG_PaxPackIns], [DG_LQ2OrPaxMaxAmtType], [DG_LQ2OrPaxMaxAmt], [DG_LQ2OrPaxMaxAmtUQ], [DG_CargoPackAmtType], [DG_CargoPackIns], [DG_CargoMaxAmt], [DG_CargoMaxAmtUQ], [DG_EmergencyResponseGuide], [DG_Hazards], [DG_SpecialHandlingCodes], [DG_UniqueRecordId])
VALUES (N'10bc3522-f2b7-47db-9648-2c9fe83abd99', N'1802', N'', N'', N'8', N'5.1', N'', N'PERCHLORIC ACID', N'II', N'F-H,S-Q', N'', N'', CAST(1.000 AS Decimal(9, 3)), N'L', N'', N'', N'', N'264 237 561', N'P001', N'', N'IBC02', N'', N'TP28', N'T7', N'TP2', N'AaQn', N'', N'00000020000000000', N'C', N'OD S1:4.1 (ACID)', N'L', N'', N'', N'', N'E0', 1, N'IMO', 0, N'NLM', N'', N'NLM', CAST(0.000 AS Decimal(9, 3)), N'', N'NLM', N'', CAST(0.000 AS Decimal(9, 3)), N'', N'', N'', N'', N'')
";

			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
