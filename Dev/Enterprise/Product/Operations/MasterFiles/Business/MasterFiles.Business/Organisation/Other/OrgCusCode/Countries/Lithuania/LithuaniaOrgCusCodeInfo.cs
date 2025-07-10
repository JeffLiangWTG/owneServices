using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class LithuaniaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.LithuaniaCodeTypes.PVM, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.LithuaniaCodeTypes.PVM)); // Accounting consumption code

			list.AddPair(OrgCusCode.LithuaniaCodeTypes.IMK, string.Format((NoResString)"Įmonės Kodą / {0}", Res.GetString("653FCAB9-D5BC-46AD-9ED7-530CC4279591", "Company Registration Code")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Lithuania);
			result.Add(OrgCusCode.LithuaniaCodeTypes.IMK);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Lithuania);
			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			if (orgCusCode.OK_CodeType == OrgCusCode.LithuaniaCodeTypes.PVM)
			{
				new LTPVMCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
			}

			OrgCusCodeValidation.ValidateCustomsCodeForEU(orgCusCode);
			OrgCusCodeValidation.ValidateCustomsCodeEORI(orgCusCode);
		}
	}
}
