using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class SKDPHCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 10, 12 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(SK|)\d{10}$" };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("04a38a26-5563-4330-b4ba-48618a26b58e", "VAT Business Registration Number structures for Slovakia are either 'SK9999999999' or '9999999999'.");
	}
}
