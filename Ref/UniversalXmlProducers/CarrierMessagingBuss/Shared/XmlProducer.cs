using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared
{
	public class XmlProducer<T>(
		IEntityTypeConfiguration entityTypeConfiguration,
		DateTime publicationDate,
		string dataSource,
		IEnumerable<T> sourceList,
		string exportFilePath) where T : class
	{
		public void ExportXml()
		{
			var writer = XmlHelper.GenerateXmlWriter(entityTypeConfiguration, publicationDate, dataSource);
			foreach (var source in sourceList)
			{
				writer.PopulateData(source);
			}
			writer.Validate();
			XmlHelper.ExportToXMLFile(writer, exportFilePath);
		}
	}
}
