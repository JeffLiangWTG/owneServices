using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class FIVATCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 8, 10 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(FI|)\d{8}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("78912b9b-a808-45de-bdbb-59981267cabb", "VAT Business Registration Number structures for Finland are either 'FI99999999' or '99999999'.");
	}
}
