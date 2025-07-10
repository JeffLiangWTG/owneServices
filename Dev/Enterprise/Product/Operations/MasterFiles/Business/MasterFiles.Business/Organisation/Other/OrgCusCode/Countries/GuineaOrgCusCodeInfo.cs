using System.Collections.Generic;
using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GuineaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.TVACode, OrgCusCodeDescription.TVA); // Accounting consumption code

			list.AddPair(OrgCusCodes.NIF, OrgCusCodeDescription.NIF);
			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			return OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Guinea);
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Guinea);
		}

		public static class OrgCusCodes
		{
			public const string NIF = "NIF";
		}

		public static class OrgCusCodeDescription
		{
			public static string TVA => Res.GetString("7879ccc8-c333-444a-8a88-f27d7c8aadf5", "VAT (TVA) Business Registration Number");
			public static string NIF => string.Format(CultureInfo.InvariantCulture, (NoResString)"Numéro d'Identification Fiscale / {0}", Res.GetString("9c71f22e-58a3-4032-997e-faae590e4d70", "Tax Identification Number"));
		}
	}
}
