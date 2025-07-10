using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class RFCCodeValidator
	{
		string CodeType => MexicoOrgCusCodeInfo.OrgCusCodes.RFC;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.MXRFC;

		HashSet<string> ValidPatternStrings => new HashSet<string> { @"^[A-Z&]{3}[0-9]{6}[A-Z0-9]{3}$", @"^[A-Z&]{4}[0-9]{6}[A-Z0-9]{3}$", @"^[A-Z&]{4}[0-9]{9}$" };
		HashSet<int> ValidLengths => new HashSet<int> { 12, 13 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidLengthMessage
		{
			get { return Res.GetString("1CFFCA4E-83D8-4524-ACF7-8557E82827F9", @"The {0} registration code length is invalid.

{0} codes must be 12 or 13 digits long.", CodeType); }
		}

		string InvalidPatternMessage
		{
			get
			{
				return Res.GetString("AA7A541E-1370-4A74-880B-D87DCC6EA12E", @"The {0} registration code pattern is invalid.

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
