using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USOrganisationDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckOrganisationPK()
		{
			var org = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Importer = org.PK;

			var ultimateConsigneeDocAddress = invoice.UltimateConsigneeDocAddress;
			ultimateConsigneeDocAddress.OrganisationPK = org2.PK;
			ultimateConsigneeDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertNoMessageErrorContaining(ultimateConsigneeDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			ultimateConsigneeDocAddress.OrganisationPK = ZGuid.Empty;
			Assert(invoice.JZ_OH_Buyer.IsEmpty);
			AssertHasMessageErrorContaining(ultimateConsigneeDocAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			var supplierPickupAddress = invoice.SupplierPickupAddress;
			supplierPickupAddress.OrganisationPK = org2.PK;
			supplierPickupAddress.E2_OA_Address = org.MainAddress.PK;
			AssertNoMessageErrorContaining(supplierPickupAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			supplierPickupAddress.OrganisationPK = ZGuid.Empty;
			AssertHasMessageErrorContaining(supplierPickupAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			AssertUSPPIMustHaveEIN(invoice, Core.Constants.CountryCodes.UnitedStates);
			AssertUSPPIMustHaveEIN(invoice, Core.Constants.CountryCodes.PuertoRico);
			AssertUSPPIMustHaveEIN(invoice, Core.Constants.CountryCodes.VirginIslands);

			AssertUSPPIMustHaveEitherEINOrFRNOrDuns(invoice, Core.Constants.CountryCodes.Australia);
			AssertUSPPIMustHaveEitherEINOrFRNOrDuns(invoice, Core.Constants.CountryCodes.Canada);
			AssertUSPPIMustHaveEitherEINOrFRNOrDuns(invoice, Core.Constants.CountryCodes.SouthAfrica);
		}

		void AssertUSPPIMustHaveEIN(JobComInvoiceHeader invoice, ZString countryCode)
		{
			var org3 = Factory.New<OrgHeader>();
			org3.MainAddress.OA_RN_NKCountryCode = countryCode;
			org3.CustomsCodes.RemoveAndDeleteAll();
			invoice.JZ_OH_Supplier = org3.PK;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEIN);

			var cusCode = org3.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123");
			invoice.JZ_OH_Supplier = org3.PK;
			AssertNoMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEIN);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ForeignRegistrationNumber;
			invoice.JZ_OH_Supplier = org3.PK;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEIN);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			invoice.JZ_OH_Supplier = org3.PK;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEIN);

			org3.CustomsCodes.RemoveAndDeleteAll();
			org3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123456789");
			invoice.JZ_OH_Supplier = org3.PK;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEIN);
		}

		void AssertUSPPIMustHaveEitherEINOrFRNOrDuns(JobComInvoiceHeader invoice, ZString countryCode)
		{
			var org3 = Factory.New<OrgHeader>();
			org3.MainAddress.OA_RN_NKCountryCode = countryCode;
			org3.CustomsCodes.RemoveAndDeleteAll();
			invoice.JZ_OH_Supplier = org3.PK;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEitherEINOrFRNOrDuns);

			var cusCode = org3.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123");
			invoice.JZ_OH_Supplier = org3.PK;
			AssertNoMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEitherEINOrFRNOrDuns);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ForeignRegistrationNumber;
			invoice.JZ_OH_Supplier = org3.PK;
			AssertNoMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEitherEINOrFRNOrDuns);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			invoice.JZ_OH_Supplier = org3.PK;
			AssertHasMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEitherEINOrFRNOrDuns);

			org3.CustomsCodes.RemoveAndDeleteAll();
			org3.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123456789");
			invoice.JZ_OH_Supplier = org3.PK;
			AssertNoMessageError(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.USPPIMustHaveEitherEINOrFRNOrDuns);
		}

		public void TestAlphabeticCity()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "3234", Core.Constants.CountryCodes.UnitedStates);
			org.MainAddress.OA_City = "SYDNEY!";
			var address2 = org.Addresses.AddNew();
			address2.OA_City = "!";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;

			invoice.SupplierPickupAddress.E2_OA_Address = address2.PK;
			AssertHasMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);

			invoice.SupplierPickupAddress.E2_OA_Address = org.MainAddress.PK;
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);

			invoice.UltimateConsigneeDocAddress.E2_OA_Address = address2.PK;
			AssertHasMessageError(invoice.UltimateConsigneeDocAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);

			invoice.UltimateConsigneeDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertNoMessageError(invoice.UltimateConsigneeDocAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);

			invoice.IntermediateConsigneeDocAddress.E2_OA_Address = address2.PK;
			AssertHasMessageError(invoice.IntermediateConsigneeDocAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);

			invoice.IntermediateConsigneeDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertNoMessageError(invoice.IntermediateConsigneeDocAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);
		}

		public void TestCheckSupplierPickupAddress_E2_OA_Address()
		{
			invoice.SupplierPickupAddress.E2_OA_Address = ZGuid.Empty;
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);

			helper.ShippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000", Core.Constants.CountryCodes.UnitedStates);
			OrgAddress orgAddress = helper.ShippingLine.MainAddress;
			orgAddress.OA_Address1 = "";
			orgAddress.OA_City = "";
			orgAddress.OA_RL_NKRelatedPortCode = "";
			orgAddress.OA_State = "";
			orgAddress.OA_PostCode = "";

			invoice.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
			invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.Address1Required);
			AssertHasMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);
			AssertHasMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);

			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_RL_NKRelatedPortCode = "USLAX";
			orgAddress.OA_State = "";
			invoice.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
			invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CountryRequired);
			AssertHasMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.StateRequired);
			AssertHasMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);

			orgAddress.OA_RL_NKRelatedPortCode = "MXPMX";
			orgAddress.OA_State = "";
			invoice.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
			invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CountryRequired);
			AssertHasMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);

			orgAddress.OA_RL_NKRelatedPortCode = "PRGUY";
			orgAddress.OA_State = "";
			invoice.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
			invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.StateRequired);
			AssertHasMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);

			orgAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			orgAddress.OA_State = "";
			invoice.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
			invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);

			orgAddress.OA_RL_NKRelatedPortCode = "USNYC";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "9685";
			invoice.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
			invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
			AssertHasMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);

			orgAddress.OA_PostCode = "96852";
			invoice.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
			invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
			string postalCodeValidForNY = string.Format(AESAddressValidator.PostalCodeNotValidForState, "NY", "'004' and '005', or '090' and '149'");
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);
			AssertHasMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, postalCodeValidForNY);

			string[] validPostalCodesForNY = new string[] { "00400", "00500", "09000", "14900" };
			foreach (string validPostalCodeForNY in validPostalCodesForNY)
			{
				orgAddress.OA_PostCode = validPostalCodeForNY;
				invoice.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
				invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
				AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
				AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);
				AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, postalCodeValidForNY);
			}

			orgAddress.OA_RL_NKRelatedPortCode = "PRGUY";
			orgAddress.OA_State = "NW";
			invoice.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
			invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
			string postalCodeValidForPR = string.Format(AESAddressValidator.PostalCodeNotValidForState, "PR", "'006' and '007', or '009' and '009'");
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);
			AssertHasMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, postalCodeValidForPR);

			string[] validPostalCodesForPR = new string[] { "00600", "00700", "00900", "00999" };
			foreach (string validPostalCodeForPR in validPostalCodesForPR)
			{
				orgAddress.OA_PostCode = validPostalCodeForPR;
				invoice.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
				invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
				AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
				AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);
				AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, postalCodeValidForPR);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.SupplierPickupAddress.E2_OA_Address = ZGuid.Empty;
			invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);

			var address2 = helper.ShippingLine.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = ZString.Empty;
			address2.OA_Address1 = "ADDRESS 1";
			address2.OA_City = "CITY";
			address2.OA_State = "NS";
			address2.OA_PostCode = "23430";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.SupplierPickupAddress.E2_OA_Address = address2.PK;
			invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeRequired);
			AssertNoMessageError(invoice.SupplierPickupAddress.E2_OA_AddressInfo, AESAddressValidator.PostalCodeInvalidFormat);

			address2.OA_IsActive = false;
			invoice.SupplierPickupAddress.E2_OA_Address = address2.PK;
			invoice.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageErrorContaining(invoice.SupplierPickupAddress.E2_OA_AddressInfo, "You have selected an inactive address.");
		}

		public void TestCheckUltimateConsigneeDocAddress_E2_OA_Address()
		{
			AssertCheckBasicAddressDetails(declaration, invoice, invoice.UltimateConsigneeDocAddress.OrganisationPKInfo, invoice.UltimateConsigneeDocAddress.E2_OA_AddressInfo, () => invoice.UltimateConsigneeDocAddress.Validation.ValidateE2_OA_Address());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			SetupInActiveAddress();

			invoice.UltimateConsigneeDocAddress.E2_OA_Address = SetupInActiveAddress().PK;
			AssertHasMessageErrorContaining(invoice.UltimateConsigneeDocAddress.E2_OA_AddressInfo, "You have selected an inactive address.");
		}

		public void TestCheckE2_Contact()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "1233", Core.Constants.CountryCodes.UnitedStates);
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Jason";

			var uSPPIDocAddress = invoice.USPPIDocAddress;
			uSPPIDocAddress.OrganisationPK = org.PK;
			uSPPIDocAddress.E2_OA_Address = org.MainAddress.PK;

			uSPPIDocAddress.E2_Contact = "";
			uSPPIDocAddress.Validation.ValidateE2_Contact();
			AssertHasMessageError(uSPPIDocAddress.E2_ContactInfo, ContactValidator.ContactNameRequired);
			AssertHasMessageError(invoice.USPPIDocAddress.E2_ContactInfo, ContactValidator.ContactNameRequired);
			uSPPIDocAddress.E2_Contact = "11";
			AssertHasMessageError(uSPPIDocAddress.E2_ContactInfo, ContactValidator.NameInvalid);
			AssertHasMessageError(invoice.USPPIDocAddress.E2_ContactInfo, ContactValidator.NameInvalid);
			uSPPIDocAddress.E2_Contact = "Jason";
			AssertHasMessageError(uSPPIDocAddress.E2_ContactInfo, USOrganisationDocAddressValidation.ContactPhoneRequired);
			AssertHasMessageError(invoice.USPPIDocAddress.E2_ContactInfo, USOrganisationDocAddressValidation.ContactPhoneRequired);
			uSPPIDocAddress.E2_AddressOverride = true;
			uSPPIDocAddress.E2_Contact = "";
			AssertHasMessageError(uSPPIDocAddress.E2_ContactInfo, ContactValidator.ContactNameRequired);
			AssertHasMessageError(invoice.USPPIDocAddress.E2_ContactInfo, ContactValidator.ContactNameRequired);
			uSPPIDocAddress.E2_Contact = "11";
			AssertHasMessageError(uSPPIDocAddress.E2_ContactInfo, ContactValidator.NameInvalid);
			AssertHasMessageError(invoice.USPPIDocAddress.E2_ContactInfo, ContactValidator.NameInvalid);
			uSPPIDocAddress.E2_Contact = "Jason";
			AssertNoMessageError(uSPPIDocAddress.E2_ContactInfo, USOrganisationDocAddressValidation.ContactPhoneRequired);
			AssertNoMessageError(invoice.USPPIDocAddress.E2_ContactInfo, USOrganisationDocAddressValidation.ContactPhoneRequired);
			uSPPIDocAddress.E2_Contact = "A SEIDEL";
			AssertHasMessageError(uSPPIDocAddress.E2_ContactInfo, ContactValidator.NameInvalid);
			uSPPIDocAddress.E2_Contact = "A. SEIDEL";
			AssertHasMessageError(uSPPIDocAddress.E2_ContactInfo, ContactValidator.NameInvalid);
			uSPPIDocAddress.E2_Contact = "AB SEIDEL";
			AssertNoMessageError(uSPPIDocAddress.E2_ContactInfo, ContactValidator.NameInvalid);

			org.Contacts.RemoveAll();
			contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Jane";
			contact.OC_Phone = "121555512123";

			uSPPIDocAddress.OrganisationPK = org.PK;
			uSPPIDocAddress.E2_OA_Address = org.MainAddress.PK;

			uSPPIDocAddress.E2_Contact = "Jane";
			AssertHasMessageError(uSPPIDocAddress.E2_ContactInfo, USOrganisationDocAddressValidation.ContactPhoneExactLength);

			contact.OC_Phone = "12155551212";

			uSPPIDocAddress.OrganisationPK = org.PK;
			uSPPIDocAddress.E2_OA_Address = org.MainAddress.PK;

			uSPPIDocAddress.E2_Contact = "Jane";
			AssertNoMessageErrors(uSPPIDocAddress.E2_ContactInfo);
			AssertNoMessageErrors(invoice.USPPIDocAddress.E2_ContactInfo);

			var ultimateConsigneeDocAddress = invoice.UltimateConsigneeDocAddress;
			ultimateConsigneeDocAddress.OrganisationPK = org.PK;
			ultimateConsigneeDocAddress.E2_OA_Address = org.MainAddress.PK;
			ultimateConsigneeDocAddress.E2_Contact = "";
			ultimateConsigneeDocAddress.Validation.ValidateE2_Contact();
			AssertNoMessageErrors(ultimateConsigneeDocAddress.E2_ContactInfo);
			ultimateConsigneeDocAddress.E2_Contact = "11";
			AssertHasMessageError(ultimateConsigneeDocAddress.E2_ContactInfo, ContactValidator.NameInvalid);
			ultimateConsigneeDocAddress.E2_Contact = "Jason";
			AssertNoMessageErrors(ultimateConsigneeDocAddress.E2_ContactInfo);

			var intermediateConsigneeDocAddress = invoice.IntermediateConsigneeDocAddress;
			intermediateConsigneeDocAddress.OrganisationPK = org.PK;
			intermediateConsigneeDocAddress.E2_OA_Address = org.MainAddress.PK;
			intermediateConsigneeDocAddress.E2_Contact = "";
			intermediateConsigneeDocAddress.Validation.ValidateE2_Contact();
			AssertNoMessageErrors(intermediateConsigneeDocAddress.E2_ContactInfo);
			intermediateConsigneeDocAddress.E2_Contact = "11";
			AssertHasMessageError(intermediateConsigneeDocAddress.E2_ContactInfo, ContactValidator.NameInvalid);
			intermediateConsigneeDocAddress.E2_Contact = "Jason";
			AssertNoMessageErrors(intermediateConsigneeDocAddress.E2_ContactInfo);
		}

		public void TestCheckE2_Phone_Formatted()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var uSPPIDocAddress = invoice.USPPIDocAddress;
			uSPPIDocAddress.E2_AddressOverride = true;
			uSPPIDocAddress.E2_Phone_FormattedInfo.ClearAllNotifications();

			uSPPIDocAddress.Validation.ValidateE2_Phone_Formatted();
			AssertHasMessageError(uSPPIDocAddress.E2_Phone_FormattedInfo, USOrganisationDocAddressValidation.ContactPhoneRequired);

			uSPPIDocAddress.E2_Phone_Formatted = "57984578345623424";
			AssertHasMessageError(uSPPIDocAddress.E2_Phone_FormattedInfo, USOrganisationDocAddressValidation.ContactPhoneExactLength);

			uSPPIDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			uSPPIDocAddress.E2_Phone_Formatted = "579845783456224";
			AssertNoMessageError(uSPPIDocAddress.E2_Phone_FormattedInfo, USOrganisationDocAddressValidation.ContactPhoneExactLength);

			uSPPIDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSPPIDocAddress.E2_Phone_Formatted = "5798-4578";
			AssertHasMessageError(uSPPIDocAddress.E2_Phone_FormattedInfo, USOrganisationDocAddressValidation.ContactPhoneExactLength);

			uSPPIDocAddress.E2_Phone_Formatted = "61 5798-4578";
			AssertNoMessageError(uSPPIDocAddress.E2_Phone_FormattedInfo, USOrganisationDocAddressValidation.ContactPhoneExactLength);
		}

		public void TestUnmatchedOrganization()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			invoice.UltimateConsigneeDocAddress.OrganisationPK = orgHeader.PK;
			AssertNoMessageErrorContaining(invoice.UltimateConsigneeDocAddress.OrganisationPKInfo, "We recommend selecting an existing Org");

			var unmatchedOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "UNMATCHED"));
			AssertNotNull("Precondition", unmatchedOrg);

			invoice.UltimateConsigneeDocAddress.OrganisationPK = unmatchedOrg.PK;
			AssertHasMessageErrorContaining(invoice.UltimateConsigneeDocAddress.OrganisationPKInfo, "We recommend selecting an existing Org");
		}

		void AssertCheckBasicAddressDetails(JobDeclaration declaration, JobComInvoiceHeader invoice, ZPropertyInfo organisationPKInfo, ZPropertyInfo orgAddressInfo, Action validateAddressAction)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			orgAddressInfo.Value = ZGuid.Empty;
			validateAddressAction();
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.PostalCodeRequired);

			OrgAddress orgAddress = helper.ShippingLine.MainAddress;
			orgAddress.OA_Address1 = "";
			orgAddress.OA_City = "";
			orgAddress.OA_RL_NKRelatedPortCode = "";
			orgAddress.OA_State = "";
			orgAddress.OA_PostCode = "";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			organisationPKInfo.Value = helper.ShippingLine.PK;
			orgAddressInfo.Value = orgAddress.PK;
			validateAddressAction();
			AssertHasMessageError(orgAddressInfo, AESAddressValidator.Address1Required);
			AssertHasMessageError(orgAddressInfo, AESAddressValidator.CityRequired);
			AssertHasMessageError(orgAddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.PostalCodeRequired);

			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_RL_NKRelatedPortCode = "USLAX";
			orgAddress.OA_State = "";
			orgAddressInfo.Value = orgAddress.PK;
			validateAddressAction();
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.CountryRequired);
			AssertHasMessageError(orgAddressInfo, AESAddressValidator.StateRequired);
			AssertHasMessageError(orgAddressInfo, AESAddressValidator.PostalCodeRequired);

			orgAddress.OA_RL_NKRelatedPortCode = "MXPMX";
			orgAddress.OA_State = "";
			orgAddressInfo.Value = orgAddress.PK;
			validateAddressAction();
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.CountryRequired);
			AssertHasMessageError(orgAddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.PostalCodeRequired);

			orgAddress.OA_RL_NKRelatedPortCode = "PRGUY";
			orgAddress.OA_State = "";
			orgAddressInfo.Value = orgAddress.PK;
			validateAddressAction();
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.StateRequired);
			AssertHasMessageError(orgAddressInfo, AESAddressValidator.PostalCodeRequired);

			orgAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			orgAddress.OA_State = "";
			orgAddressInfo.Value = orgAddress.PK;
			validateAddressAction();
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.PostalCodeRequired);

			orgAddress.OA_RL_NKRelatedPortCode = "USNYC";
			orgAddress.OA_State = "NY";
			orgAddress.OA_PostCode = "12312";
			orgAddressInfo.Value = orgAddress.PK;
			validateAddressAction();
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.Address1Required);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.CityRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.CountryRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.StateRequired);
			AssertNoMessageError(orgAddressInfo, AESAddressValidator.PostalCodeRequired);
		}

		OrgAddress SetupInActiveAddress()
		{
			var address = helper.ShippingLine.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = ZString.Empty;
			address.OA_Address1 = "ADDRESS 1";
			address.OA_City = "CITY";
			address.OA_State = "NS";
			address.OA_PostCode = "23430";
			address.OA_IsActive = false;
			return address;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice = declaration.Invoices.AddNew();
			helper = new DeclarationTestHelper(Factory);
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		DeclarationTestHelper helper;

		#endregion
	}
}
