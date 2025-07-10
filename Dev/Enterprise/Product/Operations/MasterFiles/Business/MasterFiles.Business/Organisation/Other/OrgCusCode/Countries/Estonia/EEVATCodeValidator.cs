using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class EEVATCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 9, 11 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(EE|)\d{9}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("959101c9-9733-446b-8bd2-e9dc1193e3a3", "VAT Business Registration Number structures for Estonia are either 'EE999999999' or '999999999'.");
	}
}
