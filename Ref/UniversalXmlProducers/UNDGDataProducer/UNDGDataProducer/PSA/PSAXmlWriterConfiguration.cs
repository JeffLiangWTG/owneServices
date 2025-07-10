using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class PSAXmlWriterConfiguration
	{
		public static XmlWriter GetXmlWriter()
		{
			var xmlWriter = new XmlWriter(GetWriterConfiguration());
			xmlWriter.SetDataSource(Constants.DataSources.PSAGroup);
			xmlWriter.SetPublicationTime(DateTime.UtcNow);
			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var undgSubstanceConfiguration = new EntityTypeConfiguration<UNDGSubstance>(false);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_UNNO, true);
			undgSubstanceConfiguration.IncludeColumn(x => x.DG_Variant, true);
			undgSubstanceConfiguration.IncludeColumnWithConstantValue(x => x.DG_Standard, true, Constants.UNDGStandards.IMODangerousGoodsCode);
			undgSubstanceConfiguration.IncludeColumn(x => x.UNDGReferences);

			var undgReferencesConfiguration = new EntityTypeConfiguration<UNDGReference>(true);
			undgReferencesConfiguration.IncludeColumn(x => x.DR_Code, true);
			undgReferencesConfiguration.IncludeColumnWithConstantValue(x => x.DR_Type, true, Constants.DataSources.PSA);
			undgReferencesConfiguration.IncludeColumnWithConstantValue(x => x.DR_RN_NKCountry, true, Constants.DataSources.PSACountry);
			undgReferencesConfiguration.IncludeColumn(x => x.DR_HasFlashPointLower, true);
			undgReferencesConfiguration.IncludeColumn(x => x.DR_FlashPointLower, true);
			undgReferencesConfiguration.IncludeColumn(x => x.DR_HasFlashPointUpper, false);
			undgReferencesConfiguration.IncludeColumn(x => x.DR_FlashPointUpper, false);

			var xmlWriterConfig = new XmlWriterConfiguration();
			xmlWriterConfig.IncludeEntityTypeConfiguration(undgSubstanceConfiguration);
			xmlWriterConfig.IncludeEntityTypeConfiguration(undgReferencesConfiguration);

			return xmlWriterConfig;
		}
	}
}
