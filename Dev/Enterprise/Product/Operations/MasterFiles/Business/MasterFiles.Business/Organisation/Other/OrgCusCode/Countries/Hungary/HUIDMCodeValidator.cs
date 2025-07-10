using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class HUIDMCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.HUIDM;

		HashSet<int> ValidLengths => new HashSet<int> { 3, 5, 10, 7 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)"EDI", (NoResString)"PAPER", (NoResString)"ELECTRONIC", (NoResString)"UNKNOWN" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("b394fbb3-247d-4f42-814f-7a9ef0c81d69", "Expected values for Hungary Invoice Delivery Method are 'EDI', 'PAPER', 'ELECTRONIC' or 'UNKNOWN'.");
	}
}
