using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgCusCodePatternValidator
	{
		public static bool Validate(ZPropertyInfo codeInfo, HashSet<string> validPattern, string organisationRegistryCodeType, string invalidMessage)
		{
			var result = false;
			var validPatternString = validPattern.GetEnumerator();

			while (validPatternString.MoveNext() && !result)
			{
				var isValid = new Regex(validPatternString.Current);
				var code = (ZString)codeInfo.Value;
				result = isValid.IsMatch(code);
			}
			if (!result)
			{
				codeInfo.AddErrorIfEnforced(invalidMessage, organisationRegistryCodeType);
			}
			return result;
		}

		public static bool ValidateWarning(ZPropertyInfo codeInfo, HashSet<string> validPattern, string invalidMessage)
		{
			var result = false;
			var validPatternString = validPattern.GetEnumerator();

			while (validPatternString.MoveNext() && !result)
			{
				var isValid = new Regex(validPatternString.Current);
				var code = (ZString)codeInfo.Value;
				result = isValid.IsMatch(code);
			}
			if (!result)
			{
				codeInfo.AddWarning(invalidMessage);
			}
			return result;
		}
	}
}
