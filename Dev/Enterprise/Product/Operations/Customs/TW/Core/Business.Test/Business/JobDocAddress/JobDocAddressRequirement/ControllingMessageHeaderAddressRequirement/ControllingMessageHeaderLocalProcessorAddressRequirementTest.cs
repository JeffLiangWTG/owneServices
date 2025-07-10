using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ControllingMessageHeaderLocalProcessorAddressRequirement))]
	sealed class ControllingMessageHeaderLocalProcessorAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckChineseCompanyLength()
		{
			var warning = "Only the first 70 characters will be sent to the customs.";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			controllingMessageHeader.TW1_ControllingMessageType = "NX601";
			controllingMessageHeader.TW1_ControllingAgency = "CD";
			controllingMessageHeader.LocalProcessorAddress.E2_AddressOverride = true;
			var localAddress = controllingMessageHeader.LocalProcessorAddress.LocalAddress;
			var targetInfo = localAddress.E2_CompanyNameInfo;

			localAddress.E2_CompanyName = new ZString('A', 70);
			AssertNoWarning(targetInfo, warning);

			localAddress.E2_CompanyName = new ZString('A', 71);
			AssertHasWarning(targetInfo, warning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			localAddress.Validation.ValidateE2_CompanyName();
			AssertNoWarning(targetInfo, warning);
		}

		public void TestCheckChineseAddressLength()
		{
			var warning = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			controllingMessageHeader.TW1_ControllingMessageType = "NX601";
			controllingMessageHeader.TW1_ControllingAgency = "CD";
			controllingMessageHeader.LocalProcessorAddress.E2_AddressOverride = true;
			var localAddress = controllingMessageHeader.LocalProcessorAddress.LocalAddress;
			var targetInfo = localAddress.Address1Info;

			localAddress.E2_RN_NKCountryCode = "TW";
			localAddress.E2_Postcode = "239";
			localAddress.E2_City = "Taipei";
			localAddress.AdditionalAddressInformation = "Add";
			localAddress.Address2 = new ZString('D', 40);
			localAddress.Address1 = new ZString('C', 48);
			AssertNoWarning(targetInfo, warning);

			localAddress.Address1 = new ZString('A', 49);
			AssertHasWarning(targetInfo, warning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			localAddress.Validation.ValidateE2_Address1();
			AssertNoWarning(targetInfo, warning);
		}

		public void TestCheckE2_OA_AddressChineseCompanyNameLength()
		{
			var warning = "Chinese Company Name Only the first 70 characters will be sent to the customs.";
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			controllingMessageHeader.TW1_ControllingMessageType = "NX601";
			controllingMessageHeader.TW1_ControllingAgency = "CD";
			var localProcessorAddress = controllingMessageHeader.LocalProcessorAddress;
			var targetInfo = localProcessorAddress.E2_OA_AddressInfo;

			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = new ZString('A', 70);

			localProcessorAddress.OrganisationPK = org.PK;
			localProcessorAddress.E2_OA_Address = mainAddress.PK;
			AssertNoWarning(targetInfo, warning);

			transAddress.CompanyName = new ZString('A', 71);
			localProcessorAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarning(targetInfo, warning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			localProcessorAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, warning);
		}

		public void TestCheckE2_OA_AddressChineseAddressLength()
		{
			var warning = "Chinese Address Only the first 100 characters will be sent to the customs.";
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			controllingMessageHeader.TW1_ControllingMessageType = "NX601";
			controllingMessageHeader.TW1_ControllingAgency = "CD";
			var localProcessorAddress = controllingMessageHeader.LocalProcessorAddress;
			var targetInfo = localProcessorAddress.E2_OA_AddressInfo;

			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.City = "Taipei";
			transAddress.CompanyName = "company";
			transAddress.Address2 = new ZString('D', 46);
			transAddress.Address1 = new ZString('C', 48);

			localProcessorAddress.OrganisationPK = org.PK;
			localProcessorAddress.E2_OA_Address = mainAddress.PK;
			AssertNoWarning(targetInfo, warning);

			transAddress.Address1 = new ZString('A', 49);
			localProcessorAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarning(targetInfo, warning);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			localProcessorAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, warning);
		}

		public void TestCheckE2_OA_Address_NX601_LocalProcessor()
		{
			var localNameMessage = "You have not entered a Local Processor Local Company Name.";
			var localAddressMessage = "You have not entered a Local Processor Local Address.";
			var phoneMessage = "You have not entered a Local Processor Telephone Number.";
			var orgHeader = Factory.New<OrgHeader>();
			var addressNoLocalAddressNoPhone = orgHeader.Addresses.AddNew();

			var addressNoLocalAddressHasPhone = orgHeader.Addresses.AddNew();
			addressNoLocalAddressHasPhone.OA_Phone = "12345678";

			var addressHasLocalAddressHasPhone = orgHeader.Addresses.AddNew();
			addressHasLocalAddressHasPhone.OA_Phone = "12345678";
			var translatedAddress = addressHasLocalAddressHasPhone.TranslatedAddresses.AddNew();
			translatedAddress.Language = Core.SharedConstants.Languages.ChineseTraditional;

			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			var localProcessorAddress = cMHeader.LocalProcessorAddress;
			localProcessorAddress.OrganisationPK = orgHeader.PK;

			var validation = localProcessorAddress.Validation;
			localProcessorAddress.E2_OA_Address = addressNoLocalAddressNoPhone.PK;
			validation.ValidateE2_OA_Address();
			CombineAssertions("No Local address and No Phone", () =>
			{
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, localNameMessage);
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, localAddressMessage);
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, phoneMessage);
			});

			localProcessorAddress.E2_OA_Address = addressNoLocalAddressHasPhone.PK;
			validation.ValidateE2_OA_Address();
			CombineAssertions("No Local address and Has Phone", () =>
			{
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, localNameMessage);
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, localAddressMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, phoneMessage);
			});

			localProcessorAddress.E2_OA_Address = addressHasLocalAddressHasPhone.PK;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Has Local address and Has Phone, but company name and address are empty", () =>
			{
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, localNameMessage);
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, localAddressMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, phoneMessage);
			});

			translatedAddress.CompanyName = "公司名稱";
			translatedAddress.Address1 = "公司地址";
			validation.ValidateE2_OA_Address();
			CombineAssertions("Has Local address and Has Phone, and company name and address are not empty", () =>
			{
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, localNameMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, localAddressMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, phoneMessage);
			});
		}

		public void TestCheckOrganisationPK_NX601_LocalProcessor()
		{
			var localNameMessage = "You have not entered a Local Processor Local Company Name.";
			var localAddressMessage = "You have not entered a Local Processor Local Address.";
			var phoneMessage = "You have not entered a Local Processor Telephone Number.";
			var orgHeader = Factory.New<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			var localProcessorAddress = cMHeader.LocalProcessorAddress;
			var validation = localProcessorAddress.Validation;
			validation.ValidateOrganisationPK();
			CombineAssertions("NX401, Organisation is null", () =>
			{
				AssertNoMessageError(localProcessorAddress.OrganisationPKInfo, localNameMessage);
				AssertNoMessageError(localProcessorAddress.OrganisationPKInfo, localAddressMessage);
				AssertNoMessageError(localProcessorAddress.OrganisationPKInfo, phoneMessage);
			});

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			validation.ValidateOrganisationPK();
			CombineAssertions("NX601, Organisation is null", () =>
			{
				AssertHasMessageError(localProcessorAddress.OrganisationPKInfo, localNameMessage);
				AssertHasMessageError(localProcessorAddress.OrganisationPKInfo, localAddressMessage);
				AssertHasMessageError(localProcessorAddress.OrganisationPKInfo, phoneMessage);
			});

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			localProcessorAddress.OrganisationPK = orgHeader.PK;
			CombineAssertions("NX601, Organisation has value", () =>
			{
				AssertNoMessageError(localProcessorAddress.OrganisationPKInfo, localNameMessage);
				AssertNoMessageError(localProcessorAddress.OrganisationPKInfo, localAddressMessage);
				AssertNoMessageError(localProcessorAddress.OrganisationPKInfo, phoneMessage);
			});
		}

		public void TestCheckOrganisationPK_NX101_LocalProcessor()
		{
			var idMessage = "You have not entered a Local Processor ID: A valid TW-VAT or TW-PAS or TW-PID or TW-FRI number is required for Local Processor. To create a valid TW-VAT or TW-PAS or TW-PID or TW-FRI, visit Organization > Details > Config > Registration.";
			var nameMessage = "You have not entered a Local Processor Company Name.";
			var localNameMessage = "You have not entered a Local Processor Local Company Name.";
			var addressMessage = "You have not entered a Local Processor Address.";
			var localAddressMessage = "You have not entered a Local Processor Local Address.";
			var orgHeader = Factory.New<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			var taiwan = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Taiwan);
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var localProcessorAddress = cMHeader.LocalProcessorAddress;
			var validation = localProcessorAddress.Validation;
			validation.ValidateOrganisationPK();
			CombineAssertions("Organisation is null", () =>
			{
				AssertHasMessageError(localProcessorAddress.OrganisationPKInfo, idMessage);
				AssertHasMessageError(localProcessorAddress.OrganisationPKInfo, nameMessage);
				AssertHasMessageError(localProcessorAddress.OrganisationPKInfo, localNameMessage);
				AssertHasMessageError(localProcessorAddress.OrganisationPKInfo, addressMessage);
				AssertHasMessageError(localProcessorAddress.OrganisationPKInfo, localAddressMessage);
			});

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			validation.ValidateOrganisationPK();
			CombineAssertions("ControllingMessageType is not NX101, organisation is null", () =>
			{
				AssertNoMessageError(localProcessorAddress.OrganisationPKInfo, idMessage);
				AssertNoMessageError(localProcessorAddress.OrganisationPKInfo, nameMessage);
				AssertNoMessageError(localProcessorAddress.OrganisationPKInfo, localNameMessage);
				AssertNoMessageError(localProcessorAddress.OrganisationPKInfo, addressMessage);
				AssertNoMessageError(localProcessorAddress.OrganisationPKInfo, localAddressMessage);
			});

			localProcessorAddress.OrganisationPK = orgHeader.PK;
			foreach (var manufactureIDType in localProcessorAddress.ManufactureIDTypes)
			{
				cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
				orgHeader.SetCustomsCode(manufactureIDType, taiwan, "12345670");
				validation.ValidateOrganisationPK();
				AssertNoMessageError($"Organisation has {manufactureIDType}", localProcessorAddress.OrganisationPKInfo, idMessage);

				orgHeader.CustomsCodes.RemoveAndDeleteAll();
				validation.ValidateOrganisationPK();
				AssertHasMessageError("Organisation has no ID", localProcessorAddress.OrganisationPKInfo, idMessage);

				cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
				validation.ValidateOrganisationPK();
				AssertNoMessageError($"ControllingMessageType is not NX101 and organisation has {manufactureIDType}", localProcessorAddress.OrganisationPKInfo, idMessage);
			}
		}

		public void TestCheckE2_OA_Address_NX101_LocalProcessor()
		{
			var nameMessage = "You have not entered a Local Processor Company Name.";
			var localNameMessage = "You have not entered a Local Processor Local Company Name.";
			var addressMessage = "You have not entered a Local Processor Address.";
			var localAddressMessage = "You have not entered a Local Processor Local Address.";
			var contactInformationMessage = "You have not entered a Local Processor Contact Information.";
			var orgHeader = Factory.New<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var localProcessorAddress = cMHeader.LocalProcessorAddress;
			localProcessorAddress.OrganisationPK = orgHeader.PK;
			var validation = localProcessorAddress.Validation;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name, Address are empty, no ZH-TW tranlated address", () =>
			{
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, nameMessage);
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, localNameMessage);
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, addressMessage);
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Not NX101, Company Name, Address are empty, no ZH-TW tranlated address", () =>
			{
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, nameMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, localNameMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var translatedAddress = mainAddress.TranslatedAddresses.AddNew();
			translatedAddress.Language = Core.SharedConstants.Languages.ChineseTraditional;
			translatedAddress.CompanyName = "公司名稱";
			translatedAddress.Address1 = "公司地址";
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name, Address are empty, has ZH-TW tranlated address", () =>
			{
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, nameMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, localNameMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			translatedAddress.CompanyName = ZString.Empty;
			translatedAddress.Address1 = ZString.Empty;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name, Local Company Name, Address, Local Address are all empty", () =>
			{
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, nameMessage);
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, localNameMessage);
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, addressMessage);
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			validation.ValidateE2_OA_Address();
			CombineAssertions("Not NX101, Company Name, Local Company Name, Address, Local Address are all empty", () =>
			{
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, nameMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, localNameMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			mainAddress.CompanyName = "Company Name";
			mainAddress.Address1 = "Company Address";
			validation.ValidateE2_OA_Address();
			CombineAssertions("Company Name and Address have values, Local Company Name and Local Address are empty", () =>
			{
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, nameMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, localNameMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, addressMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, localAddressMessage);
			});

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			validation.ValidateE2_OA_Address();
			CombineAssertions("CertificateType is 15, Company Name and Address have values, Local Company Name and Local Address are empty", () =>
			{
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, localNameMessage);
				AssertHasMessageError(localProcessorAddress.E2_OA_AddressInfo, localAddressMessage);
			});
			AssertHasMessageError("CertificateType is 15, contact information is empty", localProcessorAddress.E2_OA_AddressInfo, contactInformationMessage);

			mainAddress.OA_Email = "user@wtg.com";
			validation.ValidateE2_OA_Address();
			AssertNoMessageError("CertificateType 15, Email has value", localProcessorAddress.E2_OA_AddressInfo, contactInformationMessage);

			mainAddress.OA_Email = ZString.Empty;
			mainAddress.OA_Phone = "+88621234567";
			validation.ValidateE2_OA_Address();
			AssertNoMessageError("CertificateType 15, Phone has value", localProcessorAddress.E2_OA_AddressInfo, contactInformationMessage);

			mainAddress.OA_Phone = ZString.Empty;
			mainAddress.OA_Mobile = "+88621234567";
			validation.ValidateE2_OA_Address();
			AssertNoMessageError("CertificateType 15, Mobile has value", localProcessorAddress.E2_OA_AddressInfo, contactInformationMessage);

			mainAddress.OA_Mobile = ZString.Empty;
			mainAddress.OA_Fax = "+88621234567";
			validation.ValidateE2_OA_Address();
			AssertNoMessageError("CertificateType 15, Fax has value", localProcessorAddress.E2_OA_AddressInfo, contactInformationMessage);

			mainAddress.OA_Fax = ZString.Empty;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			validation.ValidateE2_OA_Address();
			CombineAssertions("CertificateType is not 15, Company Name and Address have values, Local Company Name and Local Address are empty", () =>
			{
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, localNameMessage);
				AssertNoMessageError(localProcessorAddress.E2_OA_AddressInfo, localAddressMessage);
			});
			AssertNoMessageError("CertificateType not 15, contact information is empty", localProcessorAddress.E2_OA_AddressInfo, contactInformationMessage);
		}

		public void TestCheckE2_OA_Address_NotNX101OrNX601_LocalProcessor()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mainAddress = orgHeader.MainAddress;
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			var localProcessorAddress = cMHeader.LocalProcessorAddress;
			localProcessorAddress.E2_OA_Address = mainAddress.PK;
			var validation = localProcessorAddress.Validation;
			var propertyInfo = localProcessorAddress.E2_OA_AddressInfo;

			CombineAssertions(() =>
			{
				foreach (var messageType in new ControllingMessageTypeList().GetAllCodes())
				{
					if (messageType == ControllingMessageTypeList.Codes.NX101 ||
						messageType == ControllingMessageTypeList.Codes.NX601)
					{
						continue;
					}
					cMHeader.TW1_ControllingMessageType = messageType;
					validation.ValidateE2_OA_Address();
					Assert($"{messageType} should not has any message error", !propertyInfo.HasNotifications());
				}
			});
		}

		public void TestCheckE2_CompanyName_NX601_LocalProcessor()
		{
			var localCompanyLocalNameMessage = "You have not entered a Local Processor Local Company Name.";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			controllingMessageHeader.LocalProcessorAddress.E2_AddressOverride = true;
			var localAddress = controllingMessageHeader.LocalProcessorAddress.LocalAddress;
			var localProcessorLocalAddressValidation = localAddress.Validation;
			localProcessorLocalAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError(localAddress.E2_CompanyNameInfo, localCompanyLocalNameMessage);

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			localProcessorLocalAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError(localAddress.E2_CompanyNameInfo, localCompanyLocalNameMessage);

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			localAddress.E2_CompanyName = "Company Name";
			localProcessorLocalAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError(localAddress.E2_CompanyNameInfo, localCompanyLocalNameMessage);
		}

		public void TestCheckE2_Address1_NX601_LocalProcessor()
		{
			var localCompanyLocalAddressMessage = "You have not entered a Local Processor Local Address.";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			controllingMessageHeader.LocalProcessorAddress.E2_AddressOverride = true;
			var localAddress = controllingMessageHeader.LocalProcessorAddress.LocalAddress;
			var localProcessorLocalAddressValidation = localAddress.Validation;
			localProcessorLocalAddressValidation.ValidateE2_Address1();
			AssertHasMessageError(localAddress.E2_Address1Info, localCompanyLocalAddressMessage);

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			localProcessorLocalAddressValidation.ValidateE2_Address1();
			AssertNoMessageError(localAddress.E2_Address1Info, localCompanyLocalAddressMessage);

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			localAddress.E2_Address1 = "E2_Address1";
			localProcessorLocalAddressValidation.ValidateE2_Address1();
			AssertNoMessageError(localAddress.E2_Address1Info, localCompanyLocalAddressMessage);
		}

		public void TestCheckE2_Phone_NX601_LocalProcessor()
		{
			var localProcessorPhoneMessage = "You have not entered a Local Processor Telephone Number.";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			controllingMessageHeader.LocalProcessorAddress.E2_AddressOverride = true;
			var localAddress = controllingMessageHeader.LocalProcessorAddress;
			var localProcessorLocalAddressValidation = localAddress.Validation;
			localProcessorLocalAddressValidation.ValidateE2_Phone();
			AssertHasMessageError(localAddress.E2_PhoneInfo, localProcessorPhoneMessage);

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			localProcessorLocalAddressValidation.ValidateE2_Phone();
			AssertNoMessageError(localAddress.E2_PhoneInfo, localProcessorPhoneMessage);

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			localAddress.E2_Phone = "12345678";
			localProcessorLocalAddressValidation.ValidateE2_Phone();
			AssertNoMessageError(localAddress.E2_PhoneInfo, localProcessorPhoneMessage);
		}

		public void TestCheckE2_CompanyName_NX101_LocalProcessor()
		{
			var companyNameMessage = "You have not entered a Local Processor Company Name.";
			var localCompanyNameMessage = "You have not entered a Local Processor Local Company Name.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var localProcessorAddress = cMHeader.LocalProcessorAddress;
			localProcessorAddress.E2_AddressOverride = true;
			var localProcessorAddressValidation = localProcessorAddress.Validation;
			localProcessorAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("LocalProcessorAddress - Both Company Name and Local Company Name are empty", localProcessorAddress.E2_CompanyNameInfo, companyNameMessage);
			var localAddress = localProcessorAddress.LocalAddress;
			var localAddressValidation = localAddress.Validation;
			localAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("LocalAddress - Both Company Name and Local Company Name are empty", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			localProcessorAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalProcessorAddress - Not NX101, both Company Name and Local Company Name are empty", localProcessorAddress.E2_CompanyNameInfo, companyNameMessage);
			localAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalAddress - Not NX101, both Company Name and Local Company Name are empty", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			localProcessorAddress.E2_CompanyName = "Company Name";
			localProcessorAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalProcessorAddress - Company Name has value, Local Company Name is empty", localProcessorAddress.E2_CompanyNameInfo, companyNameMessage);
			localAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalAddress - Company Name has value, Local Company Name is empty", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			localProcessorAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalProcessorAddress - CertificateType is 15, Company Name has value, Local Company Name is empty", localProcessorAddress.E2_CompanyNameInfo, companyNameMessage);
			localAddressValidation.ValidateE2_CompanyName();
			AssertHasMessageError("LocalAddress - CertificateType is 15, Company Name has value, Local Company Name is empty", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);

			localProcessorAddress.E2_CompanyName = ZString.Empty;
			localAddress.E2_CompanyName = "公司名稱";
			localProcessorAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalProcessorAddress - CertificateType is 15, Company Name is empty, Local Company Name has value", localProcessorAddress.E2_CompanyNameInfo, companyNameMessage);
			localAddressValidation.ValidateE2_CompanyName();
			AssertNoMessageError("LocalAddress - CertificateType is 15, Company Name is empty, Local Company Name has value", localAddress.E2_CompanyNameInfo, localCompanyNameMessage);
		}

		public void TestCheckE2_Address1_NX101_LocalProcessor()
		{
			var addressMessage = "You have not entered a Local Processor Address.";
			var localAddressMessage = "You have not entered a Local Processor Local Address.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var localProcessorAddress = cMHeader.LocalProcessorAddress;
			localProcessorAddress.E2_AddressOverride = true;
			var localProcessorAddressValidation = localProcessorAddress.Validation;
			localProcessorAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("LocalProcessorAddress - Both Address and Local Address are empty", localProcessorAddress.E2_Address1Info, addressMessage);
			var localAddress = localProcessorAddress.LocalAddress;
			var localAddressValidation = localAddress.Validation;
			localAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("LocalAddress - Both Address and Local Address are empty", localAddress.E2_Address1Info, localAddressMessage);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			localProcessorAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("LocalProcessorAddress - Not NX101, both Address and Local Address are empty", localProcessorAddress.E2_Address1Info, addressMessage);
			localAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("LocalAddress - Not NX101, both Address and Local Address are empty", localAddress.E2_Address1Info, localAddressMessage);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			localProcessorAddress.E2_Address1 = "Company Address";
			localProcessorAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("LocalProcessorAddress - Address has value, Local Address is empty", localProcessorAddress.E2_Address1Info, addressMessage);
			localAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("LocalAddress - Address has value, Local Address is empty", localAddress.E2_Address1Info, localAddressMessage);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			localProcessorAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("LocalProcessorAddress - CertificateType is 15, Address has value, Local Address is empty", localProcessorAddress.E2_Address1Info, addressMessage);
			localAddressValidation.ValidateE2_Address1();
			AssertHasMessageError("LocalAddress - CertificateType is 15, Address has value, Local Address is empty", localAddress.E2_Address1Info, localAddressMessage);

			localProcessorAddress.E2_Address1 = ZString.Empty;
			localAddress.E2_Address1 = "公司地址";
			localProcessorAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("LocalProcessorAddress - CertificateType is 15, Address is empty, Local Address has value", localProcessorAddress.E2_Address1Info, addressMessage);
			localAddressValidation.ValidateE2_Address1();
			AssertNoMessageError("LocalAddress - CertificateType is 15, Address is empty, Local Address has value", localAddress.E2_Address1Info, localAddressMessage);
		}

		public void TestCheckLocalProcessorContact()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var localProcessorAddress = cMHeader.LocalProcessorAddress;
			localProcessorAddress.E2_AddressOverride = true;
			var notification = "You have not entered a Local Processor Contact Information.";
			var validation = localProcessorAddress.Validation;
			AssertContactHasMessageError("Email, Phone, Fax are all empty.", true);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			AssertContactHasMessageError("ControllingMessageType is not NX101, Email, Phone, Fax are all empty.", false);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			AssertContactHasMessageError("CertificateTyp is not 15, Email, Phone, Fax are all empty.", false);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			localProcessorAddress.E2_Email = "user@wtg.com";
			AssertContactHasMessageError("Email has value.", false);

			localProcessorAddress.E2_Email = ZString.Empty;
			localProcessorAddress.E2_Phone = "+88621234567";
			AssertContactHasMessageError("Phone has value.", false);

			localProcessorAddress.E2_Phone = ZString.Empty;
			localProcessorAddress.E2_Fax = "+88621234567";
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
						AssertHasMessageError(localProcessorAddress.E2_EmailInfo, notification);
						AssertHasMessageError(localProcessorAddress.E2_PhoneInfo, notification);
						AssertHasMessageError(localProcessorAddress.E2_FaxInfo, notification);
					}
					else
					{
						AssertNoMessageError(localProcessorAddress.E2_EmailInfo, notification);
						AssertNoMessageError(localProcessorAddress.E2_PhoneInfo, notification);
						AssertNoMessageError(localProcessorAddress.E2_FaxInfo, notification);
					}
				});
			}
		}

		public void TestCheckIDCode()
		{
			var targetInfo = localProcessorAddress.IDCodeInfo;
			localProcessorAddress.E2_AddressOverride = true;
			localProcessorAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			localProcessorAddress.IDCode = new ZString('1', 9);
			AssertNoWarning(targetInfo, "The length of FRI must be 8.");

			localProcessorAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
			localProcessorAddress.Validation.ValidateIDCodeType();
			AssertHasWarning(targetInfo, "The length of FRI must be 8.");

			localProcessorAddress.IDCode = new ZString('1', 8);
			AssertNoWarning(targetInfo, "The length of FRI must be 8.");

			localProcessorAddress.IDCode = new ZString('1', 7);
			AssertHasWarning(targetInfo, "The length of FRI must be 8.");

			localProcessorAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
			localProcessorAddress.IDCode = "012345678901234";
			AssertHasMessageErrorContaining(targetInfo, "The length of PAS (Passport Number) shouldn't be more than 14.");

			localProcessorAddress.IDCode = "01234567890123";
			AssertNoMessageErrors(targetInfo);

			localProcessorAddress.IDCode = "NO1234";
			AssertHasMessageErrorContaining(targetInfo, "PAS number is formatted as NOxxxxxxxxx, but you only need to enter the suffix component of the number here.");
		}

		public void TestCheckIDCode_NX101_LocalProcessor()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var localProcessorAddress = cMHeader.LocalProcessorAddress;
			localProcessorAddress.E2_AddressOverride = true;
			var notification = "You have not entered a Local Processor ID: A valid TW-VAT or TW-PAS or TW-PID or TW-FRI number is required for Local Processor.";
			var validation = localProcessorAddress.Validation;
			validation.ValidateIDCode();
			AssertHasMessageError("All IDs are empty.", localProcessorAddress.IDCodeInfo, notification);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			validation.ValidateIDCode();
			AssertNoMessageError("ControllingMessageType is not NX101, all IDs are empty.", localProcessorAddress.IDCodeInfo, notification);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			localProcessorAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			localProcessorAddress.IDCode = "12345670";
			AssertNoMessageError("VAT has value.", localProcessorAddress.IDCodeInfo, notification);

			localProcessorAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
			localProcessorAddress.IDCode = "12345670";
			AssertNoMessageError("PAS has value.", localProcessorAddress.IDCodeInfo, notification);

			localProcessorAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.PID;
			localProcessorAddress.IDCode = "12345670";
			AssertNoMessageError("PID has value.", localProcessorAddress.IDCodeInfo, notification);

			localProcessorAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber;
			localProcessorAddress.IDCode = "12345670";
			AssertNoMessageError("FRI has value.", localProcessorAddress.IDCodeInfo, notification);

			localProcessorAddress.IDCodeType = MessageConstants.IdentificationTypeCodes.ZZZ;
			localProcessorAddress.IDCode = "12345670";
			AssertHasMessageError("ZZZ is not a valid ID type.", localProcessorAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			validation.ValidateIDCode();
			AssertNoMessageError("CertificateType is 09", localProcessorAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			validation.ValidateIDCode();
			AssertNoMessageError("CertificateType is 11", localProcessorAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			validation.ValidateIDCode();
			AssertNoMessageError("CertificateType is 13", localProcessorAddress.IDCodeInfo, notification);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			validation.ValidateIDCode();
			AssertNoMessageError("CertificateType is 14", localProcessorAddress.IDCodeInfo, notification);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			localProcessorAddress = controllingMessageHeader.LocalProcessorAddress;
		}

		#endregion

		JobDeclaration declaration;
		CusTWControllingMessageHeader controllingMessageHeader;
		TWJobDocAddress localProcessorAddress;
	}
}
