using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace CargoWise.RefDbRepo.SEReferenceData.Business.MeasureTypes
{
	public sealed class MeasureTypeParser : XmlParser<measureType1>
	{
		const string RateConditionClass = "RATE";
		const string CtrlConditionClass = "CTRL";
		const string RateSeriesId = "Q";

		public override string ConvertToXMLFile(measureType1[] items, string lastModified, string outputFileWithPath)
		{
			ErrorBuilder.Clear();

			var resultList = new List<RefCusConditionType>();

			if (items != null && items.Length > 0)
			{
				foreach (var item in items.Where(obj =>
					obj.dateEnd.FixIfMissingEndDate() > Constants.EarliestSupportedEndDate &&
					obj.national == Constants.NationalCodes.Sweden))
				{
					var outputLanguages = new List<RefCusConditionTypeLanguage>();

					var startDate = item.dateStart;
					var endDate = item.dateEnd.FixIfMissingEndDate();

					var (swedishDescription, englishDescription) = GetDescriptions(item);

					AddEnglishLanguage(outputLanguages, englishDescription);

					var conditionType = new RefCusConditionType
					{
						ZX2_ConditionType = item.measureType,
						ZX2_Description = swedishDescription,
						ZX2_ConditionClass = item.measureTypeSeriesId == RateSeriesId ? RateConditionClass : CtrlConditionClass,
						RefCusConditionTypeLanguages = outputLanguages.ToArray()
					};

					resultList.Add(conditionType);
				}
			}

			if (resultList.Any())
			{
				var refCusConditionTypeConfig = GetRefCusConditionTypeWriterConfiguration();
				var (isValidLastModifiedTime, lastModifiedTime) = Helper.GetDateTime(lastModified, "yyMMdd");
				if (isValidLastModifiedTime)
				{
					Helper.ExportToXMLFile("SE MeasureTypes", outputFileWithPath, refCusConditionTypeConfig, lastModifiedTime, resultList);
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

		static (string descriptionSE, string descriptionEN) GetDescriptions(measureType1 measureType)
		{
			var swedishDescription = string.Empty;
			var englishDescription = string.Empty;

			var descriptions = measureType.measureTypeDescription
				.Select(
					description => new
					{
						text = description.description,
						language = description.languageId
					}
				);

			foreach (var description in descriptions.ToList())
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

		static void AddEnglishLanguage(List<RefCusConditionTypeLanguage> languages, string description)
		{
			if (!string.IsNullOrEmpty(description))
			{
				languages.Add(new RefCusConditionTypeLanguage
				{
					ZXW_Description = description
				});
			}
		}

		static XmlWriterConfiguration GetRefCusConditionTypeWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var conditionTypeConfiguration = new EntityTypeConfiguration<RefCusConditionType>(true);
			conditionTypeConfiguration.IncludeColumnWithConstantValue(x => x.ZX2_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.DataGrouping.Sweden);
			conditionTypeConfiguration.IncludeColumn(x => x.ZX2_ConditionClass, isKeyColumn: false);
			conditionTypeConfiguration.IncludeColumn(x => x.ZX2_ConditionType, isKeyColumn: true);
			conditionTypeConfiguration.IncludeColumn(x => x.ZX2_Description, isKeyColumn: false);
			conditionTypeConfiguration.IncludeColumn(x => x.RefCusConditionTypeLanguages);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionTypeConfiguration);

			var conditionTypeLanguageConfiguration = new EntityTypeConfiguration<RefCusConditionTypeLanguage>(true);
			conditionTypeLanguageConfiguration.IncludeColumnWithConstantValue(x => x.ZXW_ZX6_NKLanguage, isKeyColumn: true, Constants.LanguageCode.English);
			conditionTypeLanguageConfiguration.IncludeColumn(x => x.ZXW_Description, false);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionTypeLanguageConfiguration);

			return writerConfiguration;
		}
	}
}
