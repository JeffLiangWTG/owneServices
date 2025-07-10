using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance.DominicanRepublic;

namespace Enterprise.MasterFiles.Business
{
	public class DOCEDCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.DOCED;

		HashSet<string> ValidPatternStrings => new HashSet<string> { @"^[0-9]{3}-[0-9]{7}-[0-9]{1}$", @"^[0-9]{10}-[0-9]{1}$", @"^[0-9]{11}$" };

		HashSet<int> ValidLengths => new HashSet<int> { 11, 12, 13 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage) &&
				OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage) &&
				OrgCusCodeValidator.Validate(codeInfo, code => IsValidCheckDigit(code), OrganisationRegistryCodeType, InvalidCheckDigitMessage);
		}

		public bool IsValidCheckDigit(ZString code)
		{
			ZString number = code.Replace("-", string.Empty);

			return DomicanRepublicValidatorHelper.ValidMod10CheckDigit(number);
		}

		string InvalidLengthMessage => Res.GetString("53EF75EE-67B6-445A-A627-D1406A840E9B", @"The CED registration code length is invalid.

The CED Registration code needs to be 11, 12 or 13 in length.");

		string InvalidPatternMessage => Res.GetString("4CFE2F00-35E5-4B37-9177-EF1D1A698E28", @"The CED registration code pattern is invalid.

Valid patterns are:

{0}   
 
where 'n' is a digit from 0 to 9.", validPatterns);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Specific Technical Formatting")]
		const string validPatterns = @"nnn-nnnnnnn-n OR
nnnnnnnnnn-n  OR 
nnnnnnnnnnn";

		string InvalidCheckDigitMessage => Res.GetString("67CDF2F5-E0D0-4AF8-95BB-03B09499FA48", "The check digit in the CED registration code is incorrect.");
	}
}
