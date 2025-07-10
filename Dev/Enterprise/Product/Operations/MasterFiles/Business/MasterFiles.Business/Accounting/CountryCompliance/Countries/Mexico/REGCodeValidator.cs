using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class REGCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.MXREG;
		HashSet<string> ValidPatternStrings => new HashSet<string> { "^601$", "^603$", "^605$", "^606$", "^607$", "^608$", "^610$", "^611$", "^612$", "^614$", "^615$", "^616$", "^620$", "^621$", "^622$", "^623$", "^624$", "^625$", "^626$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidCodeMessage);
		}

		string InvalidCodeMessage => Res.GetString(@"16479981-5A85-4922-A00F-E7CBDCFADBDE", @"The 'MX REG' registration code is invalid.

Valid codes are:
601, 603, 605, 606, 607, 608, 610, 611, 612, 614, 615, 616, 620, 621, 622, 623, 624, 625 or 626

Please verify that you are entering a valid code.");
	}
}
