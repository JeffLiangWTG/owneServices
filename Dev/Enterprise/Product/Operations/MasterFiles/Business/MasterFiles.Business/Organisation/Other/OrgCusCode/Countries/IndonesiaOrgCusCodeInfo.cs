using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class IndonesiaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.IndonesiaCodeTypes.PPN, Res.GetString("9fde8582-8796-400b-a2a1-a4e19b243272", "Government Tax Payer Registration NPWP and Business VAT (PPN)")); // Accounting consumption code

			list.AddPair(OrgCusCode.IndonesiaCodeTypes.PP2, Res.GetString("6319a939-185e-4b4a-8c9b-466f6c183882", "Government Treasurer VAT Collector ") + (NoResString)"(Bendahara Pemerintah)");
			list.AddPair(OrgCusCode.IndonesiaCodeTypes.PP3, Res.GetString("8170ce5d-6f38-43fb-861c-fb8ffb31e182", "Government State Owned Enterprise VAT Collector ") + "(BUMN)");
			list.AddPair(OrgCusCode.IndonesiaCodeTypes.NIT, Res.GetString("90A2CBEC-B670-4651-9C28-9BAB98DEF5AE", "Business Activity Location Identity Number (NITKU)"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Indonesia);
			result.Add(OrgCusCode.IndonesiaCodeTypes.PP3);
			result.Add(OrgCusCode.IndonesiaCodeTypes.PP2);
			result.Add(OrgCusCode.IndonesiaCodeTypes.NIT);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Indonesia);
			result.Add(OrgCusCode.IndonesiaCodeTypes.NIT);
			return result;
		}
	}
}
