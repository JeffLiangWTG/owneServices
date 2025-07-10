using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineProducer
{
	public static class RefAirlineXmlConfiguration
	{
		public static XmlWriter GetXmlWriter(string filePath, bool containMultipleKeys)
		{
			var xmlWriter = new XmlWriter(GetWriterConfiguration(containMultipleKeys));
			if (containMultipleKeys)
			{
				xmlWriter.SetDataSource(Constants.AirlineWithMultipleKeysDataSource);
			}
			else
			{
				xmlWriter.SetDataSource(Constants.AirlineName1DataSource);
			}
			xmlWriter.SetPublicationTime(filePath);
			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static IXmlWriterConfiguration GetWriterConfiguration(bool containMultipleKeys)
		{
			var airlineConfiguration = new EntityTypeConfiguration<RefAirline>(true, false);
			if (containMultipleKeys)
			{
				airlineConfiguration.IncludeColumn(x => x.RM_EagleAddedAirlinePrefixOrAccountingCode, true, order: 0);
				airlineConfiguration.IncludeColumn(x => x.RM_ThreeLetterCode, true, order: 1);
				airlineConfiguration.IncludeColumn(x => x.RM_AccountingCode);
				airlineConfiguration.IncludeColumn(x => x.RM_AirlinePrefix);
				airlineConfiguration.IncludeColumn(x => x.RM_AirlineName1);
			}
			else
			{
				airlineConfiguration.IncludeColumn(x => x.RM_AirlineName1, true);
			}

			airlineConfiguration.IncludeColumn(x => x.RM_AirlineName2);
			airlineConfiguration.IncludeColumn(x => x.RM_TwoCharacterCode);
			airlineConfiguration.IncludeColumn(x => x.RM_DuplicateFlagIndicator);
			airlineConfiguration.IncludeColumn(x => x.RM_AddressLine1);
			airlineConfiguration.IncludeColumn(x => x.RM_AddressLine2);
			airlineConfiguration.IncludeColumn(x => x.RM_AirlineCity);
			airlineConfiguration.IncludeColumn(x => x.RM_AirlineState);
			airlineConfiguration.IncludeColumn(x => x.RM_AirlineCountry);
			airlineConfiguration.IncludeColumn(x => x.RM_AirlinePostalCode);
			airlineConfiguration.IncludeColumn(x => x.RM_ReservationsDeptTeletype);
			airlineConfiguration.IncludeColumn(x => x.RM_ReservationsContactName);
			airlineConfiguration.IncludeColumn(x => x.RM_ReservationsContactTitle);
			airlineConfiguration.IncludeColumn(x => x.RM_ReservationsContactTeletype);
			airlineConfiguration.IncludeColumn(x => x.RM_EmergencyTeletype);
			airlineConfiguration.IncludeColumn(x => x.RM_EmergencyContactName);
			airlineConfiguration.IncludeColumn(x => x.RM_EmergencyContactTitle);
			airlineConfiguration.IncludeColumn(x => x.RM_MembershipFlagSITA);
			airlineConfiguration.IncludeColumn(x => x.RM_MembershipFlagARINC);
			airlineConfiguration.IncludeColumn(x => x.RM_MembershipFlagIATA);
			airlineConfiguration.IncludeColumn(x => x.RM_MembershipFlagATA);
			airlineConfiguration.IncludeColumn(x => x.RM_TypeOfOperationsCode);
			airlineConfiguration.IncludeColumn(x => x.RM_AccountingSecondaryFlag);
			airlineConfiguration.IncludeColumn(x => x.RM_AirlinePrefixSecondaryFlag);
			airlineConfiguration.IncludeColumnWithConstantValue(x => x.RM_IsActive, false, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(airlineConfiguration);

			return writerConfiguration;
		}
	}
}
