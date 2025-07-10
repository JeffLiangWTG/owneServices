using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public static class WriteXmlHelper
	{
		public static void ExportToXMLFile<T>(string outputFile, string dataSource, DateTime publicationDateTime, List<T> refDataList, XmlWriterConfiguration xmlWriterConfig)
			where T : RefDataRepoModelEntityType
		{
			Argument.NotNullOrEmpty(outputFile, nameof(outputFile));
			Argument.NotNull(refDataList, nameof(refDataList));

			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(UpdateType.Full);
			writer.SetDataSource(dataSource);

			foreach (var refData in refDataList)
			{
				writer.PopulateData(refData);
			}
			writer?.SaveXml(outputFile);
		}
	}
}
