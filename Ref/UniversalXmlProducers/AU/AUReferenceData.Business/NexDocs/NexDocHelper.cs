using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.AUReferenceData.Services;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public static class NexDocHelper
	{
		const string DateFormat = "yyyy-MM-ddTHH:mm:ss.fff";

		public static string GetStringCodeValue(this IItemCodeSet[] listItemCodeSet, string code)
		{
			var result = listItemCodeSet.SingleOrDefault(x => x.Key == code && x.ValueType == CodeSetValueType.@string)?.Value?.Trim();
			if (!string.IsNullOrEmpty(result) && result == "null")
			{
				result = string.Empty;
			}
			return result;
		}

		public static (bool SuccessfullyParsed, DateTime DateTime) GetDateTimeCodeValue(this IItemCodeSet[] listItemCodeSet, string code)
		{
			var result = false;
			var dateTime = DateTime.MinValue;
			var dateTimeAsString = listItemCodeSet.SingleOrDefault(x => x.Key == code && x.ValueType == CodeSetValueType.dateTime)?.Value?.Trim();
			if (!string.IsNullOrEmpty(dateTimeAsString) && dateTimeAsString != "null")
			{
				result = DateTime.TryParseExact(dateTimeAsString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
				var maximumDateTime = Constants.RefData_Common.MaximumDateTime;
				if (result && dateTime > maximumDateTime)
				{
					dateTime = maximumDateTime;
				}
			}
			return (result, dateTime);
		}

		public static void AppendErrorDetails(StringBuilder errorBuilder, string listCodeDescription, IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to import {listCodeDescription} due to empty Code, Description or an invalid Start Date.");
			AppendListCodeSetDetails(errorBuilder, listCodeDescription, keyValues, listCodeSet);
		}

		public static void AppendDuplicateDetails(StringBuilder errorBuilder, string listCodeDescription, IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Duplicate {listCodeDescription} exists in downloaded List.");
			AppendListCodeSetDetails(errorBuilder, listCodeDescription, keyValues, listCodeSet);
		}

		static void AppendListCodeSetDetails(StringBuilder errorBuilder, string listCodeDescription, IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			var listItemCodeSet = listCodeSet.Items;
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"{listCodeDescription} Details:");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Position: {listCodeSet.Position}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {keyValues.Code}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {keyValues.Description}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Start Date: {listItemCodeSet.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.StartDate && x.ValueType == CodeSetValueType.dateTime)?.Value}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"End Date: {listItemCodeSet.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.EndDate && x.ValueType == CodeSetValueType.dateTime)?.Value}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Updated Date: {listItemCodeSet.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.UpdatedDate && x.ValueType == CodeSetValueType.dateTime)?.Value}");
		}

		public static bool CheckCodeAndDescriptionIsValid(IKeyValues keyValues) => !string.IsNullOrWhiteSpace(keyValues.Code) && !string.IsNullOrWhiteSpace(keyValues.Description);

		public static bool CheckCodeDescriptionAndStartDateIsValid(IKeyValues keyValues) => CheckCodeAndDescriptionIsValid(keyValues) && keyValues.IsStartDateSuccesfullyParsed;
	}
}
