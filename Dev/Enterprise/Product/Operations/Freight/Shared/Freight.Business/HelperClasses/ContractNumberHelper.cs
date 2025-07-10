using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using CustomsReferenceNumberCodes = Enterprise.Registry.Business.CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes;

namespace Enterprise.Freight.Business
{
	public static class ContractNumberHelper
	{
		/// <summary>
		/// Checks for 2 Client Contract Number rules:
		/// 1. A country should have only one or zero CLC.
		/// 2. A non-blank number if exists must be unique across countries. There shouldn't be different non-blank CLCs.
		/// </summary>
		/// <returns>
		/// - true: CLCs setup violates rules.
		/// - false: CLCs setup is good.
		/// </returns>
		public static bool HasClientContractNumberRulesViolation(this CusEntryNumAdditionalReferenceCollection numbers)
		{
			var clcNumbers = numbers.OfType<CusEntryNumber>()
				.Where(number => number.CE_EntryType == CustomsReferenceNumberCodes.CLC)
				.ToList();

			// Check rule 1
			var currentCountryCode = GlbCompany.CurrentCompany?.GC_RN_NKCountryCode ?? ZString.Empty;
			var numbersInCurrentCountryCount = clcNumbers.Count(number => number.CE_RN_NKCountryCode == currentCountryCode);
			if (numbersInCurrentCountryCount > 1)
			{
				return true;
			}

			// Check rule 2
			var nonBlankNumbersInAllCountries = clcNumbers
				.Where(number => !number.CE_EntryNum.IsEmpty)
				.Select(number => number.CE_EntryNum.ToString())
				.ToHashSet(StringComparer.InvariantCultureIgnoreCase);

			return nonBlankNumbersInAllCountries.Count > 1;
		}

		/// <summary>
		/// Get all contract number values by number type.
		/// </summary>
		/// <param name="numbers">The collection</param>
		/// <param name="type">Number Type</param>
		/// <param name="countryCode">If present, it also filters the result by country</param>
		public static IEnumerable<CusEntryNumber> ListContractNumbers(
			this CusEntryNumAdditionalReferenceCollection numbers,
			string type = CustomsReferenceNumberCodes.CON,
			string countryCode = "")
		{
			return numbers
				.OfType<CusEntryNumber>()
				.Where(x =>
					x.CE_EntryType == type &&
					(string.IsNullOrEmpty(countryCode) || x.CE_RN_NKCountryCode == countryCode));
		}

		/// <summary>
		/// Add a new contract number to collection.
		/// In general, CLC can be added multiple times without countryCode,
		/// CusEntryNumber constructor automatically assigns current login country to CE_RN_NKCountryCode.
		/// If countryCode presents, CLC should be strictly to be a single value per country.
		/// </summary>
		/// <param name="numbers">Numbers collection</param>
		/// <param name="contractNumber">Value of the number</param>
		/// <param name="type">Number type. ie. CON or CLC</param>
		/// <param name="isEmptyAllowed">Allow contract number to be empty</param>
		/// <param name="countryCode">2 char code of the country issues the number</param>
		/// <returns>
		/// true - input contract number is added or skipped gracefully. The places calling this method can continue.
		/// false - input contract number is not added. Adding the number may cause conflicts in business rules. The places calling this method should show errors and stop.
		/// </returns>
		public static bool AddOrSkipContractNumber(
			this CusEntryNumAdditionalReferenceCollection numbers,
			ZString contractNumber,
			string type = CustomsReferenceNumberCodes.CON,
			bool isEmptyAllowed = false,
			string countryCode = "")
		{
			if (!isEmptyAllowed && contractNumber.IsEmpty && type != CustomsReferenceNumberCodes.CLC)
			{
				return true;
			}

			var checkResult = CanAddNumber(numbers.OfType<CusEntryNumber>().ToList(), contractNumber, type, countryCode);
			if (checkResult == CheckingResult.ShouldFail)
			{
				return false;
			}

			if (checkResult == CheckingResult.ShouldAdd)
			{
				var number = numbers.AddNew();
				number.CE_EntryType = type;
				number.CE_EntryNum = contractNumber;
				if (!string.IsNullOrEmpty(countryCode))
				{
					number.CE_RN_NKCountryCode = countryCode;
				}
			}

			return true;
		}

