using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class ROTVACodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(RO|)\d{2,10}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("a1c092bf-9b3c-4f0c-83f6-cbe14527de34", "VAT Business Registration Number structures for Romania are either 'RO9999999999' or '9999999999'.");
	}
}
