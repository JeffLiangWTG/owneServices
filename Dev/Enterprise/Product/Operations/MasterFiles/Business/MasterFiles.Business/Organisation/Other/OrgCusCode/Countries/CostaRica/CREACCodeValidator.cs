using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	class CREACCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CREAC;

		HashSet<string> ValidPatternStrings => new HashSet<string> { "^[0-9]{6}$" };

		HashSet<int> ValidLengths => new HashSet<int> { 6 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidLengthMessage => Res.GetString("8939B4FE-9651-4D43-8F35-F508D2765B9A", @"The EAC registration code length is invalid.

The EAC Registration code needs to be 6 in length.");

		string InvalidPatternMessage => Res.GetString("2278D1F0-912F-4100-9BBF-18A3300E1F1A", @"The EAC registration code pattern is invalid.

Valid pattern is:

nnnnnn

where 'n' is a digit from 0 to 9.");
	}
}
