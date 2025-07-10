using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class BRCCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.PABRC;
		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^[a-zA-Z0-9]{1,4}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidPatternMessage
		{
			get { return Res.GetString("B8E99B42-A2A4-4FC7-8CA6-5B5B9A618B95", @"Branch Registration Code must be an alphanumeric code from 1 to 4 digits long."); }
		}
	}
}
