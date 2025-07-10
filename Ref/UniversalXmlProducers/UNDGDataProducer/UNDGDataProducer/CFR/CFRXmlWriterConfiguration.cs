using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class CFRXmlWriterConfiguration
	{
		public static IXmlWriter GetXmlWriter(DateTime publicationTime)
		{
			var configuration = GetWriterConfiguration();
			var xmlWriter = new XmlWriter(configuration);
			xmlWriter.SetDataSource(Constants.DataSources.CFR);
			xmlWriter.SetPublicationTime(publicationTime);

			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var undgSubstanceCFRConfiguration = new EntityTypeConfiguration<UNDGSubstanceCFR>(true);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_UNNO, true);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_Variant, true);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_Prefix);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_CVL);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_PSN);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_Variation);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_PrimaryClass);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_SecondaryClass);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_TertiaryClass);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_MarinePollutant);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_ExceptedQuantity);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_LimitedQuantityPermitted);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_LQMaxAmt);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_LQMaxAmtUQ);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_ReportableQuantity);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_ReportableQuantityUnit);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_GeneralStowage);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_PassengerStowage);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_StowageCategory);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_StowageCodes);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_StowageIMDGCodes);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_BulkPackingInstructions);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_BulkPackingProvisions);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_IBCInstructions);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_IBCProvisions);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_PackingExceptions);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_PackingInstructions);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_PackingProvisions);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_PackingGroup);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_SpecialProvisions);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_TankInstructions);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_TankProvisions);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_PoisonInhalationHazard);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_State);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_IsFixedPSN);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_AppliesForAirTransport);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_AppliesForDomesticTransport);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_AppliesForInternationalTransport);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_AppliesForVesselTransport);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_RequiresTechnicalNameInParenthesis);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_EmergencyResponseGuide);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_TechnicalName);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_TreatAs);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_PAXAirRailLimitType);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_CargoAirRailLimitType);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_PAXAirRailLimit);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_PAXAirRailLimitUnit);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_CargoAirRailLimit);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_CargoAirRailLimitUnit);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_SecondaryPAXAirRailLimit);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_SecondaryPAXAirRailLimitUnit);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_SecondaryCargoAirRailLimit);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.CFR_SecondaryCargoAirRailLimitUnit);
			undgSubstanceCFRConfiguration.IncludeColumnWithConstantValue(x => x.CFR_IsActive, false, true);
			undgSubstanceCFRConfiguration.IncludeColumn(x => x.UNDGAttributeZZs);

			var undgAttributesConfiguration = new EntityTypeConfiguration<UNDGAttributeZZ>(true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Descriptor);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Index, true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Type, true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Language, true);
			undgAttributesConfiguration.IncludeColumnWithConstantValue(x => x.DAZ_ParentCode, false, Constants.UNDGStandards.CFRDangerousGoodsCode);

			var xmlWriterConfiguration = new XmlWriterConfiguration();
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(undgSubstanceCFRConfiguration);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(undgAttributesConfiguration);

			return xmlWriterConfiguration;
		}
	}
}
