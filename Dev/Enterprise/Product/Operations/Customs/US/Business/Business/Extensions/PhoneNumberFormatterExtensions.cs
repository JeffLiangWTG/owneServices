using System;
using System.Text.RegularExpressions;
using CargoWise.Tools.Telephony;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class PhoneNumberFormatterExtensions
	{
		public static ZString GetLocalPhoneNumber(this ZString workPhone, ZString defaultCountryCode)
		{
			var result = workPhone;
			var phoneNumberFormatter = new PhoneNumberFormatter();
			var countryCode = phoneNumberFormatter.GetCountryCode(workPhone, defaultCountryCode);
			if (countryCode != null)
			{
				var diallingCode = countryCode.DiallingCode;
				var prefix = ZString.Format("+{0}", diallingCode);
				if (workPhone.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
				{
					result = workPhone.RemoveSafe(0, prefix.Length);
				}
			}
			result = result.KeepNumericCharacters();
			return result;
		}

		public static ZString GetUSFormattPhoneNumber(this ZString workPhone)
		{
			return ((ZString)new Regex("[^0-9]").Replace(workPhone, "")).Right(10);
		}
	}
}
