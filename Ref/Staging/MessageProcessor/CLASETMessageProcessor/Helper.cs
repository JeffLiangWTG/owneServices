using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor
{
	public static class Helper
	{
		public static (XmlWriterConfiguration writerConfiguration, EntityTypeConfiguration<RefCusCodeList> codeListConfiguration) GetRefCusCodeListWriterConfiguration(string codeType, string dataGrouping)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, dataGrouping);
			codeListConfiguration.IncludeColumn(x => x.ZZD_StartDate, false);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			return (writerConfiguration, codeListConfiguration);
		}

		public static EntityTypeConfiguration<RefCusCodeListAttribute> AddRefCusCodeListAttributeConfiguration(XmlWriterConfiguration writerConfiguration, EntityTypeConfiguration<RefCusCodeList> codeListConfiguration)
		{
			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			var codelistAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistAttributeConfiguration);
			return codelistAttributeConfiguration;
		}

		public static (XmlWriterConfiguration writerConfiguration, EntityTypeConfiguration<RefCusProcedure> procedureConfiguration) GetRefCusProcedureWriterConfiguration(string dataGrouping)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var procedureConfiguration = new EntityTypeConfiguration<RefCusProcedure>(true);
			procedureConfiguration.IncludeColumn(x => x.ZZ6_Concession, true);
			procedureConfiguration.IncludeColumn(x => x.ZZ6_ProcedureCode, true);
			procedureConfiguration.IncludeColumn(x => x.ZZ6_StartDate, false);
			procedureConfiguration.IncludeColumn(x => x.ZZ6_Description, false);
			procedureConfiguration.IncludeColumn(x => x.ZZ6_EndDate, false);
			procedureConfiguration.IncludeColumn(x => x.ZZ6_Group, false);
			procedureConfiguration.IncludeColumn(x => x.ZZ6_ShipmentType, false);
			procedureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ6_ZZZ_NKDataGrouping, true, dataGrouping);
			procedureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ6_PreviousProcedureCode, true, string.Empty);
			procedureConfiguration.IncludeColumnWithConstantValue(x => x.ZZ6_Category, true, string.Empty);
			writerConfiguration.IncludeEntityTypeConfiguration(procedureConfiguration);
			return (writerConfiguration, procedureConfiguration);
		}

		public static EntityTypeConfiguration<RefCusProcedureAttribute> AddRefCusProcedureAttributeConfiguration(XmlWriterConfiguration writerConfiguration, EntityTypeConfiguration<RefCusProcedure> procedureCfg)
		{
			procedureCfg.IncludeColumn(x => x.RefCusProcedureAttributes, false);

			var procedureAttributeConfiguration = new EntityTypeConfiguration<RefCusProcedureAttribute>(true);
			procedureAttributeConfiguration.IncludeColumn(x => x.ZXB_Name, true);
			procedureAttributeConfiguration.IncludeColumn(x => x.ZXB_Value, false);
			writerConfiguration.IncludeEntityTypeConfiguration(procedureAttributeConfiguration);

			return procedureAttributeConfiguration;
		}

		public static (XmlWriterConfiguration writerConfiguration, EntityTypeConfiguration<RefCusTariff> tariffCfg) GetRefCusTariffWriterConfiguration(string tariffType, string dataGrouping)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var tariffCfg = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffCfg.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffCfg.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, tariffType);
			tariffCfg.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, dataGrouping);
			tariffCfg.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, dataGrouping);
			tariffCfg.IncludeColumn(nameof(RefCusTariffRelationship), true);
			tariffCfg.IncludeColumn(x => x.ZZ1_Description);
			tariffCfg.IncludeColumn(x => x.ZZ1_StartDate);
			tariffCfg.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			writerConfiguration.IncludeEntityTypeConfiguration(tariffCfg);
			return (writerConfiguration, tariffCfg);
		}

		public static EntityTypeConfiguration<RefCusTariffAttribute> AddRefCusTariffAttributeConfiguration(XmlWriterConfiguration writerConfiguration, EntityTypeConfiguration<RefCusTariff> tariffCfg)
		{
			tariffCfg.IncludeColumn(x => x.RefCusTariffAttributes, false);
			var tariffAttributeConfiguration = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Name, true);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Value, false);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffAttributeConfiguration);
			return tariffAttributeConfiguration;
		}

		public static EntityTypeConfiguration<RefCusTariffRelationship> AddRefCusTariffRelationshipConfiguration(XmlWriterConfiguration writerConfiguration, EntityTypeConfiguration<RefCusTariff> tariffCfg, string tariffType, string dataGrouping)
		{
			tariffCfg.IncludeColumn(x => x.RefCusTariffRelationships, false);
			var relationshipCfg = new EntityTypeConfiguration<RefCusTariffRelationship>(true);
			relationshipCfg.IncludeColumn(x => x.ZZH_TariffCode, true);
			relationshipCfg.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_NKTariffType, true, tariffType);
			relationshipCfg.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_ZZZ_NKDataGrouping, true, dataGrouping);
			writerConfiguration.IncludeEntityTypeConfiguration(relationshipCfg);
			return relationshipCfg;
		}

		public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, UpdateType updateType, IEnumerable<T> dataList)
			where T : RefDataRepoModelEntityType
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var data in dataList)
			{
				writer.PopulateData(data);
			}
			writer.SaveXml(outputFile);
		}

		public static DateTime GetValidValue(DateTime date)
		{
			var result = date;
			var smallDateMaxValue = SmallDateMaxValue;
			if (result > smallDateMaxValue)
			{
				result = smallDateMaxValue;
			}
			else
			{
				var smallDateMinValue = SmallDateMinValue;
				if (result < smallDateMinValue)
				{
					result = smallDateMinValue;
				}
			}
			return result;
		}

		public static DateTime SmallDateMaxValue => new DateTime(2079, 06, 06, 23, 59, 00);
		public static DateTime SmallDateMinValue => new DateTime(1900, 01, 01, 00, 00, 00);
	}
}
