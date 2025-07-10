using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public class TINValidator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Valid Pattern strings")]
	readonly HashSet<string> validPatternStrings = new HashSet<string>
	{
		@"^(PL)\d{10}$",
		@"^(ATU)\d{8}$",
		@"^(BE)(\d{10}|\d{9})$",
		@"^(DK)\d{8}$",
		@"^(FI)\d{8}$",
		@"^(FR)\d{11}$",
		@"^(FR)[A-HJ-KP-Z]\d{10}$",
		@"^(FR)\d[A-HJ-KP-Z]\d{9}$",
		@"^(FR)[A-HJ-KP-Z]{2}\d{9}$",
		@"^(EL)\d{9}$",
		@"^(ES)[A-Z]\d{8}$",
		@"^(ES)\d{8}[A-Z]$",
		@"^(ES)[A-Z]\d{7}[A-Z]$",
		@"^(NL)\d{9}B(?!00)\d{2}$",
		@"^(IE)\d{7}[A-Z]$",
		@"^(IE)\d([A-Z]|[*]|[+])\d{5}[A-Z]$",
		@"^(LU)\d{8}$",
		@"^(DE)\d{9}$",
		@"^(PT)\d{9}$",
		@"^(SE)\d{12}$",
		@"^(GB)(\d{9}|\d{12})$",
		@"^(GB)(GD|HA)\d{3}$",
		@"^(IT)\d{11}$",
		@"^(CY)\d{8}[A-Z]$",
		@"^(CZ)(\d{8}|\d{9}|\d{10})$",
		@"^(EE)\d{9}$",
		@"^(LT)(\d{9}|\d{12})$",
		@"^(LV)\d{11}$",
		@"^(MT)\d{8}$",
		@"^(SK)(\d{9}|\d{10})$",
		@"^(SI)\d{8}$",
		@"^(HU)\d{8}$",
		@"^(RO)\d{8}$",
	};

	internal bool Validate(ZPropertyInfo codeInfo)
	{
		if (codeInfo.Value.IsEmpty)
		{
			return true;
		}

		foreach (var patternString in validPatternStrings)
		{
			var isValid = new Regex(patternString);
			var code = (ZString)codeInfo.Value;
			if (isValid.IsMatch(code))
			{
				return true;
			}
		}

		codeInfo.AddError(Res.GetString("PLTINValidator|InvalidPatternMessage",
			@"The TIN number must meet the format requirements – G3 rule."));

		return false;
	}
}
