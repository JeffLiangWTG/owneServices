using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UruguayOrgCusCodeInfo :
		OrgCusCodeInfo,
		IOrgCusCodeCustomsRegNoValidationProvider,
		IOrgCusCodeProvider
	{
		ZString CountryCode => Core.Constants.CountryCodes.Uruguay;

		public static class OrgCusCodes
		{
			public const string AEO = "AEO";
			public const string BRC = "BRC";
			public const string CID = "CID";
			public const string FZU = "FZU";
			public const string RUT = "RUT";
		}

		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCodes.RUT, string.Format((NoResString)"Registro Unico Tributario  / {0}", Res.GetString("3a6063f2-ec99-43c2-9d21-15c273bb0534", "Tax and VAT Registration"))); // Accounting consumption code

			list.AddPair(OrgCusCodes.AEO, Res.GetString("21702873-07EA-42E2-B680-A1B95D98A17C", "Authorized Economic Operator"));
			list.AddPair(OrgCusCodes.BRC, string.Format(CultureInfo.InvariantCulture, (NoResString)"Código de Sucursal / {0}", Res.GetString("c1de578d-5381-42a2-849f-d288d68603cd", "Branch Code")));
			list.AddPair(OrgCusCodes.CID, string.Format(CultureInfo.InvariantCulture, (NoResString)"Cédula de identidad / {0}", Res.GetString("62C5AB72-999A-4B4E-B5E7-A0BD7B83872D", "Identity Card number")));
			list.AddPair(OrgCusCodes.FZU, Res.GetString("6894604D-FE69-4749-97DE-B50D9DC19F81", "Free Trade Zone User Number"));

			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = OrgCusCodeInfo.GetMainOrganizationNumberTypes(CountryCode);
			result.Add(OrgCusCodes.RUT);
			result.Add(OrgCusCodes.CID);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);

			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = OrgCusCodeInfo.GetPrimaryCusCodes(CountryCode);
			result.Add(OrgCusCodes.BRC);
			result.Add(OrgCusCodes.CID);
			result.Add(OrgCusCodes.FZU);
			result.Add(OrgCusCodes.RUT);

			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCodes.AEO:
					new UYAEOValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.BRC:
					new UYBRCValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.CID:
					new UYCIDValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
				case OrgCusCodes.RUT:
					new UYRUTCodeValidator().Validate(orgCusCode.OK_CustomsRegNoInfo);
					break;
			}
		}
	}
}
