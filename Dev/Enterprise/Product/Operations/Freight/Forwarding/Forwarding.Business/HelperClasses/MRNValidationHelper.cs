using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class MRNValidationHelper
	{
		public static IEnumerable<ZString> CheckMRNFormat(ZString mrn, BusinessObjectFactory factory)
		{
			var regex = new Regex("^[0-9]{2}(?<CountryCode>[A-Z]{2})[A-Z0-9]{13}(?<CheckDigit>[0-9])$");
			var match = regex.Match(mrn);

			var errors = new List<ZString>();
			if (!match.Success)
			{
				errors.Add(ResString.GetMultilingualString("6e59cabd-1241-489b-b48a-25dac707c28d", @"MRN does not conform to the correct format. Please enter the MRN in the following format with only numbers and upper case letters:
• two numbers for the year of issue,
• two letters for the ISO country/region code for country/region of issue,
• thirteen alphanumeric characters for unique identification and
• one number check digit"));
			}
			else
			{
				errors.Add(CheckCountryCode(match.Groups["CountryCode"].Value, factory));
				errors.Add(CheckMRNCheckDigit(mrn, match.Groups["CheckDigit"].Value));
			}

			return errors.Where(x => !x.IsEmpty);
		}

		#region Validate MRN Components

		static ZString CheckCountryCode(ZString countryCode, BusinessObjectFactory factory)
		{
			bool isValidCountryCode = factory.LoadFromNaturalKey(ObjectFactory.GetType<IRefCountry>(), RefCountrySchema.RN_Code, countryCode) != null;
			if (!isValidCountryCode)
			{
				return ResString.GetMultilingualString("f4437a09-ddb5-48e9-90cf-af3b551dc29e", "Please enter a valid country/region code.");
			}

			return ZString.Empty;
		}

		static ZString CheckMRNCheckDigit(ZString mrn, ZString expectedCheckDigit)
		{
			int calculatedResult = CalculatedTotalMRNValue(mrn);

			calculatedResult %= 11;
			if (calculatedResult == 10)
			{
				calculatedResult = 0;
			}

			if (expectedCheckDigit != calculatedResult.ToString(CultureInfo.InvariantCulture))
			{
				return ResString.GetMultilingualString("68ff85e9-5cc1-4257-a549-e4cfe97954f6", "MRN does not have a valid check (last) digit. The check digit should be {0}", calculatedResult);
			}

			return ZString.Empty;
		}

		#endregion

		#region Calculate Total MRN Value For Check Digit

		static int CalculatedTotalMRNValue(ZString mrn)
		{
			int totalCalculatedResult = 0;
			double position = 0;
			var mrnWithoutCheckDigit = mrn.SubstringSafe(0, 17);

			foreach (char c in mrnWithoutCheckDigit)
			{
				int charValue = Dictionary[c];
				double factor = Math.Pow(2.0, position);

				totalCalculatedResult += (charValue * Convert.ToInt32(factor));
				position++;
			}

			return totalCalculatedResult;
		}

		#region Dictionary

		static IReadOnlyDictionary<char, int> Dictionary => dictionary ?? (dictionary = new Dictionary<char, int>()
		{
			{ '0', 0 },
			{ '1', 1 },
			{ '2', 2 },
			{ '3', 3 },
			{ '4', 4 },
			{ '5', 5 },
			{ '6', 6 },
			{ '7', 7 },
			{ '8', 8 },
			{ '9', 9 },
			{ 'A', 10 },
			{ 'B', 12 },
			{ 'C', 13 },
			{ 'D', 14 },
			{ 'E', 15 },
			{ 'F', 16 },
			{ 'G', 17 },
			{ 'H', 18 },
			{ 'I', 19 },
			{ 'J', 20 },
			{ 'K', 21 },
			{ 'L', 23 },
			{ 'M', 24 },
			{ 'N', 25 },
			{ 'O', 26 },
			{ 'P', 27 },
			{ 'Q', 28 },
			{ 'R', 29 },
			{ 'S', 30 },
			{ 'T', 31 },
			{ 'U', 32 },
			{ 'V', 34 },
			{ 'W', 35 },
			{ 'X', 36 },
			{ 'Y', 37 },
			{ 'Z', 38 }
		});

		[ThreadStatic]
		static IReadOnlyDictionary<char, int> dictionary;

		#endregion

		#endregion
	}
}
