using System;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared
{
	public sealed class XmlHelper
	{
		public static XmlWriter GenerateXmlWriter(IEntityTypeConfiguration entityTypeConfiguration, DateTime publicationDate, string dataSource)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(entityTypeConfiguration);

			var writer = new XmlWriter(writerConfiguration);
			writer.SetPublicationTime(publicationDate);
			writer.SetUpdateType(UpdateType.Full);
			writer.SetDataSource(dataSource);

			return writer;
		}

		public static void ExportToXMLFile(XmlWriter writer, string outputFile)
		{
			ArgumentNullException.ThrowIfNull(writer, nameof(writer));
			ArgumentException.ThrowIfNullOrEmpty(outputFile, nameof(outputFile));

			var directoryName = Path.GetDirectoryName(outputFile);

			if (directoryName != null && !Path.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}

			writer.SaveXml(outputFile);
		}
	}
}
