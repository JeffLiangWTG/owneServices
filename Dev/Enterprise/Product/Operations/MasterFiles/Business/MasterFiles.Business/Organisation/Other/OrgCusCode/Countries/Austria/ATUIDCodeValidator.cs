using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class ATUIDCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 9, 11 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(AT|)[A-Za-z0-9]{9}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("c4b0d9c5-0dfd-4623-b9b9-5de36df2ea99", "VAT Business Registration Number structures for Austria are either 'ATU99999999' or 'U99999999'.");
	}
}
