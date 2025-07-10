using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.VNReferenceData.Business
{
	public static class XmlHelper
	{
		public static XmlWriterConfiguration GetRefCusCodeListWriterConfiguration(string codeType, DateTime publicationDateTime)
		{
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, publicationDateTime);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 0));
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "VN");

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);

			return writerConfiguration;
		}

		public static XmlWriter GenerateXmlWriter(string dataSource, string codeType, DateTime publicationDateTime, List<RefCusCodeList> refCusCodeList)
		{
			Argument.NotNull(refCusCodeList, nameof(refCusCodeList));

			var xmlWriterConfig = GetRefCusCodeListWriterConfiguration(codeType, publicationDateTime);
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(UpdateType.Full);

			foreach (var code in refCusCodeList)
			{
				writer.PopulateData(code);
			}

			return writer;
		}

		public static void ExportToXMLFile(string outputFile, string dataSource, string codeType, DateTime publicationDateTime, List<RefCusCodeList> refCusCodeList)
		{
			Argument.NotNullOrEmpty(outputFile, nameof(outputFile));
			Argument.NotNull(refCusCodeList, nameof(refCusCodeList));

			var writer = GenerateXmlWriter(dataSource, codeType, publicationDateTime, refCusCodeList);
			if (writer != null)
			{
				var directoryName = Path.GetDirectoryName(outputFile);
				Directory.CreateDirectory(directoryName);
				writer.SaveXml(outputFile);
			}
		}
	}
}
