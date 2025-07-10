using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineProducer
{
	public static class XmlWriterHelper
	{
		public static void ExportToXml<T>(IXmlWriter xmlWriter, IEnumerable<T> collection, string filePath) where T : RefDataRepoModelEntityType
		{
			foreach (var data in collection)
			{
				xmlWriter.PopulateData(data);
			}
			xmlWriter.SaveXml(filePath);
		}

		public static void SetPublicationTime(this IXmlWriter xmlWriter, string filePath)
		{
			var fileInfo = new FileInfo(filePath);
			var publicationTime = fileInfo.CreationTimeUtc;
			xmlWriter.SetPublicationTime(publicationTime);
		}
	}
}
