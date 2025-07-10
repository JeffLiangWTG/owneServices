using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class BEBTWCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 10, 12 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(BE|)\d{10}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("e2c2a3cb-aa66-4ea4-80a7-09b325e874fd", "VAT Business Registration Number structures for Belgium are either 'BE0999999999', 'BE1999999999', '0999999999' or '1999999999'.");
	}
}
