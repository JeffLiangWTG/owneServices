using System.Data;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public static class RefDataHelper
	{
		public static void PopulateDummyTariffData(IDbConnection conn)
		{
			conn.ExecuteNonQuery(@"
INSERT RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping)
			VALUES('595F5657-9AC9-4381-B990-F37D1940FCBE', 'ZA', 'South Africa', NULL)");

			conn.ExecuteNonQuery(@"
INSERT RefCusRateType (ZZR_PK,	ZZR_RateType,	ZZR_Description,	ZZR_IsPayable,	ZZR_ZZZ_NKDataGrouping,	ZZR_CustomsValueFormula)
VALUES('60533AE5-BE77-4617-9C42-1A944CEDB4DB', 'AAA', 'Dummy Record',1,'ZA','')");

			conn.ExecuteNonQuery(@"
INSERT RefCusTariffType(ZZI_PK,	ZZI_TariffType,	ZZI_Description,	ZZI_ZZZ_NKDataGrouping)
VALUES('178B58CB-F2A3-41DC-9CC3-3EBBC54F3C37',	'999',	'Dummy Record',	'ZA')");

			conn.ExecuteNonQuery(@"
INSERT RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description) 
VALUES ('C1CDFDC1-C94B-4919-A658-761EDA88F952','TEST','60533AE5-BE77-4617-9C42-1A944CEDB4DB','Test Description')
");

			conn.ExecuteNonQuery(@"
INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES ('494D2ACB-742E-4C34-906B-E01EAD292F6F', 'EN', 'EN')");

			conn.ExecuteNonQuery(@"
INSERT INTO RefCusPreference (ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping) 
VALUES ('D3A2079C-76DA-4DEA-AF43-861758DEEB1C','TEST','Test Description','ZA')");

			conn.ExecuteNonQuery(@"
INSERT INTO RefCusPreferenceLanguage (ZX9_PK,ZX9_ZX6_NKLanguage,ZX9_ZZS_Preference,ZX9_Description)
VALUES ('A1087111-5122-419A-B8C4-FECD70206BB2','EN','D3A2079C-76DA-4DEA-AF43-861758DEEB1C','English')");

			conn.ExecuteNonQuery(@"
INSERT INTO RefCusConditionValueType (ZX4_PK,ZX4_ValueType,ZX4_Description,ZX4_IsFormula,ZX4_ZZZ_NKDataGrouping) 
VALUES ('CB81B42C-3B0E-4149-804A-DC38A81CB7BA','TEST','Test Description',1,'ZA');
");

			conn.ExecuteNonQuery(@"
INSERT INTO RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES ('E65619BA-21C3-46C7-87ED-87C0C4DED1D1', 'RATE', 'B', 'C', 'ZA');
");

			conn.ExecuteNonQuery(@"
INSERT INTO RefCusTradeGroup (ZZA_PK, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_TradeGroup, ZZA_ZZZ_NKDataGrouping)
VALUES ('D239E4BE-99E2-45E1-ABE8-41AF310721A1', 'A', '2010-12-10 00:00:00', '2079-06-06 23:59:00', 'ZT', 'ZA');
");

			conn.ExecuteNonQuery(@"
INSERT RefCusTariff(ZZ1_PK,	ZZ1_ZZI_TariffType,	ZZ1_TariffCode,	ZZ1_IAMUnique,	ZZ1_Description,	ZZ1_StartDate,	ZZ1_EndDate,	ZZ1_ZZF_NKTaxOrFeeCode,	ZZ1_ZZZ_NKDataGrouping,	ZZ1_CompositeKeyOnZZ5)
VALUES('15E7547E-0840-466A-90A9-7281028F441D',	'178B58CB-F2A3-41DC-9CC3-3EBBC54F3C37',	'888888',	0,	'Dummy Tariff 1',	'2010-12-10 00:00:00', '2079-06-06 23:59:00', 'VAT', 'ZA', '')");

			conn.ExecuteNonQuery(@"
INSERT INTO RefCusTariffLanguage (ZX7_PK,ZX7_ZX6_NKLanguage,ZX7_ZZ1_Tariff,ZX7_Description)
VALUES ('D599D36A-1F44-4A07-8D06-3F426185D983','EN','15E7547E-0840-466A-90A9-7281028F441D','English')");

			conn.ExecuteNonQuery(@"
INSERT RefCusTariffAttribute(ZZ3_PK, ZZ3_ZZ1_Tariff, ZZ3_Name, ZZ3_Value)
VALUES(NEWID(), '15E7547E-0840-466A-90A9-7281028F441D', 'CheckDigit', '3')");

			conn.ExecuteNonQuery(@"
INSERT RefCusTariffUOM(ZZ8_PK,	ZZ8_ZZ1_Tariff,	ZZ8_Type,	ZZ8_UOM)
VALUES(NEWID(),	'15E7547E-0840-466A-90A9-7281028F441D',	'RU1',	'NO')");

			conn.ExecuteNonQuery(@"
INSERT RefCusRate(ZZ2_PK,	ZZ2_ZZ1_Tariff,	ZZ2_StartDate,	ZZ2_EndDate,	ZZ2_RateFormula, ZZ2_ZZS_Preference, ZZ2_ZY1_RateCode)
VALUES('99EE39EB-0361-4ABF-9A41-EC3E82CAB605',	'15E7547E-0840-466A-90A9-7281028F441D',	'2013-08-16 00:00:00',	'2079-06-06 23:59:00',	0, 'D3A2079C-76DA-4DEA-AF43-861758DEEB1C', 'C1CDFDC1-C94B-4919-A658-761EDA88F952')");

			conn.ExecuteNonQuery(@"
INSERT RefCusTariff(ZZ1_PK,	ZZ1_ZZI_TariffType,	ZZ1_TariffCode,	ZZ1_IAMUnique,	ZZ1_Description,	ZZ1_StartDate,	ZZ1_EndDate,	ZZ1_ZZF_NKTaxOrFeeCode,	ZZ1_ZZZ_NKDataGrouping,	ZZ1_CompositeKeyOnZZ5)
VALUES('1A703EEC-1C4A-46A0-8D2F-9ABF66A0A120',	'178B58CB-F2A3-41DC-9CC3-3EBBC54F3C37',	'999999',	0,	'Dummy Tariff 2',	'2010-12-10 00:00:00', '2079-06-06 23:59:00', 'VAT', 'ZA', '')");

			conn.ExecuteNonQuery(@"
INSERT RefCusTariffRelationship(ZZH_PK,	ZZH_ZZ1_Tariff,	ZZH_ZZI_TariffType,	ZZH_TariffCode)
VALUES(NEWID(),	'1A703EEC-1C4A-46A0-8D2F-9ABF66A0A120',	'178B58CB-F2A3-41DC-9CC3-3EBBC54F3C37',	'8888')");

			conn.ExecuteNonQuery(@"
INSERT RefCusCondition(ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZZ_NKDataGrouping)
VALUES('44C4934F-BD7A-4C12-9A38-0603C71266A9',	'E65619BA-21C3-46C7-87ED-87C0C4DED1D1',	'15E7547E-0840-466A-90A9-7281028F441D',	'2013-08-16 00:00:00',	'2079-06-06 23:59:00', 'ZA')");

			conn.ExecuteNonQuery(@"
INSERT RefCusConditionValue(ZX3_PK, ZX3_ZX4_ValueType, ZX3_LogicalORWithinGroup, ZX3_ZX1_Condition, ZX3_Value)
VALUES(newid(),	'CB81B42C-3B0E-4149-804A-DC38A81CB7BA',	0, '44C4934F-BD7A-4C12-9A38-0603C71266A9',	'A')");

			conn.ExecuteNonQuery(@"
INSERT RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_StartDate, ZZT_EndDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber)
VALUES(newid(),	'99EE39EB-0361-4ABF-9A41-EC3E82CAB605',	'2013-08-16 00:00:00',	'2079-06-06 23:59:00', 'D239E4BE-99E2-45E1-ABE8-41AF310721A1', 'A', '1')");

			conn.ExecuteNonQuery(@"
INSERT RefCusNomenclatureGroupType(ZZ9_PK, ZZ9_GroupType, ZZ9_Description)
VALUES(newid(),'A', 'A')");

			conn.ExecuteNonQuery(@"
INSERT RefCusTariffAdditionalCodeCategory(ZY3_PK, ZY3_Category, ZY3_Description, ZY3_ZZZ_NKDataGrouping)
VALUES(newid(), 'A', 'Desc', 'ZA')");
		}

		public static void PopulateDummyCusCodeData(IDbConnection conn)
		{
			conn.ExecuteNonQuery(@"
IF NOT EXISTS(SELECT 1 FROM RefDataGrouping WHERE ZZZ_PK = '595F5657-9AC9-4381-B990-F37D1940FCBE')
BEGIN
INSERT RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES('595F5657-9AC9-4381-B990-F37D1940FCBE', 'ZA', 'South Africa', NULL)
END

INSERT INTO RefLanguageType(ZX6_PK, ZX6_Language, ZX6_Description)
VALUES('84354A76-B5CF-4B37-98EC-E8C5A645B5B9', 'CH', 'CH')

INSERT RefCusCodeType(ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_ZZZ_NKDataGrouping) VALUES(N'6a10b0de-d88e-44ec-b640-9d05bbb48d8b', N'ADDIN', N'Additional Information', 'ZA')
INSERT RefCusCodeType(ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_ZZZ_NKDataGrouping) VALUES(N'199a159d-9aae-47e3-a81f-670b6e42f2ec', N'CSTA', N'Customs Status', 'ZA')

INSERT RefCusCodeTypeLanguage(ZXI_PK, ZXI_ZX6_NKLanguage, ZXI_ZZK_CodeType, ZXI_Description) VALUES(N'FE8DF1DC-52C1-4D4F-8F19-6100908AD8F3', 'CH', N'6a10b0de-d88e-44ec-b640-9d05bbb48d8b', N'Additonal Information in Chinese')
INSERT RefCusCodeTypeLanguage(ZXI_PK, ZXI_ZX6_NKLanguage, ZXI_ZZK_CodeType, ZXI_Description) VALUES(N'7D19DCD4-56C2-4793-9447-7F230A3C8534', 'CH', N'199a159d-9aae-47e3-a81f-670b6e42f2ec', N'Customs Status in Chinese')

INSERT RefCusCodeList(ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(N'df2ada34-b0f4-486f-92af-feef2ac6cf84', N'ADDIN', N'EPC', N'Export Permit Control', CAST(N'1900-01-01 12:00:00' AS SmallDateTime), CAST(N'2079-06-06 23:59:00' AS SmallDateTime), N'ZA')
INSERT RefCusCodeList(ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) VALUES(N'd5f4df8f-b612-4a4a-8585-ffd49a596541', N'CSTA', N'39', N'Technical Issue Notification', CAST(N'1900-01-01 12:00:00' AS SmallDateTime), CAST(N'2079-06-06 23:59:00' AS SmallDateTime), N'ZA')

INSERT RefCusCodeListAttributeName(ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping) VALUES('FBC8C291-1357-49B0-BBE6-B1893EA78D7E', 'Export', 'Desc.', 'ADDIN', 'ZA')
INSERT RefCusCodeListAttributeName(ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping) VALUES('B24D042F-EA83-4A66-AF6F-DCF16CFE452F', 'LicenceNumber', 'Desc.', 'ADDIN', 'ZA')
INSERT RefCusCodeListAttributeName(ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping) VALUES('CBF4BDB1-59D4-4F03-A20C-BE71B1C70589', 'INotify', 'Desc.', 'CSTA', 'ZA')
INSERT RefCusCodeListAttributeName(ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping) VALUES('E401723B-814F-4BB6-9B86-4469BF52F7D8', 'ISendEntryDocs', 'Desc.', 'CSTA', 'ZA')

INSERT RefCusCodeListAttributeNameLanguage(ZXH_PK, ZXH_ZX6_NKLanguage, ZXH_ZXE_CodeListAttributeName, ZXH_Description, ZXH_Name, ZXH_ColumnCaption) VALUES (newid(), 'CH', 'FBC8C291-1357-49B0-BBE6-B1893EA78D7E', 'Desc. in Chinese', 'Export', '')
INSERT RefCusCodeListAttributeNameLanguage(ZXH_PK, ZXH_ZX6_NKLanguage, ZXH_ZXE_CodeListAttributeName, ZXH_Description, ZXH_Name, ZXH_ColumnCaption) VALUES (newid(), 'CH', 'B24D042F-EA83-4A66-AF6F-DCF16CFE452F', 'Desc. in Chinese', 'LicenceNumber', '')
INSERT RefCusCodeListAttributeNameLanguage(ZXH_PK, ZXH_ZX6_NKLanguage, ZXH_ZXE_CodeListAttributeName, ZXH_Description, ZXH_Name, ZXH_ColumnCaption) VALUES (newid(), 'CH', 'CBF4BDB1-59D4-4F03-A20C-BE71B1C70589', 'Desc. in Chinese', 'INotify', '')
INSERT RefCusCodeListAttributeNameLanguage(ZXH_PK, ZXH_ZX6_NKLanguage, ZXH_ZXE_CodeListAttributeName, ZXH_Description, ZXH_Name, ZXH_ColumnCaption) VALUES (newid(), 'CH', 'E401723B-814F-4BB6-9B86-4469BF52F7D8', 'Desc. in Chinese', 'ISendEntryDocs', '')

INSERT RefCusCodeListAttribute(ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value) VALUES(N'6da6f804-eff6-445d-8617-72f234d7c6f1', N'df2ada34-b0f4-486f-92af-feef2ac6cf84', N'Export', N'')
INSERT RefCusCodeListAttribute(ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value) VALUES(N'f211392e-bb75-44be-baa5-47fd238aaafb', N'df2ada34-b0f4-486f-92af-feef2ac6cf84', N'LicenceNumber', N'LicenceNumber')
INSERT RefCusCodeListAttribute(ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value) VALUES(N'f9d8bf8c-154f-4548-8856-e54b1dff6551', N'd5f4df8f-b612-4a4a-8585-ffd49a596541', N'INotify', N'')
INSERT RefCusCodeListAttribute(ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value) VALUES(N'dd504df7-638d-4b6d-b66f-5a2ab965ea96', N'd5f4df8f-b612-4a4a-8585-ffd49a596541', N'ISendEntryDocs', N'')

INSERT RefCusCodeOrAttributeTransportMode(ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList, ZZU_ZZE_Attribute) VALUES
('23333B81-B7A5-41A3-AA66-F0DDCB40BEB5', 'AIR', 'df2ada34-b0f4-486f-92af-feef2ac6cf84', null),
('1B8E9A9E-6A87-400B-8D8A-55B3E21B2368', 'SEA', 'df2ada34-b0f4-486f-92af-feef2ac6cf84', null),
('DA04AA2B-E025-469C-9827-A856C1BF677E', 'AIR', null, '6da6f804-eff6-445d-8617-72f234d7c6f1')

INSERT INTO RefCusCodeListLanguage(ZXA_PK, ZXA_ZX6_NKLanguage, ZXA_ZZD_CodeList, ZXA_Description)
VALUES(N'80B6625C-3089-42D0-8FA3-3832C867A644', 'CH', N'df2ada34-b0f4-486f-92af-feef2ac6cf84', N'Chinese')
");
		}
	}
}
