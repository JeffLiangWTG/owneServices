using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	class CZDPHCodeValidator
	{
		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("cc465124-42a8-4744-bff8-eafdc96c62ad", "VAT Business Registration Number structures for Czech Republic are either 'CZ99999999', 'CZ999999999', 'CZ9999999999', '99999999', '999999999' or '9999999999'.");

		HashSet<int> ValidLengths => new HashSet<int> { 8, 9, 10, 11, 12 };

		HashSet<string> ValidPatternStrings => new HashSet<string> { (NoResString)@"^(CZ|)(\d{8}|\d{9}|\d{10})$" };
	}
}
