using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class NorwayPersonalNumberValidator
	{
		// A java-version of the validation-rules is available from the Norwegian authorities here:
		// https://github.com/navikt/fnrvalidator/blob/master/src/validator.js

		public struct Result
		{
			public bool Success { get; set; }
			public string ErrorMessage { get; set; }

			public static implicit operator Result(string errorMessage)
			{
				return new Result { Success = string.IsNullOrEmpty(errorMessage), ErrorMessage = errorMessage };
			}
		}

		static class StatusMessages
		{
			public static string ErrorMessageGeneric => Res.GetString("8442C79C-0406-C899-4DB8-32DB20345B8D", "Not a valid Norwegian Social Security Number.");
			public static string ErrorMessageWrongLength => Res.GetString("7026490C-324C-37B9-400D-D7448311F5AB", "Wrong length. Norwegian Social Security Numbers should be 11 digits.");
			public static string ErrorMessageDigitsOnly => Res.GetString("5A4A2ECE-16D2-9598-4FE0-6F653A02D5E0", "Norwegian Social Security Numbers can only contain digits.");
			public static string SuccessMessage => ZString.Empty;
		}

		public static Result Validate(string number)
		{
			if (number == null || number.Length != 11)
			{
				return StatusMessages.ErrorMessageWrongLength;
			}

			if (!number.All(char.IsDigit))
			{
				return StatusMessages.ErrorMessageDigitsOnly;
			}

			if (!IsChecksumValid(number))
			{
				return StatusMessages.ErrorMessageGeneric;
			}

			return StatusMessages.SuccessMessage;
		}

		static bool IsChecksumValid(string number)
		{
			var n = number.Select(c => int.Parse(c.ToString())).ToArray();

			int checkSum1 = 11 - (3 * n[0] + 7 * n[1] + 6 * n[2] + 1 * n[3] + 8 * n[4] + 9 * n[5] + 4 * n[6] + 5 * n[7] + 2 * n[8]) % 11;
			if (checkSum1 == 11)
			{
				checkSum1 = 0;
			}

			if (checkSum1 == 10 || checkSum1 != n[9])
			{
				return false;
			}

			int checkSum2 = 11 - (5 * n[0] + 4 * n[1] + 3 * n[2] + 2 * n[3] + 7 * n[4] + 6 * n[5] + 5 * n[6] + 4 * n[7] + 3 * n[8] + 2 * checkSum1) % 11;
			if (checkSum2 == 11)
			{
				checkSum2 = 0;
			}

			if (checkSum2 == 10 || checkSum2 != n[10])
			{
				return false;
			}
			return true;
		}
	}
}
