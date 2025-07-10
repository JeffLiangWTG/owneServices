using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class ADRXmlWriterConfiguration
	{
		public static XmlWriter GetXmlWriter(DateTime publicationTime)
		{
			var xmlWriter = new XmlWriter(GetWriterConfiguration());
			xmlWriter.SetDataSource(Constants.DataSources.ADR);
			xmlWriter.SetPublicationTime(publicationTime);
			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var undgSubstanceConfiguration = new EntityTypeConfiguration<UNDGSubstanceADR>(true);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_UNNO, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_Variant, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_PSN);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_Class);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_ClassificationCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_PG);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_Labels);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_SpecialProvisions);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_LQMaxAmt);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_LQMaxAmtUQ);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_ExceptedQuantityCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_PackIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_PackProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_MixedPackingProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_ADRTankCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_BulkTankIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_TankVehicle);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_TransportCategory);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_PackingSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_BulkSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_LoadingSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_OperationSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_HazardIDNumber);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_BulkTankSpecProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADR_ADRTankSpecProv);
			undgSubstanceConfiguration.IncludeColumnWithConstantValue(x => x.ADR_IsActive, false, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.UNDGAttributeZZs);

			var undgAttributesConfiguration = new EntityTypeConfiguration<UNDGAttributeZZ>(true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Descriptor);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Index, true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Type, true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Language, true);
			undgAttributesConfiguration.IncludeColumnWithConstantValue(x => x.DAZ_ParentCode, false, Constants.UNDGStandards.ADRDangerousGoodsCode);

			var xmlWriterConfig = new XmlWriterConfiguration();
			xmlWriterConfig.IncludeEntityTypeConfiguration(undgSubstanceConfiguration);
			xmlWriterConfig.IncludeEntityTypeConfiguration(undgAttributesConfiguration);

			return xmlWriterConfig;
		}
	}
}
