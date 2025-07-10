using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater
{
	public static class XmlWriterHelper
	{
		public static XmlWriterConfiguration GetRefUNLOCOWriterConfiguration(bool withCoordinates, bool addIATAInformation = true)
		{
			var unlocoConfiguration = new EntityTypeConfiguration<RefUNLOCO>(!withCoordinates);
			unlocoConfiguration.IncludeColumn(x => x.RL_Code, true);

			if (withCoordinates)
			{
				unlocoConfiguration.IncludeColumn(x => x.RL_CoOrdinates, false, IsDataValue.True);
				unlocoConfiguration.IncludeColumn(x => x.RL_GeoLocation, false, IsDataValue.True);
			}
			else
			{
				unlocoConfiguration.IncludeColumn(x => x.RL_PortName, false);
				unlocoConfiguration.IncludeColumn(x => x.RL_NameWithDiacriticals, false);
				unlocoConfiguration.IncludeColumn(x => x.RL_HasAirport, false);
				unlocoConfiguration.IncludeColumn(x => x.RL_HasSeaport, false);
				unlocoConfiguration.IncludeColumn(x => x.RL_HasRail, false);
				unlocoConfiguration.IncludeColumn(x => x.RL_HasRoad, false);
				unlocoConfiguration.IncludeColumn(x => x.RL_HasPost, false);
				unlocoConfiguration.IncludeColumn(x => x.RL_HasBorderCrossing, false);
				unlocoConfiguration.IncludeColumn(x => x.RL_RN_NKCountryCode, false);
				unlocoConfiguration.IncludeColumn(x => x.RL_RW_RN_NKCountryCode, false);
				unlocoConfiguration.IncludeColumn(x => x.RL_RW_NKCode, false);
				unlocoConfiguration.IncludeColumn(x => x.RL_IsActive, false);

				if (addIATAInformation)
				{
					unlocoConfiguration.IncludeColumn(x => x.RL_IATA, false);
					unlocoConfiguration.IncludeColumn(x => x.RL_IATARegionCode, false);
				}
			}

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(unlocoConfiguration);

			return writerConfiguration;
		}

		public static void SetXMLWriter(IXmlWriter xmlWriter, string dataSource, DateTime publicationTime, UpdateType updateType = UpdateType.Full)
		{
			xmlWriter.SetDataSource(dataSource);
			xmlWriter.SetPublicationTime(publicationTime);
			xmlWriter.SetUpdateType(updateType);
		}
	}
}
