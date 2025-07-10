using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business;

class PTUValidator
{
	readonly HashSet<string> validPatternStrings = new HashSet<string> { (NoResString)@"^(PL|)\d{10}$" };

	internal bool Validate(ZPropertyInfo codeInfo)
	{
		return OrgCusCodePatternValidator.ValidateWarning(codeInfo, validPatternStrings, Res.GetString("PLPTUValidator|InvalidPatternMessage",
			"VAT Business Registration Number structures for Poland are either 'PL9999999999' or '9999999999'."));
	}
}
