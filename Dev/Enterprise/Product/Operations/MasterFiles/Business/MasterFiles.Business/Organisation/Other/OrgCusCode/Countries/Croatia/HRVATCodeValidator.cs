using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class HRVATCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 11, 13 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(HR|)\d{11}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("d3af711f-2e02-43cc-a0a9-eec39d60c408", "VAT Business Registration Number structures for Croatia are either 'HR99999999999' or '99999999999'.");
	}
}
