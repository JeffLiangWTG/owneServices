using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ControllingMessageHeaderImporterAddressRequirement))]
	sealed class ControllingMessageHeaderImporterAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckOrganisationPK_NX101_ImporterWithCertificate()
		{
			var localCompanyNameMessage = "You have not entered an Importer Local Company Name.";
			var localAddressMessage = "You have not entered an Importer Local Address.";
			var importerContactInformationMessage = "You have not entered an Importer Contact Information.";
			var importerIDIsReruiredMessage = "You have not entered an Importer ID: A valid TW-VAT or TW-PAS or TW-PID number is required for Importer. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration.";
			var orgHeader1 = Factory.New<OrgHeader>();
			var mainAddress1 = orgHeader1.MainAddress;
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;
			importerDocumentaryAddress.OrganisationPK = orgHeader1.PK;
			CombineAssertions("Organisation has value", () =>
			{
				AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, localCompanyNameMessage);
				AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, localAddressMessage);
				AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerContactInformationMessage);
			});

			var orgHeader2 = Factory.New<OrgHeader>();
			var mainAddress2 = orgHeader2.MainAddress;
			mainAddress2.OA_Phone = "0288858888";
			var translatedAddress = mainAddress2.TranslatedAddresses.AddNew();
			translatedAddress.Language = Enterprise.Core.Constants.Languages.ChineseTraditional;
			translatedAddress.CompanyName = "公司名稱";
			translatedAddress.Address1 = "公司地址";
			importerDocumentaryAddress.OrganisationPK = orgHeader2.PK;
			CombineAssertions("Organisation has Chinese traditional translated address", () =>
			{
				AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, localCompanyNameMessage);
				AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, localAddressMessage);
				AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerContactInformationMessage);
				AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);
			});

			var taiwan = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Taiwan);
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code19;
			orgHeader2.CustomsCodes.RemoveAndDeleteAll();
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);

			orgHeader2.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, taiwan, "12345670");
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, importerIDIsReruiredMessage);
		}

		public void TestCheckOrganisationPK_NX601_Importer()
		{
			var errorMessage = "You have not entered an Importer.";
			var orgHeader = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var header = instruction.ControllingMessageHeaders.AddNew();
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			var importerDocumentaryAddress = header.ImporterDocumentaryAddress;
			var validation = importerDocumentaryAddress.Validation;
			validation.ValidateOrganisationPK();
			AssertHasMessageError("Organisation is null", importerDocumentaryAddress.OrganisationPKInfo, errorMessage);

			importerDocumentaryAddress.OrganisationPK = orgHeader.PK;
			AssertNoMessageError("Organisation has value", importerDocumentaryAddress.OrganisationPKInfo, errorMessage);
		}

		public void TestCheckOrganisationPK_NX101_Importer()
		{
			var companyNameMessage = "You have not entered an Importer Company Name.";
			var localCompanyNameMessage = "You have not entered an Importer Local Company Name.";
			var addressMessage = "You have not entered an Importer Address.";
			var localAddressMessage = "You have not entered an Importer Local Address.";
			var orgHeader = Factory.New<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			var taiwan = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Taiwan);
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;
			var validation = importerDocumentaryAddress.Validation;
			validation.ValidateOrganisationPK();
			CombineAssertions("Organisation is null", () =>
			{
				AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, companyNameMessage);
				AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, localCompanyNameMessage);
				AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, addressMessage);
				AssertHasMessageError(importerDocumentaryAddress.OrganisationPKInfo, localAddressMessage);
			});

			importerDocumentaryAddress.OrganisationPK = orgHeader.PK;
			CombineAssertions("Organisation has value", () =>
			{
				AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, companyNameMessage);
				AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, localCompanyNameMessage);
				AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, addressMessage);
				AssertNoMessageError(importerDocumentaryAddress.OrganisationPKInfo, localAddressMessage);
			});
		}

		public void TestCheckE2_OA_Address_NX101_Importer()
		{
			var companyNameMessage = "You have not entered an Importer Company Name.";
			var localCompanyNameMessage = "You have not entered an Importer Local Company Name.";
			var addressMessage = "You have not entered an Importer Address.";
			var localAddressMessage = "You have not entered an Importer Local Address.";
			var orgHeader = Factory.New<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;
			importerDocumentaryAddress.OrganisationPK = orgHeader.PK;
			CombineAssertions("Company Name, Address are empty, no ZH-TW tranlated address", () =>
			{
				AssertHasMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, companyNameMessage);
				AssertHasMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, localCompanyNameMessage);
				AssertHasMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, addressMessage);
				AssertHasMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			var translatedAddress = mainAddress.TranslatedAddresses.AddNew();
			translatedAddress.Language = Enterprise.Core.Constants.Languages.ChineseTraditional;
			translatedAddress.CompanyName = "公司名稱";
			translatedAddress.Address1 = "公司地址";
			var validation = importerDocumentaryAddress.Validation;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name, Address are empty, has ZH-TW tranlated address", () =>
			{
				AssertNoMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, companyNameMessage);
				AssertNoMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, localCompanyNameMessage);
				AssertNoMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			translatedAddress.CompanyName = ZString.Empty;
			translatedAddress.Address1 = ZString.Empty;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name, Local Company Name, Address, Local Address are all empty", () =>
			{
				AssertHasMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, companyNameMessage);
				AssertHasMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, localCompanyNameMessage);
				AssertHasMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, addressMessage);
				AssertHasMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			mainAddress.CompanyName = "Company Name";
			mainAddress.Address1 = "Company Address";
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name and Address have values, Local Company Name and Local Address are empty", () =>
			{
				AssertNoMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, companyNameMessage);
				AssertNoMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, localCompanyNameMessage);
				AssertNoMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			validation.ValidateE2_OA_Address();
			AssertHasMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, localAddressMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code8;
			validation.ValidateE2_OA_Address();
			AssertNoMessageError(importerDocumentaryAddress.E2_OA_AddressInfo, localAddressMessage);
		}

		public void TestCheckE2_CompanyName_NX101()
		{
			var companyNameMessage = "You have not entered an Importer Company Name.";
			var localCompanyNameMessage = "You have not entered an Importer Local Company Name.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			var importerDocumentaryAddressValidation = importerDocumentaryAddress.Validation;
			importerDocumentaryAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("ImporterDocumentaryAddress - Both Company Name and Local Company Name are empty", importerDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);
			var localAddress = importerDocumentaryAddress.LocalAddress;
			var localAddressValidation = localAddress.Validation;
			localAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("LocalAddress - Both Company Name and Local Company Name are empty", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);

			importerDocumentaryAddress.E2_CompanyName = "Company Name";
			importerDocumentaryAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("ImporterDocumentaryAddress - Company Name has value, Local Company Name is empty", importerDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);
			localAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalAddress - Company Name has value, Local Company Name is empty", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);

			importerDocumentaryAddress.E2_CompanyName = ZString.Empty;
			localAddress.E2_CompanyName = "公司名稱";
			importerDocumentaryAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("ImporterDocumentaryAddress - Company Name is empty, Local Company Name has value", importerDocumentaryAddress.E2_CompanyNameInfo, companyNameMessage);
			localAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalAddress - Company Name is empty, Local Company Name has value", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);

			importerDocumentaryAddress.E2_CompanyName = "Company Name";
			localAddress.E2_CompanyName = ZString.Empty;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			localAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("LocalAddress - TW1_CertificateType is 15, Local Company Name is empty", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code8;
			localAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalAddress - TW1_CertificateType is not 15, Local Company Name is empty", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);
		}

		public void TestCheckE2_Address1_NX101_Importer()
		{
			var addressMessage = "You have not entered an Importer Address.";
			var localAddressMessage = "You have not entered an Importer Local Address.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			var importerDocumentaryAddressValidation = importerDocumentaryAddress.Validation;
			importerDocumentaryAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("ImporterDocumentaryAddress - Both Address and Local Address are empty", importerDocumentaryAddress.E2_Address1Info, addressMessage);
			var localAddress = importerDocumentaryAddress.LocalAddress;
			var localAddressValidation = localAddress.Validation;
			localAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("LocalAddress - Both Address and Local Address are empty", localAddress.E2_Address1Info, localAddressMessage);

			importerDocumentaryAddress.E2_Address1 = "Company Address";
			importerDocumentaryAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("ImporterDocumentaryAddress - Address has value, Local Address is empty", importerDocumentaryAddress.E2_Address1Info, addressMessage);
			localAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("LocalAddress - Address has value, Local Address is empty", localAddress.E2_Address1Info, localAddressMessage);

			importerDocumentaryAddress.E2_Address1 = ZString.Empty;
			localAddress.E2_Address1 = "公司地址";
			importerDocumentaryAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("ImporterDocumentaryAddress - Address is empty, Local Address has value", importerDocumentaryAddress.E2_Address1Info, addressMessage);
			localAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("LocalAddress - Address is empty, Local Address has value", localAddress.E2_Address1Info, localAddressMessage);

			importerDocumentaryAddress.E2_Address1 = "Company Address";
			localAddress.E2_Address1 = ZString.Empty;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			localAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("ImporterDocumentaryAddress - Address has value, Local Address is empty", localAddress.E2_Address1Info, localAddressMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code7;
			localAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("ImporterDocumentaryAddress - Address has value, Local Address is empty", localAddress.E2_Address1Info, localAddressMessage);
		}

		public void TestCheckImporterContact()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			var notification = "You have not entered an Importer Contact Information.";
			var validation = importerDocumentaryAddress.Validation;
			AssertContactHasMessageError("Email, Phone, Fax are all empty.", true);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			AssertContactHasMessageError("ControllingMessageType is not NX101, Email, Phone, Fax are all empty.", false);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			AssertContactHasMessageError("CertificateTyp is not 15, Email, Phone, Fax are all empty.", false);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			importerDocumentaryAddress.E2_Email = "user@wtg.com";
			AssertContactHasMessageError("Email has value.", false);

			importerDocumentaryAddress.E2_Email = ZString.Empty;
			importerDocumentaryAddress.E2_Phone = "+88621234567";
			AssertContactHasMessageError("Phone has value.", false);

			importerDocumentaryAddress.E2_Phone = ZString.Empty;
			importerDocumentaryAddress.E2_Fax = "+88621234567";
			AssertContactHasMessageError("Fax has value.", false);

			void AssertContactHasMessageError(string message, bool expected)
			{
				validation.ValidateE2_Email();
				validation.ValidateE2_Phone();
				validation.ValidateE2_Fax();

				CombineAssertions(message, () =>
				{
					if (expected)
					{
						AssertHasMessageError(importerDocumentaryAddress.E2_EmailInfo, notification);
						AssertHasMessageError(importerDocumentaryAddress.E2_PhoneInfo, notification);
						AssertHasMessageError(importerDocumentaryAddress.E2_FaxInfo, notification);
					}
					else
					{
						AssertNoMessageError(importerDocumentaryAddress.E2_EmailInfo, notification);
						AssertNoMessageError(importerDocumentaryAddress.E2_PhoneInfo, notification);
						AssertNoMessageError(importerDocumentaryAddress.E2_FaxInfo, notification);
					}
				});
			}
		}

		public void TestCheckIDCode_NX101_Importer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			var importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			var notification = "You have not entered an Importer ID: A valid TW-VAT or TW-PAS or TW-PID number is required for Importer. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration.";
			var validation = importerDocumentaryAddress.Validation;
			validation.ValidateIDCode();
			AssertHasMessageError("All IDs are empty.", importerDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			importerDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			importerDocumentaryAddress.IDCode = "12345670";
			AssertNoMessageError("VAT has value.", importerDocumentaryAddress.IDCodeInfo, notification);

			importerDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
			importerDocumentaryAddress.IDCode = "12345670";
			AssertNoMessageError("PAS has value.", importerDocumentaryAddress.IDCodeInfo, notification);

			importerDocumentaryAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.PID;
			importerDocumentaryAddress.IDCode = "12345670";
			AssertNoMessageError("PID has value.", importerDocumentaryAddress.IDCodeInfo, notification);

			importerDocumentaryAddress.IDCodeType = ZString.Empty;
			importerDocumentaryAddress.IDCode = ZString.Empty;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			validation.ValidateIDCode();
			AssertHasMessageError("Certificate type is 11 and ID is empty.", importerDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			validation.ValidateIDCode();
			AssertHasMessageError("Certificate type is 13 and ID is empty.", importerDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			validation.ValidateIDCode();
			AssertHasMessageError("Certificate type is 14 and ID is empty.", importerDocumentaryAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			validation.ValidateIDCode();
			AssertHasMessageError("Certificate type is 18 and ID is empty.", importerDocumentaryAddress.IDCodeInfo, notification);
		}

		public void TestCheckE2_CompanyNameMaxLength()
		{
			var warningMessage = "Only the first 80 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			var importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			var targetInfo = importerDocumentaryAddress.E2_CompanyNameInfo;
			importerDocumentaryAddress.E2_CompanyName = new ZString('A', 90);
			AssertHasWarning(targetInfo, warningMessage);

			importerDocumentaryAddress.E2_CompanyName = new ZString('A', 79);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_CompanyName()
		{
			var errorMsg = "You have not entered an Importer Company Name";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var cMHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;
			var targerInfo = importerDocumentaryAddress.E2_CompanyNameInfo;
			var targetInfoNotOverrided = importerDocumentaryAddress.OrganisationPKInfo;
			CombineAssertions(() =>
			{
				var messageTypes = new ControllingMessageTypeList().GetAllCodes();
				foreach (var messageType in messageTypes)
				{
					importerDocumentaryAddress.E2_AddressOverride = true;
					cMHeader.TW1_ControllingMessageType = messageType;
					importerDocumentaryAddress.E2_CompanyName = ZString.Empty;

					importerDocumentaryAddress.Validation.ValidateE2_CompanyName();

					if (messageType == ControllingMessageTypeList.Codes.NX101 ||
						messageType == ControllingMessageTypeList.Codes.NX301 ||
						messageType == ControllingMessageTypeList.Codes.NX301_AX ||
						messageType == ControllingMessageTypeList.Codes.NX401 ||
						messageType == ControllingMessageTypeList.Codes.NX601 ||
						messageType == ControllingMessageTypeList.Codes.NX603)
					{
						AssertHasMessageErrorContaining($"Import:{messageType}: CompanyName empty", targerInfo, errorMsg);
						importerDocumentaryAddress.E2_CompanyName = "TestCompany";
						AssertNoMessageErrorContaining($"Import:{messageType}: CompanyName is TestCompany", targerInfo, errorMsg);

						importerDocumentaryAddress.E2_AddressOverride = false;
						importerDocumentaryAddress.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining($"Import:{messageType}: Organisation not set", targetInfoNotOverrided, errorMsg);
					}
					else
					{
						AssertNoMessageErrorContaining($"Import:{messageType}: not validate CompanyName", targerInfo, errorMsg);
					}
				}
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				foreach (var messageType in messageTypes)
				{
					importerDocumentaryAddress.E2_AddressOverride = true;
					cMHeader.TW1_ControllingMessageType = messageType;
					importerDocumentaryAddress.E2_CompanyName = ZString.Empty;
					importerDocumentaryAddress.Validation.ValidateE2_CompanyName();

					if (messageType == ControllingMessageTypeList.Codes.NX101 ||
						messageType == ControllingMessageTypeList.Codes.NX201_01 ||
						messageType == ControllingMessageTypeList.Codes.NX301 ||
						messageType == ControllingMessageTypeList.Codes.NX301_AX ||
						messageType == ControllingMessageTypeList.Codes.NX601 ||
						messageType == ControllingMessageTypeList.Codes.NX603)
					{
						AssertHasMessageErrorContaining($"Export:{messageType}: CompanyName empty", targerInfo, errorMsg);
						importerDocumentaryAddress.E2_CompanyName = "TestCompany";
						AssertNoMessageErrorContaining($"Export:{messageType}: CompanyName is TestCompany", targerInfo, errorMsg);

						importerDocumentaryAddress.E2_AddressOverride = false;
						importerDocumentaryAddress.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining($"Export:{messageType}: Organisation not set", targetInfoNotOverrided, errorMsg);
					}
					else
					{
						AssertNoMessageErrorContaining($"Export:{messageType}: not validate CompanyName", targerInfo, errorMsg);
					}
				}
			});
		}

		public void TestCheckE2_Address1()
		{
			var errorMsg = "You have not entered an Importer Address";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var cMHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;
			var targerInfo = importerDocumentaryAddress.E2_Address1Info;
			var targetInfoNotOverrided = importerDocumentaryAddress.OrganisationPKInfo;
			CombineAssertions(() =>
			{
				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					importerDocumentaryAddress.E2_AddressOverride = true;
					cMHeader.TW1_ControllingMessageType = messageType;
					importerDocumentaryAddress.E2_Address1 = ZString.Empty;
					importerDocumentaryAddress.Validation.ValidateE2_Address1();
					if (cMHeader.IsNX101 ||
						cMHeader.IsNX301_AX ||
						cMHeader.IsNX401 ||
						cMHeader.IsNX601)
					{
						AssertHasMessageErrorContaining($"{messageType}: Address1 empty", targerInfo, errorMsg);
						importerDocumentaryAddress.E2_Address1 = "test address";
						AssertNoMessageErrorContaining($"{messageType}: Address1 is test address", targerInfo, errorMsg);

						importerDocumentaryAddress.E2_AddressOverride = false;
						importerDocumentaryAddress.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining($"{messageType}: Organisation not set", targetInfoNotOverrided, errorMsg);
					}
					else
					{
						AssertNoMessageErrorContaining($"{messageType}: not validate Address1", targerInfo, errorMsg);
					}
				}
			});
		}

		public void TestCheckE2_RN_NKCountryCode()
		{
			var errorMsg = "You have not entered an Importer Country/Region Code.";
			var org = Factory.New<OrgHeader>();
			var addressWithCountry = org.Addresses.AddNew();
			addressWithCountry.OA_RN_NKCountryCode = "TW";
			var addressWithoutCountry = org.Addresses.AddNew();
			addressWithoutCountry.OA_RN_NKCountryCode = ZString.Empty;

			var targerInfo = importerDocumentaryAddress.E2_RN_NKCountryCodeInfo;
			var targetInfoNotOverrided = importerDocumentaryAddress.OrganisationPKInfo;
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var messageTypes = new ControllingMessageTypeList().GetAllCodes();
				foreach (var messageType in messageTypes)
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					importerDocumentaryAddress.E2_AddressOverride = false;
					importerDocumentaryAddress.E2_OA_Address = addressWithoutCountry.PK;

					if (cMHeader.IsNX201_01)
					{
						AssertHasMessageErrorContaining($"Export:{messageType}: Country empty", targetInfoNotOverrided, errorMsg);

						importerDocumentaryAddress.OrganisationPK = ZGuid.Empty;
						AssertHasMessageErrorContaining("Organisation not set", targetInfoNotOverrided, errorMsg);

						importerDocumentaryAddress.E2_OA_Address = addressWithCountry.PK;
						importerDocumentaryAddress.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining($"Export:{messageType}: Country is TW", targetInfoNotOverrided, errorMsg);

						importerDocumentaryAddress.E2_AddressOverride = true;
						importerDocumentaryAddress.E2_RN_NKCountryCode = ZString.Empty;
						AssertHasMessageErrorContaining($"Export:{messageType}: Overrided Address and Country empty", targerInfo, errorMsg);

						importerDocumentaryAddress.E2_RN_NKCountryCode = "TW";
						AssertNoMessageErrorContaining($"Export:{messageType}: Overrided Address and Country is TW", targerInfo, errorMsg);
					}
					else
					{
						AssertNoMessageErrorContaining($"Export:{messageType}: not validate Country", targetInfoNotOverrided, errorMsg);

						importerDocumentaryAddress.E2_AddressOverride = true;
						importerDocumentaryAddress.E2_RN_NKCountryCode = ZString.Empty;
						AssertNoMessageErrorContaining($"Export:{messageType}: not validate Country when overrided", targerInfo, errorMsg);
					}
				}

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				foreach (var messageType in messageTypes)
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					importerDocumentaryAddress.E2_AddressOverride = false;
					importerDocumentaryAddress.E2_OA_Address = addressWithoutCountry.PK;
					importerDocumentaryAddress.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining($"Import:{messageType}: not validate CountryCode", targetInfoNotOverrided, errorMsg);

					importerDocumentaryAddress.E2_AddressOverride = true;
					importerDocumentaryAddress.E2_RN_NKCountryCode = ZString.Empty;
					importerDocumentaryAddress.Validation.ValidateE2_RN_NKCountryCode();
					AssertNoMessageErrorContaining($"Import:{messageType}: not validate CountryCode", targerInfo, errorMsg);
				}
			});
		}

		public void TestCheckTelephoneNumber()
		{
			var errorMsg = "You have not entered an Importer Telephone Number";
			var org = Factory.New<OrgHeader>();
			var addressWithPhone = org.Addresses.AddNew();
			addressWithPhone.OA_Phone = "11111111111";
			var addressWithoutPhone = org.Addresses.AddNew();
			addressWithoutPhone.OA_Phone = ZString.Empty;

			var targerInfo = importerDocumentaryAddress.E2_PhoneInfo;
			var targetInfoNotOverrided = importerDocumentaryAddress.OrganisationPKInfo;
			CombineAssertions(() =>
			{
				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					importerDocumentaryAddress.E2_AddressOverride = false;
					importerDocumentaryAddress.E2_OA_Address = addressWithoutPhone.PK;
					importerDocumentaryAddress.Validation.ValidateOrganisationPK();
					if (cMHeader.IsNX301_AX ||
						cMHeader.IsNX301_DN ||
						cMHeader.IsNX601)
					{
						AssertHasMessageErrorContaining($"{messageType}: Phone empty", targetInfoNotOverrided, errorMsg);

						importerDocumentaryAddress.OrganisationPK = ZGuid.Empty;
						AssertHasMessageErrorContaining("Organisation not set", targetInfoNotOverrided, errorMsg);

						importerDocumentaryAddress.E2_OA_Address = addressWithPhone.PK;
						importerDocumentaryAddress.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining($"{messageType}: Phone is 11111111111", targetInfoNotOverrided, errorMsg);

						importerDocumentaryAddress.E2_AddressOverride = true;
						importerDocumentaryAddress.E2_Phone = ZString.Empty;
						AssertHasMessageErrorContaining($"{messageType}: Overrided Address and Phone empty", targerInfo, errorMsg);

						importerDocumentaryAddress.E2_Phone = "22222222222";
						AssertNoMessageErrorContaining($"{messageType}: Overrided Address and Phone is 22222222222", targerInfo, errorMsg);
					}
					else
					{
						AssertNoMessageErrorContaining($"{messageType}: not validate Phone", targetInfoNotOverrided, errorMsg);

						importerDocumentaryAddress.E2_AddressOverride = true;
						importerDocumentaryAddress.E2_Phone = ZString.Empty;
						AssertNoMessageErrorContaining($"{messageType}: not validate Phone when overrided", targerInfo, errorMsg);
					}
				}
			});
		}

		public void TestCheckIDCode()
		{
			var errorMsg = "You have not entered an Importer ID: A valid TW-VAT or TW-PAS or TW-PID number is required for Importer. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration.";
			var orgWithCode = Factory.New<OrgHeader>();
			var addressWithCode = orgWithCode.Addresses.AddNew();
			addressWithCode.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "11111111", Core.Constants.CountryCodes.Taiwan);

			var orgWithoutCode = Factory.New<OrgHeader>();
			var addressWithoutCode = orgWithoutCode.Addresses.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var cMHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;

			var targerInfo = importerDocumentaryAddress.IDCodeInfo;
			var targetInfoNotOverrided = importerDocumentaryAddress.OrganisationPKInfo;

			CombineAssertions(() =>
			{
				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					cMHeader.TW1_ControllingMessageType = messageType;
					importerDocumentaryAddress.E2_AddressOverride = false;
					importerDocumentaryAddress.OrganisationPK = orgWithoutCode.PK;
					importerDocumentaryAddress.E2_OA_Address = addressWithoutCode.PK;

					importerDocumentaryAddress.Validation.ValidateCodes();

					if (cMHeader.IsNX301 ||
						cMHeader.IsNX301_AX ||
						cMHeader.IsNX301_DN ||
						cMHeader.IsNX401 ||
						cMHeader.IsNX601 ||
						cMHeader.IsNX603)
					{
						AssertHasMessageError($"{messageType}: IDCode empty", targetInfoNotOverrided, errorMsg);

						importerDocumentaryAddress.OrganisationPK = ZGuid.Empty;
						AssertHasMessageErrorContaining("Organisation not set", targetInfoNotOverrided, errorMsg);

						importerDocumentaryAddress.OrganisationPK = orgWithCode.PK;
						importerDocumentaryAddress.E2_OA_Address = addressWithCode.PK;
						AssertNoMessageError($"{messageType}: Address has a IDCode", targetInfoNotOverrided, errorMsg);

						importerDocumentaryAddress.E2_AddressOverride = true;
						importerDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
						importerDocumentaryAddress.IDCode = ZString.Empty;
						AssertHasMessageError($"{messageType}: Overrided Address and IDCode empty", targerInfo, errorMsg);

						importerDocumentaryAddress.IDCode = "22222222";
						AssertNoMessageError($"{messageType}: Overrided Address and has a IDCode", targerInfo, errorMsg);
					}
					else
					{
						AssertNoMessageError($"{messageType}: not validate IDCode", targetInfoNotOverrided, errorMsg);
						importerDocumentaryAddress.E2_AddressOverride = true;
						importerDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
						importerDocumentaryAddress.IDCode = ZString.Empty;
						AssertNoMessageError($"{messageType}: not validate IDCode when overrided", targerInfo, errorMsg);
					}
				}
			});
		}

		public void TestCheckE2_City()
		{
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			importerDocumentaryAddress.E2_City = ZString.Empty;
			importerDocumentaryAddress.Validation.ValidateE2_City();
			AssertNoErrors(importerDocumentaryAddress.E2_CityInfo);
		}

		public void TestCheckE2_Postcode()
		{
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_Postcode = ZString.Empty;
			importerDocumentaryAddress.Validation.ValidateE2_Postcode();
			AssertNoErrors(importerDocumentaryAddress.E2_PostcodeInfo);
		}

		public void TestCheckE2_State()
		{
			var targetInfo = importerDocumentaryAddress.E2_StateInfo;
			var warningMessage = "This state code needs to be followed by valid country code.";
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_RN_NKCountryCode = "78";
			importerDocumentaryAddress.E2_State = "EE";
			AssertHasWarning(targetInfo, warningMessage);
			importerDocumentaryAddress.E2_RN_NKCountryCode = "DE";
			importerDocumentaryAddress.Validation.ValidateE2_State();
			AssertNoWarning(targetInfo, warningMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			cMHeader = instruction.ControllingMessageHeaders.AddNew();
			importerDocumentaryAddress = cMHeader.ImporterDocumentaryAddress;
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader cMHeader;
		TWJobDocAddress importerDocumentaryAddress;
	}
}
