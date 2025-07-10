using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class IEVATCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 8, 9, 10, 11 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(IE|)([A-Za-z0-9][A-Za-z0-9+*][A-Za-z0-9]{6}|[A-Za-z0-9]{9})$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("a98574b1-a143-4701-9bc2-2bd5993e31e6", "VAT Business Registration Number structures for Ireland are either 'IE9X99999L', 'IE9999999WI', '9X99999L' or '9999999WI'.");
	}
}
