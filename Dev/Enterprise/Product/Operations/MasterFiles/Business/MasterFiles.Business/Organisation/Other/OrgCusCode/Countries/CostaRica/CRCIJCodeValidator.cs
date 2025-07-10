using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	class CRCIJCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CRCIJ;

		HashSet<string> ValidPatternStrings => new HashSet<string> { "^[0-9]{10}$" };

		HashSet<int> ValidLengths => new HashSet<int> { 10 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidLengthMessage => Res.GetString("B1C4D7E3-5678-4E71-ABCD-123456DAB4E10", @"The CIJ registration code length is invalid.

The CIJ Registration code needs to be 10 in length.");

		string InvalidPatternMessage => Res.GetString("A2D5E6F7-9876-455F-A186-79DD11278428", @"The CIJ registration code pattern is invalid.

Valid pattern is:

nnnnnnnnnn

where 'n' is a digit from 0 to 9.");
	}
}
