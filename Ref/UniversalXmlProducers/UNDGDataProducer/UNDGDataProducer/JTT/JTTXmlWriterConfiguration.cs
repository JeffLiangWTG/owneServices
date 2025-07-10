using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class JTTXmlWriterConfiguration
	{
		public static XmlWriter GetXmlWriter(DateTime publicationTime)
		{
			var xmlWriter = new XmlWriter(GetWriterConfiguration());
			xmlWriter.SetDataSource(Constants.DataSources.JTT);
			xmlWriter.SetPublicationTime(publicationTime);
			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var undgSubstanceConfiguration = new EntityTypeConfiguration<UNDGSubstanceJTT>(true);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_UNNO, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_Variant, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_PSN);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_Class);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_ClassificationCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_PG);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_Labels);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_SpecialProvisions);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_LQMaxAmt);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_LQMaxAmtUQ);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_LQ2MaxAmt);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_LQ2MaxAmtUQ);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_ExceptedQuantityCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_PackIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_PackProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_MixedPackingProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_TankCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_BulkTankIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_BulkTankSpecProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_TankSpecProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_TankVehicle);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_TransportCategory);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_PackingSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_BulkSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_LoadingSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_OperationSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.JTT_HazardIDNumber);
			undgSubstanceConfiguration.IncludeColumnWithConstantValue(x => x.JTT_IsActive, false, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.UNDGAttributeZZs);

			var undgAttributesConfiguration = new EntityTypeConfiguration<UNDGAttributeZZ>(true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Descriptor);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Index, true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Type, true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Language, true);
			undgAttributesConfiguration.IncludeColumnWithConstantValue(x => x.DAZ_ParentCode, false, Constants.UNDGStandards.JTTDangerousGoodsCode);

			var xmlWriterConfig = new XmlWriterConfiguration();
			xmlWriterConfig.IncludeEntityTypeConfiguration(undgSubstanceConfiguration);
			xmlWriterConfig.IncludeEntityTypeConfiguration(undgAttributesConfiguration);

			return xmlWriterConfig;
		}
	}
}
