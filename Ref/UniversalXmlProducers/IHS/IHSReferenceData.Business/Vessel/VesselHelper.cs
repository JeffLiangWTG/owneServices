using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.IHSReferenceData.Business.Vessel
{
	class VesselHelper
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
			if (outputFile == null)
			{
				throw new ArgumentNullException(nameof(outputFile));
			}

			if (vessels == null)
			{
				throw new ArgumentNullException(nameof(vessels));
			}

			if (string.IsNullOrWhiteSpace(outputFile))
			{
				throw new ArgumentException($"Invalid argument: {nameof(outputFile)}");
			}

			var writer = GenerateXmlWriter(dataSource, publicationDateTime, vessels);
			if (writer != null)
			{
				var directoryName = Path.GetDirectoryName(outputFile);
				if (string.IsNullOrEmpty(directoryName))
				{
					throw new ArgumentException(nameof(directoryName));
				}
				Directory.CreateDirectory(directoryName);
				writer.SaveXml(outputFile);
			}
		}

		public static string GetCWVesselType(string ihsType)
		{
			switch (ihsType)
			{
				case "A33A2CC":
				case "A33A2CR":
				case "A33B2CP":
				{
					return "CNT";
				}

				case "A35A2RT":
				case "A35A2RR":
				case "A35B2RV":
				case "A35C2RC":
				case "A35D2RL":
				case "A36A2PR":
				case "A36A2PT":
				case "A36B2PL":
				{
					return "ROR";
				}

				default:
				{
					return "CV";
				}
			}
		}
	}
}
