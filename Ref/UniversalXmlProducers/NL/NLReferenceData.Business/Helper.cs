using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public static class Helper
	{
		public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, UpdateType updateType, IEnumerable<T> codeList)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			foreach (var code in codeList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputFile);
		}
	}
}
