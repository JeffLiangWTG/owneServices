using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class RIDXmlWriterConfiguration
	{
		public static XmlWriter GetXmlWriter(DateTime publicationTime)
		{
			var xmlWriter = new XmlWriter(GetWriterConfiguration());
			xmlWriter.SetDataSource(Constants.DataSources.RID);
			xmlWriter.SetPublicationTime(publicationTime);
			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var undgSubstanceConfiguration = new EntityTypeConfiguration<UNDGSubstanceRID>(true);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_UNNO, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_Variant, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_PSN);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_Class);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_ClassificationCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_PG);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_Labels);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_SpecialProvisions);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_LQMaxAmt);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_LQMaxAmtUQ);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_LQ2MaxAmt);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_LQ2MaxAmtUQ);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_ExceptedQuantityCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_PackIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_IBCIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_PackProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_MixedPackProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_TankCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_BulkContainerTankIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_BulkContainerTankProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_TankSpecProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_TransportCategory);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_CarriagePackagesSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_CarriageBulkSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_CarriageLoadingSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_ColisExpressCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.RID_HazardIDNumber);
			undgSubstanceConfiguration.IncludeColumnWithConstantValue(x => x.RID_IsActive, false, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.UNDGAttributeZZs);

			var undgAttributesConfiguration = new EntityTypeConfiguration<UNDGAttributeZZ>(true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Descriptor);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Index, true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Type, true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Language, true);
			undgAttributesConfiguration.IncludeColumnWithConstantValue(x => x.DAZ_ParentCode, false, Constants.UNDGStandards.RIDDangerousGoodsCode);

			var xmlWriterConfig = new XmlWriterConfiguration();
			xmlWriterConfig.IncludeEntityTypeConfiguration(undgSubstanceConfiguration);
			xmlWriterConfig.IncludeEntityTypeConfiguration(undgAttributesConfiguration);

			return xmlWriterConfig;
		}
	}
}
