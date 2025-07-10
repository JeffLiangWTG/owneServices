using System.Globalization;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	public class EntityConfigurationManagerTest
	{
		[Test]
		public void TestDeserializeTariffEntityConfiguration()
		{
			var configFilePath = string.Format(CultureInfo.CurrentCulture, ApplicationConfig.TariffConfigFileInputPath, "2017");
			var tariffEntityConfiguration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFilePath);
			Assert.AreEqual("xls, xlsx", tariffEntityConfiguration.DataFileExtensions);
			Assert.AreEqual(4, tariffEntityConfiguration.EntityTypes.Length);

			Assert.AreEqual("RefCusTariff", tariffEntityConfiguration.EntityTypes[0].Name);
			Assert.AreEqual(true, tariffEntityConfiguration.EntityTypes[0].Data);
			Assert.AreEqual(4, tariffEntityConfiguration.EntityTypes[0].Key.Length);
			Assert.AreEqual("ZZ1_TariffCode", tariffEntityConfiguration.EntityTypes[0].Key[0].Name);
			Assert.AreEqual("ZZ1_ZZI_NKTariffType", tariffEntityConfiguration.EntityTypes[0].Key[1].Name);
			Assert.AreEqual("ZZ1_ZZI_ZZZ_NKDataGrouping", tariffEntityConfiguration.EntityTypes[0].Key[2].Name);
			Assert.AreEqual("ZZ1_ZZZ_NKDataGrouping", tariffEntityConfiguration.EntityTypes[0].Key[3].Name);
			Assert.AreEqual(11, tariffEntityConfiguration.EntityTypes[0].Properties.Length);
			Assert.AreEqual("RefCusTariffUOM", tariffEntityConfiguration.EntityTypes[0].Properties[0].Name);
			Assert.AreEqual("RefCusTariffUOM", tariffEntityConfiguration.EntityTypes[0].Properties[0].Type);
			Assert.AreEqual("RefCusTariffLanguage", tariffEntityConfiguration.EntityTypes[0].Properties[1].Name);
			Assert.AreEqual("RefCusTariffLanguage", tariffEntityConfiguration.EntityTypes[0].Properties[1].Type);
			Assert.AreEqual("RefCusTariffAttribute", tariffEntityConfiguration.EntityTypes[0].Properties[2].Name);
			Assert.AreEqual("RefCusTariffAttribute", tariffEntityConfiguration.EntityTypes[0].Properties[2].Type);
			Assert.AreEqual("ZZ1_TariffCode", tariffEntityConfiguration.EntityTypes[0].Properties[3].Name);
			Assert.AreEqual("varchar", tariffEntityConfiguration.EntityTypes[0].Properties[3].Type);
			Assert.AreEqual(35, tariffEntityConfiguration.EntityTypes[0].Properties[3].MaxLength);
			Assert.AreEqual("ZZ1_CompositeKeyOnZZ5", tariffEntityConfiguration.EntityTypes[0].Properties[4].Name);
			Assert.AreEqual("varchar", tariffEntityConfiguration.EntityTypes[0].Properties[4].Type);
			Assert.AreEqual("ZZ1_Description", tariffEntityConfiguration.EntityTypes[0].Properties[5].Name);
			Assert.AreEqual("nvarchar", tariffEntityConfiguration.EntityTypes[0].Properties[5].Type);
			Assert.AreEqual("ZZ1_StartDate", tariffEntityConfiguration.EntityTypes[0].Properties[6].Name);
			Assert.AreEqual("datetime", tariffEntityConfiguration.EntityTypes[0].Properties[6].Type);
			Assert.AreEqual("ZZ1_EndDate", tariffEntityConfiguration.EntityTypes[0].Properties[7].Name);
			Assert.AreEqual("datetime", tariffEntityConfiguration.EntityTypes[0].Properties[7].Type);
			Assert.AreEqual("ZZ1_ZZI_NKTariffType", tariffEntityConfiguration.EntityTypes[0].Properties[8].Name);
			Assert.AreEqual("varchar", tariffEntityConfiguration.EntityTypes[0].Properties[8].Type);
			Assert.AreEqual(5, tariffEntityConfiguration.EntityTypes[0].Properties[8].MaxLength);
			Assert.AreEqual("HSN", tariffEntityConfiguration.EntityTypes[0].Properties[8].ConstantValue);
			Assert.AreEqual("ZZ1_ZZI_ZZZ_NKDataGrouping", tariffEntityConfiguration.EntityTypes[0].Properties[9].Name);
			Assert.AreEqual("varchar", tariffEntityConfiguration.EntityTypes[0].Properties[9].Type);
			Assert.AreEqual(3, tariffEntityConfiguration.EntityTypes[0].Properties[9].MaxLength);
			Assert.AreEqual("KR", tariffEntityConfiguration.EntityTypes[0].Properties[9].ConstantValue);
			Assert.AreEqual("ZZ1_ZZZ_NKDataGrouping", tariffEntityConfiguration.EntityTypes[0].Properties[10].Name);
			Assert.AreEqual("varchar", tariffEntityConfiguration.EntityTypes[0].Properties[10].Type);
			Assert.AreEqual(3, tariffEntityConfiguration.EntityTypes[0].Properties[10].MaxLength);
			Assert.AreEqual("KR", tariffEntityConfiguration.EntityTypes[0].Properties[10].ConstantValue);

			Assert.AreEqual("RefCusTariffUOM", tariffEntityConfiguration.EntityTypes[1].Name);
			Assert.AreEqual(true, tariffEntityConfiguration.EntityTypes[1].Data);
			Assert.AreEqual(1, tariffEntityConfiguration.EntityTypes[1].Key.Length);
			Assert.AreEqual("ZZ8_Type", tariffEntityConfiguration.EntityTypes[1].Key[0].Name);
			Assert.AreEqual(3, tariffEntityConfiguration.EntityTypes[1].Properties.Length);
			Assert.AreEqual("ZZ8_Type", tariffEntityConfiguration.EntityTypes[1].Properties[0].Name);
			Assert.AreEqual("varchar", tariffEntityConfiguration.EntityTypes[1].Properties[0].Type);
			Assert.AreEqual(3, tariffEntityConfiguration.EntityTypes[1].Properties[0].MaxLength);
			Assert.AreEqual("ZZ8_UOM", tariffEntityConfiguration.EntityTypes[1].Properties[1].Name);
			Assert.AreEqual("varchar", tariffEntityConfiguration.EntityTypes[1].Properties[1].Type);
			Assert.AreEqual(10, tariffEntityConfiguration.EntityTypes[1].Properties[1].MaxLength);
			Assert.AreEqual("ZZ8_ZZZ_NKDataGrouping", tariffEntityConfiguration.EntityTypes[1].Properties[2].Name);
			Assert.AreEqual("varchar", tariffEntityConfiguration.EntityTypes[1].Properties[2].Type);
			Assert.AreEqual(3, tariffEntityConfiguration.EntityTypes[1].Properties[2].MaxLength);
			Assert.AreEqual("KR", tariffEntityConfiguration.EntityTypes[1].Properties[2].ConstantValue);

			Assert.AreEqual("RefCusTariffAttribute", tariffEntityConfiguration.EntityTypes[2].Name);
			Assert.AreEqual(true, tariffEntityConfiguration.EntityTypes[2].Data);
			Assert.AreEqual(1, tariffEntityConfiguration.EntityTypes[2].Key.Length);
			Assert.AreEqual("ZZ3_Name", tariffEntityConfiguration.EntityTypes[2].Key[0].Name);
			Assert.AreEqual(2, tariffEntityConfiguration.EntityTypes[2].Properties.Length);
			Assert.AreEqual("ZZ3_Name", tariffEntityConfiguration.EntityTypes[2].Properties[0].Name);
			Assert.AreEqual("varchar", tariffEntityConfiguration.EntityTypes[2].Properties[0].Type);
			Assert.AreEqual("ZZ3_Value", tariffEntityConfiguration.EntityTypes[2].Properties[1].Name);
			Assert.AreEqual("nvarchar(max)", tariffEntityConfiguration.EntityTypes[2].Properties[1].Type);
			Assert.AreEqual(50, tariffEntityConfiguration.EntityTypes[2].Properties[0].MaxLength);

			Assert.AreEqual("RefCusTariffLanguage", tariffEntityConfiguration.EntityTypes[3].Name);
			Assert.AreEqual(true, tariffEntityConfiguration.EntityTypes[3].Data);
			Assert.AreEqual(1, tariffEntityConfiguration.EntityTypes[3].Key.Length);
			Assert.AreEqual("ZX7_ZX6_NKLanguage", tariffEntityConfiguration.EntityTypes[3].Key[0].Name);
			Assert.AreEqual(2, tariffEntityConfiguration.EntityTypes[3].Properties.Length);
			Assert.AreEqual("ZX7_Description", tariffEntityConfiguration.EntityTypes[3].Properties[0].Name);
			Assert.AreEqual("nvarchar(max)", tariffEntityConfiguration.EntityTypes[3].Properties[0].Type);
			Assert.AreEqual("ZX7_ZX6_NKLanguage", tariffEntityConfiguration.EntityTypes[3].Properties[1].Name);
			Assert.AreEqual("varchar", tariffEntityConfiguration.EntityTypes[3].Properties[1].Type);
			Assert.AreEqual(3, tariffEntityConfiguration.EntityTypes[3].Properties[1].MaxLength);
			Assert.AreEqual("KO", tariffEntityConfiguration.EntityTypes[3].Properties[1].ConstantValue);

			Assert.AreEqual(0, tariffEntityConfiguration.EntityTypeExcelColumnMapping.SheetIndex);
			Assert.AreEqual(1, tariffEntityConfiguration.EntityTypeExcelColumnMapping.StartRow);

			Assert.AreEqual(4, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes.Length);

			Assert.AreEqual("RefCusTariff", tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Name);
			Assert.AreEqual(3, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties.Length);
			Assert.AreEqual("ZZ1_TariffCode", tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[0].Name);
			Assert.AreEqual(0, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[0].ExcelColumn);
			Assert.AreEqual("ZZ1_StartDate", tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[1].Name);
			Assert.AreEqual(5, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[1].ExcelColumn);
			Assert.AreEqual("ZZ1_Description", tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[2].Name);
			Assert.AreEqual(1, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[2].ExcelColumn);

			Assert.AreEqual("RefCusTariffLanguage", tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Name);
			Assert.AreEqual(1, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties.Length);
			Assert.AreEqual("ZX7_Description", tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties[0].Name);
			Assert.AreEqual(2, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties[0].ExcelColumn);

			Assert.AreEqual("RefCusTariffUOM", tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[2].Name);
			Assert.AreEqual(1, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[2].Sequence);
			Assert.AreEqual(1, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[2].Properties.Length);
			Assert.AreEqual("ZZ8_UOM", tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[2].Properties[0].Name);
			Assert.AreEqual(3, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[2].Properties[0].ExcelColumn);

			Assert.AreEqual("RefCusTariffUOM", tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[3].Name);
			Assert.AreEqual(2, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[3].Sequence);
			Assert.AreEqual(1, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[3].Properties.Length);
			Assert.AreEqual("ZZ8_UOM", tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[3].Properties[0].Name);
			Assert.AreEqual(4, tariffEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[3].Properties[0].ExcelColumn);

			Assert.AreEqual(2, tariffEntityConfiguration.Rules.Length);
			Assert.AreEqual("InvoiceQuantity in CU1", tariffEntityConfiguration.Rules[0].Name);
			Assert.AreEqual("OR", tariffEntityConfiguration.Rules[0].Relationship);
			Assert.AreEqual(1, tariffEntityConfiguration.Rules[0].RuleValues.Length);
			Assert.AreEqual("240220", tariffEntityConfiguration.Rules[0].RuleValues[0].Value);
			Assert.AreEqual("MIN as Customs Quantity", tariffEntityConfiguration.Rules[1].Name);
			Assert.AreEqual("OR", tariffEntityConfiguration.Rules[1].Relationship);
			Assert.AreEqual(1, tariffEntityConfiguration.Rules[1].RuleValues.Length);
			Assert.AreEqual("8523292231", tariffEntityConfiguration.Rules[1].RuleValues[0].Value);
		}

		[Test]
		public void TestExportNonGAReasonEntityConfiguration()
		{
			var configFilePath = ApplicationConfig.NonGAReasonExportConfigFileInputPath;
			var nonGAReasonEntityConfiguration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFilePath);

			Assert.AreEqual("xls, xlsx", nonGAReasonEntityConfiguration.DataFileExtensions);
			Assert.AreEqual(2, nonGAReasonEntityConfiguration.EntityTypes.Length);

			Assert.AreEqual("RefCusCodeList", nonGAReasonEntityConfiguration.EntityTypes[0].Name);
			Assert.AreEqual(true, nonGAReasonEntityConfiguration.EntityTypes[0].Data);
			Assert.AreEqual(3, nonGAReasonEntityConfiguration.EntityTypes[0].Key.Length);
			Assert.AreEqual("ZZD_ZZK_NKCodeType", nonGAReasonEntityConfiguration.EntityTypes[0].Key[0].Name);
			Assert.AreEqual("ZZD_Code", nonGAReasonEntityConfiguration.EntityTypes[0].Key[1].Name);
			Assert.AreEqual("ZZD_ZZZ_NKDataGrouping", nonGAReasonEntityConfiguration.EntityTypes[0].Key[2].Name);
			Assert.AreEqual(7, nonGAReasonEntityConfiguration.EntityTypes[0].Properties.Length);
			Assert.AreEqual("RefCusCodeListAttribute", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[0].Name);
			Assert.AreEqual("RefCusCodeListAttribute", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[0].Type);
			Assert.AreEqual("ZZD_ZZK_NKCodeType", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[1].Name);
			Assert.AreEqual("varchar", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[1].Type);
			Assert.AreEqual(5, nonGAReasonEntityConfiguration.EntityTypes[0].Properties[1].MaxLength);
			Assert.AreEqual("ENGAR", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[1].ConstantValue);
			Assert.AreEqual("ZZD_Code", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[2].Name);
			Assert.AreEqual("varchar", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[2].Type);
			Assert.AreEqual(35, nonGAReasonEntityConfiguration.EntityTypes[0].Properties[2].MaxLength);
			Assert.AreEqual("ZZD_Description", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[3].Name);
			Assert.AreEqual("nvarchar", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[3].Type);
			Assert.AreEqual(2000, nonGAReasonEntityConfiguration.EntityTypes[0].Properties[3].MaxLength);
			Assert.AreEqual("ZZD_StartDate", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[4].Name);
			Assert.AreEqual("datetime", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[4].Type);
			Assert.AreEqual("1900-01-01 00:00:00", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[4].ConstantValue);
			Assert.AreEqual("ZZD_EndDate", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[5].Name);
			Assert.AreEqual("datetime", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[5].Type);
			Assert.AreEqual("2079-06-06 23:59:00", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[5].ConstantValue);
			Assert.AreEqual("ZZD_ZZZ_NKDataGrouping", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[6].Name);
			Assert.AreEqual("varchar", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[6].Type);
			Assert.AreEqual(3, nonGAReasonEntityConfiguration.EntityTypes[0].Properties[6].MaxLength);
			Assert.AreEqual("KR", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[6].ConstantValue);

			Assert.AreEqual("RefCusCodeListAttribute", nonGAReasonEntityConfiguration.EntityTypes[1].Name);
			Assert.AreEqual(true, nonGAReasonEntityConfiguration.EntityTypes[1].Data);
			Assert.AreEqual(1, nonGAReasonEntityConfiguration.EntityTypes[1].Key.Length);
			Assert.AreEqual("ZZE_ZXE_NKName", nonGAReasonEntityConfiguration.EntityTypes[1].Key[0].Name);
			Assert.AreEqual(2, nonGAReasonEntityConfiguration.EntityTypes[1].Properties.Length);
			Assert.AreEqual("ZZE_ZXE_NKName", nonGAReasonEntityConfiguration.EntityTypes[1].Properties[0].Name);
			Assert.AreEqual("varchar", nonGAReasonEntityConfiguration.EntityTypes[1].Properties[0].Type);
			Assert.AreEqual(32, nonGAReasonEntityConfiguration.EntityTypes[1].Properties[0].MaxLength);
			Assert.AreEqual("MandatoryDocWhenExempt", nonGAReasonEntityConfiguration.EntityTypes[1].Properties[0].ConstantValue);
			Assert.AreEqual("ZZE_Value", nonGAReasonEntityConfiguration.EntityTypes[1].Properties[1].Name);
			Assert.AreEqual("nvarchar", nonGAReasonEntityConfiguration.EntityTypes[1].Properties[1].Type);
			Assert.AreEqual(255, nonGAReasonEntityConfiguration.EntityTypes[1].Properties[1].MaxLength);
		}

		[Test]
		public void TestImportNonGAReasonEntityConfiguration()
		{
			var configFilePath = ApplicationConfig.NonGAReasonImportConfigFileInputPath;
			var nonGAReasonEntityConfiguration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFilePath);

			Assert.AreEqual("xls, xlsx", nonGAReasonEntityConfiguration.DataFileExtensions);
			Assert.AreEqual(2, nonGAReasonEntityConfiguration.EntityTypes.Length);

			Assert.AreEqual("RefCusCodeList", nonGAReasonEntityConfiguration.EntityTypes[0].Name);
			Assert.AreEqual(true, nonGAReasonEntityConfiguration.EntityTypes[0].Data);
			Assert.AreEqual(3, nonGAReasonEntityConfiguration.EntityTypes[0].Key.Length);
			Assert.AreEqual("ZZD_ZZK_NKCodeType", nonGAReasonEntityConfiguration.EntityTypes[0].Key[0].Name);
			Assert.AreEqual("ZZD_Code", nonGAReasonEntityConfiguration.EntityTypes[0].Key[1].Name);
			Assert.AreEqual("ZZD_ZZZ_NKDataGrouping", nonGAReasonEntityConfiguration.EntityTypes[0].Key[2].Name);
			Assert.AreEqual(7, nonGAReasonEntityConfiguration.EntityTypes[0].Properties.Length);
			Assert.AreEqual("RefCusCodeListAttribute", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[0].Name);
			Assert.AreEqual("RefCusCodeListAttribute", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[0].Type);
			Assert.AreEqual("ZZD_ZZK_NKCodeType", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[1].Name);
			Assert.AreEqual("varchar", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[1].Type);
			Assert.AreEqual(5, nonGAReasonEntityConfiguration.EntityTypes[0].Properties[1].MaxLength);
			Assert.AreEqual("INGAR", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[1].ConstantValue);
			Assert.AreEqual("ZZD_Code", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[2].Name);
			Assert.AreEqual("varchar", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[2].Type);
			Assert.AreEqual(35, nonGAReasonEntityConfiguration.EntityTypes[0].Properties[2].MaxLength);
			Assert.AreEqual("ZZD_Description", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[3].Name);
			Assert.AreEqual("nvarchar", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[3].Type);
			Assert.AreEqual(2000, nonGAReasonEntityConfiguration.EntityTypes[0].Properties[3].MaxLength);
			Assert.AreEqual("ZZD_StartDate", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[4].Name);
			Assert.AreEqual("datetime", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[4].Type);
			Assert.AreEqual("1900-01-01 00:00:00", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[4].ConstantValue);
			Assert.AreEqual("ZZD_EndDate", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[5].Name);
			Assert.AreEqual("datetime", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[5].Type);
			Assert.AreEqual("2079-06-06 23:59:00", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[5].ConstantValue);
			Assert.AreEqual("ZZD_ZZZ_NKDataGrouping", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[6].Name);
			Assert.AreEqual("varchar", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[6].Type);
			Assert.AreEqual(3, nonGAReasonEntityConfiguration.EntityTypes[0].Properties[6].MaxLength);
			Assert.AreEqual("KR", nonGAReasonEntityConfiguration.EntityTypes[0].Properties[6].ConstantValue);

			Assert.AreEqual("RefCusCodeListAttribute", nonGAReasonEntityConfiguration.EntityTypes[1].Name);
			Assert.AreEqual(true, nonGAReasonEntityConfiguration.EntityTypes[1].Data);
			Assert.AreEqual(1, nonGAReasonEntityConfiguration.EntityTypes[1].Key.Length);
			Assert.AreEqual("ZZE_ZXE_NKName", nonGAReasonEntityConfiguration.EntityTypes[1].Key[0].Name);
			Assert.AreEqual(2, nonGAReasonEntityConfiguration.EntityTypes[1].Properties.Length);
			Assert.AreEqual("ZZE_ZXE_NKName", nonGAReasonEntityConfiguration.EntityTypes[1].Properties[0].Name);
			Assert.AreEqual("varchar", nonGAReasonEntityConfiguration.EntityTypes[1].Properties[0].Type);
			Assert.AreEqual(32, nonGAReasonEntityConfiguration.EntityTypes[1].Properties[0].MaxLength);
			Assert.AreEqual("MandatoryDocWhenExempt", nonGAReasonEntityConfiguration.EntityTypes[1].Properties[0].ConstantValue);
			Assert.AreEqual("ZZE_Value", nonGAReasonEntityConfiguration.EntityTypes[1].Properties[1].Name);
			Assert.AreEqual("nvarchar", nonGAReasonEntityConfiguration.EntityTypes[1].Properties[1].Type);
			Assert.AreEqual(255, nonGAReasonEntityConfiguration.EntityTypes[1].Properties[1].MaxLength);
		}
		[Test]
		public void TestRefCusPreferenceEntityConfiguration()
		{
			var configFilePath = ApplicationConfig.PreferenceConfigFileInputPath;
			var refCusPreferenceEntityConfiguration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFilePath);

			Assert.AreEqual("xls, xlsx", refCusPreferenceEntityConfiguration.DataFileExtensions);
			Assert.AreEqual(1, refCusPreferenceEntityConfiguration.EntityTypes.Length);

			Assert.AreEqual("RefCusPreference", refCusPreferenceEntityConfiguration.EntityTypes[0].Name);
			Assert.AreEqual(true, refCusPreferenceEntityConfiguration.EntityTypes[0].Data);
			Assert.AreEqual(2, refCusPreferenceEntityConfiguration.EntityTypes[0].Key.Length);
			Assert.AreEqual("ZZS_Preference", refCusPreferenceEntityConfiguration.EntityTypes[0].Key[0].Name);
			Assert.AreEqual("ZZS_ZZZ_NKDataGrouping", refCusPreferenceEntityConfiguration.EntityTypes[0].Key[1].Name);
			Assert.AreEqual(3, refCusPreferenceEntityConfiguration.EntityTypes[0].Properties.Length);
			Assert.AreEqual("ZZS_Preference", refCusPreferenceEntityConfiguration.EntityTypes[0].Properties[0].Name);
			Assert.AreEqual("varchar", refCusPreferenceEntityConfiguration.EntityTypes[0].Properties[0].Type);
			Assert.AreEqual(10, refCusPreferenceEntityConfiguration.EntityTypes[0].Properties[0].MaxLength);
			Assert.AreEqual("ZZS_Description", refCusPreferenceEntityConfiguration.EntityTypes[0].Properties[1].Name);
			Assert.AreEqual("nvarchar", refCusPreferenceEntityConfiguration.EntityTypes[0].Properties[1].Type);
			Assert.AreEqual(500, refCusPreferenceEntityConfiguration.EntityTypes[0].Properties[1].MaxLength);
			Assert.AreEqual("ZZS_ZZZ_NKDataGrouping", refCusPreferenceEntityConfiguration.EntityTypes[0].Properties[2].Name);
			Assert.AreEqual("varchar", refCusPreferenceEntityConfiguration.EntityTypes[0].Properties[2].Type);
			Assert.AreEqual("KR", refCusPreferenceEntityConfiguration.EntityTypes[0].Properties[2].ConstantValue);
			Assert.AreEqual(3, refCusPreferenceEntityConfiguration.EntityTypes[0].Properties[2].MaxLength);

			Assert.AreEqual(0, refCusPreferenceEntityConfiguration.EntityTypeExcelColumnMapping.SheetIndex);
			Assert.AreEqual(2, refCusPreferenceEntityConfiguration.EntityTypeExcelColumnMapping.StartRow);

			Assert.AreEqual(1, refCusPreferenceEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes.Length);
			Assert.AreEqual("RefCusPreference", refCusPreferenceEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Name);
			Assert.AreEqual(3, refCusPreferenceEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties.Length);
			Assert.AreEqual("ZZS_Preference", refCusPreferenceEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[0].Name);
			Assert.AreEqual(1, refCusPreferenceEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[0].ExcelColumn);
			Assert.AreEqual("ZZS_Description", refCusPreferenceEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[1].Name);
			Assert.AreEqual(2, refCusPreferenceEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[1].ExcelColumn);

		}

		[Test]
		public void TestExportFTATypeEntityConfiguration()
		{
			var configFilePath = ApplicationConfig.ExportFTATypeConfigFilePath;
			var exportFTATypeEntityConfiguration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFilePath);

			Assert.AreEqual("xls, xlsx", exportFTATypeEntityConfiguration.DataFileExtensions);
			Assert.AreEqual(2, exportFTATypeEntityConfiguration.EntityTypes.Length);

			Assert.AreEqual("RefCusCodeList", exportFTATypeEntityConfiguration.EntityTypes[0].Name);
			Assert.AreEqual(true, exportFTATypeEntityConfiguration.EntityTypes[0].Data);
			Assert.AreEqual(3, exportFTATypeEntityConfiguration.EntityTypes[0].Key.Length);
			Assert.AreEqual("ZZD_ZZK_NKCodeType", exportFTATypeEntityConfiguration.EntityTypes[0].Key[0].Name);
			Assert.AreEqual("ZZD_Code", exportFTATypeEntityConfiguration.EntityTypes[0].Key[1].Name);
			Assert.AreEqual("ZZD_ZZZ_NKDataGrouping", exportFTATypeEntityConfiguration.EntityTypes[0].Key[2].Name);
			Assert.AreEqual(7, exportFTATypeEntityConfiguration.EntityTypes[0].Properties.Length);
			Assert.AreEqual("RefCusCodeListAttribute", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[0].Name);
			Assert.AreEqual("RefCusCodeListAttribute", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[0].Type);
			Assert.AreEqual("ZZD_ZZK_NKCodeType", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[1].Name);
			Assert.AreEqual("varchar", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[1].Type);
			Assert.AreEqual(5, exportFTATypeEntityConfiguration.EntityTypes[0].Properties[1].MaxLength);
			Assert.AreEqual("ZZD_Code", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[2].Name);
			Assert.AreEqual("varchar", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[2].Type);
			Assert.AreEqual(35, exportFTATypeEntityConfiguration.EntityTypes[0].Properties[2].MaxLength);
			Assert.AreEqual("ZZD_Description", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[3].Name);
			Assert.AreEqual("nvarchar", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[3].Type);
			Assert.AreEqual(2000, exportFTATypeEntityConfiguration.EntityTypes[0].Properties[3].MaxLength);
			Assert.AreEqual("ZZD_StartDate", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[4].Name);
			Assert.AreEqual("datetime", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[4].Type);
			Assert.AreEqual("ZZD_EndDate", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[5].Name);
			Assert.AreEqual("datetime", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[5].Type);
			Assert.AreEqual("ZZD_ZZZ_NKDataGrouping", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[6].Name);
			Assert.AreEqual("varchar", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[6].Type);
			Assert.AreEqual(3, exportFTATypeEntityConfiguration.EntityTypes[0].Properties[6].MaxLength);
			Assert.AreEqual("KR", exportFTATypeEntityConfiguration.EntityTypes[0].Properties[6].ConstantValue);

			Assert.AreEqual("RefCusCodeListAttribute", exportFTATypeEntityConfiguration.EntityTypes[1].Name);
			Assert.AreEqual(true, exportFTATypeEntityConfiguration.EntityTypes[1].Data);
			Assert.AreEqual(1, exportFTATypeEntityConfiguration.EntityTypes[1].Key.Length);
			Assert.AreEqual("ZZE_ZXE_NKName", exportFTATypeEntityConfiguration.EntityTypes[1].Key[0].Name);
			Assert.AreEqual(2, exportFTATypeEntityConfiguration.EntityTypes[1].Properties.Length);
			Assert.AreEqual("ZZE_ZXE_NKName", exportFTATypeEntityConfiguration.EntityTypes[1].Properties[0].Name);
			Assert.AreEqual("varchar", exportFTATypeEntityConfiguration.EntityTypes[1].Properties[0].Type);
			Assert.AreEqual(32, exportFTATypeEntityConfiguration.EntityTypes[1].Properties[0].MaxLength);
			Assert.Null(exportFTATypeEntityConfiguration.EntityTypes[1].Properties[0].ConstantValue);
			Assert.AreEqual("ZZE_Value", exportFTATypeEntityConfiguration.EntityTypes[1].Properties[1].Name);
			Assert.AreEqual("nvarchar", exportFTATypeEntityConfiguration.EntityTypes[1].Properties[1].Type);
			Assert.AreEqual(255, exportFTATypeEntityConfiguration.EntityTypes[1].Properties[1].MaxLength);
		}

		[Test]
		public void TestRefCusTradeGroupEntityConfiguration()
		{
			var configFilePath = ApplicationConfig.TradeGroupConfigFilePath;
			var refCusTradeGroupEntityConfiguration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFilePath);

			Assert.AreEqual("xls, xlsx", refCusTradeGroupEntityConfiguration.DataFileExtensions);
			Assert.AreEqual(2, refCusTradeGroupEntityConfiguration.EntityTypes.Length);

			Assert.AreEqual("RefCusTradeGroup", refCusTradeGroupEntityConfiguration.EntityTypes[0].Name);
			Assert.AreEqual(true, refCusTradeGroupEntityConfiguration.EntityTypes[0].Data);
			Assert.AreEqual(2, refCusTradeGroupEntityConfiguration.EntityTypes[0].Key.Length);
			Assert.AreEqual("ZZA_TradeGroup", refCusTradeGroupEntityConfiguration.EntityTypes[0].Key[0].Name);
			Assert.AreEqual("ZZA_ZZZ_NKDataGrouping", refCusTradeGroupEntityConfiguration.EntityTypes[0].Key[1].Name);

			Assert.AreEqual(6, refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties.Length);
			Assert.AreEqual("RefCusTradeGroupCountry", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[0].Name);
			Assert.AreEqual("RefCusTradeGroupCountry", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[0].Type);
			Assert.AreEqual("ZZA_TradeGroup", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[1].Name);
			Assert.AreEqual("varchar", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[1].Type);
			Assert.AreEqual(35, refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[1].MaxLength);
			Assert.AreEqual("ZZA_Description", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[2].Name);
			Assert.AreEqual("nvarchar", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[2].Type);
			Assert.AreEqual(4000, refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[2].MaxLength);
			Assert.AreEqual("ZZA_StartDate", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[3].Name);
			Assert.AreEqual("datetime", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[3].Type);
			Assert.AreEqual("ZZA_EndDate", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[4].Name);
			Assert.AreEqual("datetime", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[4].Type);
			Assert.AreEqual("ZZA_ZZZ_NKDataGrouping", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[5].Name);
			Assert.AreEqual("varchar", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[5].Type);
			Assert.AreEqual("KR", refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[5].ConstantValue);
			Assert.AreEqual(3, refCusTradeGroupEntityConfiguration.EntityTypes[0].Properties[5].MaxLength);

			Assert.AreEqual("RefCusTradeGroupCountry", refCusTradeGroupEntityConfiguration.EntityTypes[1].Name);
			Assert.AreEqual(true, refCusTradeGroupEntityConfiguration.EntityTypes[1].Data);
			Assert.AreEqual(1, refCusTradeGroupEntityConfiguration.EntityTypes[1].Key.Length);
			Assert.AreEqual("ZZB_RN_NKTradeGroupCountryCode", refCusTradeGroupEntityConfiguration.EntityTypes[1].Key[0].Name);

			Assert.AreEqual(3, refCusTradeGroupEntityConfiguration.EntityTypes[1].Properties.Length);
			Assert.AreEqual("ZZB_RN_NKTradeGroupCountryCode", refCusTradeGroupEntityConfiguration.EntityTypes[1].Properties[0].Name);
			Assert.AreEqual("char", refCusTradeGroupEntityConfiguration.EntityTypes[1].Properties[0].Type);
			Assert.AreEqual(2, refCusTradeGroupEntityConfiguration.EntityTypes[1].Properties[0].MaxLength);
			Assert.AreEqual("ZZB_StartDate", refCusTradeGroupEntityConfiguration.EntityTypes[1].Properties[1].Name);
			Assert.AreEqual("datetime", refCusTradeGroupEntityConfiguration.EntityTypes[1].Properties[1].Type);
			Assert.AreEqual("ZZB_EndDate", refCusTradeGroupEntityConfiguration.EntityTypes[1].Properties[2].Name);
			Assert.AreEqual("datetime", refCusTradeGroupEntityConfiguration.EntityTypes[1].Properties[2].Type);

			Assert.AreEqual(0, refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.SheetIndex);
			Assert.AreEqual(1, refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.StartRow);
			Assert.AreEqual(2, refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes.Length);

			Assert.AreEqual("RefCusTradeGroup", refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Name);
			Assert.AreEqual(4, refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties.Length);
			Assert.AreEqual("ZZA_TradeGroup", refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[0].Name);
			Assert.AreEqual(1, refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[0].ExcelColumn);
			Assert.AreEqual("ZZA_Description", refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[1].Name);
			Assert.AreEqual(2, refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[1].ExcelColumn);
			Assert.AreEqual("ZZA_StartDate", refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[2].Name);
			Assert.AreEqual(3, refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[2].ExcelColumn);
			Assert.AreEqual("ZZA_EndDate", refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[3].Name);
			Assert.AreEqual(4, refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[3].ExcelColumn);

			Assert.AreEqual("RefCusTradeGroupCountry", refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Name);
			Assert.AreEqual(3, refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties.Length);
			Assert.AreEqual("ZZB_RN_NKTradeGroupCountryCode", refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties[0].Name);
			Assert.AreEqual(5, refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties[0].ExcelColumn);
			Assert.AreEqual("ZZB_StartDate", refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties[1].Name);
			Assert.AreEqual(6, refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties[1].ExcelColumn);
			Assert.AreEqual("ZZB_EndDate", refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties[2].Name);
			Assert.AreEqual(7, refCusTradeGroupEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties[2].ExcelColumn);
		}

		[Test]
		public void TestKRNomenclatureEntityConfiguration()
		{
			var configFilePath = string.Format(CultureInfo.CurrentCulture, ApplicationConfig.KRNomenclatureConfigFileInputPath, "2017");
			var nomenclatureEntityConfiguration = EntityConfigurationManager.DeserializeXML<EntityConfiguration>(configFilePath);
			Assert.AreEqual("xls, xlsx", nomenclatureEntityConfiguration.DataFileExtensions);
			Assert.AreEqual(2, nomenclatureEntityConfiguration.EntityTypes.Length);

			Assert.AreEqual("RefCusNomenclatureGroup", nomenclatureEntityConfiguration.EntityTypes[0].Name);
			Assert.AreEqual(true, nomenclatureEntityConfiguration.EntityTypes[0].Data);
			Assert.AreEqual(3, nomenclatureEntityConfiguration.EntityTypes[0].Key.Length);
			Assert.AreEqual("ZZ5_Value", nomenclatureEntityConfiguration.EntityTypes[0].Key[0].Name);
			Assert.AreEqual("ZZ5_ZZ9_NKNomenclatureGroupType", nomenclatureEntityConfiguration.EntityTypes[0].Key[1].Name);
			Assert.AreEqual("ZZ5_ZZZ_NKDataGrouping", nomenclatureEntityConfiguration.EntityTypes[0].Key[2].Name);
			Assert.AreEqual(8, nomenclatureEntityConfiguration.EntityTypes[0].Properties.Length);
			Assert.AreEqual("RefCusNomenclatureLanguage", nomenclatureEntityConfiguration.EntityTypes[0].Properties[0].Name);
			Assert.AreEqual("RefCusNomenclatureLanguage", nomenclatureEntityConfiguration.EntityTypes[0].Properties[0].Type);
			Assert.AreEqual("ZZ5_CompositeKey", nomenclatureEntityConfiguration.EntityTypes[0].Properties[1].Name);
			Assert.AreEqual("varchar", nomenclatureEntityConfiguration.EntityTypes[0].Properties[1].Type);
			Assert.AreEqual(100, nomenclatureEntityConfiguration.EntityTypes[0].Properties[1].MaxLength);
			Assert.AreEqual("ZZ5_Description", nomenclatureEntityConfiguration.EntityTypes[0].Properties[2].Name);
			Assert.AreEqual("nvarchar(max)", nomenclatureEntityConfiguration.EntityTypes[0].Properties[2].Type);
			Assert.AreEqual("ZZ5_EndDate", nomenclatureEntityConfiguration.EntityTypes[0].Properties[3].Name);
			Assert.AreEqual("smalldatetime", nomenclatureEntityConfiguration.EntityTypes[0].Properties[3].Type);
			Assert.AreEqual("ZZ5_StartDate", nomenclatureEntityConfiguration.EntityTypes[0].Properties[4].Name);
			Assert.AreEqual("smalldatetime", nomenclatureEntityConfiguration.EntityTypes[0].Properties[4].Type);
			Assert.AreEqual("ZZ5_Value", nomenclatureEntityConfiguration.EntityTypes[0].Properties[5].Name);
			Assert.AreEqual("varchar", nomenclatureEntityConfiguration.EntityTypes[0].Properties[5].Type);
			Assert.AreEqual(15, nomenclatureEntityConfiguration.EntityTypes[0].Properties[5].MaxLength);
			Assert.AreEqual("ZZ5_ZZ9_NKNomenclatureGroupType", nomenclatureEntityConfiguration.EntityTypes[0].Properties[6].Name);
			Assert.AreEqual("varchar", nomenclatureEntityConfiguration.EntityTypes[0].Properties[6].Type);
			Assert.AreEqual(3, nomenclatureEntityConfiguration.EntityTypes[0].Properties[6].MaxLength);
			Assert.AreEqual("KR", nomenclatureEntityConfiguration.EntityTypes[0].Properties[6].ConstantValue);
			Assert.AreEqual("ZZ5_ZZZ_NKDataGrouping", nomenclatureEntityConfiguration.EntityTypes[0].Properties[7].Name);
			Assert.AreEqual("varchar", nomenclatureEntityConfiguration.EntityTypes[0].Properties[7].Type);
			Assert.AreEqual(3, nomenclatureEntityConfiguration.EntityTypes[0].Properties[7].MaxLength);
			Assert.AreEqual("KR", nomenclatureEntityConfiguration.EntityTypes[0].Properties[7].ConstantValue);

			Assert.AreEqual("RefCusNomenclatureLanguage", nomenclatureEntityConfiguration.EntityTypes[1].Name);
			Assert.AreEqual(true, nomenclatureEntityConfiguration.EntityTypes[1].Data);
			Assert.AreEqual(1, nomenclatureEntityConfiguration.EntityTypes[1].Key.Length);
			Assert.AreEqual("ZX8_ZX6_NKLanguage", nomenclatureEntityConfiguration.EntityTypes[1].Key[0].Name);
			Assert.AreEqual(2, nomenclatureEntityConfiguration.EntityTypes[1].Properties.Length);
			Assert.AreEqual("ZX8_Description", nomenclatureEntityConfiguration.EntityTypes[1].Properties[0].Name);
			Assert.AreEqual("nvarchar(max)", nomenclatureEntityConfiguration.EntityTypes[1].Properties[0].Type);
			Assert.AreEqual("ZX8_ZX6_NKLanguage", nomenclatureEntityConfiguration.EntityTypes[1].Properties[1].Name);
			Assert.AreEqual("varchar", nomenclatureEntityConfiguration.EntityTypes[1].Properties[1].Type);
			Assert.AreEqual(3, nomenclatureEntityConfiguration.EntityTypes[1].Properties[1].MaxLength);
			Assert.AreEqual("KO", nomenclatureEntityConfiguration.EntityTypes[1].Properties[1].ConstantValue);

			Assert.AreEqual(0, nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.SheetIndex);
			Assert.AreEqual(1, nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.StartRow);

			Assert.AreEqual(2, nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes.Length);

			Assert.AreEqual("RefCusNomenclatureGroup", nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Name);
			Assert.AreEqual(3, nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties.Length);
			Assert.AreEqual("ZZ5_Value", nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[0].Name);
			Assert.AreEqual(0, nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[0].ExcelColumn);
			Assert.AreEqual("ZZ5_StartDate", nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[1].Name);
			Assert.AreEqual(5, nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[1].ExcelColumn);
			Assert.AreEqual("ZZ5_Description", nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[2].Name);
			Assert.AreEqual(1, nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[0].Properties[2].ExcelColumn);

			Assert.AreEqual("RefCusNomenclatureLanguage", nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Name);
			Assert.AreEqual(1, nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties.Length);
			Assert.AreEqual("ZX8_Description", nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties[0].Name);
			Assert.AreEqual(2, nomenclatureEntityConfiguration.EntityTypeExcelColumnMapping.EntityTypes[1].Properties[0].ExcelColumn);
		}
	}
}
