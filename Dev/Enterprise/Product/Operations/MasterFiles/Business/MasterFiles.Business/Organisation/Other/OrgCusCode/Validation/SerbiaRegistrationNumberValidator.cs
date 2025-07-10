using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class SerbiaRegistrationNumberValidator
	{
		public static void ValidateAEO(ZPropertyInfo codeInfo)
		{
			var input = (ZString)codeInfo.Value;
			if (!Regex.IsMatch(input, "^(AEOF|AEOS)[0-9]{24}$"))
			{
				codeInfo.AddError(Res.GetString("70dbde6e-403b-472f-9dcb-3a5d329bb45a", "RS AEO number should start with \"AEOF\" or \"AEOS\" followed by 24 digits."));
			}
		}

		public static void ValidateJBK(ZPropertyInfo codeInfo)
		{
			var input = (ZString)codeInfo.Value;
			if (!Regex.IsMatch(input, @"^JBKJS\d{5}$"))
			{
				codeInfo.AddError(Res.GetString("107eaa73-c5dc-423d-8506-443a31d43b2a", "RS JBKJS number should start with \"JBKJS\", followed by 5 digits."));
			}
		}
	}
}
