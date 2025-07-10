using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ControllingMessageHeaderSupplierAddressRequirement))]
	sealed class ControllingMessageHeaderSupplierAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckOrganisationPK_NX101_Supplier()
		{
			var companyNameMessage = "You have not entered a Supplier Company Name.";
			var localCompanyNameMessage = "You have not entered a Supplier Local Company Name.";
			var addressMessage = "You have not entered a Supplier Address.";
			var localAddressMessage = "You have not entered a Supplier Local Address.";

			var orgHeader = Factory.New<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			var taiwan = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Taiwan);
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			var validation = supplierDocumentaryAddress.Validation;
			validation.ValidateOrganisationPK();
			CombineAssertions("Organisation is null", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, companyNameMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, localCompanyNameMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, addressMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, localAddressMessage);
			});

			supplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			CombineAssertions("Organisation has value", () =>
			{
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, companyNameMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, localCompanyNameMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, addressMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, localAddressMessage);
			});
		}

		public void TestCheckOrganisationPK_NX101_SupplierWithCertificate()
		{
			var localCompanyNameMessage = "You have not entered a Supplier Local Company Name.";
			var localAddressMessage = "You have not entered a Supplier Local Address.";
			var supplierContactEmailMessage = "You have not entered a Supplier Contact Email.";
			var supplierContactFaxMessage = "You have not entered a Supplier Contact Fax.";
			var supplierContactPhoneMessage = "You have not entered a Supplier Contact Phone.";
			var supplierIDIsReruiredMessage = "You have not entered a Supplier ID: A valid TW-VAT or TW-PAS or TW-PID number is required for Supplier. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration.";
			var supplierIdShouldBeSameAsApplicantIdMessage = "Please make sure the Supplier ID is same as Applicant.";
			var orgHeader1 = Factory.New<OrgHeader>();
			var mainAddress1 = orgHeader1.MainAddress;
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.OrganisationPK = orgHeader1.PK;
			var applicantDocumentaryAddress = cMHeader.ApplicantDocumentaryAddress;
			applicantDocumentaryAddress.E2_AddressOverride = true;
			applicantDocumentaryAddress.IDCode = "12345670";
			CombineAssertions("Organisation has value", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, localCompanyNameMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, localAddressMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactEmailMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			CombineAssertions("CertificateType is 9", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactFaxMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactPhoneMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			CombineAssertions("CertificateType is 11", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactFaxMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactPhoneMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			CombineAssertions("CertificateType is 13", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactFaxMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactPhoneMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			CombineAssertions("CertificateType is 14", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactFaxMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactPhoneMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			CombineAssertions("CertificateType is 15", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactFaxMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactPhoneMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactEmailMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, localCompanyNameMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, localAddressMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			CombineAssertions("CertificateType is 18", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactFaxMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactPhoneMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			CombineAssertions("CertificateType is 1", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIdShouldBeSameAsApplicantIdMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code16;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			CombineAssertions("CertificateType is 16", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIdShouldBeSameAsApplicantIdMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code19;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			CombineAssertions("CertificateType is 19", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactFaxMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactPhoneMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var orgHeader2 = Factory.New<OrgHeader>();
			var mainAddress2 = orgHeader2.MainAddress;
			mainAddress2.OA_Phone = "0288858888";
			mainAddress2.OA_Fax = "0288858889";
			mainAddress2.OA_Email = "XX1@GMAIL.COM";
			var translatedAddress = mainAddress2.TranslatedAddresses.AddNew();
			translatedAddress.Language = Enterprise.Core.Constants.Languages.ChineseTraditional;
			translatedAddress.CompanyName = "公司名稱";
			translatedAddress.Address1 = "公司地址";
			supplierDocumentaryAddress.OrganisationPK = orgHeader2.PK;
			CombineAssertions("Organisation has Chinese traditional translated address", () =>
			{
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, localCompanyNameMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, localAddressMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactEmailMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
			});

			var taiwan = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Taiwan);
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			CombineAssertions(() =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactFaxMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierContactPhoneMessage);
			});

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			CombineAssertions(() =>
			{
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIdShouldBeSameAsApplicantIdMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
			AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIdShouldBeSameAsApplicantIdMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345671");
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIdShouldBeSameAsApplicantIdMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code16;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
			AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIdShouldBeSameAsApplicantIdMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345675");
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIdShouldBeSameAsApplicantIdMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code19;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
		}

		public void TestCheckOrganisation_NX301_DN_Supplier()
		{
			var countryRegionCodeMessage = "You have not entered a Supplier Country/Region Code.";
			var orgHeader = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			var validation = supplierDocumentaryAddress.Validation;
			validation.ValidateOrganisationPK();
			AssertHasMessageError("Organisation is null", supplierDocumentaryAddress.OrganisationPKInfo, countryRegionCodeMessage);

			supplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			AssertNoMessageError("Organisation has value", supplierDocumentaryAddress.OrganisationPKInfo, countryRegionCodeMessage);

			orgHeader.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			validation.ValidateOrganisationPK();
			AssertHasMessageError("OA_RN_NKCountryCode is empty", supplierDocumentaryAddress.OrganisationPKInfo, countryRegionCodeMessage);
		}

		public void TestCheckOrganisation_NX401_Supplier()
		{
			var addressMessage = "You have not entered a Supplier Address.";
			var supplierIDIsReruiredMessage = "You have not entered a Supplier ID: A valid TW-VAT or TW-PAS or TW-PID number is required for Supplier. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration.";
			var companyNameMessage = "You have not entered a Supplier Company Name.";
			var orgHeader = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			var validation = supplierDocumentaryAddress.Validation;
			validation.ValidateOrganisationPK();
			CombineAssertions("IMP job and Organisation is null", () =>
			{
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, addressMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, companyNameMessage);
			});
			
			declaration.JE_MessageType = "EXP";
			validation.ValidateOrganisationPK();
			CombineAssertions("EXP job and Organisation is null", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, addressMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, companyNameMessage);
			});

			var address = orgHeader.MainAddress;
			address.CompanyName = "test name";
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "567", "TW");
			supplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			validation.ValidateOrganisationPK();
			CombineAssertions("Organisation has value", () =>
			{
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, addressMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, companyNameMessage);
			});

			address.CompanyName = ZString.Empty;
			address.CustomsCodes.DeleteAll();
			validation.ValidateOrganisationPK();
			CombineAssertions("CompanyName and ID are empty", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, supplierIDIsReruiredMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, companyNameMessage);
			});

			supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			validation.ValidateOrganisationPK();
			AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, addressMessage);
		}

		public void TestCheckOrganisation_NX601_Supplier()
		{
			var addressMessage = "You have not entered a Supplier Address.";
			var countryRegionCodeMessage = "You have not entered a Supplier Country/Region Code.";
			var companyNameMessage = "You have not entered a Supplier Company Name.";
			var orgHeader = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			var validation = supplierDocumentaryAddress.Validation;
			validation.ValidateOrganisationPK();
			CombineAssertions("Organisation is null", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, addressMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, countryRegionCodeMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, companyNameMessage);
			});

			var address = orgHeader.MainAddress;
			address.CompanyName = "test name";
			address.OA_RN_NKCountryCode = "US";
			supplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			CombineAssertions("Organisation has value", () =>
			{
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, addressMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, countryRegionCodeMessage);
				AssertNoMessageError(supplierDocumentaryAddress.OrganisationPKInfo, companyNameMessage);
			});

			address.CompanyName = ZString.Empty;
			address.OA_RN_NKCountryCode = ZString.Empty;
			validation.ValidateOrganisationPK();
			CombineAssertions("CompanyName and country code are empty", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, countryRegionCodeMessage);
				AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, companyNameMessage);
			});

			supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			validation.ValidateOrganisationPK();
			AssertHasMessageError(supplierDocumentaryAddress.OrganisationPKInfo, addressMessage);
		}

		public void TestSupplierIDShouldBeSameAsApplicant()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			supplierDocumentaryAddress.IDCode = "12345670";

			var applicantDocumentaryAddress = cMHeader.ApplicantDocumentaryAddress;
			applicantDocumentaryAddress.E2_AddressOverride = true;
			applicantDocumentaryAddress.IDCode = "12345670";
			var supplierIdShouldBeSameAsApplicantIdMessage = "Please make sure the Supplier ID is same as Applicant.";
			var validation = supplierDocumentaryAddress.Validation;
			validation.ValidateIDCode();
			AssertNoMessageError(supplierDocumentaryAddress.IDCodeInfo, supplierIdShouldBeSameAsApplicantIdMessage);

			supplierDocumentaryAddress.IDCode = "12345671";
			validation.ValidateIDCode();
			AssertHasMessageError(supplierDocumentaryAddress.IDCodeInfo, supplierIdShouldBeSameAsApplicantIdMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code16;
			validation.ValidateIDCode();
			AssertHasMessageError(supplierDocumentaryAddress.IDCodeInfo, supplierIdShouldBeSameAsApplicantIdMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code8;
			validation.ValidateIDCode();
			AssertNoMessageError(supplierDocumentaryAddress.IDCodeInfo, supplierIdShouldBeSameAsApplicantIdMessage);
		}

		public void TestCheckSupplierContact_Email()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var emailMessage = "You have not entered a Supplier Contact Email.";
			supplierDocumentaryAddress.Validation.ValidateE2_Email();
			AssertHasMessageError(supplierDocumentaryAddress.E2_EmailInfo, emailMessage);

			supplierDocumentaryAddress.E2_Email = "user@wtg.com";
			AssertNoMessageError(supplierDocumentaryAddress.E2_EmailInfo, emailMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			supplierDocumentaryAddress.E2_Email = ZString.Empty;
			AssertNoMessageError(supplierDocumentaryAddress.E2_EmailInfo, emailMessage);
		}

		public void TestCheckSupplierContact_PhoneOrFax()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var faxMessage = "You have not entered a Supplier Contact Fax.";
			var phoneMessage = "You have not entered a Supplier Contact Phone.";
			supplierDocumentaryAddress.Validation.ValidateE2_Fax();
			AssertHasMessageError(supplierDocumentaryAddress.E2_FaxInfo, faxMessage);
			supplierDocumentaryAddress.Validation.ValidateE2_Phone();
			AssertHasMessageError(supplierDocumentaryAddress.E2_PhoneInfo, phoneMessage);

			supplierDocumentaryAddress.E2_Fax = "0266666666";
			AssertNoMessageError(supplierDocumentaryAddress.E2_FaxInfo, faxMessage);

			supplierDocumentaryAddress.E2_Phone = "0288888888";
			AssertNoMessageError(supplierDocumentaryAddress.E2_PhoneInfo, phoneMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			supplierDocumentaryAddress.E2_Phone = ZString.Empty;
			AssertHasMessageError(supplierDocumentaryAddress.E2_PhoneInfo, phoneMessage);

			supplierDocumentaryAddress.E2_Fax = ZString.Empty;
			AssertHasMessageError(supplierDocumentaryAddress.E2_FaxInfo, faxMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			supplierDocumentaryAddress.E2_Phone = ZString.Empty;
			supplierDocumentaryAddress.Validation.ValidateE2_Phone();
			AssertHasMessageError(supplierDocumentaryAddress.E2_PhoneInfo, phoneMessage);

			supplierDocumentaryAddress.E2_Fax = ZString.Empty;
			supplierDocumentaryAddress.Validation.ValidateE2_Fax();
			AssertHasMessageError(supplierDocumentaryAddress.E2_FaxInfo, faxMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			supplierDocumentaryAddress.E2_Phone = ZString.Empty;
			supplierDocumentaryAddress.Validation.ValidateE2_Phone();
			AssertHasMessageError(supplierDocumentaryAddress.E2_PhoneInfo, phoneMessage);

			supplierDocumentaryAddress.E2_Fax = ZString.Empty;
			supplierDocumentaryAddress.Validation.ValidateE2_Fax();
			AssertHasMessageError(supplierDocumentaryAddress.E2_FaxInfo, faxMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			supplierDocumentaryAddress.E2_Phone = ZString.Empty;
			supplierDocumentaryAddress.Validation.ValidateE2_Phone();
			AssertHasMessageError(supplierDocumentaryAddress.E2_PhoneInfo, phoneMessage);

			supplierDocumentaryAddress.E2_Fax = ZString.Empty;
			supplierDocumentaryAddress.Validation.ValidateE2_Fax();
			AssertHasMessageError(supplierDocumentaryAddress.E2_FaxInfo, faxMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			supplierDocumentaryAddress.E2_Phone = ZString.Empty;
			supplierDocumentaryAddress.Validation.ValidateE2_Phone();
			AssertHasMessageError(supplierDocumentaryAddress.E2_PhoneInfo, phoneMessage);

			supplierDocumentaryAddress.E2_Fax = ZString.Empty;
			supplierDocumentaryAddress.Validation.ValidateE2_Fax();
			AssertHasMessageError(supplierDocumentaryAddress.E2_FaxInfo, faxMessage);
		}

		public void TestCheckIDCode_NX101_Supplier()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var notification = "You have not entered a Supplier ID: A valid TW-VAT or TW-PAS or TW-PID number is required for Supplier. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration.";
			var validation = supplierDocumentaryAddress.Validation;
			validation.ValidateIDCode();
			AssertHasMessageError("All IDs are empty.", supplierDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			validation.ValidateIDCode();
			AssertNoMessageError("ControllingMessageType is not NX101, all IDs are empty.", supplierDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			supplierDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			supplierDocumentaryAddress.IDCode = "12345670";
			AssertNoMessageError("VAT has value.", supplierDocumentaryAddress.IDCodeInfo, notification);

			supplierDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
			supplierDocumentaryAddress.IDCode = "12345670";
			AssertNoMessageError("PAS has value.", supplierDocumentaryAddress.IDCodeInfo, notification);

			supplierDocumentaryAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.PID;
			supplierDocumentaryAddress.IDCode = "12345670";
			AssertNoMessageError("PID has value.", supplierDocumentaryAddress.IDCodeInfo, notification);

			supplierDocumentaryAddress.IDCodeType = ZString.Empty;
			supplierDocumentaryAddress.IDCode = ZString.Empty;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			validation.ValidateIDCode();
			AssertHasMessageError("Certificate type is 11 and ID is empty.", supplierDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			validation.ValidateIDCode();
			AssertHasMessageError("Certificate type is 13 and ID is empty.", supplierDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			validation.ValidateIDCode();
			AssertHasMessageError("Certificate type is 14 and ID is empty.", supplierDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			validation.ValidateIDCode();
			AssertHasMessageError("Certificate type is 18 and ID is empty.", supplierDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			validation.ValidateIDCode();
			AssertHasMessageError("Certificate type is 1 and ID is empty.", supplierDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code16;
			validation.ValidateIDCode();
			AssertHasMessageError("Certificate type is 6 and ID is empty.", supplierDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			validation.ValidateIDCode();
			AssertHasMessageError("Certificate type is 9 and ID is empty.", supplierDocumentaryAddress.IDCodeInfo, notification);
		}

		public void TestCheckE2_Address1_NX201_01_IMP_Supplier()
		{
			var addressMessage = "You have not entered a Supplier Address.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("SupplierDocumentaryAddress - Address is empty", supplierDocumentaryAddress.E2_Address1Info, addressMessage);

			supplierDocumentaryAddress.E2_Address1 = "Company Address";
			supplierDocumentaryAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("SupplierDocumentaryAddress - Address has value", supplierDocumentaryAddress.E2_Address1Info, addressMessage);

			declaration.JE_MessageType = "EXP";
			supplierDocumentaryAddress.E2_Address1 = ZString.Empty;
			supplierDocumentaryAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("SupplierDocumentaryAddress - Export declaration job", supplierDocumentaryAddress.E2_Address1Info, addressMessage);
		}

		public void TestCheckE2_Address1_NX301_AX_Supplier()
		{
			var addressMessage = "You have not entered a Supplier Address.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_AX;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("SupplierDocumentaryAddress - Address is empty", supplierDocumentaryAddress.E2_Address1Info, addressMessage);

			supplierDocumentaryAddress.E2_Address1 = "Company Address";
			supplierDocumentaryAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("SupplierDocumentaryAddress - Address has value", supplierDocumentaryAddress.E2_Address1Info, addressMessage);
		}

		public void TestCheckE2_Address1_NX401_Supplier()
		{
			var addressMessage = "You have not entered a Supplier Address.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("SupplierDocumentaryAddress - Address is empty", supplierDocumentaryAddress.E2_Address1Info, addressMessage);

			supplierDocumentaryAddress.E2_Address1 = "Company Address";
			supplierDocumentaryAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("SupplierDocumentaryAddress - Address has value", supplierDocumentaryAddress.E2_Address1Info, addressMessage);
		}

		public void TestCheckE2_Address1_NX601_Supplier()
		{
			var addressMessage = "You have not entered a Supplier Address.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("SupplierDocumentaryAddress - Address is empty", supplierDocumentaryAddress.E2_Address1Info, addressMessage);

			supplierDocumentaryAddress.E2_Address1 = "Company Address";
			supplierDocumentaryAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("SupplierDocumentaryAddress - Address has value", supplierDocumentaryAddress.E2_Address1Info, addressMessage);
		}

		public void TestCheckE2_RN_NKCountryCode_NX201_01_IMP_Supplier()
		{
			var countryRegionCodeMessage = "You have not entered a Supplier Country/Region Code.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo.ClearValue();
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertNoMessageError("EXP: SupplierDocumentaryAddress - CountryCode is empty", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);

			declaration.JE_MessageType = "IMP";
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertHasMessageError("IMP: SupplierDocumentaryAddress - CountryCode is empty", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);

			supplierDocumentaryAddress.E2_RN_NKCountryCode = "US";
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertNoMessageError("SupplierDocumentaryAddress - CountryCode has value", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);
		}

		public void TestCheckE2_RN_NKCountryCode_NX301_Supplier()
		{
			var countryRegionCodeMessage = "You have not entered a Supplier Country/Region Code.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo.ClearValue();
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertHasMessageError("SupplierDocumentaryAddress - CountryCode is empty", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);

			supplierDocumentaryAddress.E2_RN_NKCountryCode = "US";
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertNoMessageError("SupplierDocumentaryAddress - CountryCode has value", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);
		}

		public void TestCheckE2_RN_NKCountryCode_NX301_AX_Supplier()
		{
			var countryRegionCodeMessage = "You have not entered a Supplier Country/Region Code.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_AX;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo.ClearValue();
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertHasMessageError("SupplierDocumentaryAddress - CountryCode is empty", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);

			supplierDocumentaryAddress.E2_RN_NKCountryCode = "US";
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertNoMessageError("SupplierDocumentaryAddress - CountryCode has value", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);
		}

		public void TestCheckE2_RN_NKCountryCode_NX301_DN_Supplier()
		{
			var countryRegionCodeMessage = "You have not entered a Supplier Country/Region Code.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo.ClearValue();
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertHasMessageError("SupplierDocumentaryAddress - CountryCode is empty", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);

			supplierDocumentaryAddress.E2_RN_NKCountryCode = "US";
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertNoMessageError("SupplierDocumentaryAddress - CountryCode has value", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);
		}

		public void TestCheckE2_RN_NKCountryCode_NX601_Supplier()
		{
			var countryRegionCodeMessage = "You have not entered a Supplier Country/Region Code.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo.ClearValue();
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertHasMessageError("SupplierDocumentaryAddress - CountryCode is empty", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);

			supplierDocumentaryAddress.E2_RN_NKCountryCode = "US";
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertNoMessageError("SupplierDocumentaryAddress - CountryCode has value", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);
		}

		public void TestCheckE2_RN_NKCountryCode_NX603_Supplier()
		{
			var countryRegionCodeMessage = "You have not entered a Supplier Country/Region Code.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo.ClearValue();
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertHasMessageError("SupplierDocumentaryAddress - CountryCode is empty", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);

			supplierDocumentaryAddress.E2_RN_NKCountryCode = "US";
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertNoMessageError("SupplierDocumentaryAddress - CountryCode has value", supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo, countryRegionCodeMessage);
		}

		public void TestCheckE2_CompanyName_NX301_AX_Supplier()
		{
			var companyNameMessage = "You have not entered a Supplier Company Name.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_AX;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyNameInfo.ClearValue();
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("SupplierDocumentaryAddress - Company Name is empty", supplierDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);

			supplierDocumentaryAddress.E2_CompanyName = "company name";
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertNoMessageError("SupplierDocumentaryAddress - Company Name has value", supplierDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);
		}

		public void TestCheckE2_CompanyName_NX401_EXP_Supplier()
		{
			var companyNameMessage = "You have not entered a Supplier Company Name.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyNameInfo.ClearValue();
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			AssertNoMessageError("EXP: SupplierDocumentaryAddress - Company Name is empty", supplierDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);

			declaration.JE_MessageType = "EXP";
			supplierDocumentaryAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("EXP: SupplierDocumentaryAddress - Company Name is empty", supplierDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);

			supplierDocumentaryAddress.E2_CompanyName = "company name";
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertNoMessageError("EXP: SupplierDocumentaryAddress - Company Name has value", supplierDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);
		}

		public void TestCheckE2_CompanyName_NX601_Supplier()
		{
			var companyNameMessage = "You have not entered a Supplier Company Name.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyNameInfo.ClearValue();
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("SupplierDocumentaryAddress - Company Name is empty", supplierDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);

			supplierDocumentaryAddress.E2_CompanyName = "company name";
			supplierDocumentaryAddressValidation.ValidateE2_RN_NKCountryCode();
			AssertNoMessageError("SupplierDocumentaryAddress - Company Name has value", supplierDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);
		}

		public void TestCheckIDCode_NX401_EXP_Supplier()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var notification = "You have not entered a Supplier ID: A valid TW-VAT or TW-PAS or TW-PID number is required for Supplier. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration.";
			var validation = supplierDocumentaryAddress.Validation;
			validation.ValidateIDCode();
			AssertNoMessageError("Declaration is import", supplierDocumentaryAddress.IDCodeInfo, notification);

			declaration.JE_MessageType = "EXP";
			validation.ValidateIDCode();
			AssertHasMessageError("All IDs are empty.", supplierDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			validation.ValidateIDCode();
			AssertNoMessageError("ControllingMessageType is not NX401, all IDs are empty.", supplierDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			supplierDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			supplierDocumentaryAddress.IDCode = "12345670";
			AssertNoMessageError("VAT has value.", supplierDocumentaryAddress.IDCodeInfo, notification);

			supplierDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
			supplierDocumentaryAddress.IDCode = "12345670";
			AssertNoMessageError("PAS has value.", supplierDocumentaryAddress.IDCodeInfo, notification);

			supplierDocumentaryAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.PID;
			supplierDocumentaryAddress.IDCode = "12345670";
			AssertNoMessageError("PID has value.", supplierDocumentaryAddress.IDCodeInfo, notification);
		}

		public void TestCheckE2_OA_Address_NX101_Supplier()
		{
			var companyNameMessage = "You have not entered a Supplier Company Name.";
			var localCompanyNameMessage = "You have not entered a Supplier Local Company Name.";
			var addressMessage = "You have not entered a Supplier Address.";
			var localAddressMessage = "You have not entered a Supplier Local Address.";
			var orgHeader = Factory.New<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			CombineAssertions("Company Name, Address are empty, no ZH-TW tranlated address", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, companyNameMessage);
				AssertHasMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, localCompanyNameMessage);
				AssertHasMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, addressMessage);
				AssertHasMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			var translatedAddress = mainAddress.TranslatedAddresses.AddNew();
			translatedAddress.Language = Enterprise.Core.Constants.Languages.ChineseTraditional;
			translatedAddress.CompanyName = "公司名稱";
			translatedAddress.Address1 = "公司地址";
			var validation = supplierDocumentaryAddress.Validation;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name, Address are empty, has ZH-TW tranlated address", () =>
			{
				AssertNoMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, companyNameMessage);
				AssertNoMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, localCompanyNameMessage);
				AssertNoMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			translatedAddress.CompanyName = ZString.Empty;
			translatedAddress.Address1 = ZString.Empty;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name, Local Company Name, Address, Local Address are all empty", () =>
			{
				AssertHasMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, companyNameMessage);
				AssertHasMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, localCompanyNameMessage);
				AssertHasMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, addressMessage);
				AssertHasMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			mainAddress.CompanyName = "Company Name";
			mainAddress.Address1 = "Company Address";
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name and Address have values, Local Company Name and Local Address are empty", () =>
			{
				AssertNoMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, companyNameMessage);
				AssertNoMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, localCompanyNameMessage);
				AssertNoMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			validation.ValidateE2_OA_Address();
			AssertHasMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, localAddressMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code8;
			validation.ValidateE2_OA_Address();
			AssertNoMessageError(supplierDocumentaryAddress.E2_OA_AddressInfo, localAddressMessage);
		}

		public void TestCheckE2_CompanyName_NX101_Supplier()
		{
			var companyNameMessage = "You have not entered a Supplier Company Name.";
			var localCompanyNameMessage = "You have not entered a Supplier Local Company Name.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("SupplierDocumentaryAddress - Both Company Name and Local Company Name are empty", supplierDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);
			var localAddress = supplierDocumentaryAddress.LocalAddress;
			var localAddressValidation = localAddress.Validation;
			localAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("LocalAddress - Both Company Name and Local Company Name are empty", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);

			supplierDocumentaryAddress.E2_CompanyName = "Company Name";
			supplierDocumentaryAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("SupplierDocumentaryAddress - Company Name has value, Local Company Name is empty", supplierDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);
			localAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalAddress - Company Name has value, Local Company Name is empty", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);

			supplierDocumentaryAddress.E2_CompanyName = ZString.Empty;
			localAddress.E2_CompanyName = "公司名稱";
			supplierDocumentaryAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("SupplierDocumentaryAddress - Company Name is empty, Local Company Name has value", supplierDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);
			localAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalAddress - Company Name is empty, Local Company Name has value", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);

			supplierDocumentaryAddress.E2_CompanyName = "Company Name";
			localAddress.E2_CompanyName = ZString.Empty;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			localAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("LocalAddress - TW1_CertificateType is 15, Local Company Name is empty", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code8;
			localAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalAddress - TW1_CertificateType is not 15, Local Company Name is empty", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);
		}

		public void TestCheckE2_CompanyName_NonNX101()
		{
			var companyNameMessage = "You have not entered a Supplier Company Name.";
			var targetInfo = supplierDocumentaryAddress.E2_CompanyNameInfo;
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var importerDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			importerDocumentaryAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("SupplierDocumentaryAddress - Both Company Name and Local Company Name are empty", targetInfo, companyNameMessage);
			var localAddress = supplierDocumentaryAddress.LocalAddress;

			supplierDocumentaryAddress.E2_CompanyName = "Company Name";
			AssertNoMessageError("SupplierDocumentaryAddress - Company Name has value, Local Company Name is empty", targetInfo, companyNameMessage);

			localAddress.E2_CompanyName = "公司名稱";
			supplierDocumentaryAddress.E2_CompanyName = ZString.Empty;
			importerDocumentaryAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("SupplierDocumentaryAddress - Company Name is empty, Local Company Name has value", targetInfo, companyNameMessage);
		}

		public void TestCheckE2_Address1_NX101_Supplier()
		{
			var addressMessage = "You have not entered a Supplier Address.";
			var localAddressMessage = "You have not entered a Supplier Local Address.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var supplierDocumentaryAddressValidation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("SupplierDocumentaryAddress - Both Address and Local Address are empty", supplierDocumentaryAddress.E2_Address1Info, addressMessage);
			var localAddress = supplierDocumentaryAddress.LocalAddress;
			var localAddressValidation = localAddress.Validation;
			localAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("LocalAddress - Both Address and Local Address are empty", localAddress.E2_Address1Info, localAddressMessage);

			supplierDocumentaryAddress.E2_Address1 = "Company Address";
			supplierDocumentaryAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("SupplierDocumentaryAddress - Address has value, Local Address is empty", supplierDocumentaryAddress.E2_Address1Info, addressMessage);
			localAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("LocalAddress - Address has value, Local Address is empty", localAddress.E2_Address1Info, localAddressMessage);

			supplierDocumentaryAddress.E2_Address1 = ZString.Empty;
			localAddress.E2_Address1 = "公司地址";
			supplierDocumentaryAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("SupplierDocumentaryAddress - Address is empty, Local Address has value", supplierDocumentaryAddress.E2_Address1Info, addressMessage);
			localAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("LocalAddress - Address is empty, Local Address has value", localAddress.E2_Address1Info, localAddressMessage);

			supplierDocumentaryAddress.E2_Address1 = "Company Address";
			localAddress.E2_Address1 = ZString.Empty;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			localAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("SupplierDocumentaryAddress - Address has value, Local Address is empty", localAddress.E2_Address1Info, localAddressMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code7;
			localAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("SupplierDocumentaryAddress - Address has value, Local Address is empty", localAddress.E2_Address1Info, localAddressMessage);
		}

		public void TestCheckE2_CompanyNameMaxLength()
		{
			var warningMessage = "Only the first 80 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			var supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
			var targetInfo = supplierDocumentaryAddress.E2_CompanyNameInfo;
			supplierDocumentaryAddress.E2_CompanyName = new ZString('A', 90);
			AssertHasWarning(targetInfo, warningMessage);

			supplierDocumentaryAddress.E2_CompanyName = new ZString('A', 79);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_City()
		{
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			supplierDocumentaryAddress.E2_City = ZString.Empty;
			supplierDocumentaryAddress.Validation.ValidateE2_City();
			AssertNoErrors(supplierDocumentaryAddress.E2_CityInfo);
		}

		public void TestCheckE2_Postcode()
		{
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_Postcode = ZString.Empty;
			supplierDocumentaryAddress.Validation.ValidateE2_Postcode();
			AssertNoErrors(supplierDocumentaryAddress.E2_PostcodeInfo);
		}

		public void TestCheckE2_State()
		{
			var targetInfo = supplierDocumentaryAddress.E2_StateInfo;
			var warningMessage = "This state code needs to be followed by valid country code.";
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_RN_NKCountryCode = "78";
			supplierDocumentaryAddress.E2_State = "EE";
			AssertHasWarning(targetInfo, warningMessage);
			supplierDocumentaryAddress.E2_RN_NKCountryCode = "DE";
			supplierDocumentaryAddress.Validation.ValidateE2_State();
			AssertNoWarning(targetInfo, warningMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			cMHeader = instruction.ControllingMessageHeaders.AddNew();
			supplierDocumentaryAddress = cMHeader.SupplierDocumentaryAddress;
		}

		CusTWControllingMessageHeader cMHeader;
		TWJobDocAddress supplierDocumentaryAddress;
	}
}
