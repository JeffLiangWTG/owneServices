using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	class CRCIDCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CRCID;

		HashSet<string> ValidPatternStrings => new HashSet<string> { "^[0-9]{9}$" };

		HashSet<int> ValidLengths => new HashSet<int> { 9 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidLengthMessage => Res.GetString("7A8C4B95-1234-4E71-ABCD-09876DAB4E10", @"The CID registration code length is invalid.

The CID Registration code needs to be 9 in length.");

		string InvalidPatternMessage => Res.GetString("CA3B0BFB-C4AB-455F-A186-79DD11278428", @"The CID registration code pattern is invalid.

Valid pattern is:

nnnnnnnnn

where 'n' is a digit from 0 to 9.");
	}
}
