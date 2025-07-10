using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class SEVATCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 12, 14 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(SE|)\d{12}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("19e3b4d4-e447-4f81-8c0b-274613262c7a", "VAT Business Registration Number structures for Sweden are either 'SE999999999999' or '999999999999'.");
	}
}
