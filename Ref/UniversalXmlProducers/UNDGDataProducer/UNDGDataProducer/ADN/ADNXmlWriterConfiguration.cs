using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class ADNXmlWriterConfiguration
	{
		public static XmlWriter GetXmlWriter(DateTime publicationTime)
		{
			var xmlWriter = new XmlWriter(GetWriterConfiguration());
			xmlWriter.SetDataSource(Constants.DataSources.ADN);
			xmlWriter.SetPublicationTime(publicationTime);
			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var undgSubstanceConfiguration = new EntityTypeConfiguration<UNDGSubstanceADN>(true);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_UNNO, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_Variant, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_PSN);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_Class);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_ClassificationCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_PG);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_Labels);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_ExceptedQuantityCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_LQMaxAmt);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_LQMaxAmtUQ);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_LQ2MaxAmt);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_LQ2MaxAmtUQ);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_CarriagePermittedDetails);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_CarriagePermittedTanks);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_CarriagePermittedBulk);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_CarriagePermittedPacks);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_SpecialProvisions);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_OperationSpecialProvNote);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_OperationSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_UnloadingSpecialProvNote);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_UnloadingSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_LoadingSpecialProvNote);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_LoadingSpecialProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_Ventilation);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_EquipmentDetails);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_EquipBreathingApparatus);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_EquipToximeter);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_EquipGasDetector);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_EquipEscapeDevice);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_EquipPPE);
			undgSubstanceConfiguration.IncludeColumn(x => x.ADN_BlueCones);
			undgSubstanceConfiguration.IncludeColumnWithConstantValue(x => x.ADN_IsActive, false, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.UNDGAttributeZZs);

			var undgAttributesConfiguration = new EntityTypeConfiguration<UNDGAttributeZZ>(true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Descriptor);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Index, true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Type, true);
			undgAttributesConfiguration.IncludeColumn(x => x.DAZ_Language, true);
			undgAttributesConfiguration.IncludeColumnWithConstantValue(x => x.DAZ_ParentCode, false, Constants.UNDGStandards.ADNDangerousGoodsCode);

			var xmlWriterConfig = new XmlWriterConfiguration();
			xmlWriterConfig.IncludeEntityTypeConfiguration(undgSubstanceConfiguration);
			xmlWriterConfig.IncludeEntityTypeConfiguration(undgAttributesConfiguration);

			return xmlWriterConfig;
		}
	}
}
