using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	class ESNIFCodeValidator
	{
		HashSet<int> ValidLengths => new HashSet<int> { 9, 11 };

		HashSet<string> ValidPatternStrings => new HashSet<string>
		{
			(NoResString)@"^(ES|)([A-Za-z][A-Za-z0-9]{8}|\d[A-Za-z0-9]{7}[A-Za-z])$",
		};

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.ValidateWarning(codeInfo, ValidLengths, InvalidPatternMessage)
				&& OrgCusCodePatternValidator.ValidateWarning(codeInfo, ValidPatternStrings, InvalidPatternMessage);
		}

		string InvalidPatternMessage => Res.GetString("a5fbc967-51c9-46a9-a9ae-62a8cd697706", "VAT Business Registration Number structures for Spain are either 'ESX9999999X' or 'X9999999X'.");
	}
}
