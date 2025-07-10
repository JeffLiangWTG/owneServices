using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public static class RefDataRepoFileWriter<T> where T : RefDataRepoModelEntityType
	{
		public static void StoreDataCollection(string dataSourceName, DateTime publicationDate, string outputFilePath, IXmlWriterConfiguration xmlWriterConfiguration, List<T> dataCollection)
		{
			var xmlWriter = new XmlWriter(xmlWriterConfiguration);

			if (!dataCollection.Any())
			{
				ManageExceptionIfNoResult(dataSourceName);
			}

			InitializeWriter(xmlWriter, dataSourceName, publicationDate);
			ExportToXml(xmlWriter, dataCollection, outputFilePath);
		}

		static void ManageExceptionIfNoResult(string dataSourceName) => throw new MissingInfoException($"No {dataSourceName} information was found.");

		static void InitializeWriter(XmlWriter xmlWriter, string dataSourceName, DateTime publicationDate)
		{
			UniversalDataHelper.InitializeWriter(xmlWriter, publicationDate, dataSourceName, UpdateType.Full);
		}

		static void ExportToXml(XmlWriter xmlWriter, IEnumerable<T> collection, string filePath)
		{
			UniversalDataHelper.ExportToXml(xmlWriter, collection, filePath);
		}
	}
}
