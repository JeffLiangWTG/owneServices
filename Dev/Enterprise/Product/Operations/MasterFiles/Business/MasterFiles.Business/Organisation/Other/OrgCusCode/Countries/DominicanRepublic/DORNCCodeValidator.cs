using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance.DominicanRepublic;

namespace Enterprise.MasterFiles.Business
{
	public class DORNCCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.DORNC;

		HashSet<string> ValidPatternStrings => new HashSet<string> { @"^[0-9]{3}-[0-9]{5}-[0-9]{1}$", @"^[0-9]{3}-[0-9]{7}-[0-9]{1}$", @"^[0-9]{8}-[0-9]{1}$", @"^[0-9]{10}-[0-9]{1}$", @"^[0-9]{9}$", @"^[0-9]{11}$" }; // Valid pattern of RNC for Dominican Republic

		HashSet<int> ValidLengths => new HashSet<int> { 9, 10, 11, 12, 13 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
					&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage)
					&& OrgCusCodeValidator.Validate(codeInfo, code => IsValidCheckDigit(code), OrganisationRegistryCodeType, InvalidCheckDigitMessage);
		}

		protected bool IsValidCheckDigit(ZString code)
		{
			ZString number = code.Replace("-", string.Empty);

			return number.Length == 9 ? DomicanRepublicValidatorHelper.ValidMod11CheckDigit(number) : DomicanRepublicValidatorHelper.ValidMod10CheckDigit(number);
		}

		string InvalidLengthMessage => Res.GetString("6E3480DF-0C5D-4530-BC15-59769E62567A", @"The RNC registration code length is invalid.

The RNC Registration code needs to be 9, 10, 11, 12 or 13 in length.");

		string InvalidPatternMessage => Res.GetString("E61CFAD0-CC86-4F26-BE82-8476CBB006B4", @"The RNC registration code pattern is invalid.

Valid patterns are:

{0}

where 'n' is a digit from 0 to 9.", validPatterns);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Specific Technical Formatting")]
		const string validPatterns = @"nnn-nnnnn-n OR nnn-nnnnnnn-n
nnnnnnnn-n OR nnnnnnnnnn-n
nnnnnnnnn OR nnnnnnnnnnn";

		string InvalidCheckDigitMessage => Res.GetString("46C75D8C-CA12-4809-9F44-EB90DA9BFB3A", "The check digit in the RNC registration code is incorrect.");
	}
}
