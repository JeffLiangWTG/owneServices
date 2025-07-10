using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ChinaCustomsTaxNumberHelperTest : TestCaseWithFactory
	{
		public void TestTaxCodesExistsForCountry()
		{
			var countryTaxCodeList = typeof(ChinaCustomsTaxNumberHelper)
								.GetProperty("CountryCodeDescriptions",
											System.Reflection.BindingFlags.NonPublic |
											System.Reflection.BindingFlags.Static)
								.GetValue(null, null) as IReadOnlyDictionary<string, TaxCodeInformation[]>;

			var missingList = new List<string>();
			var duplicatedList = new List<string>();
			var orgCodeLists = new OrgCodeLists();

			foreach (var taxPair in countryTaxCodeList)
			{
				foreach (var code in taxPair.Value.Select(x => x.Code).ToArray())
				{
					var availableOrgCode = orgCodeLists.CustomsCodes_List(taxPair.Key);
					if (!availableOrgCode.ContainsCode(code))
					{
						missingList.Add($"Country: {taxPair.Key} | Code: {code}");
					}
				}

				if (taxPair.Value.GroupBy(v => v.Code).Count() != taxPair.Value.Length)
				{
					duplicatedList.Add($"Country: {taxPair.Key} | Codes: {string.Join(", ", taxPair.Value.Select(c => c.Code))}");
				}
			}

			Assert(string.Join(System.Environment.NewLine,
					"The following tax config codes are missing from organisation config for the country: ",
					string.Join(System.Environment.NewLine, missingList),
					System.Environment.NewLine,
					duplicatedList.Any() ? "The following tax config codes are duplicated for the country: " : string.Empty,
					string.Join(System.Environment.NewLine, duplicatedList)),
				!missingList.Any() && !duplicatedList.Any());
		}

		public void TestGetTaxNumber()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var orgRegistrationNumberProvider = new OrgHeaderRegistrationNumberProvider(consignee);

			var cnoCode = consignee.CustomsCodes.AddNew();
			cnoCode.OK_CodeType = OrgCusCode.CodeTypes.CompanyNumber;
			cnoCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			cnoCode.OK_CustomsRegNo = "123CNO";

			var taxNumber = ChinaCustomsTaxNumberHelper.GetTaxNumberWithLabelFormatted(Core.Constants.CountryCodes.NewZealand, orgRegistrationNumberProvider);
			AssertEquals("Required Tax Number retrieved.", "Comp. ID: 123CNO", taxNumber);

			var ogrCode = consignee.CustomsCodes.AddNew();
			ogrCode.OK_CodeType = OrgCusCode.RussiaCodeTypes.OGRN;
			ogrCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Russia;
			ogrCode.OK_CustomsRegNo = "123OGR";

			taxNumber = ChinaCustomsTaxNumberHelper.GetTaxNumberWithLabelFormatted(Core.Constants.CountryCodes.Russia, orgRegistrationNumberProvider);
			AssertEquals("Required Tax Number retrieved.", "Comp. ID: 123OGR", taxNumber);

			var cuiCode = consignee.CustomsCodes.AddNew();
			cuiCode.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			cuiCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Argentina;
			cuiCode.OK_CustomsRegNo = "123CUI";

			taxNumber = ChinaCustomsTaxNumberHelper.GetTaxNumberWithLabelFormatted(Core.Constants.CountryCodes.Argentina, orgRegistrationNumberProvider);
			AssertEquals("Required Tax Number retrieved.", "Comp. ID: 123CUI", taxNumber);
		}

		public void TestGetTaxNumberWithFallback()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var orgRegistrationNumberProvider = new OrgHeaderRegistrationNumberProvider(consignee);

			var nireCode = consignee.CustomsCodes.AddNew();
			nireCode.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			nireCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			nireCode.OK_CustomsRegNo = "123NIRE";

			var taxNumber = ChinaCustomsTaxNumberHelper.GetTaxNumberWithLabelFormatted(Core.Constants.CountryCodes.Brazil, orgRegistrationNumberProvider);
			AssertEquals("Should have picked up fallback NIRE code", "Comp. ID: 123NIRE", taxNumber);
		}

		public void TestGetTaxNumberTypeForCountry()
		{
			var newZealandDescription = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForCountry(Core.Constants.CountryCodes.NewZealand);
			AssertEquals("CNO (Company Number)", newZealandDescription.ElementAt(0));

			var luxembourgDescription = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForCountry(Core.Constants.CountryCodes.Luxembourg);
			AssertEquals("TVA (VAT (TVA) Business Registration Number)", luxembourgDescription.ElementAt(0));

			var usaDescription = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("EIN (Employer Identification Number)", usaDescription.ElementAt(0));

			var bangladeshDescription = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForCountry(Core.Constants.CountryCodes.Bangladesh);
			AssertEquals("BRN (Business Registration Number)", bangladeshDescription.ElementAt(0));
		}

		public void TestGetTaxNumberTypeForCountryWithFallback()
		{
			var brazilDescription = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForCountry(Core.Constants.CountryCodes.Brazil);
			AssertEquals("CJN (CNPJ Cadastro Nacional da Pessoa Jurídica / Business Tax Payer Registration)", brazilDescription.ElementAt(0));
			AssertEquals("GCR (NIRE Número de Identificação no Registro de Empresas / Company Registration Number)", brazilDescription.ElementAt(1));

			var denmarkDescription = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForCountry(Core.Constants.CountryCodes.Denmark);
			AssertEquals("CVR (Central Business Register Number)", denmarkDescription.ElementAt(0));
			AssertEquals("PNR (Production Number)", denmarkDescription.ElementAt(1));

			var netherlandsDescription = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForCountry(Core.Constants.CountryCodes.Netherlands);
			AssertEquals("CCN (Chamber of Commerce Number)", netherlandsDescription.ElementAt(0));
			AssertEquals("BTW (VAT (BTW) Business Registration Number)", netherlandsDescription.ElementAt(1));

			var icelandDescription = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForCountry(Core.Constants.CountryCodes.Iceland);
			AssertEquals("KEN (Kennitala / Identification Number)", icelandDescription.ElementAt(0));
			AssertEquals("VSK (VAT (VSK) Business Registration Number)", icelandDescription.ElementAt(1));

			var unitedKingdomDescription = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForCountry(Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("CNO (Company Number)", unitedKingdomDescription.ElementAt(0));
			AssertEquals("VAT (VAT Business Registration Number)", unitedKingdomDescription.ElementAt(1));
		}

		public void TestGetTaxNumberTypesForTaxCodeInformation()
		{
			var afghanistanTaxFileCodeDescription = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForTaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode, Constants.CountryCodes.Afghanistan);
			AssertEquals("GTX (Government Tax File Code)", afghanistanTaxFileCodeDescription);

			var afghanistanEmptyCodeDescription = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForTaxCodeInformation("", Constants.CountryCodes.Afghanistan);
			AssertEquals(ZString.Empty, afghanistanEmptyCodeDescription);

			var emptyCountryTaxFileCodeDescription = ChinaCustomsTaxNumberHelper.GetTaxNumberTypesForTaxCodeInformation(OrgCusCode.CodeTypes.TaxFileCode, "");
			AssertEquals(ZString.Empty, emptyCountryTaxFileCodeDescription);
		}

		public void TestGetTaxInfoFromRefTable()
		{
			var refDocOrgCusCodeBO = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			refDocOrgCusCodeBO.DOC_DocumentType = "ESI";
			refDocOrgCusCodeBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;
			refDocOrgCusCodeBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.China;
			refDocOrgCusCodeBO.DOC_Notes = "Company Number TST";
			refDocOrgCusCodeBO.DOC_CodeType = OrgCusCode.CodeTypes.CompanyNumber;
			refDocOrgCusCodeBO.DOC_Priority = 1;

			var refDocOrgCusCodeBO2 = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			refDocOrgCusCodeBO2.DOC_DocumentType = "ESI";
			refDocOrgCusCodeBO2.DOC_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;
			refDocOrgCusCodeBO2.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.China;
			refDocOrgCusCodeBO2.DOC_Notes = "Trade Register Number TST";
			refDocOrgCusCodeBO2.DOC_CodeType = OrgCusCode.CodeTypes.VATCode;
			refDocOrgCusCodeBO2.DOC_Priority = 2;

			var refDocOrgCusCodeNotChina = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			refDocOrgCusCodeNotChina.DOC_DocumentType = "ESI";
			refDocOrgCusCodeNotChina.DOC_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;
			refDocOrgCusCodeNotChina.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Australia;
			refDocOrgCusCodeNotChina.DOC_Notes = "Trade Register Number TST";
			refDocOrgCusCodeNotChina.DOC_CodeType = OrgCusCode.CodeTypes.VATCode;
			refDocOrgCusCodeNotChina.DOC_Priority = 2;

			var refDocOrgCusCodeBO3 = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			refDocOrgCusCodeBO3.DOC_DocumentType = "ESI";
			refDocOrgCusCodeBO3.DOC_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			refDocOrgCusCodeBO3.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.China;
			refDocOrgCusCodeBO3.DOC_Notes = "Trade Register Number TST";
			refDocOrgCusCodeBO3.DOC_CodeType = OrgCusCode.CodeTypes.VATCode;
			refDocOrgCusCodeBO3.DOC_Priority = 2;
			Factory.Save();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			SetupOrgHeaderCustomsCodes(consignee);
			var orgRegistrationNumberProvider = new OrgHeaderRegistrationNumberProvider(consignee);
			var ukValues = ChinaCustomsTaxNumberHelper.GetTaxInfoFromRefTable(Constants.CountryCodes.UnitedKingdom, orgRegistrationNumberProvider);
			AssertEquals(2, ukValues.Count);
			var brazilValues = ChinaCustomsTaxNumberHelper.GetTaxInfoFromRefTable(Constants.CountryCodes.Brazil, orgRegistrationNumberProvider);
			AssertEquals(1, brazilValues.Count);
		}

		void SetupOrgHeaderCustomsCodes(OrgHeader consignee)
		{
			void SetupVatAndCompanyNumberForCountry(ZString country)
			{
				var cnoCode = consignee.CustomsCodes.AddNew();
				cnoCode.OK_CodeType = OrgCusCode.CodeTypes.CompanyNumber;
				cnoCode.OK_RN_NKCodeCountry = country;
				cnoCode.OK_CustomsRegNo = "123CNO";
				var vstCode = consignee.CustomsCodes.AddNew();
				vstCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				vstCode.OK_RN_NKCodeCountry = country;
				vstCode.OK_CustomsRegNo = "123VAT";
			}
			SetupVatAndCompanyNumberForCountry(Constants.CountryCodes.UnitedKingdom);
			SetupVatAndCompanyNumberForCountry(Constants.CountryCodes.Australia);
			SetupVatAndCompanyNumberForCountry(Constants.CountryCodes.China);
			SetupVatAndCompanyNumberForCountry(Constants.CountryCodes.Brazil);
		}
	}
}
