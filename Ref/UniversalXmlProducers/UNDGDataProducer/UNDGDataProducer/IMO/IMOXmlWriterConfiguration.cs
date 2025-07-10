using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class IMOXmlWriterConfiguration
	{
		public static XmlWriter GetXmlWriter(DateTime publicationTime)
		{
			var xmlWriter = new XmlWriter(GetWriterConfiguration());
			xmlWriter.SetDataSource(Constants.DataSources.IMO);
			xmlWriter.SetPublicationTime(publicationTime);
			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var undgSubstanceConfiguration = new EntityTypeConfiguration<UNDGSubstance>(true);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_UNNO, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_Variant, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_PSN);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_TechName);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_Class);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_SubLabel1);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_SubLabel2);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_PG);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_PackIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_LQMaxAmt);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_LQMaxAmtUQ);
			undgSubstanceConfiguration.IncludeColumnWithDefaultValue(x => x.DG_LQMaxAmtType, false, Constants.AmtTypes.NetWeightLimitAmtType);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_LQ2OrPaxMaxAmt);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_LQ2OrPaxMaxAmtUQ);
			undgSubstanceConfiguration.IncludeColumnWithDefaultValue(x => x.DG_LQ2OrPaxMaxAmtType, false, Constants.AmtTypes.NetWeightLimitAmtType);
			undgSubstanceConfiguration.IncludeColumnWithConstantValue(x => x.DG_CargoPackAmtType, false, Constants.AmtTypes.NetWeightLimitAmtType);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_EMS);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_ExceptedQuantityCode);
			undgSubstanceConfiguration.IncludeColumnWithConstantValue(x => x.DG_Standard, true, Constants.UNDGStandards.IMODangerousGoodsCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_MP);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_FlashPoint);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_TreatAs);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_DglPhrase);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_PackProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_IBCIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_IBCProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_UNTankIns);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_TankProv);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_StowCat);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_ExpLim);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_UlineEMS);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_LQSpecProvIndex);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_Variation);
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
