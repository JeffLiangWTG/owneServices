using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class IATAXmlWriterConfiguration
	{
		public static XmlWriter GetXmlWriter(DateTime publicationTime)
		{
			var xmlWriter = new XmlWriter(GetWriterConfiguration());
			xmlWriter.SetDataSource(Constants.DataSources.IATA);
			xmlWriter.SetPublicationTime(publicationTime);
			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var undgSubstanceConfiguration = new EntityTypeConfiguration<UNDGSubstance>(true);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_UniqueRecordId, true, order: 0);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_UNNO, true, order: 1);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_Variant, true, order: 1);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_PSN);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_IsNotOtherwiseSpecified);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_TechName);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_Class);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_SubLabel1);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_SubLabel2);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_PG);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_PackIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_LQMaxAmt);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_LQMaxAmtUQ);
			undgSubstanceConfiguration.IncludeColumnWithDefaultValue(x => x.DG_LQMaxAmtType, false, Constants.AmtTypes.NetWeightLimitAmtType);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_PaxPackIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_LQ2OrPaxMaxAmt);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_LQ2OrPaxMaxAmtUQ);
			undgSubstanceConfiguration.IncludeColumnWithDefaultValue(x => x.DG_LQ2OrPaxMaxAmtType, false, Constants.AmtTypes.NetWeightLimitAmtType);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_CargoPackIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_CargoMaxAmt);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_CargoMaxAmtUQ);
			undgSubstanceConfiguration.IncludeColumnWithDefaultValue(x => x.DG_CargoPackAmtType, false, Constants.AmtTypes.NetWeightLimitAmtType);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_EmergencyResponseGuide);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_ExceptedQuantityCode);
			undgSubstanceConfiguration.IncludeColumnWithConstantValue(x => x.DG_Standard, true, Constants.UNDGStandards.IATADangerousGoodsCode, order: 0);
			undgSubstanceConfiguration.IncludeColumnWithConstantValue(x => x.DG_Standard, true, Constants.UNDGStandards.IATADangerousGoodsCode, order: 1);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_Hazards);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_SpecialHandlingCodes);
			undgSubstanceConfiguration.IncludeColumnWithConstantValue(x => x.DG_IsActive, false, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.UNDGAttributes);

			var undgAttributesConfiguration = new EntityTypeConfiguration<UNDGAttribute>(true);
			undgAttributesConfiguration.IncludeColumn(x => x.DA_Descriptor);
			undgAttributesConfiguration.IncludeColumn(x => x.DA_Language, true);
			undgAttributesConfiguration.IncludeColumn(x => x.DA_Index, true);
			undgAttributesConfiguration.IncludeColumn(x => x.DA_Type, true);

			var xmlWriterConfig = new XmlWriterConfiguration();
			xmlWriterConfig.IncludeEntityTypeConfiguration(undgSubstanceConfiguration);
			xmlWriterConfig.IncludeEntityTypeConfiguration(undgAttributesConfiguration);

			return xmlWriterConfig;
		}
	}
}
