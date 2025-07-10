using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class IVACodeValidator
	{
		string CodeType => OrgCusCode.CodeTypes.IVA;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.MXIVA;
		HashSet<string> ValidPatternStrings => new HashSet<string> { @"^[A-Z&]{3}[0-9]{6}[A-Z0-9]{3}$", @"^[A-Z&]{4}[0-9]{6}[A-Z0-9]{3}$", @"^[A-Z&]{4}[0-9]{9}$" };
		HashSet<int> ValidLengths => new HashSet<int> { 12, 13 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidLengthMessage
		{
			get { return Res.GetString("986240B0-66C1-4165-917E-D4E1B3801BCA", @"The {0} registration code length is invalid.

{0} codes must be 12 or 13 digits long.", CodeType); }
		}

		string InvalidPatternMessage
		{
			get
			{
				return Res.GetString("AE208AE4-412F-4008-916E-BE5D7A3441AB", @"The {0} registration code pattern is invalid.

Valid patterns are:
	{1}

with 'X' an uppercase alphabetic character or '&' character; 'n' a digit from 0 to 9; 'Y' an alphanumeric character.", CodeType, ValidPatterns);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Specific Technical Formatting")]
		const string ValidPatterns = @"XXXnnnnnnYYY
	XXXXnnnnnnYYY
	XXXXnnnnnnnnn";
	}
}
