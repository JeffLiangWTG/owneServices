using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class UYAEOValidator
	{
		string CodeType => UruguayOrgCusCodeInfo.OrgCusCodes.AEO;

		HashSet<int> ValidLengths => new HashSet<int> { 12 };
		string InvalidLengthMessage => Res.GetString("954D6C35-CE9F-4BAB-8BBF-D5098A6FB2C6", @"The {0} registration code length is invalid.
{0} codes must be 12 digits long.", CodeType);

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^\d{12}$" };
		string InvalidPatternMessage => Res.GetString("5B1E6507-4C08-4E9F-A134-6EFCC076D1DB", @"The {0} registration code pattern is invalid.
{0} should be 12 numbers, e.g. 123456789012.", CodeType);

		internal bool Validate(ZPropertyInfo targetInfo)
		{
			return OrgCusCodeLengthValidator.Validate(targetInfo, ValidLengths, string.Empty, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(targetInfo, ValidPatternStrings, string.Empty, InvalidPatternMessage);
		}
	}
}
