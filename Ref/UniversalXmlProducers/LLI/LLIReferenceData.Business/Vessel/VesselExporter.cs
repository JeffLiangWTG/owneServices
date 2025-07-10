using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.LLIReferenceData.Business.Vessel;

class VesselExporter
{
	static XmlWriterConfiguration GetRefVesselListWriterConfiguration()
	{
		var configuration = new EntityTypeConfiguration<RefVessel>(true);
		configuration.IncludeColumn(x => x.RV_LloydsNumber, true);
		configuration.IncludeColumn(x => x.RV_Code, false);
		configuration.IncludeColumn(x => x.RV_RadioCallSign, false);
		configuration.IncludeColumn(x => x.RV_YearOfConstruction, false);
		configuration.IncludeColumn(x => x.RV_VesselType, false);
		configuration.IncludeColumn(x => x.RV_RN_NKCountryOfReg, false);
		configuration.IncludeColumnWithConstantValue(x => x.RV_IsActive, false, true);
		configuration.IncludeColumn(x => x.RV_StatusCode, false);
		configuration.IncludeColumn(x => x.RV_StatCode5, false);
		configuration.IncludeColumn(x => x.RV_MaritimeMobileServiceIdentity, false);
		configuration.IncludeColumn(x => x.RV_TEU, false);
		configuration.IncludeColumn(x => x.RV_Breadth, false);
		configuration.IncludeColumn(x => x.RV_Length, false);
		configuration.IncludeColumn(x => x.RV_Deadweight, false);
		configuration.IncludeColumn(x => x.RV_GrossTonnage, false);
		configuration.IncludeColumn(x => x.RV_Draught, false);
		configuration.IncludeColumn(x => x.RV_IsGearless, false);
		configuration.IncludeColumn(x => x.RV_GrainCapacity, false);
		configuration.IncludeColumn(x => x.RV_LiquidCapacity, false);
		configuration.IncludeColumn(x => x.RV_CarsNumber, false);
		configuration.IncludeColumn(x => x.RV_TanksNumber, false);
		configuration.IncludeColumn(x => x.RV_ReeferPointsNumber, false);
		configuration.IncludeColumn(x => x.RV_RoroLanesClearHeight, false);
		configuration.IncludeColumn(x => x.RV_RoroLanesWidth, false);
		configuration.IncludeColumn(x => x.RV_RoroLanesNumber, false);
		configuration.IncludeColumn(x => x.RV_RoroRampsNumber, false);
		configuration.IncludeColumn(x => x.RV_RoroLanesLength, false);

		var writerConfiguration = new XmlWriterConfiguration();
		writerConfiguration.IncludeEntityTypeConfiguration(configuration);

		return writerConfiguration;
	}

	static XmlWriter GenerateXmlWriter(string dataSource, DateTime publicationDateTime, IEnumerable<RefVessel> vessels)
	{
		if (vessels == null)
		{
			throw new ArgumentNullException(nameof(vessels));
		}

		var writer = new XmlWriter(GetRefVesselListWriterConfiguration());
		writer.SetDataSource(dataSource);
		writer.SetPublicationTime(publicationDateTime);
		writer.SetUpdateType(UpdateType.Full);

		foreach (var vessel in vessels)
		{
			writer.PopulateData(vessel);
		}

		return writer;
	}

	public static void ExportToXMLFile(string outputFile, string dataSource, DateTime publicationDateTime, IEnumerable<RefVessel> vessels)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(outputFile);
		ArgumentNullException.ThrowIfNull(vessels);

		Console.WriteLine("Starting exporting vessels to XML.");

		var writer = GenerateXmlWriter(dataSource, publicationDateTime, vessels);
		if (writer != null)
		{
			var directoryName = Path.GetDirectoryName(outputFile);

			if (string.IsNullOrEmpty(directoryName))
			{
				throw new ArgumentException($"Argument {nameof(outputFile)} with value '{outputFile}' does not include directory information.");
			}

			Directory.CreateDirectory(directoryName);
			writer.SaveXml(outputFile);
		}

		Console.WriteLine("Finished exporting vessels to XML.");
	}
}
