using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MacauOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(MacauOrgCusCodeInfo.OrgCusCodes.AuthorizedEconomicOperator, Res.GetString("OrgCusCode.Macau.AuthorizedEconomicOperator", "Authorized Economic Operator"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Macau);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Macau);
			return result;
		}

		public static class OrgCusCodes
		{
			public const string AuthorizedEconomicOperator = "AEO";
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode customsRegNo)
		{
			switch (customsRegNo.OK_CodeType)
			{
				case OrgCusCodes.AuthorizedEconomicOperator:
					if (!Regex.IsMatch(customsRegNo.OK_CustomsRegNo, "^[0-9]{9}$", RegexOptions.IgnoreCase))
					{
						customsRegNo.OK_CustomsRegNoInfo.AddError(Res.GetString("e1c9f603-bce7-4f1f-9dbd-8d9f13349475", "MO AEO number should consist of 9 numeric characters."));
					}
					break;
			}
		}
	}
}
