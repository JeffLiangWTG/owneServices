using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MexicoOrgCusCodeInfo :
		OrgCusCodeInfo,
		IOrgCusCodeCustomsRegNoValidationProvider,
		IOrgCusCodeProvider,
		IOrgCusCodeNonUniqueProvider,
		IOrgCusCodeUniqueValidation
	{
		public static class OrgCusCodes
		{
			public const string RFC = "RFC";
			public const string CUR = "CUR";
			public const string RFG = "RFG";
			public const string CFD = "CFD";
			public const string REG = "REG";
			public const string ELN = "ELN";
		}

		ZString CountryCode => Core.Constants.CountryCodes.Mexico;

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCodes.RFG:
					new RFGCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.RFC:
					new RFCCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCode.CodeTypes.IVA:
					new IVACodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.CFD:
					new CFDCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.REG:
					new REGCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
			}
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.CodeTypes.IVA, Res.GetString("Organisation|CustomsCodes|IVAMexico", "VAT (IVA) Business Registration Number")); // Accounting consumption code

			list.AddPair(OrgCusCodes.RFC, Res.GetString("OrgCusCode.RFC", "RFC / Tax Identification Number"));
			list.AddPair(OrgCusCodes.CUR, string.Format(CultureInfo.InvariantCulture, (NoResString)"CURP Clave Unica de Registro de Población / {0}", Res.GetString("C28F30FC-0229-423E-83B3-8978CF29632F", "Individual Identification Number")));
			list.AddPair(OrgCusCodes.RFG, Res.GetString("OrgCusCode.CodeTypes.RFG", "RFC for General Public or Foreign Countries/Regions"));
			list.AddPair(OrgCusCodes.CFD, Res.GetString("OrgCusCode.CodeTypes.CFD", "{0} / Use of the Tax Document", "Uso del CFDI"));
			list.AddPair(OrgCusCodes.REG, Res.GetString("OrgCusCode.CodeTypes.REG", "{0} / Tax Regime", "Regimen Fiscal"));
			list.AddPair(OrgCusCodes.ELN, Res.GetString("OrgCusCode.CodeTypes.ELN", "Electronic Invoicing Organization Legal Name"));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCodes.RFG);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(CountryCode);
			result.Add(OrgCusCodes.RFG);
			result.Add(OrgCusCodes.CFD);
			result.Add(OrgCusCodes.REG);
			result.Add(OrgCusCodes.ELN);
			return result;
		}

		HashSet<string>[] IOrgCusCodeUniqueValidation.GetCodesCannotCoexist()
		{
			var result = new[]  {
				new HashSet<string> { OrgCusCodes.RFC, OrgCusCodes.RFG }
			};

			return result;
		}

		HashSet<string> IOrgCusCodeNonUniqueProvider.GetNonUniqueCodes()
		{
			return new HashSet<string>() { OrgCusCodes.RFG };
		}
	}
}
