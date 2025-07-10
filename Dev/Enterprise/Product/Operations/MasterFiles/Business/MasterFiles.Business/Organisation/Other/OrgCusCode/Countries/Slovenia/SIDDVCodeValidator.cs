using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class SIDDVCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 8, 10 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(SI|)\d{8}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("17ab1c6d-122a-4c8f-b3de-bd010975f569", "VAT Business Registration Number structures for Slovenia are either 'SI99999999' or '99999999'.");
	}
}
