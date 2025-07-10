using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class AdditionalCodesParser
	{
		public AdditionalCodesParser()
		{
			errorBuilder = new StringBuilder();
		}

		public (List<RefCusCodeList> result, string processingErrors) ConvertToRefCusCodeListXML(IList<AdditionalCodesData> additionalCodeDescriptionsData)
		{
			errorBuilder.Clear();
			var result = new List<RefCusCodeList>();

			if (additionalCodeDescriptionsData.Count > 0)
			{
				foreach (var record in additionalCodeDescriptionsData)
				{
					if (ValidateData(record))
					{
						result.Add(CreateRefCusCodeListWithLanguage(record));
					}
					else
					{
						AppendInvalidDataErrorDetails(record);
					}
				}
			}
			else
			{
				errorBuilder.AppendLine("AdditionalCodesData is empty => Nothing to import.");
			}

			return (result, errorBuilder.ToString());
		}

		static RefCusCodeList CreateRefCusCodeListWithLanguage(AdditionalCodesData recordToAdd)
		{
			var refCusCodeListLanguages = new List<RefCusCodeListLanguage>();
			foreach (var descriptionItem in recordToAdd.MultilingualDescriptions.Where(x => x.Key != DefaultLanguage))
			{
				if (ValidateLanguageData(descriptionItem))
				{
					refCusCodeListLanguages.Add(new RefCusCodeListLanguage()
					{
						ZXA_ZX6_NKLanguage = descriptionItem.Key,
						ZXA_Description = CleanUp(descriptionItem.Value)
					});
				}
			}

			var englishDescription = CleanUp(recordToAdd.MultilingualDescriptions[DefaultLanguage]);

			return new RefCusCodeList
			{
				ZZD_Code = recordToAdd.Code,
				ZZD_Description = englishDescription.Length <= DefaultLanguageDescriptionMaxLength ? englishDescription : englishDescription.Substring(0, DefaultLanguageDescriptionMaxLength),
				ZZD_StartDate = recordToAdd.StartDate.Value,
				ZZD_EndDate = recordToAdd.EndDate.Value,
				RefCusCodeListLanguages = refCusCodeListLanguages.ToArray()
			};
		}

		static bool ValidateData(AdditionalCodesData record)
		{
			var code = record.Code;

			return !string.IsNullOrWhiteSpace(code)
				&& code.Length <= 35
				&& record.MultilingualDescriptions.TryGetValue(DefaultLanguage, out var englishDescription)
				&& !string.IsNullOrWhiteSpace(englishDescription)
				&& record.StartDate != null
				&& record.EndDate != null;
		}

		static bool ValidateLanguageData(KeyValuePair<string, string> descriptionRecord) => descriptionRecord.Key.Length <= 3
			&& !string.IsNullOrWhiteSpace(descriptionRecord.Value);

		void AppendInvalidDataErrorDetails(AdditionalCodesData invalidRecord)
		{
			errorBuilder.AppendLine(@"Unable to import record from AdditionalCodes into RefCusCodeList.");
			errorBuilder.AppendLine("DETAILS:");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {invalidRecord.Code}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Has English record: {invalidRecord.MultilingualDescriptions.TryGetValue(DefaultLanguage, out var englishDescription)}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"English Description: {englishDescription}");
			var reportedStartDate = invalidRecord.StartDate != null ? invalidRecord.StartDate?.ToString("dd-MM-yyy", CultureInfo.InvariantCulture) : "Badly formatted";
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Start Date: {reportedStartDate}");
			var reportedEndDate = invalidRecord.EndDate != null ? invalidRecord.EndDate?.ToString("dd-MM-yyy", CultureInfo.InvariantCulture) : "Badly formatted";
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"End Date: {reportedEndDate}");
		}


		static string CleanUp(string description)
		{
			var cleanList = new string[] { "\r\n", "\r", "\n" };

			foreach (var special in cleanList)
			{
				description = description.Replace(special, "; ");
			}

			return description;
		}

		const int DefaultLanguageDescriptionMaxLength = 2000;
		const string DefaultLanguage = "EN";
		readonly StringBuilder errorBuilder;
	}
}