		static CheckingResult CanAddNumber(
			IReadOnlyCollection<CusEntryNumber> numbers,
			string contractNumber = "",
			string type = CustomsReferenceNumberCodes.CON,
			string countryCode = "")
		{
			// Rules for contract numbers:
			// 1. Country specific: a number should not repeat in the country. Eg. [AU-"A", US-"A"] is correct but not [AU-"A", AU-"A"]
			// 2. Not country specific: A number should not repeat in ANY country. Eg. ["", "A"] is correct but ["", "A", "A"] or ["", "", "A"] are not.
			// 3. For number type CLC specifically, see more rules in CanAddCountrySpecificCLCNumber.

			// Rule 1
			if (type != CustomsReferenceNumberCodes.CLC && !string.IsNullOrEmpty(countryCode))
			{
				var hasExistingNumberInCountry = numbers.Any(number =>
					number.CE_EntryType == type &&
					number.CE_EntryNum.EqualsIgnoringCase(contractNumber) &&
					number.CE_RN_NKCountryCode == countryCode);

				return hasExistingNumberInCountry ? CheckingResult.ShouldSkip : CheckingResult.ShouldAdd;
			}

			// Rule 2
			// Coming here because either the number is not country specific OR the number type is CLC
			if (string.IsNullOrEmpty(countryCode))
			{
				var hasExistingNumber = numbers.Any(number =>
					number.CE_EntryType == type &&
					number.CE_EntryNum.EqualsIgnoringCase(contractNumber));

				return hasExistingNumber ? CheckingResult.ShouldSkip : CheckingResult.ShouldAdd;
			}

			// Rule 3
			return CanAddCountrySpecificCLCNumber(numbers, contractNumber, countryCode);
		}

		static CheckingResult CanAddCountrySpecificCLCNumber(
			IReadOnlyCollection<CusEntryNumber> numbers,
			string contractNumber,
			string countryCode)
		{
			// Rules for country specific CLCs:
			// 1. Each country has only one or zero number.
			// 2. With bad data like [AU-"A", CN-"B"] (breaks rule 4), if we add US-"A" or US-"B", the result should still be Fail.
			//   This should have been handled early at the beginning of Autorating but let's check again and return Fail here.
			// 3. A number issued by a country can be blank OR non-blank. They can be mixed for different countries. For example [AU-"", NZ-"", CN-"A", US-"A"] is good.
			// 4. All non-blank numbers should be the same for all countries. For example: [AU-"A", CN-"A"] is correct but [AU-"A", CN-"B"] is not.

			// Rule 1
			if (numbers.Any(number => number.CE_EntryType == CustomsReferenceNumberCodes.CLC && number.CE_RN_NKCountryCode == countryCode))
			{
				return CheckingResult.ShouldSkip;
			}

			var existingNonBlankNumbers = numbers
				.Where(number => number.CE_EntryType == CustomsReferenceNumberCodes.CLC && !number.CE_EntryNum.IsEmpty)
				.Select(number => number.CE_EntryNum.ToString())
				.ToHashSet(StringComparer.InvariantCultureIgnoreCase);

			// Rule 2
			// Detect bad config and exit
			if (existingNonBlankNumbers.Count > 1)
			{
				return CheckingResult.ShouldFail;
			}

			// Rule 3
			// Coming here because there is no CLC at all with countryCode (Rule 1).
			// It's safe to just add a blank number. For non-blank number, see Rule 4.
			if (string.IsNullOrEmpty(contractNumber))
			{
				return CheckingResult.ShouldAdd;
			}

			// Rule 4
			// We are having a non-blank number added with countryCode
			if (existingNonBlankNumbers.Count == 0)
			{
				return CheckingResult.ShouldAdd;
			}

			// Rule 4 continue
			return existingNonBlankNumbers.Single().Equals(contractNumber, StringComparison.InvariantCultureIgnoreCase)
				? CheckingResult.ShouldAdd
				: CheckingResult.ShouldFail;
		}

		enum CheckingResult
		{
			ShouldAdd,
			ShouldSkip,
			ShouldFail
		}
	}
}
