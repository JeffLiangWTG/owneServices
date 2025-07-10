using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class VenezuelaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.VenezuelaCodeTypes.RegistroUnicoDeInformacionFiscal, Res.GetString("Organisation|CustomsCodes|RIFVenezuela", "Fiscal Registration Number / Tax and VAT Registration")); // Accounting consumption code

			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);
			list.RemoveCode(OrgCusCode.CodeTypes.IVA);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Venezuela);
			result.Add(OrgCusCode.CodeTypes.GSTCode);
			result.Add(OrgCusCode.CodeTypes.IVA);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Venezuela);
			return result;
		}
	}
}
