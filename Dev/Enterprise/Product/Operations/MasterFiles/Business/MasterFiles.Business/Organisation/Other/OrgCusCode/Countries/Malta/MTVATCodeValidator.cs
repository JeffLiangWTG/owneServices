using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class MTVATCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 8, 10 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(MT|)\d{8}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("897b1ce1-c67d-4003-9fdc-07ec8174f3be", "VAT Business Registration Number structures for Malta are either 'MT99999999' or '99999999'.");
	}
}
