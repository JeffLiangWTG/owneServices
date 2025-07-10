using System.Collections.Generic;
using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ElSalvadorOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.NRC, string.Format((NoResString)"Numero de Registro de Contribuyente / {0}", Res.GetString("8c3993f3-16e4-4c84-953d-0a9687d95fa5", "VAT Registration"))); // Accounting consumption code

			list.AddPair(OrgCusCodes.NIT, string.Format(CultureInfo.InvariantCulture, (NoResString)"Número de Identificación Tributaria / {0}", Res.GetString("78c48dc7-9658-4b96-9fe1-dba3c4df8676", "Tax Identification Number")));
			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);
			list.RemoveCode(OrgCusCode.CodeTypes.IVA);

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.ElSalvador);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);
			result.Add(OrgCusCodes.NIT);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.ElSalvador);
			result.Add(OrgCusCodes.NIT);

			return result;
		}

		public static class OrgCusCodes
		{
			public const string NRC = "NRC";
			public const string NIT = "NIT";
		}
	}
}
