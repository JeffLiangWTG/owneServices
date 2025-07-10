using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class BGVATCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 9, 10, 11, 12 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(BG|)(\d{9}|\d{10})$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("ba1d7d4b-563c-4068-8149-fd1dc88744a3", "VAT Business Registration Number structures for Bulgaria are either 'BG999999999', 'BG9999999999', '999999999' or '9999999999'.");
	}
}
