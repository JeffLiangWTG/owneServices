using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class HUVATCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 8, 10 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(HU|)\d{8}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("e0d13827-c357-4ce3-ae0e-c9dd57d26599", "VAT Business Registration Number structures for Hungary are either 'HU99999999' or '99999999'.");
	}
}
