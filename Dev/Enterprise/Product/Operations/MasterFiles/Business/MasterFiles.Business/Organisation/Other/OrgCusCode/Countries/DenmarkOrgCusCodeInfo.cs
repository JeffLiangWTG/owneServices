using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class DenmarkOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string MOM = "MOM";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATCodeTemplate((NoResString)"Moms")); // Accounting consumption code

			list.AddPair(OrgCusCode.DenmarkCodeTypes.EANLocationNumber, Res.GetString("OrgCusCode.DenmarkCodeTypes.EANLocationNumber", "EAN Location Number"));
			list.AddPair(OrgCusCode.DenmarkCodeTypes.ProductionNumber, Res.GetString("52095eae-b171-4f74-af24-0fb8beeb1025", "Production Number"));
			list.AddPair(OrgCusCode.DenmarkCodeTypes.CentralBusinessRegister, Res.GetString("OrgCusCode.DenmarkCodeTypes.CentralBusinessRegister", "Central Business Register Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Denmark);
			result.Add(OrgCusCode.DenmarkCodeTypes.CentralBusinessRegister);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Denmark);
			return result;
		}
	}
}
