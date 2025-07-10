using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class UgandaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(UgandaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, Res.GetString("ee577aa6-0c60-46de-b2e7-d937f122ddd2", "Tax Identification Number"));
			list.AddPair(UgandaOrgCusCodeInfo.OrgCusCodes.AuthorizedEconomicOperator, Res.GetString("OrgCusCode.IranCodeTypes.AEO", "Authorized Economic Operator"));
			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Uganda);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Uganda);
			return result;
		}

		public static class OrgCusCodes
		{
			public const string TaxIdentificationNumber = "TIN";
			public const string AuthorizedEconomicOperator = "AEO";
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode customsRegNo)
		{
			switch (customsRegNo.OK_CodeType)
			{
				case OrgCusCodes.AuthorizedEconomicOperator:
					if (!Regex.IsMatch(customsRegNo.OK_CustomsRegNo, "^[0-9]{10}$", RegexOptions.IgnoreCase))
					{
						customsRegNo.OK_CustomsRegNoInfo.AddError(Res.GetString("d7ed0c69-6200-40a7-bfbc-9775889d31da", "UG AEO number should consist of 10 numeric characters."));
					}
					break;
			}
		}
	}
}
