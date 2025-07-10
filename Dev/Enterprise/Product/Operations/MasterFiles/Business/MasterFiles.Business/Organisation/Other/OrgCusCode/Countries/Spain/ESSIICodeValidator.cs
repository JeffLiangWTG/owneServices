using System.Linq;

namespace Enterprise.MasterFiles.Business
{
	class ESSIICodeValidator
	{
		internal void Validate(OrgCusCode orgCusCode)
		{
			if (!IsValidCode(orgCusCode))
			{
				orgCusCode.OK_CustomsRegNoInfo.AddError(Res.GetString("0f9f3f58-5e43-4536-ac22-0aaf6f1a73e0",
				"SII registration should match the VAT (NIF) business registration of the taxpayer."));
			}
		}

		bool IsValidCode(OrgCusCode orgCusCode)
		{
			var nifCode = orgCusCode.Header?.CustomsCodes.Cast<OrgCusCode>()
				.Where(x => x.OK_CodeType == SpainOrgCusCodeInfo.OrgCusCodes.NIF
					&& x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Spain)
				.Select(x => x.OK_CustomsRegNo).FirstOrDefault() ?? string.Empty;

			return !nifCode.IsEmpty && orgCusCode.OK_CustomsRegNo == nifCode;
		}
	}
}
