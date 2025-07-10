using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class LUTVACodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 8, 10 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(LU|)\d{8}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("90940a68-0ade-41b2-9789-85f155baf490", "VAT Business Registration Number structures for Luxembourg are either 'LU99999999' or '99999999'.");
	}
}
