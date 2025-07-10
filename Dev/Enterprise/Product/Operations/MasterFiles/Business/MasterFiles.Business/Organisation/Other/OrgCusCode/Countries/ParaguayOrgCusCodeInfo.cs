using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ParaguayOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.ParaguayCodeTypes.RUC, string.Format((NoResString)"Registro Unico de Contribuyentes / {0}", Res.GetString("3a6063f2-ec99-43c2-9d21-15c273bb0534", "Tax and VAT Registration"))); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);
			list.RemoveCode(OrgCusCode.CodeTypes.IVA);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Paraguay);
			result.Add(OrgCusCode.CodeTypes.GSTCode);
			result.Add(OrgCusCode.CodeTypes.IVA);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Paraguay);
			return result;
		}
	}
}
