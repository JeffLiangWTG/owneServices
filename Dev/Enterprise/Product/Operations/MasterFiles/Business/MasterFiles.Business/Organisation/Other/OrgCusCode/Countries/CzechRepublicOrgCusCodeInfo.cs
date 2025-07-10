using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CzechRepublicOrgCusCodeInfo :
		OrgCusCodeInfo,
		IOrgCusCodeProvider,
		IOrgCusCodeCustomsRegNoValidationProvider
	{
		ZString CountryCode => Core.Constants.CountryCodes.CzechRepublic;

		public static class OrgCusCodes
		{
			public const string DPH = "DPH";
		}

		#region IOrgCusCodeProvider

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.DPH, string.Format(CultureInfo.InvariantCulture, (NoResString)"Daňové identifikační číslo / {0}", Res.GetString("9B486674-CF24-4337-B239-F560203F83CE", "VAT (DPH) Business Registration"))); // Accounting consumption code

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return GetPrimaryCusCodes(CountryCode);
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}

		#endregion

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCodes.DPH:
					new CZDPHCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
			}

			OrgCusCodeValidation.ValidateCustomsCodeForEU(orgCusCode);
			OrgCusCodeValidation.ValidateCustomsCodeEORI(orgCusCode);
		}
	}
}
