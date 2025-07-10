using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public class TariffMeasureConditionsBuilder
	{
		public TariffMeasureConditionsBuilder(StringBuilder errorCollector)
		{
			if (errorCollector == null)
			{
				throw new ArgumentNullException(nameof(errorCollector));
			}
			ErrorCollector = errorCollector;
		}
		StringBuilder ErrorCollector;

		public List<RefCusConditionCode> ConvertMeasureConditionsToRefCusConditionCode(IEnumerable<MeasureCondition> data)
		{
			var refCusConditionCodes = ConvertToRefCusConditionCode(data);
			var content = new List<RefCusConditionCode>();

			foreach (var refCusConditionCode in refCusConditionCodes)
			{
				if (!IsDuplicate(refCusConditionCode, content) && IsValid(refCusConditionCode))
				{
					content.Add(refCusConditionCode);
				}
			}
			return content;
		}

		public static void GenerateUniversalReferenceDataXml(IEnumerable<RefCusConditionCode> content, DateTime publicationDate, string outputPath)
		{
			if (content.Any())
			{
				var dataSource = $"{XMLWriterDataSource}";
				Helper.ExportToXMLFile(dataSource, Path.Combine(outputPath, GetOutputFileName(dataSource, publicationDate)), GetRefCusConditionCodeWriterConfiguration(), publicationDate, UpdateType.Full, content);
			}
		}

		protected static IEnumerable<RefCusConditionCode> ConvertToRefCusConditionCode(IEnumerable<MeasureCondition> data)
		{
			var results = new List<RefCusConditionCode>();
			foreach (var ad in data)
			{

				var refCusConditionCodeLanguages = new List<RefCusConditionCodeLanguage>();
				if (!ad.Descriptions.IsNullOrEmpty())
				{
					foreach (var description in ad.Descriptions.Where(x => x.Language != Constants.DefaultValues.ENLanguage))
					{
						var refCusConditionLanguage = new RefCusConditionCodeLanguage
						{
							ZY8_Description = description.Description,
							ZY8_ZX6_NKLanguage = description.Language,
						};
						refCusConditionCodeLanguages.Add(refCusConditionLanguage);
					}
				}

				var newRefCusConditionCode = new RefCusConditionCode()
				{
					ZY7_ConditionCode = ad.ConditionCode,
					ZY7_ZZZ_NKDataGrouping = ad.National == "0" ? Constants.DefaultValues.EUNCountryCode : Constants.DefaultValues.NLDataGrouping,
					ZY7_Description = ad.Descriptions.Where(d => d.Language == Constants.DefaultValues.ENLanguage).FirstOrDefault()?.Description,
					RefCusConditionCodeLanguages = refCusConditionCodeLanguages.ToArray(),
				};
				results.Add(newRefCusConditionCode);
			}
			return results;
		}

		protected bool IsValid(RefCusConditionCode refCusConditionCode)
		{
			bool valid = true;

			var validationErrors = new StringBuilder();

			if (string.IsNullOrWhiteSpace(refCusConditionCode.ZY7_ConditionCode))
			{
				validationErrors.Append("ZY7_ConditionCode is required. ");
				valid = false;
			}

			if (string.IsNullOrWhiteSpace(refCusConditionCode.ZY7_Description))
			{
				validationErrors.Append("ZY7_Description is required. ");
				valid = false;
			}

			if (refCusConditionCode.RefCusConditionCodeLanguages != null)
			{
				foreach (var refCusConditionCodeLanguage in refCusConditionCode.RefCusConditionCodeLanguages)
				{
					if (string.IsNullOrWhiteSpace(refCusConditionCodeLanguage.ZY8_ZX6_NKLanguage))
					{
						validationErrors.Append("ZY8_ZX6_NKLanguage is required. ");
						valid = false;
					}
					if (string.IsNullOrWhiteSpace(refCusConditionCodeLanguage.ZY8_Description))
					{
						validationErrors.Append("ZY8_Description is required.");
						valid = false;
					}
				}
			}

			if (!valid)
			{
				var msg = FormattableString.Invariant($"RefCusConditionCode validation error: Key '{refCusConditionCode.ZY7_ConditionCode}_{refCusConditionCode.ZY7_ZZZ_NKDataGrouping}' Errors: {validationErrors}");
				ErrorCollector.AppendLine(msg);
			}

			return valid;
		}

		protected bool IsDuplicate(RefCusConditionCode refCusConditionCode, List<RefCusConditionCode> content)
		{
			if (content.Any(x => x.ZY7_ConditionCode == refCusConditionCode.ZY7_ConditionCode &&
								 x.ZY7_ZZZ_NKDataGrouping == refCusConditionCode.ZY7_ZZZ_NKDataGrouping))
			{
				var msg = FormattableString.Invariant($"RefCusConditionCode validation error: Key '{refCusConditionCode.ZY7_ConditionCode}_{refCusConditionCode.ZY7_ZZZ_NKDataGrouping}' Errors: RefCusCondition already exist, no duplicate entry is created.");
				ErrorCollector.AppendLine(msg);
				return true;
			}
			return false;
		}

		protected static XmlWriterConfiguration GetRefCusConditionCodeWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var conditionCodeConfiguration = new EntityTypeConfiguration<RefCusConditionCode>(false);
			conditionCodeConfiguration.IncludeColumn(x => x.ZY7_ConditionCode, true);
			conditionCodeConfiguration.IncludeColumn(x => x.ZY7_ZZZ_NKDataGrouping, true);
			conditionCodeConfiguration.IncludeColumn(x => x.ZY7_Description, false);
			conditionCodeConfiguration.IncludeColumn(x => x.RefCusConditionCodeLanguages, false);

			var conditionCodeLanguageConfiguration = new EntityTypeConfiguration<RefCusConditionCodeLanguage>(true);
			conditionCodeLanguageConfiguration.IncludeColumn(x => x.ZY8_ZX6_NKLanguage, true);
			conditionCodeLanguageConfiguration.IncludeColumn(x => x.ZY8_Description, false);

			writerConfiguration.IncludeEntityTypeConfiguration(conditionCodeConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionCodeLanguageConfiguration);

			return writerConfiguration;
		}

		static string FilePrefix => "RefCusConditionCode";
		static string XMLWriterDataSource => "NL MeasureConditionCode";
		static string GetOutputFileName(string sourceName, DateTime publicationDate) => FormattableString.Invariant($"{FilePrefix}_{sourceName}_{publicationDate:HHmmssfff}.xml");
	}
}
