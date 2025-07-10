using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public static class CountryReferenceXmlWriterConfiguration
	{
		public static IXmlWriter GetXmlWriter(DateTime publicationTime)
		{
			var configuration = GetWriterConfiguration();
			var xmlWriter = new XmlWriter(configuration);
			xmlWriter.SetDataSource(Constants.DataSources.UNDGCountryReference);
			xmlWriter.SetPublicationTime(publicationTime);

			xmlWriter.SetUpdateType(UpdateType.Full);
			return xmlWriter;
		}

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var undgCountryReferenceConfiguration = new EntityTypeConfiguration<UNDGCountryReference>(true);
			undgCountryReferenceConfiguration.IncludeColumn(x => x.DCR_Type, true);
			undgCountryReferenceConfiguration.IncludeColumn(x => x.DCR_RN_NKCountry, true);
			undgCountryReferenceConfiguration.IncludeColumn(x => x.DCR_Code, true);
			undgCountryReferenceConfiguration.IncludeColumn(x => x.DCR_Description);
			undgCountryReferenceConfiguration.IncludeColumn(x => x.DCR_HasFlashPointLower, true);
			undgCountryReferenceConfiguration.IncludeColumn(x => x.DCR_FlashPointLowerCentigrade, true);
			undgCountryReferenceConfiguration.IncludeColumn(x => x.DCR_HasFlashPointUpper);
			undgCountryReferenceConfiguration.IncludeColumn(x => x.DCR_FlashPointUpperCentigrade);
			undgCountryReferenceConfiguration.IncludeColumn(x => x.UNDGCountryReferencePivots);

			var undgCountryReferencePivotConfiguration = new EntityTypeConfiguration<UNDGCountryReferencePivot>(true);
			undgCountryReferencePivotConfiguration.IncludeColumn(x => x.DCP_UNNO, true);
			undgCountryReferencePivotConfiguration.IncludeColumn(x => x.DCP_Variant, true);
			undgCountryReferencePivotConfiguration.IncludeColumnWithConstantValue(x => x.DCP_Standard, true, Constants.UNDGStandards.IMODangerousGoodsCode);

			var xmlWriterConfiguration = new XmlWriterConfiguration();
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(undgCountryReferenceConfiguration);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(undgCountryReferencePivotConfiguration);

			return xmlWriterConfiguration;
		}
	}
}
