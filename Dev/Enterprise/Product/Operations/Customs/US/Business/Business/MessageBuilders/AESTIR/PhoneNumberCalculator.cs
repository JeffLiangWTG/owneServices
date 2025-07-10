using System;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	static class PhoneNumberCalculator
	{
		public static ZString GetUnformattedPhoneNumber(ZString phone, bool keepCountryCode)
		{
			ZString result = phone;
			if (!keepCountryCode)
			{
				result = phone.Trim();

				if (result.Length > 0)
				{
					int indexOfAreaCode = result.IndexOf(MasterFiles.Business.PhoneNumberFormatAndValidation.AreaCodeBeginDelimiter);

					if (indexOfAreaCode > 0)
					{
						result = result.SubstringSafe(indexOfAreaCode).KeepNumericCharacters();
					}
					else
					{
						if (result[0] == MasterFiles.Business.PhoneNumberFormatAndValidation.PlusPhoneNumberPrefix || result.StartsWith(USInternationalAccessCode, StringComparison.CurrentCulture))
						{
							var phoneSplit = phone.Split(' ');

							if (phoneSplit.Length > 2)
							{
								result = ZString.Empty;
								for (int index = 1; index < phoneSplit.Length; index++)
								{
									result += phoneSplit[index];
								}
							}
						}

						if (result.StartsWith(USNatiionalCode, StringComparison.CurrentCulture))
						{
							result = result.SubstringSafe(2);
						}
					}
				}
			}

			result = result.KeepNumericCharacters();
			if (result.Left(3) == USInternationalAccessCode)
			{
				result = result.SubstringSafe(3);
			}

			return result.Right(13);
		}
		const string USInternationalAccessCode = "011";
		const string USNatiionalCode = "+1";
	}
}
