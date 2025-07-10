using System.Text.RegularExpressions;

namespace Enterprise.MasterFiles.Business
{
	sealed class FRALTCodeValidator
	{
		internal void Validate(OrgCusCode orgCusCode)
		{
			if (!Regex.IsMatch(orgCusCode.OK_CustomsRegNo, @"^FR[0-9]{9}$"))
			{
				orgCusCode.OK_CustomsRegNoInfo.AddMessageError(InvalidFormatMessage);
			}
		}

		static string InvalidFormatMessage => Res.GetString("867d004b-12d8-4f9e-bc3e-14dcbf2be5c3", "ALT code should be in the format: FRNNNNNNNNN, where N is a number.");
	}
}
