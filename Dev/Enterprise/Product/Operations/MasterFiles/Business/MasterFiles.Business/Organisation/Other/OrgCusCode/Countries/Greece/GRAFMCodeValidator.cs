using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class GRAFMCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 9, 11 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(EL|)\d{9}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("0533336e-4b89-48d8-bf69-d321aeee3f59", "VAT Business Registration Number structures for Greece are either 'EL999999999' or '999999999'.");
	}
}
