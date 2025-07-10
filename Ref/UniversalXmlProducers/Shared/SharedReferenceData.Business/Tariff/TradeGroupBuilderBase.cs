using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff
{
	public abstract class TradeGroupBuilderBase : BuilderBase<RefCusTradeGroup>
	{
		protected TradeGroupBuilderBase(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override IEnumerable<RefCusTradeGroup> ConvertToRefModels(List<ITariffModel> data)
		{
			var models = data.Cast<GeographicalArea>()
				.ToList();

			return models.Select(x => ConvertToRefModel(x));
		}

		RefCusTradeGroup ConvertToRefModel(GeographicalArea geoArea)
		{
			var result = new RefCusTradeGroup
			{
				ZZA_TradeGroup = geoArea.GeographicalAreaId,
				ZZA_Description = geoArea.Description,
				ZZA_StartDate = geoArea.CalcStartDate,
				ZZA_EndDate = geoArea.CalcEndDate
			};

			if (geoArea.Countries?.Any() ?? false)
			{
				result.RefCusTradeGroupCountries = CreateCountries(geoArea.Countries);
			}
			else if (geoArea.GeographicalAreaId.Length == 2)
			{
				result.RefCusTradeGroupCountries = new RefCusTradeGroupCountry[]
				{
					new RefCusTradeGroupCountry
					{
						ZZB_RN_NKTradeGroupCountryCode = geoArea.GeographicalAreaId,
						ZZB_StartDate = geoArea.CalcStartDate,
						ZZB_EndDate = geoArea.CalcEndDate,
						ZZB_Description = geoArea.Description
					}
				};
			}

			if (SupportsMultipleLanguages && (geoArea.Descriptions?.Any() ?? false))
			{
				result.RefCusTradeGroupLanguages = CreateDescriptions(geoArea.Descriptions);
			}

			return result;
		}

		RefCusTradeGroupCountry[] CreateCountries(IEnumerable<GeographicalArea.GeographicalAreaCountry> countries)
		{
			var countryList = new List<RefCusTradeGroupCountry>();

			foreach (var c in countries
								.Where(x => x.CalcEndDate >= dateTimeProvider.UTCHistoricalDate)
								.OrderByDescending(x => x.CalcEndDate)
								.ThenByDescending(x => x.CalcStartDate))
			{
				var country = countryList.FirstOrDefault(x => x.ZZB_RN_NKTradeGroupCountryCode == c.CountryCode && x.ZZB_StartDate <= c.CalcEndDate && x.ZZB_EndDate > c.StartDate);
				if (country == null)
				{
					countryList.Add(new RefCusTradeGroupCountry
					{
						ZZB_RN_NKTradeGroupCountryCode = c.CountryCode,
						ZZB_StartDate = c.CalcStartDate,
						ZZB_EndDate = c.CalcEndDate,
						ZZB_Description = c.Description
					});
				}
				else
				{
					if (c.CalcStartDate < country.ZZB_StartDate)
					{
						country.ZZB_StartDate = c.CalcStartDate;
					}

					if (c.CalcEndDate > country.ZZB_EndDate)
					{
						country.ZZB_EndDate = c.CalcEndDate;
					}
				}
			}

			return countryList.ToArray();
		}

		static RefCusTradeGroupLanguage[] CreateDescriptions(IEnumerable<DescriptionPeriods.DescriptionModel> descriptions)
		{
			var languageList = new List<RefCusTradeGroupLanguage>();

			foreach (var d in descriptions)
			{
				var description = languageList.FirstOrDefault(x => x.ZXD_ZX6_NKLanguage == d.LanguageCode);
				if (description == null)
				{
					languageList.Add(new RefCusTradeGroupLanguage
					{
						ZXD_ZX6_NKLanguage = d.LanguageCode,
						ZXD_Description = d.Description,
					});
				}
			}

			return languageList.ToArray();
		}

		protected override void DuplicateError(RefCusTradeGroup refModel, string uniqueId, string chapterFilter)
		{
			var msg = Invariant($"TradeGroup duplicate exists. Key: '{uniqueId}'");
			ErrorCollector.AppendLine(msg);
		}

		protected override bool IsExpired(RefCusTradeGroup refModel) => refModel.ZZA_EndDate.Date < dateTimeProvider.UTCHistoricalDate;

		protected override bool IsValid(RefCusTradeGroup refModel, string chapterFilter)
		{
			bool valid = true;

			var validationErrors = new StringBuilder();

			if (string.IsNullOrWhiteSpace(refModel.ZZA_TradeGroup))
			{
				validationErrors.Append("ZZA_TradeGroup is required. ");
				valid = false;
			}

			if (string.IsNullOrWhiteSpace(refModel.ZZA_Description))
			{
				validationErrors.Append("ZZA_Description is required. ");
				valid = false;
			}

			if (refModel.RefCusTradeGroupCountries?.Any(x => string.IsNullOrWhiteSpace(x.ZZB_RN_NKTradeGroupCountryCode)) ?? false)
			{
				validationErrors.Append("ZZB_RN_NKTradeGroupCountryCode is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"TradeGroup validation error. Key: '{refModel.ZZA_TradeGroup}' Errors: '{validationErrors}'");
				ErrorCollector.AppendLine(msg);
			}

			return valid;
		}

		protected override string UniqueId(RefCusTradeGroup refModel) => $"{refModel.ZZA_TradeGroup}_{refModel.ZZA_StartDate:yyyyMMddHHmmss}_{refModel.ZZA_EndDate:yyyyMMddHHmmss}";

		protected override XmlWriterConfiguration XmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();
			var entityConfig = new EntityTypeConfiguration<RefCusTradeGroup>(true);

			entityConfig.IncludeColumn(x => x.ZZA_TradeGroup, true);
			entityConfig.IncludeColumn(x => x.ZZA_Description);
			entityConfig.IncludeColumn(x => x.ZZA_EndDate);
			entityConfig.IncludeColumn(x => x.ZZA_StartDate);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZA_ZZZ_NKDataGrouping, true, DataGrouping);
			entityConfig.IncludeColumn(x => x.RefCusTradeGroupCountries);

			writerConfig.IncludeEntityTypeConfiguration(entityConfig);

			var countryConfig = new EntityTypeConfiguration<RefCusTradeGroupCountry>(true);
			countryConfig.IncludeColumn(x => x.ZZB_RN_NKTradeGroupCountryCode, true);
			countryConfig.IncludeColumn(x => x.ZZB_EndDate);
			countryConfig.IncludeColumn(x => x.ZZB_StartDate);
			countryConfig.IncludeColumn(x => x.ZZB_Description);

			writerConfig.IncludeEntityTypeConfiguration(countryConfig);

			if (SupportsMultipleLanguages)
			{
				entityConfig.IncludeColumn(x => x.RefCusTradeGroupLanguages);
				var languageConfig = new EntityTypeConfiguration<RefCusTradeGroupLanguage>(true);
				languageConfig.IncludeColumn(x => x.ZXD_ZX6_NKLanguage, true);
				languageConfig.IncludeColumn(x => x.ZXD_Description);

				writerConfig.IncludeEntityTypeConfiguration(languageConfig);
			}

			return writerConfig;
		}

		protected abstract string DataGrouping { get; }

		protected virtual bool SupportsMultipleLanguages => false;
	}
}
