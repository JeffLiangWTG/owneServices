using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class VietnamVATCodeValidator
	{
		public static string CheckCodeAndGetInvalidMessage(ZString code)
		{
			return IsValidCode(code) ? string.Empty : GetInvalidCheckMessage(code);
		}

		internal static bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeValidator.Validate(codeInfo, code => IsValidCode(code), OrganisationRegistry.RegistrationNumberFormatFields.VNVAT, GetInvalidCheckMessage((ZString)codeInfo.Value));
		}

		static bool IsValidCode(ZString code)
		{
			return Regex.IsMatch(code, @"^\d{10}(-\d{3})?$") && CheckComponentNumber(code) && CheckDigitNumber(code) && CheckAffiliateNumber(code);
		}

		static string GetInvalidCheckMessage(ZString code)
		{
			return string.Format(Res.GetString("59572594-D288-42D6-AD53-88B2F4B5E5DC", @"The VN VAT number '{0}' is invalid. It should be in format 'NNNNNNNNNN' or 'NNNNNNNNNN-NNN' and complies with check digit validation."), code);
		}

		static bool CheckComponentNumber(ZString regNo)
		{
			const int indexOfComponentNumber = 2;
			const int lengthOfComponentNumber = 7;
			const string exceptionNumber = "0000000";
			return regNo.Substring(indexOfComponentNumber, lengthOfComponentNumber) != exceptionNumber;
		}

		static bool CheckDigitNumber(ZString regNo)
		{
			const int indexOfCheckDigitNumber = 0;
			const int lengthOfCheckDigitNumber = 9;
			const int indexOfDigitNumber = 9;
			const int lengthOfDigitNumber = 1;
			const int digitCheckBase = 10;
			const double digitCheckSeed = 11;
			var pendingCheckNumber = regNo.Substring(indexOfCheckDigitNumber, lengthOfCheckDigitNumber).ToString();
			var digitNumber = int.Parse(regNo.Substring(indexOfDigitNumber, lengthOfDigitNumber));
			var digitCheckWeights = new[] { 31, 29, 23, 19, 17, 13, 7, 5, 3 };
			var total = digitCheckWeights.Select((t, i) => t * int.Parse(pendingCheckNumber.Substring(i, 1))).Sum();
			return digitNumber == digitCheckBase - (int)(total % digitCheckSeed);
		}

		static bool CheckAffiliateNumber(ZString regNo)
		{
			if (!regNo.Contains('-'))
			{
				return true;
			}

			const int indexOfAffiliateNumber = 11;
			const int lengthOfAffiliateNumber = 3;
			const string exceptionNumber = "000";
			return regNo.Substring(indexOfAffiliateNumber, lengthOfAffiliateNumber) != exceptionNumber;
		}
	}
}
