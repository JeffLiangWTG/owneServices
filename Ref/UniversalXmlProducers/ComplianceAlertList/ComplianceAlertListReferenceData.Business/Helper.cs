using System;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ComplianceAlertListReferenceData.Business
{
	public static class Helper
	{
		static XmlWriterConfiguration GetConfiguration(DateTime publicationDate)
		{
			var complianceListConfiguration = new EntityTypeConfiguration<RefComplianceCommodityAlert>(true);
			complianceListConfiguration.IncludeColumn(x => x.RCR_AlertCode, true);
			complianceListConfiguration.IncludeColumn(x => x.RCR_AlertDescription, false);
			complianceListConfiguration.IncludeColumn(x => x.RCR_AlertName, false);
			complianceListConfiguration.IncludeColumn(x => x.RCR_AlertType, false);
			complianceListConfiguration.IncludeColumn(x => x.RCR_CountryRegion, true);
			complianceListConfiguration.IncludeColumn(x => x.RCR_PublishYear, false);
			complianceListConfiguration.IncludeColumn(x => x.RCR_SourceURL, false);
			complianceListConfiguration.IncludeColumn(x => x.RCR_TradeDirection, true);
			complianceListConfiguration.IncludeColumn(x => x.RCR_IsActive, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(complianceListConfiguration);
			return writerConfiguration;
		}

		public static XmlWriter GenerateXmlWriter(DateTime publicationDate)
		{
			var writer = new XmlWriter(GetConfiguration(publicationDate));
			writer.SetDataSource("Compliance Commodity Alert List");
			writer.SetPublicationTime(publicationDate);
			writer.SetUpdateType(UpdateType.Full);
			return writer;
		}

		public static void ExportToXMLFile(XmlWriter writer, string outputFile)
		{
			Argument.NotNull(writer, nameof(writer));
			Argument.NotNullOrEmpty(outputFile, nameof(outputFile));

			var directoryName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(directoryName);
			writer.SaveXml(outputFile);
		}
	}
}
