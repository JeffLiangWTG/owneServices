using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class PolandOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.PolandCodeTypes.PTU, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.PolandCodeTypes.PTU)); // Accounting consumption code

			list.AddPair(OrgCusCode.PolandCodeTypes.NIP, Res.GetString("OrgCusCode.PolandCodeTypes.NIP", "Company Number"));
			list.AddPair(OrgCusCode.PolandCodeTypes.TIN, Res.GetString("OrgCusCode.PolandCodeTypes.TIN", "Tax Identification Number"));
			list.AddPair(OrgCusCode.PolandCodeTypes.PES, Res.GetString("OrgCusCode.PolandCodeTypes.PES", "PESEL Number"));
			list.AddPair(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, Res.GetString("OrgCusCode.CodeTypes.CPW", "Customs Warehouse Authorization Number"));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Poland);
			result.Add(OrgCusCode.PolandCodeTypes.PES);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Poland);
			result.Add(OrgCusCode.PolandCodeTypes.PES);
			return result;
		}
	}
}
