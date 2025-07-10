using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class PeruOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string IGV = "IGV";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode, Res.GetString("Organisation|CustomsCodes|RUCPeru", "Government Tax File Code")); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			list.AddPair(OrgCusCode.PeruCodeTypes.DNI, MultilingualString.Join(" ", (NoResString)"Documento Nacional de Identidad", ResString.GetMultilingualString("64776c7f-d19a-4327-bd88-8397bb457d0c", "(Individual / Natural Person)")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Peru);
			result.Add(OrgCusCode.CodeTypes.TaxFileCode);
			result.Add(OrgCusCode.PeruCodeTypes.DNI);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Peru);
			return result;
		}
	}
}
