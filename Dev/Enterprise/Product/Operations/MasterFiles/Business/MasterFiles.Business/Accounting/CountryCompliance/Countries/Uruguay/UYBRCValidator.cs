using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class UYBRCValidator
	{
		string CodeType => UruguayOrgCusCodeInfo.OrgCusCodes.BRC;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.UYBRC;
		HashSet<string> ValidPatternStrings => new HashSet<string> { "^[0-9]{1,4}$" }; // Valid pattern of BRC Code for Uruguay
		HashSet<int> ValidLengths => Enumerable.Range(1, 4).ToHashSet();

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidLengthMessage => Res.GetString("b56658c9-7e94-4f61-9805-c17f6640932c", @"The {0} registration code length is invalid.

{0} codes must be from 1 to 4 digits long.", CodeType);

		string InvalidPatternMessage => Res.GetString("daf33707-9b04-40bb-a06f-627f8f8f0a38", @"The {0} registration code pattern is invalid.

Valid patterns are:
	n
	nn
	nnn
	nnnn
Where 'n' is a digit from 0 to 9.
Please verify that you are entering a correct number.", CodeType);
	}
}
