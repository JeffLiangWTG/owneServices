using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class RFGCodeValidator
	{
		HashSet<string> ValidPatternStrings => new HashSet<string> { @"XAXX010101000", @"XEXX010101000" };
		HashSet<int> ValidLengths => new HashSet<int> { 13 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, "", InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, "", InvalidPatternMessage);
		}

		static string InvalidLengthMessage
		{
			get { return Res.GetString("3D51B640-305D-4CE3-BEAC-48A02F205B57", "The RFG registration code needs to be 13 characters in length."); }
		}

		static string InvalidPatternMessage
		{
			get
			{
				return Res.GetString(@"BB1FB192-D22A-4B8E-A40B-3D6E2A73E444", @"The RFG registration code pattern is invalid.

Valid patterns are:
	XAXX010101000 (for General Public Organizations)
	XEXX010101000 (for Foreign Country Organizations)

Please verify that you are entering a correct registration code.");
			}
		}
	}
}
