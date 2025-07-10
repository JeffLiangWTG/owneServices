using System.Collections.Generic;
using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MoldovaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.TVACode, Res.GetString("a8ace5f8-2aac-4c36-a725-1c7cad3b30c7", "VAT (TVA) Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.NCF, string.Format(CultureInfo.InvariantCulture, (NoResString)"Codul Fiscal / {0}", Res.GetString("49cb5239-7705-41da-99d1-986976609d0c", "Fiscal Code")));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Moldova);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Moldova);
			result.Add(OrgCusCodes.NCF);

			return result;
		}

		public static class OrgCusCodes
		{
			public const string NCF = "NCF";
		}
	}
}
