using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using WTG.Foundation.FrameworkExtensions;

namespace Enterprise.MasterFiles.Business
{
	public class AccValidationHelper : ValidationProvider
	{
		public AccValidationHelper()
		{
		}

		public void ValidateChequeDigits(ZPropertyInfo chequeReferenceNoInfo, ZByte numberOfChequeDigits)
		{
			ZString chequeNo = chequeReferenceNoInfo.Value.ToString();

			if (chequeNo.Length > numberOfChequeDigits)
			{
				chequeReferenceNoInfo.AddError(Res.GetString("69fef8cf-86a5-4d23-9408-6d000454148c", "The check number exceeds the number of digits ({0} digits) set for the current bank account", numberOfChequeDigits));
			}
		}

		public ZString PadChequeDigitsWithLeadingZeros(AccBankAccount bankAccount, ZString chequeNumber)
		{
			AccBankAccount bankAccountCached = bankAccount;
			if (bankAccountCached != null)
			{
				if (chequeNumber.Length > 0 && chequeNumber.Length < bankAccountCached.AB_ChequeNumDigits)
				{
					chequeNumber = chequeNumber.PadLeft(bankAccountCached.AB_ChequeNumDigits, '0');
				}
			}
			return chequeNumber;
		}

		public static bool CheckBankSWIFT(ZString sWIFTToCheck)
		{
			Regex regex = new Regex(@"^[A-Z]{6}[A-Z2-9][A-NP-Z0-9]([A-Z0-9]{3})?$");
			return regex.IsMatch(sWIFTToCheck);
		}

		public static void CheckBankIBAN(ZPropertyInfo ibanInfo, string bankAccountCountry = null)
		{
			var ibanInfoValue = (ZString)ibanInfo.Value;
			var ibanCountry = ibanInfoValue.Left(2);

			if (!string.IsNullOrEmpty(bankAccountCountry) && !ibanCountry.EqualsIgnoringCase(bankAccountCountry))
			{
				ibanInfo.AddWarning(Res.GetString("0DA3A855-A276-4A17-AB39-6432FA27C5C2", "[Bank Account] IBAN Number: The first two characters of the IBAN Number do not match the Bank's Country/Region Code."));
			}

			var regex = new Regex(@"^[A-Z]{2}[0-9]{2}[a-zA-Z0-9]{1,30}$");
			if (!regex.IsMatch(ibanInfoValue))
			{
				ibanInfo.AddError(Res.GetString("F467F5BB-3F64-46A9-A717-912A11B93ADC", "The third and fourth characters of the IBAN Number must be numeric. From fifth character onward, it should only contain alphanumeric values only."));
				return;
			}

			if (CountryAndLengthDictionary.TryGetValue(ibanCountry, out var len) && ibanInfoValue.Length != len)
			{
				ibanInfo.AddError(Res.GetString("06347197-A25F-4A70-A594-14DDA9AACB0F", "The length of IBAN Number is incorrect, it should be '{0}' in Country/Region '{1}'.", len, ibanCountry));
				return;
			}

			CheckMod97(ibanInfo);
		}

		static void CheckMod97(ZPropertyInfo ibanInfo)
		{
			var ibanInfoValue = (ZString)ibanInfo.Value;
			var ibanToCheck = ibanInfoValue.ToString().ToUpperInvariant();
			var modifiedIban = ibanToCheck.Substring(4) + ibanToCheck.Substring(0, 4);
			modifiedIban = Regex.Replace(modifiedIban, @"\D", m => (m.Value[0] - 55).ToString(CultureInfo.InvariantCulture));

			var remain = 0;
			while (modifiedIban.Length > 7)
			{
				remain = int.Parse(remain + modifiedIban.Substring(0, 7), CultureInfo.InvariantCulture) % 97;
				modifiedIban = modifiedIban.Substring(7);
			}
			remain = int.Parse(remain + modifiedIban, CultureInfo.InvariantCulture) % 97;

			if (remain != 1)
			{
				ibanInfo.AddError(Res.GetString("9673FA59-FE53-490A-9FEA-6427A64ADDE9", "The remainder calculated using the MOD 97 algorithm should be equal to 1. Please check the IBAN to confirm it is entered correctly."));
				return;
			}
		}

		public static void CheckIbanForEUCountry(BusinessObjectFactory factory, ZPropertyInfo ibanInfo, string bankAccountCountry = null)
		{
			if (bankAccountCountry.IsNullOrEmpty() || !ibanInfo.Value.IsEmpty)
			{
				return;
			}

			var country = factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, bankAccountCountry) as RefCountry;

			if (country != null && country.IsPartOfEuropeanUnion)
			{
				ibanInfo.AddWarning(Res.GetString("7462550f-0ca5-4289-bd5a-7f15b8865218",
					"When saving a bank account in an EU country, it is recommended to specify the IBAN account number for more efficient transfer of payments."));
			}
		}

