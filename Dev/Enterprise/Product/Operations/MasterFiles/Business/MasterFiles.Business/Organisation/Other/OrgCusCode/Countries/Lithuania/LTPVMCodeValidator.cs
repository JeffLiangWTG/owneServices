using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class LTPVMCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 9, 11, 12, 14 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(LT|)(\d{9}|\d{12})$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("52e76686-6d4b-4115-ab33-e87cd80a6bae", "VAT Business Registration Number structures for Lithuania are either 'LT999999999', 'LT999999999999', '999999999' or '999999999999'.");
	}
}
