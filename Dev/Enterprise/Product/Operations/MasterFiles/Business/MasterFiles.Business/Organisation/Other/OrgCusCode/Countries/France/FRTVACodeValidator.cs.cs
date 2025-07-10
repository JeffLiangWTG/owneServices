using System.Text.RegularExpressions;

namespace Enterprise.MasterFiles.Business
{
	sealed class FRTVACodeValidator
	{
		internal void Validate(OrgCusCode orgCusCode)
		{
			if (!Regex.IsMatch(orgCusCode.OK_CustomsRegNo, @"^(FR|)[A-Za-z0-9]{2}\d{9}$"))
			{
				orgCusCode.OK_CustomsRegNoInfo.AddWarning(InvalidFormatMessage);
			}
		}

		static string InvalidFormatMessage => Res.GetString("4c15ddfe-3577-4caf-97ba-2084cdd4c345", "VAT Business Registration Number structures for France are either 'FRXX999999999' or 'XX999999999'.");
	}
}