		static Dictionary<string, int> CountryAndLengthDictionary
		{
			get
			{
				return countryAndLengthDictionary ??
					(countryAndLengthDictionary = new Dictionary<string, int>()
													{
														{ Core.Constants.CountryCodes.Albania, 28 },
														{ Core.Constants.CountryCodes.Andorra, 24 },
														{ Core.Constants.CountryCodes.Austria, 20 },
														{ Core.Constants.CountryCodes.Azerbaijan, 28 },
														{ Core.Constants.CountryCodes.Bahrain, 22 },
														{ Core.Constants.CountryCodes.Belgium, 16 },
														{ Core.Constants.CountryCodes.BosniaAndHerzegovina, 20 },
														{ Core.Constants.CountryCodes.Brazil, 29 },
														{ Core.Constants.CountryCodes.Bulgaria, 22 },
														{ Core.Constants.CountryCodes.CostaRica, 22 },
														{ Core.Constants.CountryCodes.Croatia, 21 },
														{ Core.Constants.CountryCodes.Cyprus, 28 },
														{ Core.Constants.CountryCodes.CzechRepublic, 24 },
														{ Core.Constants.CountryCodes.Denmark, 18 },
														{ Core.Constants.CountryCodes.DominicanRepublic, 28 },
														{ Core.Constants.CountryCodes.EastTimor, 23 },
														{ Core.Constants.CountryCodes.Estonia, 20 },
														{ Core.Constants.CountryCodes.FaeroeIslands, 18 },
														{ Core.Constants.CountryCodes.Finland, 18 },
														{ Core.Constants.CountryCodes.France, 27 },
														{ Core.Constants.CountryCodes.Georgia, 22 },
														{ Core.Constants.CountryCodes.Germany, 22 },
														{ Core.Constants.CountryCodes.Gibraltar, 23 },
														{ Core.Constants.CountryCodes.Greece, 27 },
														{ Core.Constants.CountryCodes.Greenland, 18 },
														{ Core.Constants.CountryCodes.Guatemala, 28 },
														{ Core.Constants.CountryCodes.Hungary, 28 },
														{ Core.Constants.CountryCodes.Iceland, 26 },
														{ Core.Constants.CountryCodes.Ireland, 22 },
														{ Core.Constants.CountryCodes.Israel, 23 },
														{ Core.Constants.CountryCodes.Italy, 27 },
														{ Core.Constants.CountryCodes.Jordan, 30 },
														{ Core.Constants.CountryCodes.Kazakhstan, 20 },
														{ Core.Constants.CountryCodes.Kuwait, 30 },
														{ Core.Constants.CountryCodes.Latvia, 21 },
														{ Core.Constants.CountryCodes.Lebanon, 28 },
														{ Core.Constants.CountryCodes.Liechtenstein, 21 },
														{ Core.Constants.CountryCodes.Lithuania, 20 },
														{ Core.Constants.CountryCodes.Luxembourg, 20 },
														{ Core.Constants.CountryCodes.Macedonia, 19 },
														{ Core.Constants.CountryCodes.Malta, 31 },
														{ Core.Constants.CountryCodes.Mauritania, 27 },
														{ Core.Constants.CountryCodes.Mauritius, 30 },
														{ Core.Constants.CountryCodes.Monaco, 27 },
														{ Core.Constants.CountryCodes.Moldova, 24 },
														{ Core.Constants.CountryCodes.Montenegro, 22 },
														{ Core.Constants.CountryCodes.Netherlands, 18 },
														{ Core.Constants.CountryCodes.Norway, 15 },
														{ Core.Constants.CountryCodes.Pakistan, 24 },
														{ Core.Constants.CountryCodes.Poland, 28 },
														{ Core.Constants.CountryCodes.Portugal, 25 },
														{ Core.Constants.CountryCodes.Qatar, 29 },
														{ Core.Constants.CountryCodes.Romania, 24 },
														{ Core.Constants.CountryCodes.SanMarino, 27 },
														{ Core.Constants.CountryCodes.SaudiArabia, 24 },
														{ Core.Constants.CountryCodes.Serbia, 22 },
														{ Core.Constants.CountryCodes.Slovakia, 24 },
														{ Core.Constants.CountryCodes.Slovenia, 19 },
														{ Core.Constants.CountryCodes.Spain, 24 },
														{ Core.Constants.CountryCodes.Sweden, 24 },
														{ Core.Constants.CountryCodes.Switzerland, 21 },
														{ Core.Constants.CountryCodes.Tunisia, 24 },
														{ Core.Constants.CountryCodes.Turkey, 26 },
														{ Core.Constants.CountryCodes.UnitedArabEmirates, 23 },
														{ Core.Constants.CountryCodes.UnitedKingdom, 22 },
														{ Core.Constants.CountryCodes.VirginIslands, 24 }
													});
			}
		}

		[ThreadStatic]
		static Dictionary<string, int> countryAndLengthDictionary;
	}
}
