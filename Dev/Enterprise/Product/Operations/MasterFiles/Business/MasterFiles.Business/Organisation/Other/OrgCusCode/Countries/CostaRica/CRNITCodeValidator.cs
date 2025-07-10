using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	class CRNITCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CRNIT;

		HashSet<string> ValidPatternStrings => new HashSet<string> { "^[0-9]{10}$" };

		HashSet<int> ValidLengths => new HashSet<int> { 10 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidLengthMessage => Res.GetString("A8F3C28-53F9-421B-BBCA-BFBC87233907", @"The NIT registration code length is invalid.

The NIT Registration code needs to be 10 in length.");

		string InvalidPatternMessage => Res.GetString("A5FAC62E-2046-47F9-A504-4CD86405E4DC", @"The NIT registration code pattern is invalid.

Valid pattern is:

nnnnnnnnnn

where 'n' is a digit from 0 to 9.");
	}
}
