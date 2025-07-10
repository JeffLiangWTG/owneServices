using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.AEReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.AEReferenceData.Business;

public static class XmlWriterHelper
{
	public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, List<T> dataList, UpdateType updateType = UpdateType.Full)
	{
		var writer = new XmlWriter(xmlWriterConfig);
		writer.SetDataSource(dataSource);
		writer.SetPublicationTime(publicationDateTime);
		writer.SetUpdateType(updateType);

		var dirName = Path.GetDirectoryName(outputFile);
		Directory.CreateDirectory(dirName);

		foreach (var code in dataList)
		{
			writer.PopulateData(code);
		}
		writer.SaveXml(outputFile);
	}

	public static XmlWriterConfiguration GetRefCusCodeListWriterConfiguration(string defalutCodeType = null, string defaultDataGrouping = null)
	{
		var refCusCodeList = GetSimpleRefCusCodeListConfiguration(defalutCodeType, defaultDataGrouping);
		var writerConfiguration = new XmlWriterConfiguration();
		writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);

		return writerConfiguration;
	}

	public static XmlWriterConfiguration GetRefCusCodeListForCustomsOfficesWriterConfiguration(string codeType, string dataGrouping)
	{
		var writerConfiguration = new XmlWriterConfiguration();

		var refCusCodeListConfiguration = GetSimpleRefCusCodeListConfiguration(codeType, dataGrouping);
		refCusCodeListConfiguration.IncludeColumn(x => x.RefCusCodeListLanguages, false);
		refCusCodeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);

		writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeListConfiguration);
		writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusCodeListLanguageConfiguration());
		writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusCodeListAttributeConfiguration());

		return writerConfiguration;
	}

	static EntityTypeConfiguration<RefCusCodeList> GetSimpleRefCusCodeListConfiguration(string codeType = null, string dataGrouping = null)
	{
		var simpleRefCusCodeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);

		simpleRefCusCodeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
		simpleRefCusCodeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
		simpleRefCusCodeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, DefaultValues.MinDateTime);
		simpleRefCusCodeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, DefaultValues.MaxDateTime);

		if (string.IsNullOrEmpty(codeType))
		{
			simpleRefCusCodeListConfiguration.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
		}
		else
		{
			simpleRefCusCodeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
		}

		if (string.IsNullOrEmpty(dataGrouping))
		{
			simpleRefCusCodeListConfiguration.IncludeColumn(x => x.ZZD_ZZZ_NKDataGrouping, true);
		}
		else
		{
			simpleRefCusCodeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, dataGrouping);
		}
		return simpleRefCusCodeListConfiguration;
	}

	static EntityTypeConfiguration<RefCusCodeListLanguage> GetRefCusCodeListLanguageConfiguration()
	{
		var configuration = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);

		configuration.IncludeColumnWithConstantValue(x => x.ZXA_ZX6_NKLanguage, true, RefLanguageTypes.Arabic);
		configuration.IncludeColumn(x => x.ZXA_Description, false);

		return configuration;
	}

	static EntityTypeConfiguration<RefCusCodeListAttribute> GetRefCusCodeListAttributeConfiguration()
	{
		var configuration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);

		configuration.IncludeColumn(x => x.ZZE_Value, true);
		configuration.IncludeColumnWithConstantValue(x => x.ZZE_ZXE_NKName, true, DefaultValues.GCCCode);

		return configuration;
	}

	public static XmlWriterConfiguration GetRefCusProcedureWriterConfiguration()
	{
		var writerConfiguration = new XmlWriterConfiguration();

		var refCusProcedureConfiguration = GetRefCusProcedureConfiguration();
		refCusProcedureConfiguration.IncludeColumn(x => x.RefCusProcedureAttributes, isKeyColumn: false);

		writerConfiguration.IncludeEntityTypeConfiguration(refCusProcedureConfiguration);
		writerConfiguration.IncludeEntityTypeConfiguration(GetRefCusProcedureAttributeConfiguration());

		return writerConfiguration;
	}

	static EntityTypeConfiguration<RefCusProcedure> GetRefCusProcedureConfiguration()
	{
		var configuration = new EntityTypeConfiguration<RefCusProcedure>(true);

		configuration.IncludeColumnWithConstantValue(x => x.ZZ6_ZZZ_NKDataGrouping, isKeyColumn: true, constantValue: DataGrouping.Dubai);
		configuration.IncludeColumn(x => x.ZZ6_ProcedureCode, isKeyColumn: true);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_PreviousProcedureCode, isKeyColumn: true, defaultValue: string.Empty);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_Concession, isKeyColumn: true, defaultValue: string.Empty);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_StartDate, isKeyColumn: false, defaultValue: DefaultValues.MinDateTime);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_EndDate, isKeyColumn: false, defaultValue: DefaultValues.MaxDateTime);
		configuration.IncludeColumn(x => x.ZZ6_Description, isKeyColumn: false);
		configuration.IncludeColumn(x => x.ZZ6_ShipmentType, isKeyColumn: false);
		configuration.IncludeColumn(x => x.ZZ6_CalculateDuty, isKeyColumn: false);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_LandedCost, isKeyColumn: false, defaultValue: 0);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_CalculateVAT, isKeyColumn: false, defaultValue: false);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoWarehouse, isKeyColumn: false, defaultValue: DefaultValues.NotApplicableXmlValue);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_OutOfWarehouse, isKeyColumn: false, defaultValue: DefaultValues.NotApplicableXmlValue);
		configuration.IncludeColumn(x => x.ZZ6_IntoTemporaryImport, isKeyColumn: false);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_OutOfTemporaryImport, isKeyColumn: false, defaultValue: DefaultValues.NotApplicableXmlValue);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoTemporaryExport, isKeyColumn: false, defaultValue: DefaultValues.NotApplicableXmlValue);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_OutOfTemporaryExport, isKeyColumn: false, defaultValue: DefaultValues.NotApplicableXmlValue);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoInwardProcessing, isKeyColumn: false, defaultValue: DefaultValues.NotApplicableXmlValue);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_OutOfInwardProcessing, isKeyColumn: false, defaultValue: DefaultValues.NotApplicableXmlValue);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoOutwardProcessing, isKeyColumn: false, defaultValue: DefaultValues.NotApplicableXmlValue);
		configuration.IncludeColumnWithDefaultValue(x => x.ZZ6_OutofOutwardProcessing, isKeyColumn: false, defaultValue: DefaultValues.NotApplicableXmlValue);
		configuration.IncludeColumn(x => x.ZZ6_Category, isKeyColumn: false);
		configuration.IncludeColumnWithConstantValue(x => x.ZZ6_Group, isKeyColumn: false, constantValue: string.Empty);
		configuration.IncludeColumn(x => x.ZZ6_IsTransit, isKeyColumn: false);
		configuration.IncludeColumnWithConstantValue(x => x.ZZ6_IsGuaranteeConsumed, isKeyColumn: false, constantValue: DefaultValues.NotApplicableXmlValue);
		configuration.IncludeColumnWithConstantValue(x => x.ZZ6_IsGuaranteeReleased, isKeyColumn: false, constantValue: DefaultValues.NotApplicableXmlValue);
		configuration.IncludeColumnWithConstantValue(x => x.ZZ6_IntoVATWarehouse, isKeyColumn: false, constantValue: DefaultValues.NotApplicableXmlValue);
		configuration.IncludeColumnWithConstantValue(x => x.ZZ6_OutOfVATWarehouse, isKeyColumn: false, constantValue: DefaultValues.NotApplicableXmlValue);

		return configuration;
	}

	static EntityTypeConfiguration<RefCusProcedureAttribute> GetRefCusProcedureAttributeConfiguration()
	{
		var configuration = new EntityTypeConfiguration<RefCusProcedureAttribute>(true);

		configuration.IncludeColumnWithConstantValue(x => x.ZXB_Name, isKeyColumn: true, constantValue: DefaultValues.DeclarationTypeCode);
		configuration.IncludeColumn(x => x.ZXB_Value, isKeyColumn: true);

		return configuration;
	}
}
