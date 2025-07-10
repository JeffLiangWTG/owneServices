using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class PanamaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeCustomsRegNoValidationProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.RUC, Res.GetString("89BBE19C-A640-4ECC-80F8-EA7370F5CEE4", "RUC / Taxpayer Identification Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.NAO, string.Format((NoResString)"Número de Aviso de Operación / {0}", Res.GetString("6B3BCE35-43BC-432B-949E-D68240F48E6D", "Operation Notice Number (Commercial License)")));
			list.AddPair(OrgCusCodes.BRC, string.Format((NoResString)"Código de Sucursal / {0}", Res.GetString("FB7BFCC3-691E-4866-B318-ADF88440E7AC", "Branch Code")));
			list.AddPair(OrgCusCodes.CON, string.Format((NoResString)"Consumidor Final / {0}", Res.GetString("E998D7FB-53C3-4170-A418-DD0940450613", "Final Consumer")));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Panama);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Panama);
			result.Add(OrgCusCodes.NAO);
			result.Add(OrgCusCodes.BRC);
			result.Add(OrgCusCodes.CON);

			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCodes.BRC:
					new BRCCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
			}
		}

		public static class OrgCusCodes
		{
			public const string RUC = "RUC";
			public const string NAO = "NAO";
			public const string BRC = "BRC";
			public const string CON = "CON";
		}
	}
}
