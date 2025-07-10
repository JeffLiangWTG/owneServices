using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Customs.ZA;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSupplierBuyerLinkValidationTestCase : BusinessObjectValidationTestCase
	{
		public void TestCheckOL_EFreightStatus()
		{
			Link.OL_EFreightStatus = "XXX";
			AssertHasError(Link.OL_EFreightStatusInfo, "Enter a valid e-freight Status.");

			Link.OL_EFreightStatus = "NON";
			AssertNoErrors(Link.OL_EFreightStatusInfo);

			Link.OL_EFreightStatus = "";
			AssertNoErrors(Link.OL_EFreightStatusInfo);
		}

		public void TestOL_ProductRelation()
		{
			Link.OL_ProductRelation = "XX";
			AssertHasError(Link.OL_ProductRelationInfo, "Enter a valid selection.");
		}

		public void TestCheckOL_AuthorityToLeave()
		{
			Link.OL_AuthorityToLeave = "XXX";
			AssertHasError(Link.OL_AuthorityToLeaveInfo, "Enter a valid Authority To Leave.");

			Link.OL_AuthorityToLeave = "DEF";
			AssertNoErrors("DEF is a valid Authority To Leave option.", Link.OL_AuthorityToLeaveInfo);

			Link.OL_AuthorityToLeave = "YES";
			AssertNoErrors("YES is a valid Authority To Leave option.", Link.OL_AuthorityToLeaveInfo);

			Link.OL_AuthorityToLeave = "NO";
			AssertNoErrors("NO is a valid Authority To Leave option.", Link.OL_AuthorityToLeaveInfo);

			Link.OL_AuthorityToLeave = "";
			AssertHasError(Link.OL_AuthorityToLeaveInfo, "Please enter an Authority To Leave.");
		}

		public void TestCheckOL_RN_NKImporterCountry()
		{
			Link.OL_RN_NKImporterCountry = "AU";
			AssertNoErrors("Transport Mode should be valid", Link.OL_RN_NKImporterCountryInfo);

			Link.OL_RN_NKImporterCountry = "";
			AssertHasErrors("Transport Mode should NOT be valid", Link.OL_RN_NKImporterCountryInfo);

			Link.OL_RN_NKImporterCountry = Link.Countries[0].RN_Code;
			AssertNoErrors("Transport Mode should be valid", Link.OL_RN_NKImporterCountryInfo);
		}

		public void TestCheckOL_RelatedPartyHasRelatedIndicator()
		{
			Link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.SouthAfrica;

			Link.OL_RelatedParty = RelatedIndicatorList.Codes.Yes;
			Link.OL_ValuationBasis = ZString.Empty;
			AssertHasMessageErrorContaining(Link.OL_ValuationBasisInfo, ValuationCodeListValidation.ValuationCodeRequired);
			Link.OL_ValuationBasis = ValuationCodeList.Codes.Section1;
			AssertNoNotifications(Link.OL_ValuationBasisInfo);

			Link.OL_RelatedParty = RelatedIndicatorList.Codes.No;
			Link.OL_ValuationBasis = ValuationCodeList.Codes.Section1;
			AssertNoNotifications(Link.OL_ValuationBasisInfo);
			Link.OL_ValuationBasis = ZString.Empty;
			AssertHasMessageErrorContaining(Link.OL_ValuationBasisInfo, ValuationCodeListValidation.ValuationCodeRequired);

			Link.OL_RelatedParty = RelatedIndicatorList.Codes.Exempt;
			Link.OL_ValuationBasis = ValuationCodeList.Codes.Section1;
			AssertHasMessageErrorContaining(Link.OL_ValuationBasisInfo, ValuationCodeListValidation.ValuationCodeIsNowAllowed);
		}

		public void TestCheckOL_RX_NKDefaultCurrency()
		{
			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_RX_NKDefaultCurrency = "XXX";
			AssertHasErrors("Invalid Currency", link.OL_RX_NKDefaultCurrencyInfo);

			link.OL_RX_NKDefaultCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertNoErrors("Valid Currency", link.OL_RX_NKDefaultCurrencyInfo);
		}

		public void TestDuplicateRelationship()
		{
			Link.OL_OH_Supplier = Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "ABABEU").PK;
			Link.OL_RN_NKImporterCountry = "AU";

			OrgSupplierBuyerLink link2 = Organisation.SupplierLinks.AddNew();
			link2.OL_OH_Supplier = Link.OL_OH_Supplier;
			link2.OL_RN_NKImporterCountry = "NZ";

			Assert("Link should have no errors", !Link.HasRowErrors);
			Assert("Link2 should have no errors", !link2.HasRowErrors);

			link2.OL_RN_NKImporterCountry = "AU";

			Assert("Link should have no errors", !Link.HasRowErrors);
			Assert("Link2 should have errors", link2.HasRowErrors);
		}

		public void TestCheckOL_RelatedParty()
		{
			Link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.SouthAfrica;
			Link.OL_RelatedParty = "";
			AssertNoErrors(Link.OL_RelatedPartyInfo);
			Link.OL_RelatedParty = "ZZA";
			AssertHasErrors(Link.OL_RelatedPartyInfo);

			Link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			Link.OL_RelatedParty = Customs.RelatedPartyList.Codes.Related;
			AssertNoErrors(Link.OL_RelatedPartyInfo);
			Link.OL_RelatedParty = Customs.RelatedPartyList.Codes.Unrelated;
			AssertNoErrors(Link.OL_RelatedPartyInfo);
			Link.OL_RelatedParty = "ZZA";
			AssertHasErrors(Link.OL_RelatedPartyInfo);

			Link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.NewZealand;
			Link.OL_RelatedParty = Customs.RelatedPartyList.Codes.Related;
			AssertNoErrors(Link.OL_RelatedPartyInfo);
			Link.OL_RelatedParty = Customs.RelatedPartyList.Codes.Unrelated;
			AssertNoErrors(Link.OL_RelatedPartyInfo);
			Link.OL_RelatedParty = "ZZA";
			AssertHasErrors(Link.OL_RelatedPartyInfo);

			Link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
			Link.OL_RelatedParty = Customs.RelatedPartyList.Codes.Related;
			AssertNoErrors(Link.OL_RelatedPartyInfo);
			Link.OL_RelatedParty = Customs.RelatedPartyList.Codes.Unrelated;
			AssertNoErrors(Link.OL_RelatedPartyInfo);
			Link.OL_RelatedParty = "ZZA";
			AssertHasErrors(Link.OL_RelatedPartyInfo);
		}

		public void TestCheckOL_ValuationBasisDeterminationNum()
		{
			Link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.SouthAfrica;
			Organisation.OH_IsConsignor = true;
			Link.OL_ValuationBasisDeterminationNum = "123";
			Assert("VDN Has Warning", Link.OL_ValuationBasisDeterminationNumInfo.HasWarnings());
			Assert("VDN Has No Error", !Link.OL_ValuationBasisDeterminationNumInfo.HasErrors());
			Link.OL_ValuationBasisDeterminationNum = "";
			AssertNoNotifications(Link.OL_ValuationBasisDeterminationNumInfo);

			Link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			Link.OL_ValuationBasisDeterminationNum = "123";
			Assert("VDN Has No Warning", !Link.OL_ValuationBasisDeterminationNumInfo.HasWarnings());
			Assert("VDN Has Error", Link.OL_ValuationBasisDeterminationNumInfo.HasErrors());
			Link.OL_ValuationBasisDeterminationNum = "";
			AssertNoNotifications(Link.OL_ValuationBasisDeterminationNumInfo);

			//check on EU\country if available
			Link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Netherlands;
			Organisation.OH_IsConsignor = true;
			Link.OL_ValuationBasisDeterminationNum = "123";
			Assert("VDN Has Warning", Link.OL_ValuationBasisDeterminationNumInfo.HasWarnings());
			Assert("VDN Has No Error", !Link.OL_ValuationBasisDeterminationNumInfo.HasErrors());
			Link.OL_ValuationBasisDeterminationNum = "";
			AssertNoNotifications(Link.OL_ValuationBasisDeterminationNumInfo);
		}

		void AssertShipmentMonths(string testInput, bool expectedResult)
		{
			Link.ExpectedShipmentMonthsAddition = testInput;
			AssertEquals("Test validity for " + testInput, expectedResult, !Link.HasErrors);
		}

		public void TestValidateExpectedShipmentMonthsAddition()
		{
			Organisation.OH_IsConsignor = true;
			Link.OL_OH_Supplier = Organisation.PK;
			Link.UpdateShipmentDate = true;
			AssertShipmentMonths("0", true);
			AssertShipmentMonths("3", true);
			AssertShipmentMonths("10", true);
			AssertShipmentMonths("12", true);
			AssertShipmentMonths("", true);
			AssertShipmentMonths("-1", false);
			AssertShipmentMonths("13", false);
			AssertShipmentMonths("s", false);
			AssertShipmentMonths("1d", false);
			AssertShipmentMonths("d4", false);
			AssertShipmentMonths("	", true);
			AssertShipmentMonths("+", false);
		}

		public void TestValidateExpectedShipmentMonthsAdditionNotCalledOnValidateAll()
		{
			OrgHeader consignor = OrgHeader.New(Factory);
			consignor.OH_IsConsignor = true;
			Organisation.SupplierLinks.RemoveAndDeleteAll();

			Organisation.OH_IsConsignee = true;
			OrgSupplierBuyerLink link = Organisation.SupplierLinks.AddNew();
			link.OL_OH_Supplier = consignor.PK;

			AssertEquals("No errors on Link", false, link.HasErrors);

			link.Validation.ValidateAll();
			AssertEquals("No errors on Link", false, link.HasErrors);

			link.ExpectedShipmentMonthsAddition = "33";
			AssertEquals("Errors on link", true, link.HasErrors);
		}

		public void TestGetExistingOrgSupplierBuyerLink()
		{
			OrgHeader buyer = OrgHeader.New(Factory);
			buyer.OH_RL_NKClosestPort = "AUSYD";
			OrgHeader supplier = OrgHeader.New(Factory);
			OrgSupplierBuyerLink link = buyer.SupplierLinks.AddNew(supplier);

			AssertEquals("Link", link, OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, buyer, ZString.Empty));
			AssertEquals("Link", link, OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, buyer, "AU"));
			AssertEquals("Link", link, OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, buyer, "US"));
			buyer.OH_RL_NKClosestPort = "USLAX";
			AssertNull(OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, buyer, ZString.Empty));
			AssertNull(OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, buyer, "US"));
			AssertEquals("Link", link, OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, buyer, "AU"));
		}

		public void TestGetExistingOrgSupplierBuyerLink2()
		{
			OrgHeader buyer = OrgHeader.New(Factory);
			buyer.OH_RL_NKClosestPort = "AUSYD";
			OrgHeader supplier = OrgHeader.New(Factory);
			OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew(buyer);

			AssertEquals("Link", link, OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, buyer, ZString.Empty));
			AssertEquals("Link", link, OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, buyer, "AU"));
			AssertEquals("Link", link, OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, buyer, "US"));
			buyer.OH_RL_NKClosestPort = "USLAX";
			AssertNull(OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, buyer, ZString.Empty));
			AssertNull(OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, buyer, "US"));
			AssertEquals("Link", link, OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, buyer, "AU"));
		}

		public void TestOL_ValuationBasis_List()
		{
			Link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			Link.OL_ValuationBasis = Customs.AU.ValuationBasisList.Codes._1stPref_TransactionValue;
			Assert("Valuation Basis has no errors for AU", !Link.OL_ValuationBasisInfo.HasErrors());

			Link.OL_ValuationBasis = "asd";
			Assert("Valuation Basis has errors for bad input", Link.OL_ValuationBasisInfo.HasErrors());

			Link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Guadeloupe;
			Link.OL_ValuationBasis = "";
			Assert("Valuation Basis has no errors for Guadeloupe", !Link.OL_ValuationBasisInfo.HasErrors());
		}

		public void TestValuationBasisListIsUniqueToEachCountry()
		{
			CheckListsAreSameForCountry("AU", new Customs.AU.ValuationBasisList());
			CheckListsAreSameForCountry("ZA", new ValuationCodeList());
			CheckListsAreSameForCountry("NL", new Customs.EU.ValuationMethodList());
		}

		public void TestCheckOL_OH_ControllingCustomer_EnableControllingCustomerFunctionalityAndValidationsIsTrue()
		{
			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Link.OL_OH_ControllingCustomer = BuyerSupplier.PK;
			BuyerSupplier.OH_IsControllingCustomer = false;

			Link.Validation.ValidateOL_OH_ControllingCustomer();
			AssertHasError(Link.OL_OH_ControllingCustomerInfo, "Only an organization flagged as Controlling Customer can be used as a Controlling Customer.");

			BuyerSupplier.OH_IsControllingCustomer = true;
			Link.Validation.ValidateOL_OH_ControllingCustomer();
			AssertNoErrors(Link.OL_OH_ControllingCustomerInfo);
		}

		public void TestCheckOL_OH_ControllingCustomer_EnableControllingCustomerFunctionalityAndValidationsIsFalse()
		{
			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Link.OL_OH_ControllingCustomer = BuyerSupplier.PK;
			BuyerSupplier.OH_IsControllingCustomer = false;

			Link.Validation.ValidateOL_OH_ControllingCustomer();
			AssertNoErrors(Link.OL_OH_ControllingCustomerInfo);

			BuyerSupplier.OH_IsControllingCustomer = true;
			Link.Validation.ValidateOL_OH_ControllingCustomer();
			AssertNoErrors(Link.OL_OH_ControllingCustomerInfo);
		}

		public void TestWarningOnVDNWhenIsConsigneeButNoImporterCodeIsSpecified()
		{
			OrgHeader consignee = OrgHeader.New(Factory);
			consignee.OH_Code = "EDI";
			consignee.OH_IsConsignee = true;
			OrgSupplierBuyerLink consigneeLink = consignee.SupplierLinks.AddNew();
			consigneeLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.SouthAfrica;
			consigneeLink.OL_ValuationBasisDeterminationNum = "123";
			AssertHasWarnings(consigneeLink.OL_ValuationBasisDeterminationNumInfo);

			OrgCusCode cusCode = consignee.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode; //Importer Code
			cusCode.OK_CustomsRegNo = "555";
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			consigneeLink.OL_ValuationBasisDeterminationNum = "123"; //Invoking Validation
			AssertNoWarnings(consigneeLink.OL_ValuationBasisDeterminationNumInfo);
		}

		public void TestWarningOnVDNWhenIsConsignorButNoSupplierCodeIsSpecified()
		{
			OrgHeader consignor = OrgHeader.New(Factory);
			consignor.OH_Code = "EDI";
			consignor.OH_IsConsignor = true;
			OrgSupplierBuyerLink consignorLink = consignor.BuyerLinks.AddNew();
			consignorLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.SouthAfrica;
			consignorLink.OL_ValuationBasisDeterminationNum = "123";
			AssertHasWarnings(consignorLink.OL_ValuationBasisDeterminationNumInfo);

			OrgCusCode cusCode = consignor.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode; //Supplier Code
			cusCode.OK_CustomsRegNo = "555";
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			consignorLink.OL_ValuationBasisDeterminationNum = "123"; //Invoking Validation
			AssertNoWarnings(consignorLink.OL_ValuationBasisDeterminationNumInfo);
		}

		void CheckListsAreSameForCountry(string countryCode, CodeDescriptionPairList expectedList)
		{
			Link.OL_RN_NKImporterCountry = countryCode;
			Assert("Expected List MUST be > 0", expectedList.Count > 0);
			AssertEquals("Expected List MUST have same count as Link.List", expectedList.Count, Link.OL_ValuationBasis_List.Count);

			for (int i = 0; i < expectedList.Count; i++)
			{
				AssertEquals("Code in list for Country " + countryCode + " is invalid", expectedList[i].Code, Link.OL_ValuationBasis_List[i].Code);
				AssertEquals("Desc in list for Country " + countryCode + " is invalid", expectedList[i].Description, Link.OL_ValuationBasis_List[i].Description);
				AssertEquals("PK in list for Country " + countryCode + " is invalid", expectedList[i].PK, Link.OL_ValuationBasis_List[i].PK);
			}
		}

		public void TestResetExpectedShipmentMonthsAddition()
		{
			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			link.ExpectedShipmentMonthsAddition = "ZZ";
			link.OL_InitialShipmentExpected = ZDateTime.Empty;
			link.ResetExpectedShipmentMonthsAddition();
			AssertEquals("The Shipment Months should be 3", "3", link.ExpectedShipmentMonthsAddition);

			link.OL_InitialShipmentExpected = ZDateTime.Now;
			link.ResetExpectedShipmentMonthsAddition();
			AssertEquals("The Shipment Months should be BLANK", "", link.ExpectedShipmentMonthsAddition);
		}

		public void TestCheckOL_ValuationBasisMarkupPercent()
		{
			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			AssertNoErrors("Precondition - no errors", link.OL_ValuationBasisMarkupPercentInfo);

			link.OL_ValuationBasisMarkupPercent = 0;
			AssertNoErrors("Valid %", link.OL_ValuationBasisMarkupPercentInfo);

			link.OL_ValuationBasisMarkupPercent = 999;
			AssertNoErrors("Valid %", link.OL_ValuationBasisMarkupPercentInfo);

			link.OL_ValuationBasisMarkupPercent = 50;
			AssertNoErrors("Valid %", link.OL_ValuationBasisMarkupPercentInfo);

			link.OL_ValuationBasisMarkupPercent = -1;
			AssertHasErrors("Invalid %", link.OL_ValuationBasisMarkupPercentInfo);

			link.OL_ValuationBasisMarkupPercent = 1000;
			AssertHasErrors("Invalid %", link.OL_ValuationBasisMarkupPercentInfo);
		}

		#region Implementation

		OrgSupplierBuyerLink Link;
		OrgHeader BuyerSupplier;
		OrgHeader Organisation;

		protected override void SetUp()
		{
			base.SetUp();
			Organisation = Factory.New<OrgHeader>();
			Organisation.OH_IsConsignee = true;
			BuyerSupplier = Factory.New<OrgHeader>();
			BuyerSupplier.OH_RL_NKClosestPort = "AUSYD";

			Link = Organisation.SupplierLinks.AddNew();
		}

		#endregion
	}
}
