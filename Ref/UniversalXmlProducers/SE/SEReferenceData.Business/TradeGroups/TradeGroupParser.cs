using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.SEReferenceData.Business.TradeGroups
{
	public sealed class TradeGroupParser : XmlParser<geographicalArea>
	{
		public override string ConvertToXMLFile(geographicalArea[] items, string lastModified, string outputFileWithPath)
		{
			ErrorBuilder.Clear();

			var resultTradeGroup = new List<RefCusTradeGroup>();

			if (items != null && items.Length > 0)
			{
				foreach (var geographicalArea in items.Where(ga =>
					ga.dateEnd.FixIfMissingEndDate() > Constants.EarliestSupportedEndDate &&
					ga.national == Constants.NationalCodes.Sweden &&
					ga.geographicalAreaId.Length == Constants.TradegroupLength.TradeGroupCodeLength))
				{
					var outputGroupLanguages = new List<RefCusTradeGroupLanguage>();
					var outputCountries = new List<RefCusTradeGroupCountry>();

					var tradegroupCode = geographicalArea.geographicalAreaId;
					var tradegroupId = geographicalArea.SID;

					var startDate = geographicalArea.dateStart;
					var endDate = geographicalArea.dateEnd.FixIfMissingEndDate();

					var (swedishDescription, englishDescription) = GetDescriptions(geographicalArea);

					AddTradeGroupEnglishLanguage(outputGroupLanguages, englishDescription);
					AddTradeGroupCountries(outputCountries, tradegroupId, items);

					var tradeGroup = new RefCusTradeGroup
					{
						ZZA_TradeGroup = tradegroupCode,
						ZZA_Description = swedishDescription,
						ZZA_StartDate = startDate,
						ZZA_EndDate = endDate,
						RefCusTradeGroupLanguages = outputGroupLanguages.ToArray(),
						RefCusTradeGroupCountries = outputCountries.ToArray()
					};

					resultTradeGroup.Add(tradeGroup);
				}
			}

			if (resultTradeGroup.Any())
			{
				var refCusTradegroupConfig = GetRefTradeGroupsWriterConfiguration();
				var (isValidLastModifiedTime, lastModifiedTime) = Helper.GetDateTime(lastModified, "yyMMdd");
				if (isValidLastModifiedTime)
				{
					Helper.ExportToXMLFile("SE TradeGroups", outputFileWithPath, refCusTradegroupConfig, lastModifiedTime, resultTradeGroup);
				}
				else
				{
					ErrorBuilder.Append(CultureInfo.InvariantCulture, $"Could not parse lastModified date, was expecting format yyMMdd but found {lastModified}");
				}
			}
			else
			{
				ErrorBuilder.Append("Not able to extract any data");
			}

			return ErrorBuilder.Append(MissingDescriptionError).ToString();
		}

		static (string descriptionSE, string descriptionEN) GetDescriptions(geographicalArea area)
		{
			var swedishDescription = string.Empty;
			var englishDescription = string.Empty;

			var query = area.geographicalAreaDescriptionPeriod
				.Where(gadp => gadp.dateEnd.FixIfMissingEndDate() > Constants.EarliestSupportedEndDate)
				.SelectMany(gadp => gadp.geographicalAreaDescription
					.Where(gad => gad.description.Length > 0),
					(gadp, gad) => new
					{
						endDate = gadp.dateEnd,
						text = gad.description,
						language = gad.languageId
					});

			foreach (var description in query.ToList())
			{
				switch (description.language)
				{
					case Constants.LanguageCode.Swedish:
						swedishDescription = description.text;
						break;
					case Constants.LanguageCode.English:
						englishDescription = description.text;
						break;
				}
			}

			return (swedishDescription, englishDescription);
		}

		static void AddTradeGroupCountries(List<RefCusTradeGroupCountry> outputCountries, long tradegroupID, geographicalArea[] xmlGeographicalAreas)
		{
			if (xmlGeographicalAreas != null && xmlGeographicalAreas.Length > 0)
			{
				foreach (var geographicalArea in xmlGeographicalAreas.Where(ga =>
					 ga.dateEnd.FixIfMissingEndDate() > Constants.EarliestSupportedEndDate &&
					 ga.geographicalAreaId.Length == Constants.TradegroupLength.CountryCodeLength))
				{
					if (geographicalArea?.geographicalAreaMembership?.Length > 0)
					{
						var query = geographicalArea.geographicalAreaMembership
							.Where(gam => gam.SIDGeographicalAreaGroup == tradegroupID)
							.SelectMany(gam => geographicalArea.geographicalAreaDescriptionPeriod
								.SelectMany(gadp => gadp.geographicalAreaDescription
									.Where(gad => gad.languageId == Constants.LanguageCode.English),
									(gadp, gad) => new
									{
										countryCode = geographicalArea.geographicalAreaId,
										startDate = gam.dateStart,
										endDate = gam.dateEnd,
										countryName = gad.description,
										countryEndDate = gadp.dateEnd
									})
						);

						{
							foreach (var country in query.ToList())
							{
								var countryEndDate = country.endDate.FixIfMissingEndDate();
								if (countryEndDate < Constants.EarliestSupportedEndDate || country.countryEndDate.FixIfMissingEndDate() < Constants.EarliestSupportedEndDate)
								{
									continue;
								}

								var tradeGroupCountry = new RefCusTradeGroupCountry
								{
									ZZB_RN_NKTradeGroupCountryCode = country.countryCode,
									ZZB_Description = country.countryName,
									ZZB_StartDate = country.startDate,
									ZZB_EndDate = countryEndDate
								};
								outputCountries.Add(tradeGroupCountry);
							}
						}
					}
				}
			}
		}

		static void AddTradeGroupEnglishLanguage(List<RefCusTradeGroupLanguage> languages, string description)
		{
			if (!string.IsNullOrEmpty(description))
			{
				languages.Add(new RefCusTradeGroupLanguage
				{
					ZXD_Description = description
				});
			}
		}

		static XmlWriterConfiguration GetRefTradeGroupsWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var tradeGroupsConfiguration = new EntityTypeConfiguration<RefCusTradeGroup>(true);
			tradeGroupsConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.DataGrouping.Sweden);
			tradeGroupsConfiguration.IncludeColumn(x => x.ZZA_TradeGroup, isKeyColumn: true);
			tradeGroupsConfiguration.IncludeColumn(x => x.ZZA_Description, isKeyColumn: false);
			tradeGroupsConfiguration.IncludeColumn(x => x.ZZA_StartDate, isKeyColumn: false);
			tradeGroupsConfiguration.IncludeColumn(x => x.ZZA_EndDate, isKeyColumn: false);
			tradeGroupsConfiguration.IncludeColumn(x => x.RefCusTradeGroupCountries);
			tradeGroupsConfiguration.IncludeColumn(x => x.RefCusTradeGroupLanguages);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupsConfiguration);

			var tradeGroupsCountryConfiguration = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			tradeGroupsCountryConfiguration.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, isKeyColumn: true);
			tradeGroupsCountryConfiguration.IncludeColumn(x => x.ZZB_Description, isKeyColumn: false);
			tradeGroupsCountryConfiguration.IncludeColumn(x => x.ZZB_StartDate, isKeyColumn: false);
			tradeGroupsCountryConfiguration.IncludeColumn(x => x.ZZB_EndDate, isKeyColumn: false);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupsCountryConfiguration);

			var tradeGroupsLanguageConfiguration = new EntityTypeConfiguration<RefCusTradeGroupLanguage>(true);
			tradeGroupsLanguageConfiguration.IncludeColumnWithConstantValue(x => x.ZXD_ZX6_NKLanguage, isKeyColumn: true, Constants.LanguageCode.English);
			tradeGroupsLanguageConfiguration.IncludeColumn(x => x.ZXD_Description, false);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupsLanguageConfiguration);

			return writerConfiguration;
		}
	}
}
