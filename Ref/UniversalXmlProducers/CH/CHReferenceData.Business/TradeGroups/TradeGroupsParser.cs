using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.TradeGroups.CountryCodes;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.TradeGroups
{
	public class TradeGroupsParser
	{
		readonly DownloadResult download;

		public TradeGroupsParser(DownloadResult download)
		{
			this.download = download;
		}

		public void ConvertToRefXML(string outputFilePath, string dataSource, DateTime actualDate)
		{
			var writerConfiguration = GetRefTradeGroupWriterConfiguration();
			var inputDoc = Helper.DeserializeXML<countryCodes>(download.Content);

			var outputTradeGroup = new List<RefCusTradeGroup>();
			foreach (var inputCountryGroup in from countryGroup in inputDoc.countryGroups
											  where countryGroup.validFrom < Helper.MaximumDateTime
											  select countryGroup)
			{
				var outputCountries = new List<RefCusTradeGroupCountry>();
				foreach (var inputCountry in from country in inputDoc.countries
											 where country.countryGroupAssignment != null
											 from assignment in country.countryGroupAssignment
											 where assignment.grpNr == inputCountryGroup.grpNr
											 orderby country.validFrom descending
											 group (country, assignment) by country.isoCode into g
											 select new
											 {
												 countryIsoCode = g.Key,
												 countryNameEn = g.First().country.nameEn,
												 countryValidFrom = g.Min(x => x.country.validFrom),
												 countryValidTo = g.Max(x => x.country.validTo),
												 assignmentValidFrom = g.Min(x => x.assignment.validFrom),
												 assignmentValidTo = g.Max(x => x.assignment.validTo),
											 })
				{
					var tradeGroupCountry = new RefCusTradeGroupCountry
					{
						ZZB_RN_NKTradeGroupCountryCode = inputCountry.countryIsoCode,
						ZZB_StartDate = Helper.Max(inputCountryGroup.validFrom, Helper.Max(inputCountry.countryValidFrom, inputCountry.assignmentValidFrom)).Truncate(),
						ZZB_EndDate = Helper.Min(inputCountryGroup.validTo, Helper.Min(inputCountry.countryValidTo, inputCountry.assignmentValidTo)).Truncate().EndOfDay(),
						ZZB_Description = inputCountry.countryNameEn.Truncate(MaxDescriptionLength)
					};
					outputCountries.Add(tradeGroupCountry);
				}

				var outputGroupLanguages = new List<RefCusTradeGroupLanguage>();
				AddTradeGroupLanguage(outputGroupLanguages, "DE", inputCountryGroup.nameDe);
				AddTradeGroupLanguage(outputGroupLanguages, "FR", inputCountryGroup.nameFr);
				AddTradeGroupLanguage(outputGroupLanguages, "IT", inputCountryGroup.nameIt);

				outputTradeGroup.Add(new RefCusTradeGroup
				{
					ZZA_TradeGroup = inputCountryGroup.grpNr.ToString(CultureInfo.InvariantCulture),
					ZZA_Description = inputCountryGroup.nameEn,
					ZZA_StartDate = inputCountryGroup.validFrom.Truncate(),
					ZZA_EndDate = inputCountryGroup.validTo.Truncate().EndOfDay(),
					RefCusTradeGroupCountries = outputCountries.ToArray(),
					RefCusTradeGroupLanguages = outputGroupLanguages.ToArray()
				});
			}

			DateTime published = inputDoc.created;

			Helper.ExportToXMLFile(dataSource, outputFilePath, writerConfiguration, published, outputTradeGroup);
		}

		static void AddTradeGroupLanguage(List<RefCusTradeGroupLanguage> languages, string code, string description)
		{
			if (!string.IsNullOrEmpty(description))
			{
				languages.Add(new RefCusTradeGroupLanguage
				{
					ZXD_ZX6_NKLanguage = code,
					ZXD_Description = description
				});
			}
		}

		static XmlWriterConfiguration GetRefTradeGroupWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tradeGroupConfiguration = new EntityTypeConfiguration<RefCusTradeGroup>(true);
			tradeGroupConfiguration.IncludeColumn(x => x.ZZA_TradeGroup, true);
			tradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, true, "CH");
			tradeGroupConfiguration.IncludeColumn(x => x.ZZA_Description, false);
			tradeGroupConfiguration.IncludeColumn(x => x.ZZA_StartDate, false);
			tradeGroupConfiguration.IncludeColumn(x => x.ZZA_EndDate, false);
			tradeGroupConfiguration.IncludeColumn(x => x.RefCusTradeGroupCountries);
			tradeGroupConfiguration.IncludeColumn(x => x.RefCusTradeGroupLanguages);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupConfiguration);

			var tradeGroupCountryConfiguration = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, true);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_Description, false);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_StartDate, false);
			tradeGroupCountryConfiguration.IncludeColumn(x => x.ZZB_EndDate, false);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupCountryConfiguration);

			var tradeGroupLanguageConfiguration = new EntityTypeConfiguration<RefCusTradeGroupLanguage>(true);
			tradeGroupLanguageConfiguration.IncludeColumn(x => x.ZXD_ZX6_NKLanguage, true);
			tradeGroupLanguageConfiguration.IncludeColumn(x => x.ZXD_Description, false);
			writerConfiguration.IncludeEntityTypeConfiguration(tradeGroupLanguageConfiguration);

			return writerConfiguration;
		}

		const int MaxDescriptionLength = 200;
	}
}
