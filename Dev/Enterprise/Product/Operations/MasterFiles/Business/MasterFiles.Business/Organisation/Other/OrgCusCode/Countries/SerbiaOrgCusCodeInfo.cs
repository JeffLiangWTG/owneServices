using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class SerbiaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeCustomsRegNoValidationProvider
	{
		public static class OrgCusCodes
		{
			public const string PDV = "PDV";
			public const string PIB = "PIB";
			public const string AEO = "AEO";
			public const string JBK = "JBK";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.PIB, string.Format((NoResString)"PIB Poreski Identifikacioni Broj / {0}", Res.GetString("852d1ac3-1f3d-42ea-9fe1-c8ae06ae9383", "Tax Identification and Business VAT Registration"))); // Accounting consumption code

			list.AddPair(OrgCusCodes.AEO, Res.GetString("OrgCusCode.Serbia.AuthorizedEconomicOperator", "Authorized Economic Operator"));
			list.AddPair(OrgCusCodes.JBK, string.Format((NoResString)"JBKJS Jedinstveni Broj Korisnika Javnih Sredstava / {0}", Res.GetString("64dd8845-a1e3-46b8-b421-a3a21b6320f9", "Identification for a User of Public Funds")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.Serbia);
			result.Add(OrgCusCodes.AEO);
			result.Add(OrgCusCodes.JBK);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Serbia);
			result.Remove(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode customsRegNo)
		{
			switch (customsRegNo.OK_CodeType)
			{
				case OrgCusCodes.JBK:
					SerbiaRegistrationNumberValidator.ValidateJBK(customsRegNo.OK_CustomsRegNoInfo);
					break;

				case OrgCusCodes.AEO:
					SerbiaRegistrationNumberValidator.ValidateAEO(customsRegNo.OK_CustomsRegNoInfo);
					break;
			}
		}
	}
}
