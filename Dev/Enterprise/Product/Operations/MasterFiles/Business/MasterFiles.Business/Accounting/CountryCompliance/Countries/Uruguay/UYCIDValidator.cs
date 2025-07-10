using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class UYCIDValidator
	{
		string CodeType => UruguayOrgCusCodeInfo.OrgCusCodes.CID;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.UYCID;
		HashSet<string> ValidPatternStrings => new HashSet<string> { "^[0-9]{8}$", "^[0-9]{1}\\.[0-9]{3}\\.[0-9]{3}\\-[0-9]{1}$" }; // Valid pattern of CID for Uruguay
		HashSet<int> ValidLengths => new HashSet<int> { 8, 11 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidLengthMessage => Res.GetString("1E34976E-ABA9-4D7B-AA0D-E1268D8F7392", @"The {0} ({1} / Identity Card number) needs to be 8 or 11 characters in length, including the check digit, points and dash.", CodeType, "Cédula de identidad");

		string InvalidPatternMessage => Res.GetString("00F5D517-F026-421B-A4D5-A1875B5C88BF", @"The {0} ({1} / Identity Card number) pattern is invalid.

Valid patterns are:
	n.nnn.nnn-n
	nnnnnnnn

with 'n' a digit from 0 to 9", CodeType, "Cédula de identidad");
	}
}
