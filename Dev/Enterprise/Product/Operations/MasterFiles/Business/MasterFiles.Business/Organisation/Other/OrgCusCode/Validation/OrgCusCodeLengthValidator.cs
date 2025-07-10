using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgCusCodeLengthValidator
	{
		public static bool Validate(ZPropertyInfo codeInfo, HashSet<int> validLengths, string organisationRegistryCodeType, string invalidMessage)
		{
			var result = false;
			foreach (var validLength in validLengths)
			{
				var code = (ZString)codeInfo.Value;
				result = code.Length == validLength;
				if (result)
				{
					break;
				}
			}
			if (!result)
			{
				codeInfo.AddErrorIfEnforced(invalidMessage, organisationRegistryCodeType);
			}
			return result;
		}

		public static bool ValidateWarning(ZPropertyInfo codeInfo, HashSet<int> validLengths, string invalidMessage)
		{
			var result = false;
			foreach (var validLength in validLengths)
			{
				var code = (ZString)codeInfo.Value;
				result = code.Length == validLength;
				if (result)
				{
					break;
				}
			}
			if (!result)
			{
				codeInfo.AddWarning(invalidMessage);
			}
			return result;
		}
	}
}
