using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	class CLACTValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CLACT;
		HashSet<string> ValidPatternStrings => new HashSet<string> { @"^[0-9]{6}$", @"^[0-9]{6},[0-9]{6}$", @"^[0-9]{6},[0-9]{6},[0-9]{6}$", @"^[0-9]{6},[0-9]{6},[0-9]{6},[0-9]{6}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("459CA88D-5A52-47F8-ABD5-57D362D013C8", @"The ACT registration code pattern is invalid.

Valid patterns are:

{0}

Where 'n' is a digit from 0 to 9.", validPatterns);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Specific Technical Formatting")]
		const string validPatterns = @"nnnnnn
nnnnnn,nnnnnn
nnnnnn,nnnnnn,nnnnnn
nnnnnn,nnnnnn,nnnnnn,nnnnnn";
	}
}
