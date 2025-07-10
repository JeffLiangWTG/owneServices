using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class TradeGroupReferenceDataFileGenerator : BaseUniversalReferenceDataFileGenerator<RefCusTradeGroup>
	{
		public override string OutputFile => ApplicationConfig.Instance.FRTradeGroupOutputFile;

		protected override List<RefCusTradeGroup> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var provider = new RITADataProvider(new RITADataDownloader());
			var tradeGroups = provider.GetFRTradeGroups()
				.Concat(provider.GetFRCountriesAsTradeGroups())
				.Concat(RITADataProvider.GetApplicationTerritories())
				.ToList();

			return tradeGroups;
		}

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var tradeGroup = new EntityTypeConfiguration<RefCusTradeGroup>(true);
			tradeGroup.IncludeColumn(x => x.ZZA_TradeGroup, true);
			tradeGroup.IncludeColumn(x => x.ZZA_Description, false);
			tradeGroup.IncludeColumn(x => x.ZZA_StartDate, false);
			tradeGroup.IncludeColumnWithConstantValue(x => x.ZZA_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			tradeGroup.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.France);
			tradeGroup.IncludeColumn(x => x.RefCusTradeGroupCountries, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(tradeGroup);

			var tradeGroupCountry = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			tradeGroupCountry.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, true);
			tradeGroupCountry.IncludeColumn(x => x.ZZB_StartDate, false);
			tradeGroupCountry.IncludeColumnWithConstantValue(x => x.ZZB_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			tradeGroupCountry.IncludeColumn(x => x.ZZB_Description, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(tradeGroupCountry);

			return xmlWriterConfiguration;
		}

		public override string DataSource => "FR - Trade Groups";
	}
}
