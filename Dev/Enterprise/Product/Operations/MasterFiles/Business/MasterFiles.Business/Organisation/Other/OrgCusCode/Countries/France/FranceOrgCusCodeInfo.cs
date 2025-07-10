using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public sealed class FranceOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider, IOrgCusCodeCustomsRegNoValidationProvider, IOrgCusCodeDependencyProvider, IOrgCusCodeUniqueValidation
	{
		CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.FranceCodeTypes.TVA, Country.TaxCodeDescriptions.VATCodeTemplate(OrgCusCode.FranceCodeTypes.TVA)); // Accounting consumption code

			list.OverridePair(OrgCusCode.CodeTypes.CorporationCode, MultilingualString.Join(" / ", (NoResString)"Nr. De Registre", ResString.GetMultilingualString("c600d210-01dc-4ece-ac89-cf690603fc09", "Business Registration Number")));
			list.AddPair(OrgCusCode.FranceCodeTypes.NAF, MultilingualString.Join(" / ", (NoResString)"Nomenclature des Activitees", ResString.GetMultilingualString("fc07cd94-62f3-4172-8757-bb83f38b91c9", "Primary Business Activity")));
			list.AddPair(OrgCusCode.FranceCodeTypes.Siren, Res.GetString("OrgCusCode.FranceCodeTypes.Siren", "SIREN Business Registration"));
			list.AddPair(OrgCusCode.FranceCodeTypes.Siret, Res.GetString("OrgCusCode.FranceCodeTypes.Siret", "SIRET Establishment Identifier"));
			list.AddPair(OrgCusCode.FranceCodeTypes.ALT, Res.GetString("396198D2-79B6-46AC-B4A4-8471FB4FB2E4", "VAT Reverse Charge Authorization"));
			list.AddPair(OrgCusCode.FranceCodeTypes.IST, Res.GetString("0A2A83C8-1175-46F8-A620-DF13D36369A3", "Temporary Storage Agreement"));
			list.AddPair(OrgCusCode.FranceCodeTypes.CIN, Res.GetString("9F6D1A0E-AD95-4578-9C8D-2DDC21B5D607", "CIN Registration Code"));
			list.AddPair(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, Res.GetString("F89F572F-D5DC-489B-9762-FC434508D5E8", "EORI branch suffix"));
			list.AddPair(OrgCusCode.FranceCodeTypes.ROU, MultilingualString.Join(" ", (NoResString)"Routage", ResString.GetMultilingualString("E1C8AA67-52DB-4D53-A909-223B17A12191", "Recipient ID")));
			list.AddPair(OrgCusCode.FranceCodeTypes.SUF, MultilingualString.Join(" ", (NoResString)"Suffixe", ResString.GetMultilingualString("54529606-05D3-4A2F-9688-CEDB69A19EA7", "Extended Establishment Identifier")));
			return list;
		}

		HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
		{
			var result = GetPrimaryCusCodes(Core.Constants.CountryCodes.France);
			result.Add(OrgCusCode.CodeTypes.CorporationCode);
			return result;
		}

		HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
		{
			var result = GetMainOrganizationNumberTypes(Core.Constants.CountryCodes.France);
			return result;
		}

		void IOrgCusCodeCustomsRegNoValidationProvider.Validate(OrgCusCode orgCusCode)
		{
			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCode.FranceCodeTypes.ALT:
					new FRALTCodeValidator().Validate(orgCusCode);
					break;
				case OrgCusCode.FranceCodeTypes.EoriBranchSuffix:
					new FREBSCodeValidator().Validate(orgCusCode);
					break;
				case OrgCusCode.FranceCodeTypes.IST:
					new FRISTCodeValidator().Validate(orgCusCode);
					break;
				case OrgCusCode.FranceCodeTypes.TVA:
					new FRTVACodeValidator().Validate(orgCusCode);
					break;
			}

			OrgCusCodeValidation.ValidateCustomsCodeForEU(orgCusCode);
			OrgCusCodeValidation.ValidateCustomsCodeEORI(orgCusCode);
		}

		IEnumerable<OrgCusCodeDependency> IOrgCusCodeDependencyProvider.GetOrgCusCodeDependencies(OrgCusCode orgCusCode)
		{
			switch (orgCusCode.OK_CodeType.ToUpperInvariant())
			{
				case OrgCusCode.FranceCodeTypes.ROU:
					return new OrgCusCodeDependency[] {
						new OrgCusCodeDependency(OrgCusCode.FranceCodeTypes.Siret, RoutageIdRequiresSiretIdPresentMessage)
					};
			}

			return null;
		}

		HashSet<string>[] IOrgCusCodeUniqueValidation.GetCodesCannotCoexist()
		{
			return new HashSet<string>[]
			{
				new HashSet<string>() { OrgCusCode.FranceCodeTypes.SUF, OrgCusCode.FranceCodeTypes.Siret },
			};
		}

		static string RoutageIdRequiresSiretIdPresentMessage => Res.GetString("c096910a-5277-4e2d-9035-92d79ce61a1d", "It is only possible to record a {0} ID (ROU) in combination with a SIRET ID (SRT)", (NoResString)"Routage");
	}
}
