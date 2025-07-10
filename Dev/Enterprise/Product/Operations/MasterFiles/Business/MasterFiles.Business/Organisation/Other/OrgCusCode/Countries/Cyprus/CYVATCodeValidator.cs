using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class CYVATCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 9, 11 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(CY|)[A-Za-z0-9]{9}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("a100e4a0-8d1c-4302-a306-aa2b571ca112", "VAT Business Registration Number structures for Cyprus are either 'CY99999999L' or '99999999L'.");
	}
}
