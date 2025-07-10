using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgCusCodeValidator
	{
		public static bool Validate(ZPropertyInfo codeInfo, Func<string, bool> isValid, string organisationRegistryCodeType, string invalidMessage)
		{
			var result = isValid((ZString)codeInfo.Value);
			if (!result)
			{
				codeInfo.AddErrorIfEnforced(invalidMessage, organisationRegistryCodeType);
			}
			return result;
		}
	}
}
