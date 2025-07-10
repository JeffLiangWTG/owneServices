using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ArgentinaOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeUniqueValidation, IOrgCusCodeNonUniqueProvider, IOrgCusCodeCustomsRegNoValidationProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.IVA, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.IVA)); // Accounting consumption code

			list.AddPair(OrgCusCodes.CUIT, Res.GetString("E1EC8574-F345-4608-8C60-7706786A7763", "CUIT Number"));
			list.AddPair(OrgCusCodes.CUIL, Res.GetString("6FA78EC2-7D91-40ED-B07F-35AB3593150E", "CUIL Number"));
			list.AddPair(OrgCusCodes.DNI, Res.GetString("EEAC57FA-E323-4541-818B-C0915C22CC35", "DNI Number"));
			list.AddPair(OrgCusCodes.CUF, Res.GetString("09B1B7FA-B2D7-45AF-A75C-678073BBC266", "CUIT Number for Foreign Countries/Regions"));
			list.AddPair(OrgCusCodes.IVE, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IVE", "IVA EXENTO"));
			list.AddPair(OrgCusCodes.IVF, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IVF", "IVA CONSUMIDOR FINAL"));
			list.AddPair(OrgCusCodes.IVI, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IVI", "IVA RESPONSABLE INSCRIPTO"));
			list.AddPair(OrgCusCodes.IVM, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IVM", "IVA RESPONSABLE MONOTRIBUTO"));
			list.AddPair(OrgCusCodes.IVN, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IVN", "IVA NO RESPONSABLE"));
			list.AddPair(OrgCusCodes.IVR, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IVR", "IVA RESPONSABLE NO INSCRIPTO"));
			list.AddPair(OrgCusCodes.IBL, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IBL", "IB LOCAL"));
			list.AddPair(OrgCusCodes.IBM, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IBM", "IB CONVENIO MULTILATERAL"));
			list.AddPair(OrgCusCodes.IBS, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IBS", "IB REGIMEN SIMPLIFICADO"));
			list.AddPair(OrgCusCodes.IBN, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IBN", "IB NO INSCRIPTO"));
			list.AddPair(OrgCusCodes.IVP, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IVP", "IVA PROVEEDOR DEL EXTERIOR"));
			list.AddPair(OrgCusCodes.IVS, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IVS", "IVA SUJETO NO CATEGORIZADO"));
			list.AddPair(OrgCusCodes.IVX, Res.GetString("OrgCusCode.ArgentinaCodeTypes.IVX", "IVA CLIENTE DEL EXTERIOR"));
			list.AddPair(OrgCusCodes.MIP, Res.GetString("OrgCusCode.ArgentinaCodeTypes.MIP", "{0} Regime", "MiPyme"));
			list.AddPair(OrgCusCodes.OLS, Res.GetString("OrgCusCode.ArgentinaCodeTypes.OLS", "Secure Logistics Operator"));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.Argentina);
			result.Add(OrgCusCodes.CUF);
			result.Add(OrgCusCodes.CUIL);
			result.Add(OrgCusCodes.DNI);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.Argentina);
			result.Add(OrgCusCodes.CUF);
			result.Add(OrgCusCodes.IVE);
			result.Add(OrgCusCodes.IVF);
			result.Add(OrgCusCodes.IVI);
			result.Add(OrgCusCodes.IVM);
			result.Add(OrgCusCodes.IVN);
			result.Add(OrgCusCodes.IVR);
			result.Add(OrgCusCodes.IVP);
			result.Add(OrgCusCodes.IVS);
			result.Add(OrgCusCodes.IVX);
			result.Add(OrgCusCodes.IBL);
			result.Add(OrgCusCodes.IBM);
			result.Add(OrgCusCodes.IBS);
			result.Add(OrgCusCodes.IBN);
			result.Add(OrgCusCodes.CUIL);
			result.Add(OrgCusCodes.DNI);
			result.Add(OrgCusCodes.MIP);
			result.Add(OrgCusCodes.OLS);

			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCodes.CUIT:
					new CUITCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.CUIL:
					new CUILCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.DNI:
					new DNICodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.CUF:
					new CUFCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.IBL:
					new IBCodesValidator(OrganisationRegistry.RegistrationNumberFormatFields.ARIBL).Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.IBM:
					new IBCodesValidator(OrganisationRegistry.RegistrationNumberFormatFields.ARIBM).Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.IBS:
					new IBCodesValidator(OrganisationRegistry.RegistrationNumberFormatFields.ARIBS).Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
			}
		}

		HashSet<string>[] IOrgCusCodeUniqueValidation.GetCodesCannotCoexist()
		{
			var result = new[]  {
				new HashSet<string> { OrgCusCodes.CUIT, OrgCusCodes.CUF },
				new HashSet<string> { OrgCusCodes.IBL, OrgCusCodes.IBM, OrgCusCodes.IBS, OrgCusCodes.IBN },
				new HashSet<string> { OrgCusCodes.IVE, OrgCusCodes.IVF, OrgCusCodes.IVI, OrgCusCodes.IVM, OrgCusCodes.IVN, OrgCusCodes.IVR, OrgCusCodes.IVP, OrgCusCodes.IVS, OrgCusCodes.IVX },
				new HashSet<string> { OrgCusCodes.MIP, OrgCusCodes.IVX, OrgCusCodes.IVF, OrgCusCodes.IVP, OrgCusCodes.IVS }
			};

			return result;
		}

		HashSet<string> IOrgCusCodeNonUniqueProvider.GetNonUniqueCodes()
		{
			return new HashSet<string>() { OrgCusCodes.CUF };
		}

		public static class OrgCusCodes
		{
			public const string CUIT = "CUI";
			public const string CUIL = "CUL";
			public const string DNI = "DNI";
			public const string CUF = "CUF";
			public const string IVE = "IVE";
			public const string IVF = "IVF";
			public const string IVI = "IVI";
			public const string IVM = "IVM";
			public const string IVN = "IVN";
			public const string IVP = "IVP";
			public const string IVR = "IVR";
			public const string IVS = "IVS";
			public const string IVX = "IVX";
			public const string IBL = "IBL";
			public const string IBM = "IBM";
			public const string IBS = "IBS";
			public const string IBN = "IBN";
			public const string MIP = "MIP";
			public const string OLS = "OLS";
		}
	}
}
