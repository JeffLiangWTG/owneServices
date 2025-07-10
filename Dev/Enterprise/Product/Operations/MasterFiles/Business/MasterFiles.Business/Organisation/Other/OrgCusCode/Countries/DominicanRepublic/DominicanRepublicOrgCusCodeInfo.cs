using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class DominicanRepublicOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeUniqueValidation, IOrgCusCodeCustomsRegNoValidationProvider
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.RNC, string.Format((NoResString)"Registro Nacional de Contribuyentes / {0}", Res.GetString("6d708cde-0556-4d97-b9c7-cfd327f7550b", "National Taxpayers Registry Number & ITBIS (VAT) Business Registration"))); // Accounting consumption code

			list.AddPair(OrgCusCodes.CED, Res.GetString("7e2a422e-6131-45da-94f5-a0840f0854e6", "{0} / Individual Tax ID", "Cedula"));
			list.AddPair(OrgCusCodes.REG, Res.GetString("F8F3EA81-8EFA-4102-95E0-A41AF3670815", "{0} / Government Organizations", "Empresa Gubernamental"));
			list.AddPair(OrgCusCodes.RCS, Res.GetString("E0EE854A-BADD-4DCB-860D-65C4ED4A71E5", "{0} / Common/Simplified Regime", "Régimen Común/Simplificado de Tributación"));
			list.RemoveCode(OrgCusCode.CodeTypes.TaxFileCode);
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.DominicanRepublic);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);
			result.Add(OrgCusCodes.CED);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(Core.Constants.CountryCodes.DominicanRepublic);
			result.Add(OrgCusCodes.RCS);
			result.Add(OrgCusCodes.REG);
			result.Add(OrgCusCodes.CED);
			result.Remove(OrgCusCode.CodeTypes.TaxFileCode);
			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCodes.RNC:
					new DORNCCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.CED:
					new DOCEDCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
			}
		}

		HashSet<string>[] IOrgCusCodeUniqueValidation.GetCodesCannotCoexist()
		{
			var result = new[]  {
				new HashSet<string> { OrgCusCodes.RCS, OrgCusCodes.REG },
			};

			return result;
		}

		public static class OrgCusCodes
		{
			public const string RNC = "RNC";
			public const string CED = "CED";
			public const string RCS = "RCS";
			public const string REG = "REG";
		}
	}
}
