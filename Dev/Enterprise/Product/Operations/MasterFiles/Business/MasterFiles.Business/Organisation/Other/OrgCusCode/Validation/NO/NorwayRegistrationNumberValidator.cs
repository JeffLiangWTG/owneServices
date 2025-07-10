using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class NorwayRegistrationNumberValidator
	{
		public static string GetValidateMVAGBRNotifyInformation(string number, string type)
		{
			string mVAErrorMessage = Res.GetString("6F0695A6-84BA-49CC-9554-1F233DAA129D", "Registration Number / Code: Norway MVA (VAT Tax ID) should be 9 digits NNNNNNNNN and compiled with last digit check sum calculation to ensure its validity. An error in this field indicates that the entered value is invalid - please double check you have inputted the correct values.");
			string gBRErrorMessage = Res.GetString("E1E9D9952-21EF-404E-8D33-CA8D4018C0EB", "Registration Number / Code: Norway Government Business Code should be 9 digits NNNNNNNNN and compiled with last digit check sum calculation to ensure its validity. An error in this field indicates that the entered value is invalid - please double check you have inputted the correct values.");
			string result = string.Empty;

			if (IsValidRegistrationNumber(number))
			{
				if (!IsValidVATNumber(number))
				{
					result = type.Equals(OrgCusCode.NorwayCodeTypes.MVA) ? mVAErrorMessage : gBRErrorMessage;
				}
			}
			else
			{
				result = type.Equals(OrgCusCode.NorwayCodeTypes.MVA) ? mVAErrorMessage : gBRErrorMessage;
			}
			return result;
		}

		public static void ValidateGBR(ZPropertyInfo targetInfo, ZString code, INotificationType notificationType)
		{
			string validateNotifyInformation = GetValidateMVAGBRNotifyInformation(code, OrgCusCode.CodeTypes.GovBusinessCode);
			if (!string.IsNullOrEmpty(validateNotifyInformation))
			{
				targetInfo.AddErrorIfEnforced(validateNotifyInformation, OrgCusCode.CodeTypes.GovBusinessCode);
			}
		}
		public static void ValidateMVA(ZPropertyInfo targetInfo, ZString code, INotificationType notificationType)
		{
			string validateNotifyInformation = GetValidateMVAGBRNotifyInformation(code, OrgCusCode.NorwayCodeTypes.MVA);
			if (!string.IsNullOrEmpty(validateNotifyInformation))
			{
				targetInfo.AddErrorIfEnforced(validateNotifyInformation, OrgCusCode.NorwayCodeTypes.MVA);
			}
		}

		public static void ValidateEMD(ZPropertyInfo targetInfo, ZString code, INotificationType notificationType)
		{
			if(code.Length <= 10)
			{
				return;
			}

			string errorMessage = Res.GetString("FEF68640-8A74-4C40-BF36-DD50C0A1D833", "The length of Registration Number / Code for Type 'EMD' cannot exceed 10 characters.");
			targetInfo.AddMessageError(errorMessage);
		}

		static bool IsValidRegistrationNumber(string number)
		{
			return new Regex("^[0-9]{9}$").IsMatch(number);
		}
		static bool IsValidVATNumber(string number)
		{
			int[] weightedCodes = new int[8] { 3, 2, 7, 6, 5, 4, 3, 2 };
			int lastDigit = number[number.Length - 1] - '0';
			number = number.Substring(0, number.Length - 1);
			int weightedCodePointer = 0, sum = 0, checkDigit;
			foreach (var digit in number)
			{
				sum += (int.Parse(digit.ToString()) * weightedCodes[weightedCodePointer]);
				weightedCodePointer++;
			}
			if (sum % 11 == 0)
			{
				checkDigit = 0;
			}
			else if (sum % 11 == 1)
			{
				checkDigit = 1;
			}
			else
			{
				checkDigit = 11 - (sum % 11);
			}

			return checkDigit == lastDigit;
		}
	}
}
