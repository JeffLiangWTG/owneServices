using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Organizations.CodeGeneration;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.Business
{
	public static class AddressCleansingResultItemExtensions
	{
		public static AddressCleansingResultItem FilterUnparsedInformation(this AddressCleansingResultItem resultItem)
		{
			if (!string.IsNullOrEmpty(resultItem.ValidationResultItem?.UnparsedAddressInformation))
			{
				var terms = resultItem
					.ValidationResultItem.UnparsedAddressInformation
					.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
					.Where(term => !Regexes.UselessUnparsedTerm.IsMatch(term))
					.ToArray();

				resultItem.ValidationResultItem.UnparsedAddressInformation = string.Join(" ", terms);
			}

			return resultItem;
		}

		public static AddressCleansingResultItem TransliterateAddressToEnglish(this AddressCleansingResultItem resultItem)
		{
			resultItem.ValidationResultItem?.TransliterateAddressToEnglish();
			return resultItem;
		}

		static void TransliterateAddressToEnglish(this ValidationResultItem resultItem)
		{
			resultItem.Address1 = resultItem.Address1.SafelyTransliterateAddressComponent();
			resultItem.Address2 = resultItem.Address2.SafelyTransliterateAddressComponent();
			resultItem.City = resultItem.City.SafelyTransliterateAddressComponent();

			resultItem.UnmatchedApartmentPrefix = resultItem.UnmatchedApartmentPrefix.SafelyTransliterateAddressComponent();
			resultItem.UnmatchedApartmentSuffix = resultItem.UnmatchedApartmentSuffix.SafelyTransliterateAddressComponent();

			resultItem.County = resultItem.County.SafelyTransliterateAddressComponent();
			resultItem.Locality = resultItem.Locality.SafelyTransliterateAddressComponent();

			resultItem.Postcode = resultItem.Postcode.SafelyTransliterateAddressComponent();
			resultItem.State = resultItem.State.SafelyTransliterateAddressComponent();
		}

		static string SafelyTransliterateAddressComponent(this string addressComponent)
		{
			var result = addressComponent;
			if (!string.IsNullOrEmpty(addressComponent))
			{
				result = WesternLanguageTransliterationHelper.TransliterateToEnglish(addressComponent);
			}
			return result;
		}

		public static AddressCleansingResultItem AdjustAddressCase(
			this AddressCleansingResultItem resultItem,
			ISupportWebAddressValidation address,
			bool isMixedCasingAllowed)
		{
			resultItem.ValidationResultItem?.AdjustAddressCase(address, isMixedCasingAllowed);

			resultItem
				.Suggestions
				.Where(suggestion => suggestion != null)
				.ToList()
				.ForEach(suggestion => suggestion.AdjustAddressCase(address, isMixedCasingAllowed));

			return resultItem;
		}

		static void AdjustAddressCase(
			this ValidationResultItem resultItem,
			ISupportWebAddressValidation address,
			bool isMixedCasingAllowed)
		{
			resultItem.Address1 = resultItem.Address1.ToAddressFieldCasing(address.Address1, isMixedCasingAllowed);
			resultItem.Address2 = resultItem.Address2.ToAddressFieldCasing(address.Address1, isMixedCasingAllowed);
			resultItem.City = resultItem.City.ToAddressFieldCasing(address.City, isMixedCasingAllowed);

			resultItem.UnmatchedApartmentPrefix = resultItem.UnmatchedApartmentPrefix.ToAddressFieldCasing(
				address.Address1,
				isMixedCasingAllowed);

			resultItem.UnmatchedApartmentSuffix = resultItem.UnmatchedApartmentSuffix.ToAddressFieldCasing(
				address.Address1,
				isMixedCasingAllowed);

			resultItem.County = resultItem.County.ToAddressFieldCasing(address.State, isMixedCasingAllowed);
			resultItem.Locality = resultItem.Locality.ToAddressFieldCasing(address.State, isMixedCasingAllowed);

			// Set the following fields to UPPER case regardless of the 'Mixed Case Allowed' setting.
			resultItem.Postcode = resultItem.Postcode?.ToUpper(CultureInfo.CurrentCulture);
			resultItem.State = resultItem.State?.ToUpper(CultureInfo.CurrentCulture);
		}

		public static void GetApplicableCityCasing(List<CandidateCityTown> cityTowns, ISupportWebAddressValidation address, bool isMixedCasingAllowed)
		{
			cityTowns.ForEach(candidate => candidate.City = candidate.City.ToAddressFieldCasing(address.Address1, isMixedCasingAllowed));
		}

		public static void TransliterateCityTownsToEnglish(List<CandidateCityTown> cityTowns)
		{
			cityTowns.ForEach(cityTown => cityTown.TransliterateCityTownToEnglish());
		}

		static void TransliterateCityTownToEnglish(this CandidateCityTown candidateCityTown)
		{
			candidateCityTown.City = candidateCityTown.City.SafelyTransliterateAddressComponent();
			candidateCityTown.State = candidateCityTown.State.SafelyTransliterateAddressComponent();
			candidateCityTown.Postcode = candidateCityTown.Postcode.SafelyTransliterateAddressComponent();
		}

		static string ToAddressFieldCasing(
			this string validationValue,
			string referenceValue,
			bool isMixedCasingAllowed)
		{
			if (string.IsNullOrEmpty(validationValue))
			{
				return validationValue;
			}

			return !isMixedCasingAllowed || referenceValue.IsAllUpperCase()
				? validationValue.ToUpper(CultureInfo.CurrentCulture)
				: validationValue.ToAddressTitleCase();
		}

		static bool IsAllUpperCase(this string value)
		{
			return
				string.IsNullOrEmpty(value) ||
				value.Where(char.IsLetter).All(char.IsUpper);
		}

		static string ToAddressTitleCase(this string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}

			var words = value
				.Split(' ')
				.Select(word =>
				{
					var subwords = word
						.Split('-')
						.Select(subword => subword.ToTitleCase());

					return string.Join("-", subwords);
				});

			return string.Join(" ", words);
		}

		static string ToTitleCase(this string value)
		{
			return string.IsNullOrEmpty(value)
				? value
				: string.Format(
					CultureInfo.CurrentCulture,
					"{0}{1}",
					value.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture),
					value.Substring(1, value.Length - 1).ToLower(CultureInfo.CurrentCulture));
		}

		static class Regexes
		{
			public static readonly Regex UselessUnparsedTerm = new Regex(
				@"^[_\W]+$",
				RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}
	}
}
