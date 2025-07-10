using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	#region TaxCodeInformation

	public sealed class TaxCodeInformation
	{
		public TaxCodeInformation(ZString code) : this(code, ZString.Empty)
		{
			Code = code;
		}

		public TaxCodeInformation(ZString code, ZString label)
		{
			Code = code;
			Label = label;
		}

		public TaxCodeInformation(ZString code, ZString label, ZString description, ZString number)
			: this(code, label)
		{
			Description = description;
			Number = number;
		}

		public TaxCodeInformation(ZString code, ZString shortLabel, ZString longLabel, ZString description, ZString number, ZString countryCode, ZInt priority, ZString documentType, ZString comment, ZString regulatingCountryCode)
			: this(code)
		{
			ShortLabel = shortLabel;
			LongLabel = longLabel;
			Description = description;
			Number = number;
			CountryCode = countryCode;
			Priority = priority;
			DocumentType = documentType;
			Comments = comment;
			RegulatingCountryCode = regulatingCountryCode;
		}

		public TaxCodeInformation(ZString code, ZString shortLabel, ZString longLabel, ZString description, ZString number, ZString countryCode, ZInt priority, ZString documentType, ZString comment, ZString regulatingCountryCode, ZString direction)
			: this(code, shortLabel, longLabel, description, number, countryCode, priority, documentType, comment, regulatingCountryCode)
		{
			Direction = direction;
		}

		public ZString Code { get; }
		public ZString Label { get; }
		public ZString ShortLabel { get; }
		public ZString LongLabel { get; }

		public ZString Description { get; }
		public ZString Number { get; }
		public ZString CountryCode { get; }
		public ZInt Priority { get; }
		public ZString DocumentType { get; }
		public ZString Comments { get; }
		public ZString RegulatingCountryCode { get; }
		public ZString Direction { get; }
	}

	#endregion

	public static class ChinaCustomsTaxNumberHelper
	{
		public static ZString GetTaxNumberWithLabelFormatted(ZString countryCode, IRegistrationNumberProvider org)
		{
			var taxInfo = GetTaxNumberInfo(countryCode, org);

			return taxInfo != null
				? ZString.Format((NoResString)"Comp. ID: {0}", taxInfo.Number)
				: ZString.Empty;
		}

		public static TaxCodeInformation GetTaxNumberInfo(ZString countryCode, IRegistrationNumberProvider org)
		{
			if (countryCode.IsEmpty || org == null)
			{
				return null;
			}

			if (CountryCodeDescriptions.TryGetValue(countryCode, out var taxCodeInfoCollection))
			{
				foreach (var taxCodeInfo in taxCodeInfoCollection)
				{
					var number = org.GetTaxNumber(countryCode, taxCodeInfo.Code);
					if (!number.IsEmpty)
					{
						var cusCodeList = new OrgCodeLists().CustomsCodes_List(countryCode);
						return new TaxCodeInformation(
							taxCodeInfo.Code,
							taxCodeInfo.Label,
							cusCodeList.GetDescriptionFromCode(taxCodeInfo.Code),
							number);
					}
				}
			}

			return null;
		}

		public static IReadOnlyCollection<string> GetListOfApplicableCountryCodes() => CountryCodeDescriptions.Keys.ToArray();

		public static List<TaxCodeInformation> GetTaxInfoFromRefTable(ZString countryCode, IRegistrationNumberProvider org, BusinessObjectFactory factory = null)
		{
			if (factory == null)
			{
				factory = new BusinessObjectFactory();
			}

			var filter = new ZQuery(RefDocOrgCusCodeSchema.DOC_RN_NKCodeCountry, countryCode);
			filter.AddToFilter(new ZQuery(RefDocOrgCusCodeSchema.DOC_RN_NKRegulatingCountry, Constants.CountryCodes.China));
			filter.OrderBy = RefDocOrgCusCodeSchema.Constants.DOC_Priority + OrderByClause.Ascending;
			var refDocOrgCusCodeBOs = factory.Load<RefDocOrgCusCode>(filter);
			var result = new List<TaxCodeInformation>();
			foreach (var refDocOrgCusCodeBO in refDocOrgCusCodeBOs)
			{
				var number = org?.GetTaxNumber(countryCode, refDocOrgCusCodeBO.DOC_CodeType) ?? ZString.Empty;
				result.Add(new TaxCodeInformation(
					code: refDocOrgCusCodeBO.DOC_CodeType,
					shortLabel: refDocOrgCusCodeBO.DOC_ShortLabel,
					longLabel: refDocOrgCusCodeBO.DOC_LongLabel,
					description: refDocOrgCusCodeBO.DOC_Description,
					number: number,
					priority: refDocOrgCusCodeBO.DOC_Priority,
					countryCode: countryCode,
					documentType: refDocOrgCusCodeBO.DOC_DocumentType,
					comment: refDocOrgCusCodeBO.DOC_Notes,
					regulatingCountryCode: refDocOrgCusCodeBO.RegulatingCountry?.Code ?? ZString.Empty,
					direction: refDocOrgCusCodeBO.DOC_Direction
				));
			}
			return result;
		}

		public static IReadOnlyCollection<ZString> GetTaxNumberTypesForCountry(ZString countryCode)
		{
			if (countryCode.IsEmpty
				|| !CountryCodeDescriptions.TryGetValue(countryCode, out var taxCodeInfoCollection))
			{
				return new List<ZString>();
			}

			var cusCodeList = new OrgCodeLists().CustomsCodes_List(countryCode);
			return taxCodeInfoCollection.Select(taxCodeInfo => TaxCodeInformationCodeAndDescription(cusCodeList, taxCodeInfo.Code)).ToArray();
		}

		public static ZString GetTaxNumberTypesForTaxCodeInformation(ZString taxCodeInformationCode, ZString taxCodeInformationCountryCode)
		{
			if (taxCodeInformationCode.IsEmpty || taxCodeInformationCountryCode.IsEmpty)
			{
				return ZString.Empty;
			}

			var codeDescriptionPairList = new OrgCodeLists().CustomsCodes_List(taxCodeInformationCountryCode);
			return TaxCodeInformationCodeAndDescription(codeDescriptionPairList, taxCodeInformationCode);
		}

		static ZString TaxCodeInformationCodeAndDescription(CodeDescriptionPairList codeDescriptionPairList, ZString taxCodeInformationCode)
		{
			return ZString.Format("{0} ({1})", taxCodeInformationCode, codeDescriptionPairList.GetDescriptionFromCode(taxCodeInformationCode));
		}

		#region SuppressResourceStringsCheckRegion

		static IReadOnlyDictionary<string, TaxCodeInformation[]> CountryCodeDescriptions => countyCodeDescriptions ?? (countyCodeDescriptions = new Dictionary<string, TaxCodeInformation[]>
		{
			{ Core.Constants.CountryCodes.Afghanistan,  new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Albania,      new[] { new TaxCodeInformation(OrgCusCode.AlbaniaCodeTypes.NIT, "NIPT") } },
			{ Core.Constants.CountryCodes.Algeria,      new[] { new TaxCodeInformation(AlgeriaOrgCusCodeInfo.OrgCusCodes.NRC) } },
			{ Core.Constants.CountryCodes.Andorra,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Angola,       new[] { new TaxCodeInformation(OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal) } },
			{ Core.Constants.CountryCodes.Anguilla,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.AntiguaAndBarbuda, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Argentina,    new[] { new TaxCodeInformation(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT) } },
			{ Core.Constants.CountryCodes.Armenia,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.ArmeniaCodeTypes.TIN) } },
			{ Core.Constants.CountryCodes.Aruba,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Australia,    new[] { new TaxCodeInformation(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Austria,      new[] { new TaxCodeInformation(OrgCusCode.AustriaCodeTypes.UID, "MST"), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Azerbaijan,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Bahamas,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Bahrain,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode) } },
			{ Core.Constants.CountryCodes.Bangladesh,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.BusinessRegistrationNumber), } },
			{ Core.Constants.CountryCodes.Barbados,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), } },
			{ Core.Constants.CountryCodes.Belarus,      new[] { new TaxCodeInformation(OrgCusCode.BelarusCodeTypes.TIN), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Belgium,      new[] { new TaxCodeInformation(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Belize,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Benin,        new[] { new TaxCodeInformation(OrgCusCode.BeninCodeTypes.NRC) } },
			{ Core.Constants.CountryCodes.Bermuda,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Bhutan,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Bolivia,      new[] { new TaxCodeInformation(OrgCusCode.BoliviaCodeTypes.NIT), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.BosniaAndHerzegovina, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Botswana,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Brazil,       new[] { new TaxCodeInformation(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode, "NIRE") } },
			{ Core.Constants.CountryCodes.Brunei,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Bulgaria,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.BurkinaFaso,  new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Burundi,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Cambodia,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Cameroon,     new[] { new TaxCodeInformation(OrgCusCode.CameroonCodeTypes.NIU) } },
			{ Core.Constants.CountryCodes.Canada,       new[] { new TaxCodeInformation(OrgCusCode.CACodeTypes.BusinessNumberForCorporateIncomeTax), new TaxCodeInformation(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax) } },
			{ Core.Constants.CountryCodes.CapeVerde,    new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.CaymanIslands, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.CentralAfricanRepublic, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Chad,         new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Chile,        new[] { new TaxCodeInformation(ChileOrgCusCodeInfo.OrgCusCodes.RUT) } },
			{ Core.Constants.CountryCodes.China,        new[] { new TaxCodeInformation(OrgCusCode.ChinaCodeTypes.USC, "USCI"), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Colombia,     new[] { new TaxCodeInformation(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT) } },
			{ Core.Constants.CountryCodes.Comoros,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Congo,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.DemocraticRepublicOfCongo, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode) } },
			{ Core.Constants.CountryCodes.CostaRica,    new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.CoteDivoire,    new[] { new TaxCodeInformation(OrgCusCode.CoteDivoireCodeTypes.NCC) } },
			{ Core.Constants.CountryCodes.Croatia,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CroatiaCodeTypes.OIB) } },
			{ Core.Constants.CountryCodes.Cuba,         new[] { new TaxCodeInformation(OrgCusCode.CubaCodeTypes.GCR) } },
			{ Core.Constants.CountryCodes.Curacao,      new[] { new TaxCodeInformation(OrgCusCode.CuracaoCodeTypes.CCR, "COC"), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Cyprus,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.CzechRepublic,new[] { new TaxCodeInformation(CzechRepublicOrgCusCodeInfo.OrgCusCodes.DPH), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Denmark,      new[] { new TaxCodeInformation(OrgCusCode.DenmarkCodeTypes.CentralBusinessRegister), new TaxCodeInformation(OrgCusCode.DenmarkCodeTypes.ProductionNumber, "P-NO."), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Djibouti,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Dominica,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.DominicanRepublic,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Ecuador,      new[] { new TaxCodeInformation(OrgCusCode.EcuadorCodeTypes.RUC) } },
			{ Core.Constants.CountryCodes.Egypt,        new[] { new TaxCodeInformation(OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.ElSalvador,   new[] { new TaxCodeInformation(ElSalvadorOrgCusCodeInfo.OrgCusCodes.NRC) } },
			{ Core.Constants.CountryCodes.EquatorialGuinea, new[] { new TaxCodeInformation(OrgCusCode.EquatorialGuineaCodeTypes.NIF) } },
			{ Core.Constants.CountryCodes.Eritrea,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Estonia,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Ethiopia,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode) } },
			{ Core.Constants.CountryCodes.Fiji,         new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Finland,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.France,       new[] { new TaxCodeInformation(OrgCusCode.FranceCodeTypes.TVA), new TaxCodeInformation(OrgCusCode.FranceCodeTypes.Siren, "SIREN") } },
			{ Core.Constants.CountryCodes.Gabon,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Gambia,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Georgia,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Germany,      new[] { new TaxCodeInformation(GermanyOrgCusCodeInfo.OrgCusCodes.UST) } },
			{ Core.Constants.CountryCodes.Ghana,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Gibraltar,    new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Greece,       new[] { new TaxCodeInformation(OrgCusCode.GreeceCodeTypes.AFM) } },
			{ Core.Constants.CountryCodes.Grenada,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Guatemala,    new[] { new TaxCodeInformation(OrgCusCode.GuatemalaCodeTypes.NumeroDeIdentificacionTributaria) } },
			{ Core.Constants.CountryCodes.Guernsey,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Guinea,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.GuineaBissau, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Guyana,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Haiti,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Honduras,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.HongKong,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Hungary,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Iceland,      new[] { new TaxCodeInformation(OrgCusCode.IcelandCodeTypes.Kennitala, "KENN."), new TaxCodeInformation(OrgCusCode.IcelandCodeTypes.VSK) } },
			{ Core.Constants.CountryCodes.India,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Indonesia,    new[] { new TaxCodeInformation(OrgCusCode.IndonesiaCodeTypes.PPN), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Iran,         new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Iraq,         new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Ireland,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.IsleOfMan,    new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Israel,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Italy,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.IVA), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Jamaica,      new[] { new TaxCodeInformation(OrgCusCode.JamaicaCodeTypes.GCT), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Japan,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode) } },
			{ Core.Constants.CountryCodes.Jersey,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Jordan,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode) } },
			{ Core.Constants.CountryCodes.Kazakhstan,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Kenya,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Kiribati,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.KoreaNorth,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Kuwait,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Kyrgyzstan,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.LaoPeoplesDemocraticRepublic,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Latvia,       new[] { new TaxCodeInformation(OrgCusCode.LatviaCodeTypes.PVN), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Lebanon,      new[] { new TaxCodeInformation(OrgCusCode.LebanonCodeTypes.CRN, "COM") , new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode) } },
			{ Core.Constants.CountryCodes.Lesotho,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Liberia,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.LibyanArabJamahiriya, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Liechtenstein, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Lithuania,    new[] { new TaxCodeInformation(OrgCusCode.LithuaniaCodeTypes.PVM), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Luxembourg,   new[] { new TaxCodeInformation(OrgCusCode.FranceCodeTypes.TVA), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Macau,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Macedonia,    new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Madagascar,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Malawi,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.BusinessRegistrationNumber) } },
			{ Core.Constants.CountryCodes.Malaysia,     new[] { new TaxCodeInformation(MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfCompany), new TaxCodeInformation(MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfBusiness) } },
			{ Core.Constants.CountryCodes.Maldives,     new[] { new TaxCodeInformation(OrgCusCode.MaldivesCodeTypes.TIN) } },
			{ Core.Constants.CountryCodes.Mali,         new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Malta,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.MarshallIslands, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Mauritania,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Mauritius,    new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.BusinessRegistrationNumber) } },
			{ Core.Constants.CountryCodes.Mexico,       new[] { new TaxCodeInformation(MexicoOrgCusCodeInfo.OrgCusCodes.RFC), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Micronesia,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Monaco,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Mongolia,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Montenegro,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Morocco,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Mozambique,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Myanmar,      new[] { new TaxCodeInformation(OrgCusCode.MyanmarCodeTypes.CMT) } },
			{ Core.Constants.CountryCodes.Namibia,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Nauru,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Nepal,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Netherlands,  new[] { new TaxCodeInformation(OrgCusCode.NetherlandsCodeTypes.ChamberOfCommerceNumber, "COC"), new TaxCodeInformation(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "BTW") } },
			{ Core.Constants.CountryCodes.NewZealand,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CompanyNumber, "COM") } },
			{ Core.Constants.CountryCodes.Nicaragua,    new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Niger,        new[] { new TaxCodeInformation(OrgCusCode.NigerCodeTypes.NIF), new TaxCodeInformation(OrgCusCode.NigerCodeTypes.RCC) } },
			{ Core.Constants.CountryCodes.Nigeria,      new[] { new TaxCodeInformation(OrgCusCode.NigeriaCodeTypes.TaxIdentificationNumber), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Norway,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.OrganizationNumber) } },
			{ Core.Constants.CountryCodes.Oman,         new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode) } },
			{ Core.Constants.CountryCodes.Pakistan,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Palau,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.PalestinianTerritory,  new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode) } },
			{ Core.Constants.CountryCodes.Panama,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.PapuaNewGuinea, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Paraguay,     new[] { new TaxCodeInformation(OrgCusCode.ParaguayCodeTypes.RUC) } },
			{ Core.Constants.CountryCodes.Peru,         new[] { new TaxCodeInformation(OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode) } },
			{ Core.Constants.CountryCodes.Philippines,  new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Poland,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.PolandCodeTypes.NIP) } },
			{ Core.Constants.CountryCodes.Portugal,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.IVA) } },
			{ Core.Constants.CountryCodes.Qatar,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode) } },
			{ Core.Constants.CountryCodes.KoreaSouth,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Moldova,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Romania,      new[] { new TaxCodeInformation(OrgCusCode.RomaniaCodeTypes.TVA) } },
			{ Core.Constants.CountryCodes.Russia,       new[] { new TaxCodeInformation(OrgCusCode.RussiaCodeTypes.OGRN, "OGRN"), new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Rwanda,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.SaintKittsAndNevis, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.SaintLucia,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.SaintVincentAndTheGrenadin, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.WesternSamoa, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.SanMarino,    new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.SaoTomeAndPrincipe, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.SaudiArabia,  new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CompanyRegistrationNumber, "COM"), new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode) } },
			{ Core.Constants.CountryCodes.Senegal,      new[] { new TaxCodeInformation(OrgCusCode.SenegalCodeTypes.NIN) } },
			{ Core.Constants.CountryCodes.Serbia,       new[] { new TaxCodeInformation(SerbiaOrgCusCodeInfo.OrgCusCodes.PIB), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Seychelles,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.SierraLeone,  new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Singapore,    new[] { new TaxCodeInformation(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber) } },
			{ Core.Constants.CountryCodes.SintMaarten,  new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Slovakia,     new[] { new TaxCodeInformation(OrgCusCode.SlovakiaCodeTypes.DPH, "DPH") } },
			{ Core.Constants.CountryCodes.Slovenia,     new[] { new TaxCodeInformation(OrgCusCode.SloveniaCodeTypes.DDV) } },
			{ Core.Constants.CountryCodes.SolomonIslands, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Somalia,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.SouthAfrica,  new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.SouthSudan,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Spain,        new[] { new TaxCodeInformation(OrgCusCode.SpainCodeTypes.NIF) } },
			{ Core.Constants.CountryCodes.SriLanka,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Sudan,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Suriname,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Swaziland,    new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Sweden,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode, "MOMS"), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Switzerland,  new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.SyrianArabRepublic, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode) } },
			{ Core.Constants.CountryCodes.Taiwan,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Tajikistan,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Tanzania,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Thailand,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CompanyRegistrationNumber) } },
			{ Core.Constants.CountryCodes.TimorLeste,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Togo,         new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Tonga,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.TrinidadAndTobago, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Tunisia,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Turkey,       new[] { new TaxCodeInformation(TurkeyOrgCusCodeInfo.OrgCusCodes.TradeRegistryNumber), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Turkmenistan, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Tuvalu,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Uganda,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Ukraine,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.UnitedArabEmirates, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode) } },
			{ Core.Constants.CountryCodes.UnitedKingdom, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CompanyNumber, "COM"), new TaxCodeInformation(OrgCusCode.CodeTypes.VATCode) } },
			{ Core.Constants.CountryCodes.UnitedStates, new[] { new TaxCodeInformation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber) } },
			{ Core.Constants.CountryCodes.PuertoRico,   new[] { new TaxCodeInformation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber), new TaxCodeInformation(OrgCusCode.PuertoRicoCodeTypes.NRC) } },
			{ Core.Constants.CountryCodes.Uruguay,      new[] { new TaxCodeInformation(UruguayOrgCusCodeInfo.OrgCusCodes.RUT) } },
			{ Core.Constants.CountryCodes.Uzbekistan,   new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Vanuatu,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Vatican,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode) } },
			{ Core.Constants.CountryCodes.Venezuela,    new[] { new TaxCodeInformation(OrgCusCode.VenezuelaCodeTypes.RegistroUnicoDeInformacionFiscal) } },
			{ Core.Constants.CountryCodes.VietNam,      new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.BritishVirginIslands, new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode), new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Yemen,        new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode), new TaxCodeInformation(OrgCusCode.CodeTypes.GovBusinessCode) } },
			{ Core.Constants.CountryCodes.Zambia,       new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } },
			{ Core.Constants.CountryCodes.Zimbabwe,     new[] { new TaxCodeInformation(OrgCusCode.CodeTypes.CorporationCode) } }
		});

		[ThreadStatic]
		static IReadOnlyDictionary<string, TaxCodeInformation[]> countyCodeDescriptions;

		#endregion
	}
}
