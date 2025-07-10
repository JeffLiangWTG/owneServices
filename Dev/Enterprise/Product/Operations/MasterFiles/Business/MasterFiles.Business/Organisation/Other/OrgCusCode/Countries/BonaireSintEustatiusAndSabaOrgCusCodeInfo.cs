using System.Collections.Generic;
using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class BonaireSintEustatiusAndSabaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.CRB, string.Format(CultureInfo.InvariantCulture, (NoResString)"Business ABB (Algemene Bestedingsbelasting) {0}", Res.GetString("805fc35a-a68d-43d9-97f5-a30e817f5b13", "CRIB Registration Number"))); // Accounting consumption code

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.BonaireSintEustatiusAndSaba);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.BonaireSintEustatiusAndSaba);
			return result;
		}

		public static class OrgCusCodes
		{
			public const string CRB = "CRB";
			public const string ABB = "ABB";
		}
	}
}

