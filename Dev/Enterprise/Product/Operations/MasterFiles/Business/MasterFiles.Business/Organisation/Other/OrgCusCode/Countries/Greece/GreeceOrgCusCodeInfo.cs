using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GreeceOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.GreeceCodeTypes.AFM, Res.GetString("Organisation|CustomsCodes|AFMGreece", "VAT (AFM) Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCode.GreeceCodeTypes.DOY, Res.GetString("OrgCusCode.CodeTypes.DistrictTaxOffice", "District Tax Office"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Greece);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Greece);
			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			if (orgCusCode.OK_CodeType == OrgCusCode.GreeceCodeTypes.AFM)
			{
				new GRAFMCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
			}

			OrgCusCodeValidation.ValidateCustomsCodeForEU(orgCusCode);
			OrgCusCodeValidation.ValidateCustomsCodeEORI(orgCusCode);
		}
	}
}
