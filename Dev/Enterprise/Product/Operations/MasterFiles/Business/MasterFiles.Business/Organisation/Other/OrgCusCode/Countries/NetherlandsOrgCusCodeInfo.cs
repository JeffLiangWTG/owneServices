using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class NetherlandsOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW)); // Accounting consumption code

			list.AddPair(OrgCusCode.NetherlandsCodeTypes.FenexLocationCode, Res.GetString("OrgCusCode.NetherlandsCodeTypes.FenexLocationCode", "FENEX Location Code"));
			list.AddPair(OrgCusCode.NetherlandsCodeTypes.ChamberOfCommerceNumber, Res.GetString("OrgCusCode.NetherlandsCodeTypes.ChamberOfCommerceNumber", "Chamber of Commerce Number"));
			list.AddPair(OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode, Res.GetString("OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode", "Limited Fiscal Representation VAT Code"));
			list.AddPair(OrgCusCode.NetherlandsCodeTypes.GFRVATNumberCode, Res.GetString("OrgCusCode.NetherlandsCodeTypes.GFRVATNumberCode", "General Fiscal Representation VAT Code"));
			var cargonautString = (NoResString)"Cargonaut";
			list.AddPair(OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode, Res.GetString("6c7b4399-77b8-4f6e-a87d-91252fdd4d33", @"{0} Registration Code", cargonautString));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Netherlands);
			result.Add(OrgCusCode.NetherlandsCodeTypes.ChamberOfCommerceNumber);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Netherlands);
			return result;
		}
	}
}
