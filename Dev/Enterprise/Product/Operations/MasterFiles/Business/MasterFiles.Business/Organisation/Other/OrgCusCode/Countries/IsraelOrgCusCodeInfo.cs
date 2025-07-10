using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Organisation.Other.OrgCusCode.Validation.IL;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class IsraelOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(OrgCusCode.IsraelCodeTypes.AEO, Res.GetString("OrgCusCode.IsraelCodeTypes.AEO", "Authorized Economic Operator"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Israel);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Israel);
			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode customsRegNo)
		{
			switch (customsRegNo.OK_CodeType)
			{
				case OrgCusCode.CodeTypes.VATCode:
					MandatoryValidation.CheckEntered(customsRegNo.OK_CustomsRegNoInfo);
					ILVATValidator.ValidateVATRegistrationNumber(customsRegNo.OK_CustomsRegNoInfo);
					break;
			}
		}
	}
}
