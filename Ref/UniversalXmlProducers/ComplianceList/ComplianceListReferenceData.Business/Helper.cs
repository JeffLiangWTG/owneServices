using System;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ComplianceListReferenceData.Business
{
	public static class Helper
	{
		static XmlWriterConfiguration GetConfiguration(DateTime publicationDate)
		{
			var complianceListConfiguration = new EntityTypeConfiguration<RefComplianceList>(true);
			complianceListConfiguration.IncludeColumn(x => x.RCL_IsActive, false);
			complianceListConfiguration.IncludeColumn(x => x.RCL_ListCode, true);
			complianceListConfiguration.IncludeColumn(x => x.RCL_ListName, false);
			complianceListConfiguration.IncludeColumn(x => x.RCL_ListDescription, false);
			complianceListConfiguration.IncludeColumn(x => x.RCL_ListPublisher, false);
			complianceListConfiguration.IncludeColumn(x => x.RCL_ListType, false);
			complianceListConfiguration.IncludeColumn(x => x.RCL_PublisherJurisdiction, false);
			complianceListConfiguration.IncludeColumn(x => x.RCL_PublisherDescription, false);
			complianceListConfiguration.IncludeColumn(x => x.RCL_MainSourceURL, false);
			complianceListConfiguration.IncludeColumn(x => x.RCL_SecondarySourceURL, false);
			complianceListConfiguration.IncludeColumn(x => x.RCL_IntegrationDate, false);
			complianceListConfiguration.IncludeColumn(x => x.RCL_LastUpdatedDate, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(complianceListConfiguration);
			return writerConfiguration;
		}

		public static XmlWriter GenerateXmlWriter(DateTime publicationDate)
		{
			var writer = new XmlWriter(GetConfiguration(publicationDate));
			writer.SetDataSource("Compliance List");
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
