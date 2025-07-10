using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class GambiaOrgCusCodeInfo :
		OrgCusCodeInfo,
		IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string TIN = "TIN";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.VATCode, Country.TaxCodeDescriptions.VATBusinessRegistrationNumber); // Accounting consumption code

			list.AddPair(OrgCusCodes.TIN, Res.GetString("16EAC803-43B8-458D-B856-C27769AAF4C6", "Taxpayer Identification Number"));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return GetPrimaryCusCodes(CountryCode);
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			return GetMainOrganizationNumberTypes(CountryCode);
		}

		ZString CountryCode => Core.Constants.CountryCodes.Gambia;
	}
}
