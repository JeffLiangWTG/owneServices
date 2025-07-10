using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class NZCustomsCodeValidator
	{
		public NZCustomsCodeValidator(string type)
		{
			this.Type = type;
		}
		protected string Type;

		/// <summary>
		/// Validate Format and check digit and returns a user-friendly error message. 
		/// If the return value is empty, it means it is valid
		/// </summary>
		public string GetCasperCodeValidationErrors(string customsCode)
		{
			string errorMessage = string.Empty;
			if (!ValidateFormat(customsCode, CustomsCodeFormatNew) && !ValidateFormat(customsCode, CustomsCodeFormatOld))
			{
				errorMessage = CustomsCodeInWrongBasicFormat;
			}
			else
			{
				string expectedCheckDigitIfCurrentCheckDigitInvalid = GetExpectedCheckDigitIfCurrentCheckDigitInvalid(customsCode);
				if (!string.IsNullOrEmpty(expectedCheckDigitIfCurrentCheckDigitInvalid))
				{
					errorMessage = CustomsCodeHasWrongCheckDigit + expectedCheckDigitIfCurrentCheckDigitInvalid;
				}
			}
			return errorMessage;
		}

		public static string CustomsCodeInWrongBasicFormat
		{
			get { return Res.GetString("beb66f48-e0d5-4346-9d44-631db2977264", "This customs code is in an invalid format. It should be 6 or 8 digits and a check letter."); }
		}
		public static string CustomsCodeHasWrongCheckDigit
		{
			get { return Res.GetString("01f8deef-d59b-4966-935e-a6c1d30dbe6f", "This customs code has an invalid check letter. The last letter should be") + " "; }
		}

		#region ValidateFormat
		protected bool ValidateFormat(string customsCode, string customsCodeFormat)
		{
			bool isValid = customsCodeFormat.Length == customsCode.Length;
			try
			{
				for (int index = 0; isValid && index < customsCode.Length; index++)
				{
					isValid = customsCodeFormat[index] == 'N' ? char.IsDigit(customsCode, index) : char.IsLetter(customsCode, index);
				}
			}
			catch (IndexOutOfRangeException)
			{
				isValid = false;
			}

			return isValid;
		}

		protected const string CustomsCodeFormatOld = "NNNNNNA";
		protected const string CustomsCodeFormatNew = "NNNNNNNNA";
		#endregion

		#region ValidateCheckDigit
		protected string GetExpectedCheckDigitIfCurrentCheckDigitInvalid(string customsCode)
		{
			if (Type == OrgCusCode.CodeTypes.SupplierCode)
			{
				string expectedCheckDigit = GetExpectedSupplierCodeCheckDigit(customsCode);
				return customsCode.EndsWith(expectedCheckDigit) ? "" : expectedCheckDigit;
			}
			else if (Type == OrgCusCode.CodeTypes.CustomsClientCode)
			{
				string expectedCheckDigit = GetExpectedClientCodeCheckDigit(customsCode);
				return customsCode.EndsWith(expectedCheckDigit) ? "" : expectedCheckDigit;
			}
			return "";
		}

		protected internal string GetExpectedSupplierCodeCheckDigit(ZString supplierCode)
		{
			return GetExpectedCheckDigit(supplierCode, "MZYXWVTRQPN");
		}

		protected internal string GetExpectedClientCodeCheckDigit(ZString clientCode)
		{
			return GetExpectedCheckDigit(clientCode, "ALKJHGFEDCB");
		}

		string GetExpectedCheckDigit(string casperCode, string elevenCharacterSeed)
		{
			ZString inputString = casperCode.Trim().ToUpper().PadLeft(13, '0');
			int value = int.Parse(inputString.Substring(0, 1)) * 7;
			value += int.Parse(inputString.Substring(1, 1)) * 6;
			value += int.Parse(inputString.Substring(2, 1)) * 5;
			value += int.Parse(inputString.Substring(3, 1)) * 4;
			value += int.Parse(inputString.Substring(4, 1)) * 3;
			value += int.Parse(inputString.Substring(5, 1)) * 2;
			value += int.Parse(inputString.Substring(6, 1)) * 7;
			value += int.Parse(inputString.Substring(7, 1)) * 6;
			value += int.Parse(inputString.Substring(8, 1)) * 5;
			value += int.Parse(inputString.Substring(9, 1)) * 4;
			value += int.Parse(inputString.Substring(10, 1)) * 3;
			value += int.Parse(inputString.Substring(11, 1)) * 2;

			int modValue = value % 11;

			return elevenCharacterSeed.Substring(modValue, 1);
		}
		#endregion
	}
}
