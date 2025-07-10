using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class EcuadorOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.EcuadorCodeTypes.RUC, string.Format((NoResString)"Registro Unico de Contribuyentes / {0}", Res.GetString("3a6063f2-ec99-43c2-9d21-15c273bb0534", "Tax and VAT Registration"))); // Accounting consumption code

			list.AddPair(OrgCusCode.EcuadorCodeTypes.SRF, (NoResString)"SRI Autorizacion Fecha / Tax Office Authorization Date");
			list.AddPair(OrgCusCode.EcuadorCodeTypes.SRI, (NoResString)"SRI Autorizacion Numero / Tax Office Authorization Number");
			list.RemoveCode(OrgCusCode.CodeTypes.GSTCode);
			list.RemoveCode(OrgCusCode.CodeTypes.IVA);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Ecuador);
			result.Add(OrgCusCode.CodeTypes.GSTCode);
			result.Add(OrgCusCode.CodeTypes.IVA);
			result.Add(OrgCusCode.EcuadorCodeTypes.SRI);
			result.Add(OrgCusCode.EcuadorCodeTypes.SRF);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Ecuador);
			return result;
		}
	}
}
