using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class DomesticPhoneNoValidator
	{
		public static string Validate(ZString number)
		{
			return IsDomesticPhoneNo(number) ? "" : DomesticPhoneNoFormat;
		}

		public const string DomesticPhoneNoFormat = "For PGA contact, a US domestic phone number is required. It must be 10 numeric digits only, (area code + number - no country code).";

		public static bool IsDomesticPhoneNo(ZString phoneNo)
		{
			string phoneNumber = phoneNo.KeepNumericCharacters();
			return phoneNumber.Length == 10 && phoneNumber[0] != '1';
		}
	}
}
