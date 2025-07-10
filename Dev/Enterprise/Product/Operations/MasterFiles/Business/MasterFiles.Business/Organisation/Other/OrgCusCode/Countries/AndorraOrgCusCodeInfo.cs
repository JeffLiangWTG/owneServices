using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AndorraOrgCusCodeInfo :
		OrgCusCodeInfo,
		IOrgCusCodeProvider
	{
		public static class OrgCusCodes
		{
			public const string IGI = "IGI";
			public const string NRT = "NRT";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.IGI, string.Format(CultureInfo.InvariantCulture, (NoResString)"Número de Registre Tributari IGI / {0}", Res.GetString("6cf4b6b9-442e-4f92-b32b-9b5a62ff1af6", "Tax and VAT Registration Number"))); // Accounting consumption code

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			return GetPrimaryCusCodes(CountryCode);
		}

		ZString CountryCode => Core.Constants.CountryCodes.Andorra;
	}
}

