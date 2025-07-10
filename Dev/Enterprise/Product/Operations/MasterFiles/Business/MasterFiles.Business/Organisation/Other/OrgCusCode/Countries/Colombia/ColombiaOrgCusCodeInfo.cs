using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ColombiaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeCustomsRegNoValidationProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.NIT, Res.GetString("Organisation|CustomsCodes|NITColombia", "NIT/ Tax and VAT(IVA) Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.FID, Res.GetString("OrgCusCode.FID", "{0} / Foreign Identification Document", "Documento de Identificacion Extranjero"));
			list.AddPair(OrgCusCodes.AEC, Res.GetString("OrgCusCodes.AEC", "ACTIVIDAD ECONOMICA"));
			list.AddPair(OrgCusCodes.NRC, Res.GetString("OrgCusCodes.NRC", "{0} / Common Regime", "Contribuyente - Regimen Comun"));
			list.AddPair(OrgCusCodes.NRS, Res.GetString("OrgCusCodes.NRS", "{0} / Simplified Regime", "Contribuyente - Regimen Simplificado"));
			list.AddPair(OrgCusCodes.NGC, Res.GetString("OrgCusCodes.NGC", "{0} / Large Taxpayer", "Contribuyente - Gran Contribuyente"));
			list.AddPair(OrgCusCodes.NGA, Res.GetString("OrgCusCodes.NGA", "{0} / Self - Withholding Agent", "Contribuyente - Autorretenedor"));
			list.AddPair(OrgCusCodes.NAR, Res.GetString("OrgCusCodes.NAR", "{0} / VAT Withholding Agent", "Contribuyente - Agente de Retencion IVA"));
			list.AddPair(OrgCusCodes.CID, Res.GetString("OrgCusCodes.CID", "CÉDULA DE CIUDADANÍA"));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Colombia);
			result.Add(OrgCusCodes.NIT);
			result.Add(OrgCusCodes.FID);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Colombia);
			result.Add(OrgCusCodes.AEC);
			result.Add(OrgCusCodes.NRC);
			result.Add(OrgCusCodes.NRS);
			result.Add(OrgCusCodes.NGC);
			result.Add(OrgCusCodes.NGA);
			result.Add(OrgCusCodes.NAR);
			result.Add(OrgCusCodes.FID);

			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCodes.NIT:
					new CONITCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
			}
		}

		public static class OrgCusCodes
		{
			public const string AEC = "AEC";
			public const string CID = "CID";
			public const string NIT = "NIT";
			public const string NRC = "NRC";
			public const string NRS = "NRS";
			public const string NGC = "NGC";
			public const string NGA = "NGA";
			public const string NAR = "NAR";
			public const string FID = "FID";
		}
	}
}
