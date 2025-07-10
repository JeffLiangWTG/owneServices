using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class LatviaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(LatviaOrgCusCodeInfo.OrgCusCodes.PVN, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCodes.PVN)); // Accounting consumption code

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Latvia);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Latvia);
			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			if (orgCusCode.OK_CodeType == OrgCusCode.LatviaCodeTypes.PVN)
			{
				new LVPVNCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
			}

			OrgCusCodeValidation.ValidateCustomsCodeForEU(orgCusCode);
			OrgCusCodeValidation.ValidateCustomsCodeEORI(orgCusCode);
		}

		public static class OrgCusCodes
		{
			public const string PVN = "PVN";
		}
	}
}
