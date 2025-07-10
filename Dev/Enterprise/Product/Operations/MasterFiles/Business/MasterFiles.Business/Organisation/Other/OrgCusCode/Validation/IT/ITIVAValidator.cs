using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	sealed class ITIVAValidator
	{
		public void Validate(ZPropertyInfo codeInfo)
		{
			var code = ((ZString)codeInfo.Value);
			var cusCodeValidator = ITCusCodeValidationSelector.GetIVACodeValidator(code);
			var validationResult = cusCodeValidator.Validate(code);

			if (validationResult == ITCusCodeValidationResult.InvalidLength)
			{
				AddMessage(codeInfo, InvalidLength);
			}
			else if (validationResult == ITCusCodeValidationResult.InvalidPattern)
			{
				AddMessage(codeInfo, InvalidPattern);
			}
			else if (validationResult == ITCusCodeValidationResult.InvalidCheckDigit)
			{
				AddMessage(codeInfo, InvalidCheckDigit);
			}
		}

		void AddMessage(ZPropertyInfo codeInfo, string message)
		{
			codeInfo.AddErrorIfEnforced(message, OrganisationRegistry.RegistrationNumberFormatFields.ITIVA);
		}

		string InvalidLength => Res.GetString("84c650bd-f959-4b0a-99af-11e23ef93f76", "The IVA registration code needs to be 13 or 11 in length.");

		string InvalidPattern => Res.GetString("c615e28f-0c9b-4dbf-bf48-dc43ffa234b6", @"The IVA registration code pattern is invalid.
Valid patterns are:
	{0}", validPatterns);

		string InvalidCheckDigit => Res.GetString("b671118d-9375-4618-917a-2a9ec42dfbb8", "The check digit in the IVA registration code is incorrect.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Specific Technical Formatting")]
		const string validPatterns = @"ITnnnnnnnnnnn
	nnnnnnnnnnn";
	}
}
