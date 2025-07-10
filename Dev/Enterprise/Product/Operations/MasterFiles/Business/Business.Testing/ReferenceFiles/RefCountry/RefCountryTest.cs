using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCountry))]
	sealed class RefCountryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUseEUVatDescriptionForBrexit()
		{
			var gbCountry = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom);
			gbCountry.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
			Assert("Before Brexit", gbCountry.UseEUVatDescription);

			gbCountry.RN_EconomicGrouping = "";
			Assert("After Brexit", gbCountry.UseEUVatDescription);
		}

		public void TestCanDelete()
		{
			var countryCN = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			Assert(countryCN.RN_IsSystem);
			Assert(!countryCN.CanDelete);
			AssertEquals("Cannot delete system defined countries/regions.", countryCN.ReasonForNotAbleToDelete.ToString());

			var countryNew = Factory.NewWithValidTestData<RefCountry>();
			countryNew.Code = "AA";
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "CM1";
			company1.GC_RN_NKCountryCode = "AA";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "CM2";
			company2.GC_RN_NKCountryCode = "AA";
			Factory.Save();

			Assert(!countryNew.RN_IsSystem);
			AssertEquals(2, countryNew.InUseByCompanies.Count);
			Assert(!countryNew.CanDelete);
			AssertEquals("Cannot delete country/region AA because it is in use by companies: CM1, CM2.", countryNew.ReasonForNotAbleToDelete.ToString());
		}

		public void TestCannotDelete_IfCountryUsedInOrganization()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.Code = "AA";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			var address = org.Addresses.MainAddress;
			address.OA_RN_NKCountryCode = country.Code;
			Factory.Save();

			AssertEquals(false, country.RN_IsSystem);
			AssertEquals(1, country.InUseByOrgAddresses.Count);
			AssertEquals(false, country.CanDelete);
			AssertEquals("Cannot delete country/region AA because it is in use by organizations: ORG.", country.ReasonForNotAbleToDelete.ToString());
		}

		public void TestSupportDeclarationOfIntent()
		{
			RefCountry country = Factory.New<RefCountry>();
			country.RN_Code = Core.Constants.CountryCodes.Italy;
			Assert(country.SupportDeclarationOfIntent);

			country.RN_Code = Core.Constants.CountryCodes.Australia;
			Assert(!country.SupportDeclarationOfIntent);
		}

		public void TestGetPrefixForTaxRegistrationCode()
		{
			ZString countryCodeForGreece = "GR";
			ZString prefix1 = RefCountry.GetPrefixForTaxRegistrationCode(countryCodeForGreece);
			string expectedPrefix = "EL";
			AssertEquals("Prefix of TaxID", expectedPrefix, prefix1);

			ZString countryCodeForSomeCountry = "KR";
			ZString prefix2 = RefCountry.GetPrefixForTaxRegistrationCode(countryCodeForSomeCountry);
			expectedPrefix = "KR";
			AssertEquals("Prefix of TaxID", expectedPrefix, prefix2);
		}

		public void TestPostcodeFormattingRule()
		{
			var australia = RefCountry.LoadFromCountryCode(Factory, "AU");
			AssertNotNull("PostcodeFormattingRule should not be NULL", australia.PostcodeFormattingRule);

			var fakeCountry = Factory.NewWithValidTestData<RefCountry>();
			fakeCountry.Code = "XX";
			AssertNull("PostcodeFormattingRule should be NULL", fakeCountry.PostcodeFormattingRule);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Read Only

		public void TestEconomicGroupingIsReadOnly()
		{
			Assert(Country.RN_EconomicGroupingInfo.ReadOnly);
		}

		public void TestRN_ValidationStatus_ReadOnly()
		{
			Assert(Country.RN_ValidationStatus_ReadOnly);
		}

		public void TestRN_ValidationStatus_HasAvailableData()
		{
			var country = Factory.New<RefCountry>();

			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;
			Assert(!country.RN_ValidationStatus_HasAvailableData);

			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			Assert(country.RN_ValidationStatus_HasAvailableData);

			Env.Registry.EnableAddressValidationWebService = false;

			Assert(country.RN_ValidationStatus_HasAvailableData);
		}

		#endregion

		#region Local Business Registration Number Type

		public void TestLocalBusinessRegNoCodeType()
		{
			RefCountry country1 = Factory.New<RefCountry>();

			country1.RN_Code = Core.Constants.CountryCodes.Morocco;
			AssertEquals("Morocco Local business reg no type is ICE", OrgCusCode.MoroccoCodeTypes.ICE, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Malaysia;
			AssertEquals("MY Local business reg no type is ROC", MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfCompany, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals("ZA Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Australia;
			AssertEquals("AU Local business reg no type is ABN", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Fiji;
			AssertEquals("FJ Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			Country.RN_Code = Core.Constants.CountryCodes.Singapore;
			AssertEquals("Singapore Local business reg no type is UEN.", OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, Country.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Poland;
			AssertEquals("PL Local business reg no type is NIP", OrgCusCode.PolandCodeTypes.NIP, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Iceland;
			AssertEquals("PL Local business reg no type is KT.", OrgCusCode.IcelandCodeTypes.Kennitala, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Myanmar;
			AssertEquals("MM Local business reg no type is CRN.", OrgCusCode.CodeTypes.CompanyRegistrationNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Argentina;
			AssertEquals("AR Local business reg no type is CUI.", ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Chile;
			AssertEquals("CL Local business reg no type is RUT.", ChileOrgCusCodeInfo.OrgCusCodes.RUT, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Brazil;
			AssertEquals("BR Local business reg no type is CNPJ.", BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Colombia;
			AssertEquals("CO Local business reg no type is NIT.", ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, country1.LocalBusinessRegNoCodeType);

			foreach (string country in Core.Constants.CountryCodes.UsaAndTerritoriesList)
			{
				if (country == Constants.CountryCodes.AmericanSamoa)
				{
					continue;
				}
				country1.RN_Code = country;
				AssertEquals("US Local business reg no type is EIN.", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, country1.LocalBusinessRegNoCodeType);
			}

			country1.RN_Code = Core.Constants.CountryCodes.Thailand;
			AssertEquals("TH Local business reg no type is CRN.", OrgCusCode.CodeTypes.CompanyRegistrationNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Egypt;
			AssertEquals("TH Local business reg no type is COM.", OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Bangladesh;
			AssertEquals("BD Local business reg no type is BRN.", OrgCusCode.CodeTypes.BusinessRegistrationNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Mexico;
			AssertEquals("MX Local business reg no type is RFC.", MexicoOrgCusCodeInfo.OrgCusCodes.RFC, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Canada;
			AssertEquals("CA Local business reg no type is BRM.", OrgCusCode.CACodeTypes.BusinessNumberForImportExport, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Panama;
			AssertEquals("PA Local business reg no type is RUC.", PanamaOrgCusCodeInfo.OrgCusCodes.RUC, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Angola;
			AssertEquals("AO Local business reg no type is NIF.", OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Netherlands;
			AssertEquals("NL Local business reg no type is CCN.", OrgCusCode.NetherlandsCodeTypes.ChamberOfCommerceNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Estonia;
			AssertEquals("Estonia Local business reg no type is BRN.", OrgCusCode.CodeTypes.BusinessRegistrationNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.DemocraticRepublicOfCongo;
			AssertEquals("Congo Local business reg no type is CNO.", OrgCusCode.CodeTypes.CompanyNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Turkey;
			AssertEquals("Turkey's Local business reg no type is TRN.", TurkeyOrgCusCodeInfo.OrgCusCodes.TradeRegistryNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Finland;
			AssertEquals("Finland's Local business reg no type is GBR.", OrgCusCode.CodeTypes.GovBusinessCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.PapuaNewGuinea;
			AssertEquals("PapuaNewGuinea Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Namibia;
			AssertEquals("Namibia Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Tunisia;
			AssertEquals("Tunisia Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Peru;
			AssertEquals("Peru Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.TimorLeste;
			AssertEquals("Timor Leste Local business reg no type is GBR", OrgCusCode.CodeTypes.GovBusinessCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Reunion;
			AssertEquals("Reunion Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Guadeloupe;
			AssertEquals("Guadeloupe Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Martinique;
			AssertEquals("Martinique Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.FrenchGuyana;
			AssertEquals("FrenchGuyana Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Maldives;
			AssertEquals("Maldives Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Luxembourg;
			AssertEquals("Luxembourg Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Greece;
			AssertEquals("Greece Local business reg no type is GBR", OrgCusCode.CodeTypes.GovBusinessCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Madagascar;
			AssertEquals("Madagascar Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Austria;
			AssertEquals("Austria Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Bulgaria;
			AssertEquals("Bulgaria Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Israel;
			AssertEquals("Israel Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Romania;
			AssertEquals("Romania Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Hungary;
			AssertEquals("Hungary Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Switzerland;
			AssertEquals("Switzerland Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.SriLanka;
			AssertEquals("LK Local business reg no type is SVT.", OrgCusCode.SriLankaCodeTypes.SVATBusinessRegistrationNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Ethiopia;
			AssertEquals("ET Local business reg no type is TIN.", OrgCusCode.EthiopiaCodeTypes.TaxIdentificationNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Uganda;
			AssertEquals("UG Local business reg no type is TIN.", UgandaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Zambia;
			AssertEquals("ZM Local business reg no type is TIN.", OrgCusCode.ZambiaCodeTypes.TaxIdentificationNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Tonga;
			AssertEquals("Tonga Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.WesternSamoa;
			AssertEquals("Samoa Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Guatemala;
			AssertEquals("Guatemala Local business reg no type is NIT", OrgCusCode.GuatemalaCodeTypes.NumeroDeIdentificacionTributaria, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Nigeria;
			AssertEquals("Nigeria Local business reg no type is TIN", OrgCusCode.NigeriaCodeTypes.TaxIdentificationNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Venezuela;
			AssertEquals("Venezuela Local business reg no type is RIF", OrgCusCode.VenezuelaCodeTypes.RegistroUnicoDeInformacionFiscal, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Latvia;
			AssertEquals("Latvia Local business reg no type is PVN", OrgCusCode.LatviaCodeTypes.PVN, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Lithuania;
			AssertEquals("Lithuania Local business reg no type is PVM", OrgCusCode.LithuaniaCodeTypes.PVM, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Slovenia;
			AssertEquals("Slovenia Local business reg no type is DDV", OrgCusCode.SloveniaCodeTypes.DDV, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Ecuador;
			AssertEquals("Ecuador Local business reg no type is RUC", OrgCusCode.EcuadorCodeTypes.RUC, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.ElSalvador;
			AssertEquals("ElSalvador Local business reg no type is NRC", ElSalvadorOrgCusCodeInfo.OrgCusCodes.NRC, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Paraguay;
			AssertEquals("Paraguay Local business reg no type is RUC", OrgCusCode.ParaguayCodeTypes.RUC, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Uruguay;
			AssertEquals("Uruguay Local business reg no type is RUT", UruguayOrgCusCodeInfo.OrgCusCodes.RUT, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Bolivia;
			AssertEquals("Bolivia Local business reg no type is NIT", OrgCusCode.BoliviaCodeTypes.NIT, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.FrenchPolynesia;
			AssertEquals("FrenchPolynesia Local business reg no type is TAH", OrgCusCode.FrenchPolynesiaCodeTypes.TAH, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Honduras;
			AssertEquals("Honduras Local business reg no type is RTN", OrgCusCode.HondurasCodeTypes.RTN, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Nicaragua;
			AssertEquals("Nicaragua Local business reg no type is RUC", OrgCusCode.NicaraguaCodeTypes.RUC, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Azerbaijan;
			AssertEquals("Azerbaijan Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Kenya;
			AssertEquals("Kenya Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Mauritius;
			AssertEquals("Mauritius Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Portugal;
			AssertEquals("Portugal Local business reg no type is IVA", OrgCusCode.CodeTypes.IVA, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Mongolia;
			AssertEquals("Mongolia Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Slovakia;
			AssertEquals("Slovakia Local business reg no type is DPH", OrgCusCode.SlovakiaCodeTypes.DPH, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Botswana;
			AssertEquals("Botswana Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Kazakhstan;
			AssertEquals("Kazakhstan Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Tanzania;
			AssertEquals("Tanzania Local business reg no type is VAT", OrgCusCode.TanzaniaCodeTypes.VRN, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Mali;
			AssertEquals("Mali Local business reg no type is NIF", OrgCusCode.MaliCodeTypes.NIF, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Jordan;
			AssertEquals("Jordan Local business reg no type is GST", OrgCusCode.CodeTypes.GSTCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Lebanon;
			AssertEquals("Lebanon Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Senegal;
			AssertEquals("Senegal Local business reg no type is NIN", OrgCusCode.SenegalCodeTypes.NIN, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.CoteDivoire;
			AssertEquals("CoteDivoire Local business reg no type is NCC", OrgCusCode.CoteDivoireCodeTypes.NCC, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Cameroon;
			AssertEquals("Cameroon Local business reg no type is NIU", OrgCusCode.CameroonCodeTypes.NIU, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Mozambique;
			AssertEquals("Mozambique Local business reg no type is NUI", OrgCusCode.MozambiqueCodeTypes.NUI, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.EquatorialGuinea;
			AssertEquals("Equatorial Guinea Local business reg no type is NIF", OrgCusCode.EquatorialGuineaCodeTypes.NIF, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.DominicanRepublic;
			AssertEquals("Dominican Republic Local business reg no type is RNC", DominicanRepublicOrgCusCodeInfo.OrgCusCodes.RNC, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Yemen;
			AssertEquals("Yemen Local business reg no type is GST", OrgCusCode.CodeTypes.GSTCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Somalia;
			AssertEquals("Somalia Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Panama;
			AssertEquals("Panama Local business reg no type is RUC", PanamaOrgCusCodeInfo.OrgCusCodes.RUC, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Malawi;
			AssertEquals("Malawi Local business reg no type is TIN", OrgCusCode.MalawiCodeTypes.TIN, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Niger;
			AssertEquals("Niger Local business reg no type is NIF", OrgCusCode.NigerCodeTypes.NIF, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Kiribati;
			AssertEquals("Kiribati Local business reg no type is TIN", OrgCusCode.KiribatiCodeTypes.TaxIdentificationNumber, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Nepal;
			AssertEquals("Nepal Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Iran;
			AssertEquals("Iran Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.UnitedArabEmirates;
			AssertEquals("UnitedArabEmirates Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Bahrain;
			AssertEquals("Bahrain Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Kuwait;
			AssertEquals("Kuwait Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Oman;
			AssertEquals("Oman Local business reg no type is VAT", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.Qatar;
			AssertEquals("Qatar Local business reg no type is GCR", OrgCusCode.CodeTypes.CorporationCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.SaudiArabia;
			AssertEquals("SaudiArabia Local business reg no type is VAT.", OrgCusCode.CodeTypes.VATCode, country1.LocalBusinessRegNoCodeType);

			country1.RN_Code = Core.Constants.CountryCodes.NewCaledonia;
			AssertEquals("New Caledonia Local business reg no type is TGC.", OrgCusCode.NewCaledoniaCodeTypes.TGC, country1.LocalBusinessRegNoCodeType);
		}

		#endregion

		#region Document Manager

		public void TestDocManagerCode()
		{
			RefCountry testCountry = Factory.NewWithValidTestData<RefCountry>();
			AssertEquals("Correct doc manager code", "COU", ((IDocManagerSupport)testCountry).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region Exchange Rates

		public void TestExchangeRateLoads()
		{
			RefCountry testCountry = Factory.NewWithValidTestData<RefCountry>();
			AssertNotNull("Exchange rate exists", testCountry.ExchangeRate);
		}

		#endregion

		#region Save and Delete

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		#endregion

		#region Loading

		public void TestLoadFromCountryCode()
		{
			AssertEquals("ZA", RefCountry.LoadFromCountryCode(Factory, "ZA").RN_Code);
			AssertEquals("US", RefCountry.LoadFromCountryCode(Factory, "US").RN_Code);
			AssertNull(RefCountry.LoadFromCountryCode(Factory, "ww"));
		}

		public void TestLoadFromCountryName()
		{
			AssertEquals("GM", RefCountry.LoadFromCountryName(Factory, "Gambia").RN_Code);
			AssertEquals("AS", RefCountry.LoadFromCountryName(Factory, "American Samoa").RN_Code);
			AssertNull(RefCountry.LoadFromCountryName(Factory, "mars"));
		}
		#endregion

		#region Contains UNLOCO

		public void TestContainsUNLOCO()
		{
			RefCountry.Loader countryLoader = new RefCountry.Loader(Factory);
			RefUNLOCO.Loader unLocoLoader = new RefUNLOCO.Loader(Factory);
			RefCountry uSCountry = countryLoader.LoadForCountry(Core.Constants.CountryCodes.UnitedStates);
			RefUNLOCO uSLAXLoco = null;
			AssertEquals("ContainsUNLOCO should return false for null", false, uSCountry.ContainsUNLOCO(uSLAXLoco));
			AssertEquals("ContainsUNLOCO should return false for empty", false, uSCountry.ContainsUNLOCO(""));

			uSLAXLoco = unLocoLoader.Load("USLAX");
			RefUNLOCO aUSYDLoco = unLocoLoader.Load("AUSYD");
			RefCountry aUCountry = countryLoader.LoadForCountry(Core.Constants.CountryCodes.Australia);

			AssertEquals("US contains USLAX", true, uSCountry.ContainsUNLOCO(uSLAXLoco));
			AssertEquals("AU does not contain USLAX", false, aUCountry.ContainsUNLOCO(uSLAXLoco));
			AssertEquals("AU contains AUSYD", true, aUCountry.ContainsUNLOCO(aUSYDLoco));
			AssertEquals("US does not contain AUSYD", false, uSCountry.ContainsUNLOCO(aUSYDLoco));

			AssertEquals("US contains USLAX", true, uSCountry.ContainsUNLOCO(uSLAXLoco.Code));
			AssertEquals("AU does not contain USLAX", false, aUCountry.ContainsUNLOCO(uSLAXLoco.Code));
			AssertEquals("AU contains AUSYD", true, aUCountry.ContainsUNLOCO(aUSYDLoco.Code));
			AssertEquals("US does not contain AUSYD", false, uSCountry.ContainsUNLOCO(aUSYDLoco.Code));
		}

		public void TestContainsUNLOCOWithInvalidCountryCode()
		{
			RefCountry.Loader countryLoader = new RefCountry.Loader(Factory);
			RefUNLOCO.Loader unLocoLoader = new RefUNLOCO.Loader(Factory);
			RefCountry uSCountry = countryLoader.LoadForCountry(Core.Constants.CountryCodes.UnitedStates);
			RefUNLOCO testLoco = unLocoLoader.Load("AUSYD");

			testLoco.RL_RN_NKCountryCode = "XX";
			AssertEquals("ContainsUNLOCO should return false for null", false, uSCountry.ContainsUNLOCO(testLoco));
		}

		#endregion

		#region Tax Logic

		delegate void TestIsRecognisedByEuropeanUnionAsIssuerOfAuthorisedEconomicOperatorCredentialsMethod(string countryCode, bool expected);
		public void TestIsRecognisedByEuropeanUnionAsIssuerOfAuthorisedEconomicOperatorCredentials()
		{
			TestIsRecognisedByEuropeanUnionAsIssuerOfAuthorisedEconomicOperatorCredentialsMethod assertAeoOrEoriIssuer = (countryCode, expected) =>
			{
				var country = RefCountry.LoadFromCountryCode(Factory, countryCode);
				if (country == null)
				{
					country = Factory.New<RefCountry>();
					country.RN_Code = countryCode;
				}
				AssertEquals("Can issue EU-recognised AEO and EORI numbers for trade with EU", expected, country.IsEuOrFriend);
			};
			assertAeoOrEoriIssuer(Core.Constants.CountryCodes.UnitedKingdom, true);
			assertAeoOrEoriIssuer(Core.Constants.CountryCodes.Japan, true);
			assertAeoOrEoriIssuer(Core.Constants.CountryCodes.UnitedStates, true);
			assertAeoOrEoriIssuer(Core.Constants.CountryCodes.Switzerland, true);
			assertAeoOrEoriIssuer(Core.Constants.CountryCodes.Norway, true);
			assertAeoOrEoriIssuer(Core.Constants.CountryCodes.SanMarino, true);
			assertAeoOrEoriIssuer(Core.Constants.CountryCodes.Andorra, true);
			assertAeoOrEoriIssuer(Core.Constants.CountryCodes.Liechtenstein, true);
			assertAeoOrEoriIssuer(Core.Constants.CountryCodes.Vatican, true);
			assertAeoOrEoriIssuer(Core.Constants.CountryCodes.Australia, false);
			assertAeoOrEoriIssuer(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, true);
		}

		public void TestIsNorthernIreland()
		{
			var country = Factory.New<RefCountry>();
			CombineAssertions(() =>
			{
				country.RN_Code = Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes;
				AssertEquals("XI", true, country.IsNorthernIreland);

				country.RN_Code = Core.Constants.CountryCodes.UnitedKingdom;
				AssertEquals("GB", false, country.IsNorthernIreland);
			});
		}

		public void TestIsIcs2MemberCountries()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			Assert(country.IsIcs2Member);

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Japan);
			Assert(!country.IsIcs2Member);

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Germany);
			Assert(country.IsIcs2Member);

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Norway);
			Assert(country.IsIcs2Member);

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Switzerland);
			Assert(country.IsIcs2Member);

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SanMarino);
			Assert(country.IsIcs2Member);

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Andorra);
			Assert(country.IsIcs2Member);

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Liechtenstein);
			Assert(country.IsIcs2Member);

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Vatican);
			Assert(country.IsIcs2Member);
		}

		public void TestRN_EconomicGrouping()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;

				RefCountry country = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom);
				AccountingMasterFilesRegistry.Instance.SimulateGBOutOfEU.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertEquals(ZString.Empty, country.RN_EconomicGrouping);

				AccountingMasterFilesRegistry.Instance.SimulateGBOutOfEU.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				if (!country.RN_EconomicGrouping.IsEmpty)
				{
					AssertEquals(EconomicGroupList.Codes.EuropeanUnion, country.RN_EconomicGrouping);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCountryCode;
			}
		}

		public void TestIsPartOfEuropeanUnion()
		{
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Australia, false);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.NewZealand, false);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.China, false);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.UnitedStates, false);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.SouthAfrica, false);

			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Austria, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Belgium, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Bulgaria, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Cyprus, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.CzechRepublic, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Denmark, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Estonia, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Finland, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.France, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Germany, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Greece, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Hungary, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Ireland, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Italy, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Latvia, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Lithuania, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Luxembourg, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Malta, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Netherlands, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Poland, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Portugal, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Romania, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Slovakia, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Slovenia, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Spain, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Sweden, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.UnitedKingdom, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Monaco, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.IsleOfMan, true);
			AssertCountryIsPartOfEuropeanUnion(Core.Constants.CountryCodes.Croatia, true);

			RefCountryCollection allCountries = new RefCountryCollection(Factory);

			int countriesInEuropeanUnion = 0;

			foreach (RefCountry country in allCountries)
			{
				if (country.IsPartOfEuropeanUnion)
				{
					countriesInEuropeanUnion++;
				}
			}

			AssertEquals("Countries in European Union", 30, countriesInEuropeanUnion);
		}

		void AssertCountryIsPartOfEuropeanUnion(ZString countryCode, bool expected)
		{
			RefCountry country = RefCountry.LoadFromCountryCode(Factory, countryCode);
			AssertEquals("Country is part of European Union", expected, country.IsPartOfEuropeanUnion);
		}

		public void TestTemporarilySetIsPartOfEuropeanUnion()
		{
			var australia = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			Assert("prerequisite: AU is not part of EU", !australia.IsPartOfEuropeanUnion);

			using (australia.TemporarilySetIsPartOfEuropeanUnion(true))
			{
				Assert("temp set AU as part of EU", australia.IsPartOfEuropeanUnion);
			}

			Assert("restored AU as not part of EU", !australia.IsPartOfEuropeanUnion);

			var germany = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Germany);
			Assert("prerequisite: DE is part of EU", germany.IsPartOfEuropeanUnion);

			using (germany.TemporarilySetIsPartOfEuropeanUnion(false))
			{
				Assert("temp set DE as not part of EU", !germany.IsPartOfEuropeanUnion);
			}

			Assert("restored DE as part of EU", germany.IsPartOfEuropeanUnion);
		}

		public void TestUseEUAirCargoSecurityStandards()
		{
			using (RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom).TemporarilySetIsPartOfEuropeanUnion(false))
			{
				CombineAssertions("Country does not use EU Air Cargo Security Standards", () =>
				{
					Assert(Constants.CountryCodes.UnitedStates, !RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedStates).UseEUAirCargoSecurityStandards);
					Assert(Constants.CountryCodes.Australia, !RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia).UseEUAirCargoSecurityStandards);
					Assert(Constants.CountryCodes.NewZealand, !RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.NewZealand).UseEUAirCargoSecurityStandards);
					Assert(Constants.CountryCodes.China, !RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.China).UseEUAirCargoSecurityStandards);
					Assert(Constants.CountryCodes.SouthAfrica, !RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.SouthAfrica).UseEUAirCargoSecurityStandards);
				});

				var euCountries = Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_EconomicGrouping, EconomicGroupList.Codes.EuropeanUnion));
				var euSecurityGroup = new List<RefCountry>(euCountries);

				euSecurityGroup.AddRange(new[]
				{
					RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom),
					RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Switzerland),
					RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Iceland),
					RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Liechtenstein),
					RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Norway)
				});

				foreach (var country in euSecurityGroup)
				{
					Assert($"{country.Code} uses EU Air Cargo Security Standards", country.UseEUAirCargoSecurityStandards);
				}
			}
		}

		public void TestIsInEFTA()
		{
			CombineAssertions("Check if a country is in EFTA", () =>
			{
				AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedStates).IsInEFTA);
				AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia).IsInEFTA);
				AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.NewZealand).IsInEFTA);
				AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.China).IsInEFTA);
				AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.SouthAfrica).IsInEFTA);

				AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Switzerland).IsInEFTA);
				AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Iceland).IsInEFTA);
				AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Liechtenstein).IsInEFTA);
				AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Norway).IsInEFTA);
			});
		}

		public void TestIsBLNS()
		{
			CombineAssertions("Check if a country is in BLNS", () =>
			{
				AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedStates).IsBLNS);
				AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia).IsBLNS);
				AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.NewZealand).IsBLNS);
				AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.China).IsBLNS);
				AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.SouthAfrica).IsBLNS);

				AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Botswana).IsBLNS);
				AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Lesotho).IsBLNS);
				AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Namibia).IsBLNS);
				AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Swaziland).IsBLNS);
			});
		}

		public void TestTaxCodeIsHere()
		{
			RefCountry country = Factory.New<RefCountry>();
			country.Code = Env.CurrentCompany.Country.Code;
			AssertEquals("TaxCode", Env.CurrentCompany.Country.ConsumptionTaxDescription, country.ConsumptionTaxDescription);
		}

		public void TestIsGSTRegistered()
		{
			RefCountry country = Factory.New<RefCountry>();
			country.Code = Env.CurrentCompany.Country.Code;
			AssertEquals("IsGSTRegistered", Env.CurrentCompany.Country.IsGSTRegistered, country.IsGSTRegistered);
		}

		#endregion

		#region Government Tax Invoice

		public void TestSupportDocumentSigning()
		{
			AssertSupportDocumentSigning(Core.Constants.CountryCodes.Portugal, true);
			AssertSupportDocumentSigning(Core.Constants.CountryCodes.Peru, false);
			AssertSupportDocumentSigning(Core.Constants.CountryCodes.Australia, false);
		}

		void AssertSupportDocumentSigning(string countryToTest, bool expectedResult)
		{
			ZQuery countryFilter = new ZQuery(RefCountrySchema.RN_Code, countryToTest);
			RefCountry country = Factory.LoadTop1<RefCountry>(countryFilter);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryToTest))
			{
				AssertEquals("SupportDocumentSigning for Country (" + countryToTest + ")", expectedResult, country.SupportDocumentSigning);
			}
		}

		public void TestComplianceSubTypeRules()
		{
			var country1 = Factory.New<RefCountry>();
			var country2 = Factory.New<RefCountry>();
			var fallback = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.SetValue(Guid.Empty, fallback.BranchPK, Guid.Empty, new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch(fallback, Factory, ChinaComplianceInfo.RuleSetCodes.TXATXB));
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, new ComplianceSubTypeAttributionRuleSet(fallback, Factory, ChinaComplianceInfo.RuleSetCodes.TXATXBETA));

				AssertEquals(ChinaComplianceInfo.RuleSetCodes.TXATXB, country1.ComplianceSubTypeRules_ForTestOnly[0].RuleSetCode);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.SetValue(Guid.Empty, fallback.BranchPK, Guid.Empty, new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch(fallback, Factory, ""));
				AssertEquals(ChinaComplianceInfo.RuleSetCodes.TXATXBETA, country2.ComplianceSubTypeRules_ForTestOnly[0].RuleSetCode);
			}
		}

		public void TestSupportComplianceSubType()
		{
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Australia, false);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.NewZealand, false);

			AssertSupportComplianceSubType(Core.Constants.CountryCodes.China, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.VietNam, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Indonesia, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Peru, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Poland, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Mexico, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Chile, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Argentina, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Colombia, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Taiwan, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Uruguay, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Ecuador, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.CostaRica, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Guatemala, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Honduras, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.Portugal, true);
			AssertSupportComplianceSubType(Core.Constants.CountryCodes.ElSalvador, true);
		}

		void AssertSupportComplianceSubType(string countryToTest, bool expectedResult)
		{
			ZQuery countryFilter = new ZQuery(RefCountrySchema.RN_Code, countryToTest);
			RefCountry country = Factory.LoadTop1<RefCountry>(countryFilter);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryToTest))
			{
				AssertEquals("SupportComplianceSubType for Country (" + countryToTest + ")", expectedResult, country.SupportComplianceSubType);
			}
		}

		public void TestHasGovtTaxInvoice()
		{
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Australia, false);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.NewZealand, false);

			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.China, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.VietNam, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Indonesia, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Peru, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Poland, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Mexico, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Chile, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Argentina, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Colombia, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Taiwan, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Uruguay, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Ecuador, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.CostaRica, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Guatemala, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Honduras, true);
			AssertHasGovtTaxInvoice(Core.Constants.CountryCodes.Portugal, true);
		}

		void AssertHasGovtTaxInvoice(string countryToTest, bool expectedResult)
		{
			ZQuery countryFilter = new ZQuery(RefCountrySchema.RN_Code, countryToTest);
			RefCountry country = Factory.LoadTop1<RefCountry>(countryFilter);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryToTest))
			{
				AssertEquals("IsGovtTaxInvoice for Country (" + countryToTest + ")", expectedResult, country.HasGovtTaxInvoice);
			}
		}

		#endregion

		#region Licence Builder Logic

		public void TestIsSupportedForLicenceBuilder()
		{
			AssertEquals("IsSupportedForLicenceBuilder", true, RefCountry.IsSupportedForLicenceBuilder(Enterprise.Core.Constants.CountryCodes.Australia));
		}

		#endregion

		#region ILocation Tests

		public void TestILocationImplementation()
		{
			RefCountry testCountry = Factory.NewWithValidTestData<RefCountry>();
			RefZoneHeader testZoneHeader = Factory.NewWithValidTestData<RefZoneHeader>();

			AssertNull("Country's ILocation.UNLOCO is null", ((ILocation)testCountry).UNLOCO);

			AssertEquals("ZoneHeader's Countries collection empty", 0, testZoneHeader.Countries.Count);
			testZoneHeader.Countries.Add(testCountry);
			Assert("Added Country successfully to ZoneHeader", testZoneHeader.Countries.Count == 1);

			AssertEquals("((ILocation)TestCountry).ZoneHeader set properly", testZoneHeader, ((ILocation)testCountry).Zones[0]);
			((ILocation)testCountry).Zones[0].Delete();
			AssertEquals("((ILocation)TestCountry).ZoneHeader not caching deletes", 0, ((ILocation)testCountry).Zones.Length);
		}

		public void TestILocationCompletelyCovers()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "GBBYS";
			var abbotsAdr = org.MainAddress;
			abbotsAdr.OA_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
			abbotsAdr.OA_RL_NKRelatedPortCode = "GBBYS";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultAddressType = AddressType.DLV;
			docAddress.OrganisationPK = org.PK;

			var newYork = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USNYC"));
			var abbots = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBBYS"));
			var gbUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBABB"));

			var britain = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedKingdom);
			var america = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedStates);

			AssertEquals("US covers New York", true, america.CompletelyCovers(newYork));
			AssertEquals("Britain covers Abbotsleigh", true, britain.CompletelyCovers(abbots));
			AssertEquals("America covers America", true, america.CompletelyCovers(america));
			AssertEquals("Britain covers Address in Abbots", true, britain.CompletelyCovers(abbotsAdr));
			AssertEquals("Britain covers JobDoc in GBUNLOCO", true, britain.CompletelyCovers(docAddress));

			var usZone = Factory.NewWithValidTestData<RefZoneHeader>();
			usZone.Countries.Add(america);

			AssertEquals("US covers one country Zone.", true, america.CompletelyCovers(usZone));

			usZone.Countries.Add(britain);

			AssertEquals("US doesnt cover 2 country Zone.", false, america.CompletelyCovers(usZone));

			var gbZone = Factory.NewWithValidTestData<RefZoneHeader>();
			gbZone.UNLOCOs.Add(gbUnloco);
			gbZone.UNLOCOs.Add(abbots);

			AssertEquals("Britain covers zone with only British UNLOCO's", true, britain.CompletelyCovers(gbZone));

			var usGbZone = Factory.NewWithValidTestData<RefZoneHeader>();
			usGbZone.UNLOCOs.Add(newYork);
			usGbZone.UNLOCOs.Add(abbots);

			AssertEquals("Country cannot contain zone with UNLOCO from different countries", false, britain.CompletelyCovers(usGbZone));
		}

		public void TestILocationReferenceIsLocalInRelationTo()
		{
			ILocationReference us = RefCountry.LoadFromCountryCode(Factory, "US");

			AssertEquals("US is not local to U", false, us.IsLocalInRelationTo("U"));
			AssertEquals("US is local to US", true, us.IsLocalInRelationTo("US"));
			AssertEquals("US is local to USNYC", true, us.IsLocalInRelationTo("USNYC"));
			AssertEquals("US is not local to ZA", false, us.IsLocalInRelationTo("ZA"));
		}

		#endregion

		#region System Defined Countries

		public void TestRNCodeReadOnly()
		{
			Country.RN_IsSystem = false;
			Country.OnLoaded();
			Assert("RN_Code should not be read only", !Country.RN_CodeInfo.ReadOnly);
			Country.RN_IsSystem = true;
			Country.OnLoaded();
			Assert("RN_Code should be read only", Country.RN_CodeInfo.ReadOnly);
		}

		public void TestRNDescReadOnly()
		{
			bool originalIsController = GlbStaff.CurrentUser.GS_IsController;

			GlbStaff.CurrentUser.GS_IsController = false;
			Country.RN_IsSystem = false;
			AssertEquals("Precondition: Description is not readonly", false, Country.RN_DescInfo.ReadOnly);

			Country.RN_Desc = "Mycountry";
			Country.RN_IsSystem = true;

			AssertEquals("Description is readonly", true, Country.RN_DescInfo.ReadOnly);

			GlbStaff.CurrentUser.GS_IsController = true;
			AssertEquals("Description is not readonly", false, Country.RN_DescInfo.ReadOnly);
		}

		public void TestRNISO3ReadOnly()
		{
			Country.RN_IsSystem = false;
			Country.OnLoaded();
			Assert("RN_IsoAlpha3Code should not be read only", !Country.RN_IsoAlpha3CodeInfo.ReadOnly);
			Country.RN_IsSystem = true;
			Country.OnLoaded();
			Assert("RN_IsoAlpha3Code should be read only", Country.RN_IsoAlpha3CodeInfo.ReadOnly);
		}

		public void TestRNISONumReadOnly()
		{
			Country.RN_IsSystem = false;
			Country.OnLoaded();
			Assert("RN_IsoNumericUNM49Code should not be read only", !Country.RN_IsoNumericUNM49CodeInfo.ReadOnly);
			Country.RN_IsSystem = true;
			Country.OnLoaded();
			Assert("RN_IsoNumericUNM49Code should be read only", Country.RN_IsoNumericUNM49CodeInfo.ReadOnly);
		}

		public void TestIsSystemReadOnly()
		{
			Assert(Country.RN_IsSystem_ReadOnly);
		}

		#endregion

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(Country.GetType()));
		}
		#endregion

		#region IsSpanishSpeakingCountry

		public void TestIsSpanishSpeakingCountry()
		{
			AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, "ES").IsSpanishSpeakingCountry);

			AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, "AU").IsSpanishSpeakingCountry);

			AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, "CO").IsSpanishSpeakingCountry);

			AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, "ZA").IsSpanishSpeakingCountry);

			AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, "MX").IsSpanishSpeakingCountry);

			AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, "NZ").IsSpanishSpeakingCountry);

			AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, "UY").IsSpanishSpeakingCountry);

			AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, "FR").IsSpanishSpeakingCountry);

			AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, "PA").IsSpanishSpeakingCountry);
		}

		#endregion

		#region IsStateMustNotBeEntered

		public void TestIsStateMustNotBeEntered()
		{
			AssertIsStateMustNotBeEntered(CountryAddressValidationRuleList.Codes.MustNotBeEntered, true);

			AssertIsStateMustNotBeEntered(CountryAddressValidationRuleList.Codes.MustBeEntered, false);

			AssertIsStateMustNotBeEntered(CountryAddressValidationRuleList.Codes.NoValidationRule, false);
		}

		void AssertIsStateMustNotBeEntered(string rule, bool expectedValue)
		{
			Country.RN_StateProvinceValidationRule = rule;
			AssertEquals(expectedValue, Country.IsStateMustNotBeEntered);
		}

		#endregion

		#region IsFranceOrTerritory

		public void TestIsFranceOrTerritory()
		{
			AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.France).IsFranceOrTerritory);
			AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Reunion).IsFranceOrTerritory);
			AssertEquals(true, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Martinique).IsFranceOrTerritory);

			AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Belgium).IsFranceOrTerritory);
			AssertEquals(false, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia).IsFranceOrTerritory);
		}

		#endregion

		#region IsEuOrGspPlusOCT

		public void TestIsEUorGspPlusOCT()
		{
			var refDbArrangmentScript = @"
				--RefDataGrouping //Adding EUN
				declare @RefDataGroupingParentPK uniqueidentifier = NEWID();
				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (@RefDataGroupingParentPK, 'EUN', 'Europe', NULL)

				--RefCusTradeGroup //Adding EUC
				declare @RefCusTradeGroupParentPK_GSP uniqueidentifier = NEWID();
				declare @RefCusTradeGroupParentPK_GSPPlus uniqueidentifier = NEWID();

				INSERT RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping) VALUES (@RefCusTradeGroupParentPK_GSP, '1030', 'GSP', '1900-01-01 12:00:00', '2079-06-06 23:59:00', 'EUN')
				INSERT RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping) VALUES (@RefCusTradeGroupParentPK_GSPPlus, '2027', 'GSP+', '1900-01-01 12:00:00', '2079-06-06 23:59:00', 'EUN')

				--RefCusTradeGroupCountry //Adding VN and CG to GSP
				INSERT RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate, ZZB_EndDate) VALUES (NEWID(), @RefCusTradeGroupParentPK_GSP, 'VN', '1900-01-01 12:00:00', '2079-06-06 23:59:00')
				INSERT RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate, ZZB_EndDate) VALUES (NEWID(), @RefCusTradeGroupParentPK_GSP, 'CG', '1900-01-01 12:00:00', '2079-06-06 23:59:00')

				--RefCusTradeGroupCountry //Adding LK and BO to GSP+
				INSERT RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate, ZZB_EndDate) VALUES (NEWID(), @RefCusTradeGroupParentPK_GSPPlus, 'LK', '1900-01-01 12:00:00', '2079-06-06 23:59:00')
				INSERT RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate, ZZB_EndDate) VALUES (NEWID(), @RefCusTradeGroupParentPK_GSPPlus, 'BO', '1900-01-01 12:00:00', '2079-06-06 23:59:00')";

			((IDbConnected)Factory).Connection.ExecuteNonQuery(refDbArrangmentScript);
			Factory.Save();

			Assert(Constants.CountryCodes.VietNam, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.VietNam).IsPartOfEuOrGspPlusOCT);
			Assert(Constants.CountryCodes.Congo, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Congo).IsPartOfEuOrGspPlusOCT);
			Assert(Constants.CountryCodes.SriLanka, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.SriLanka).IsPartOfEuOrGspPlusOCT);
			Assert(Constants.CountryCodes.Bolivia, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Bolivia).IsPartOfEuOrGspPlusOCT);
			Assert(Constants.CountryCodes.Italy, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Italy).IsPartOfEuOrGspPlusOCT);
			Assert(Constants.CountryCodes.UnitedStates, !RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedStates).IsPartOfEuOrGspPlusOCT);
		}

		#endregion

		#region Address Validation

		public void TestShouldUseAddressValidation()
		{
			var australiaPK = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).PK.ToGuid();
			var indonesiaPK = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "ID")).PK.ToGuid();

			Env.Instance.Registry.EnableAddressValidationWebService = false;
			AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(australiaPK, AddressValidationSection.AdminPanel));
			AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(indonesiaPK, AddressValidationSection.AdminPanel));

			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			AssertEquals(true, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(australiaPK, AddressValidationSection.AdminPanel));
			AssertEquals(true, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(indonesiaPK, AddressValidationSection.AdminPanel));

			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(indonesiaPK, disabledForAdminPanel: true));
			AssertEquals(true, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(australiaPK, AddressValidationSection.AdminPanel));
			AssertEquals(false, OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(indonesiaPK, AddressValidationSection.AdminPanel));
		}

		#endregion

		#region Holidays

		public void TestWeekendHolidays()
		{
			var bizo = Factory.NewWithValidTestData<RefCountry>();
			Assert(!bizo.Weekends.Any());

			//defaults
			Assert(bizo.IsMondayNonWorkingDay == false);
			Assert(bizo.IsTuesdayNonWorkingDay == false);
			Assert(bizo.IsWednesdayNonWorkingDay == false);
			Assert(bizo.IsThursdayNonWorkingDay == false);
			Assert(bizo.IsFridayNonWorkingDay == false);
			Assert(bizo.IsSaturdayNonWorkingDay == false);
			Assert(bizo.IsSundayNonWorkingDay == false);

			bizo.IsMondayNonWorkingDay = true;
			Assert("Contains all week days in collection", bizo.Weekends.Count == 7);
			Assert("Other weekdays besides MON are working day", bizo.Weekends.Count(x => x.GH_IsWorkingDay == true) == 6);
			Assert("MON is non-working day", !bizo.Weekends.First(x => x.GH_RecurrDay == AutoDayOfWeekCodeList.Codes.Monday).GH_IsWorkingDay);
		}

		#endregion

		#region ComplianceRules

		public void TestComplianceRules()
		{
			Env.Security.CountriesManageSanctions.IsAllowed = false;
			var country = Factory.New<RefCountry>();
			var rules = (country.ComplianceRules as BusinessObjectCollection);
			AssertEquals(true, rules.ReadOnly);

			Env.Security.CountriesManageSanctions.IsAllowed = true;
			country = Factory.New<RefCountry>();
			rules = (country.ComplianceRules as BusinessObjectCollection);
			AssertEquals(false, rules.ReadOnly);
		}

		#endregion

		#region Implementation

		BusinessObjectFactory TestFactory;
		RefCountry Country;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();
			Country = TestFactory.New(typeof(RefCountry)) as RefCountry;
		}

		#endregion
	}
}
