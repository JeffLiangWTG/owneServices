using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class LVPVNCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 11, 13 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(LV|)\d{11}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("5820ff4e-3804-4eb1-a442-3f298d9c6cdb", "VAT Business Registration Number structures for Latvia are either 'LV99999999999' or '99999999999'.");
	}
}
