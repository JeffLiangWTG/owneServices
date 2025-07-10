using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	class CRDIMCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CRDIM;
		HashSet<string> ValidPatternStrings => new HashSet<string> { "^[0-9]{11}$", "^[0-9]{12}$" };
		HashSet<int> ValidLengths => new HashSet<int> { 11, 12 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidLengthMessage => Res.GetString("FDDD4084-78B3-408D-B9F4-F94F170C0DB",
			@"The DIM registration code length is invalid.

The DIM Registration code needs to be 11 or 12 in length.");

		string InvalidPatternMessage => Res.GetString("A12D45F89-9B12-4C9A-834E-51A8E08E2C75",
			@"The DIM registration code pattern is invalid.

Valid pattern is:

nnnnnnnnnnn or nnnnnnnnnnnn

where 'n' is a digit from 0 to 9.");
	}
}
