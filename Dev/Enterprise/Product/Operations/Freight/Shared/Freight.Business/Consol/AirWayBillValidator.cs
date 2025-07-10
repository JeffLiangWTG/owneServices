using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class AirWayBillValidator
	{
		protected enum ValidationLevel
		{
			Warning,
			MessageError
		}

		protected class ValidationResult
		{
			public string Message { get; }
			public ValidationLevel Level { get; }

			public ValidationResult(string message, ValidationLevel level)
			{
				Message = message ?? throw new ArgumentNullException(nameof(message));
				Level = level;
			}

			public static ValidationResult Valid => null;
		}

		enum WarningEscalationPolicy
		{
			None,
			WarningAsMessageError
		}

		public void Validate(ZPropertyInfo propertyInfo) => ValidateWithWarningsEscalation(propertyInfo, WarningEscalationPolicy.None);
		public void ValidateAndAddMessageError(ZPropertyInfo propertyInfo) => ValidateWithWarningsEscalation(propertyInfo, WarningEscalationPolicy.WarningAsMessageError);

		void ValidateWithWarningsEscalation(ZPropertyInfo propertyInfo, WarningEscalationPolicy policy)
		{
			var masterBill = propertyInfo.Value.ToString();
			var validationResult = GetValidationResult(masterBill);
			if (validationResult is not null && !string.IsNullOrEmpty(validationResult.Message))
			{
				var effectiveLevel = (validationResult.Level, policy) switch
				{
					(ValidationLevel.Warning, WarningEscalationPolicy.WarningAsMessageError) => ValidationLevel.MessageError,
					(_, _) => validationResult.Level,
				};

				switch (effectiveLevel)
				{
					case ValidationLevel.Warning:
						propertyInfo.AddWarning(validationResult.Message);
						break;
					case ValidationLevel.MessageError:
						propertyInfo.AddMessageError(validationResult.Message);
						break;
					default:
						throw new InvalidOperationException($"Level {effectiveLevel} is not supported.");
				}
			}
		}

		public string GetWarningMessage(string masterBillNumber)
		{
			var validationResult = GetValidationResult(masterBillNumber);
			return validationResult is not null
				? validationResult.Message
				: string.Empty;
		}

		protected virtual ValidationResult GetValidationResult(string masterBillNumber) =>
			ValidateLength(masterBillNumber)
			?? ValidateFormat(masterBillNumber)
			?? ValidateCheckDigit(masterBillNumber);

		ValidationResult ValidateLength(string masterBillNumber) =>
			(masterBillNumber.Length == MAWBLength)
				? ValidationResult.Valid
				: new (MAWBLengthWarningMessage, MAWBLengthValidationLevel);

		ValidationResult ValidateFormat(string masterBillNumber) =>
			IsMatchingBillFormat(masterBillNumber)
				? ValidationResult.Valid
				: new (InvalidMAWBFormatWarningMessage, MAWBFormatValidationLevel);

		ValidationResult ValidateCheckDigit(string masterBillNumber)
		{
			var numberToCheck = GetNumberForCheckDigit(masterBillNumber);
			var expectedCheckDigit = Convert.ToInt32(numberToCheck.Substring(3, 7)) % 7;
			var actualCheckDigit = Convert.ToInt32(numberToCheck.Substring(numberToCheck.Length - 1, 1));

			return actualCheckDigit == expectedCheckDigit
				? ValidationResult.Valid
				: new (InvalidMAWBCheckDigitWarningMessage(expectedCheckDigit), MAWBCheckDigitValidationLevel);
		}

		protected virtual string GetNumberForCheckDigit(ZString masterBillNumber)
		{
			return masterBillNumber;
		}

		protected virtual int MAWBLength
		{
			get { return 11; }
		}

		protected virtual string MAWBLengthWarningMessage
		{
			get { return Res.GetString("4d2eb5bc-43e7-4550-93ad-9c55311f9f8c", "The MAWB should contain {0} digits.", MAWBLength.ToString()); }
		}

		protected virtual ValidationLevel MAWBLengthValidationLevel => ValidationLevel.Warning;

		protected virtual bool IsMatchingBillFormat(string masterBillNumber)
		{
			return Regex.IsMatch(masterBillNumber, @"^[0-9]{11}$");
		}

		protected virtual string InvalidMAWBFormatWarningMessage
		{
			get { return Res.GetString("b084eee2-1194-4212-889a-9c872669a9ef", "The MAWB can only contain numbers."); }
		}

		protected virtual ValidationLevel MAWBFormatValidationLevel => ValidationLevel.Warning;

		protected virtual string InvalidMAWBCheckDigitWarningMessage(int expectedCheckDigit) =>
			Res.GetString("ec0d7e86-84e9-4454-b45f-181f8629c4ba", "Invalid check digit. The last digit should be '{0}'", expectedCheckDigit);

		protected virtual ValidationLevel MAWBCheckDigitValidationLevel => ValidationLevel.Warning;
	}
}
