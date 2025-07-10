using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	class CRUBICodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CRUBI;

		HashSet<string> ValidPatternStrings => new HashSet<string> { "^[0-9]{5}$" };

		HashSet<int> ValidLengths => new HashSet<int> { 5 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidLengthMessage => Res.GetString("C8B0D9C2-ABCD-4E29-9D6E-UBI001", @"The UBI registration code length is invalid.

The UBI Registration code needs to be 5 in length.");

		string InvalidPatternMessage => Res.GetString("C8B0D9C2-ABCD-4E29-9D6E-UBI002", @"The UBI registration code pattern is invalid.

Valid pattern is:

nnnnn

where 'n' is a digit from 0 to 9.");
	}
}
