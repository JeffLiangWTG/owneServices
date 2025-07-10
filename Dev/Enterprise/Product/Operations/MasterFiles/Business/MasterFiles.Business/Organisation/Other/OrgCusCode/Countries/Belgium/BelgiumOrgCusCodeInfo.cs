using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class BelgiumOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW)); // Accounting consumption code

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Belgium);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Belgium);
			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			if (orgCusCode.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.BTW)
			{
				new BEBTWCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
			}

			OrgCusCodeValidation.ValidateCustomsCodeForEU(orgCusCode);
			OrgCusCodeValidation.ValidateCustomsCodeEORI(orgCusCode);
		}
	}
}
