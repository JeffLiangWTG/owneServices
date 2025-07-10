using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	public class TradeGroupParser : ReferenceDataParser
	{
		readonly IDateTimeProvider dateTimeProvider;
		readonly string dataFileName;

		public TradeGroupParser(IDateTimeProvider dateTimeProvider, string dataFileName)
		{
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.dataFileName = Argument.NotNull(dataFileName, nameof(dataFileName));
		}

		public TradeGroupParser(string dataFileName) : this(new DateTimeProvider(), dataFileName)
		{
		}

		protected override string DataSource => "TR Trade Groups and Countries";

		protected override DateTime PublicationDateTime => dateTimeProvider.GetNow();

		protected override RefDataRepoModelEntityType[] GetEntities()
		{
			var data = TradeGroupLoader.Load(dataFileName);

			// uncomment the below line to reactivate additional trade groups for WI00379492. -M12
			//var dtyRateLoader = new HsnTariffDTYRateLoader(null);
			//var additionalTradeGroups = dtyRateLoader.LoadAdditionalTradeGroups();

			//var allTradeGroups = data.Union(additionalTradeGroups);
			var allTradeGroups = data;

			return allTradeGroups.Select(tradeGroup =>
			{
				return new RefCusTradeGroup
				{
					ZZA_TradeGroup = tradeGroup.Code,
					ZZA_Description = tradeGroup.Description,
					ZZA_StartDate = tradeGroup.StartDate.TruncateToMinute(),
					ZZA_EndDate = tradeGroup.EndDate.TruncateToMinute(),
					ZZA_ZZZ_NKDataGrouping = Constants.CountryCodeTR,
					RefCusTradeGroupCountries = tradeGroup.Countries.Select(country =>
					{
						return new RefCusTradeGroupCountry
						{
							ZZB_RN_NKTradeGroupCountryCode = country.Code,
							ZZB_StartDate = country.StartDate.Date,
							ZZB_EndDate = country.EndDate.Date,
							ZZB_Description = country.Description
						};
					}).ToArray()
				};
			}).ToArray();
		}

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var tradeGroup = new EntityTypeConfiguration<RefCusTradeGroup>(true);
			tradeGroup.IncludeColumn(x => x.ZZA_TradeGroup, true);
			tradeGroup.IncludeColumn(x => x.ZZA_Description, false);
			tradeGroup.IncludeColumn(x => x.ZZA_StartDate, false);
			tradeGroup.IncludeColumn(x => x.ZZA_EndDate, false);
			tradeGroup.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			tradeGroup.IncludeColumn(x => x.RefCusTradeGroupCountries, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(tradeGroup);

			var tradeGroupCountry = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			tradeGroupCountry.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, true);
			tradeGroupCountry.IncludeColumn(x => x.ZZB_StartDate, false);
			tradeGroupCountry.IncludeColumn(x => x.ZZB_EndDate, false);
			tradeGroupCountry.IncludeColumn(x => x.ZZB_Description, false);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(tradeGroupCountry);

			return xmlWriterConfiguration;
		}
	}
}
