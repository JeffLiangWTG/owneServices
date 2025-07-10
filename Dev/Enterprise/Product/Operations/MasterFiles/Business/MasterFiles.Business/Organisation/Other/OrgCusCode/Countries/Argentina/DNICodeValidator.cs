using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	class DNICodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.ARDNI;
		HashSet<string> ValidPatternStrings => new HashSet<string> { "^[0-9]{7}$", "^[0-9]{8}$", "^[0-9]{1}\\.[0-9]{3}\\.[0-9]{3}$", "^[0-9]{2}\\.[0-9]{3}\\.[0-9]{3}$" };
		HashSet<int> ValidLengths => new HashSet<int> { 7, 8, 9, 10 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidLengthMessage => Res.GetString("FEA52260-CCD5-4E71-9E35-DAB4E10C6BB9", @"The DNI registration code length is invalid.
DNI codes must be 7, 8, 9 or 10 digits long.");
		string InvalidPatternMessage => Res.GetString(@"86B9C184-BB38-42C3-ACD7-34C0BCA82D24", @"The DNI registration code pattern is invalid.

Valid patterns are:
	nnnnnnnn
	nn.nnn.nnn
	nnnnnnn
	n.nnn.nnn

with 'n' is a digit from 0 to 9. 
Please verify that you are entering a correct number.");
	}
}
