using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class BeninOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string IFU = "IFU";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.IFU, string.Format((NoResString)"Identifiant Fiscal Unique / {0}", Res.GetString("118d2781-f685-492d-9d7e-e0adb910f928", "Unique Fiscal Identification"))); // Accounting consumption code

			list.AddPair(OrgCusCode.BeninCodeTypes.NRC, string.Format((NoResString)"Numéro d’immatriculation au Registre du Commerce / {0}", Res.GetString("EF30014E-C12D-476D-BB6E-72DA22BB91C5", "Commerce Registration Number")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Benin);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Benin);
			return result;
		}
	}
}
