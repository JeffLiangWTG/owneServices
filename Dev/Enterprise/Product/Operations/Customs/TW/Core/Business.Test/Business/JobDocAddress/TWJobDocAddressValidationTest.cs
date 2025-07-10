using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using static Enterprise.Customs.TW.Business.Testing.TWJobDocAddressTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestBondIdCodeTypes()
		{
			CombineAssertions("BondInCodeTypesList", () =>
			{
				AssertEquals(TWJobDocAddressValidation.BondIdCodeTypes.Count(), 4);

				Assert(TWJobDocAddressValidation.BondIdCodeTypes.SequenceEqual(new ZString[] { OrgCusCode.TaiwanCodeTypes.EPZ, OrgCusCode.TaiwanCodeTypes.CBF, OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, OrgCusCode.TaiwanCodeTypes.SciencePark }));
			});
		}

		public void TestCheckIDCodeType()
		{
			var docAddress = Factory.New<TWJobDocAddress>();
			var targetInfo = docAddress.IDCodeTypeInfo;
			docAddress.E2_AddressOverride = true;
			docAddress.IDCodeType = "XX";
			AssertListValidationInvalidCodeError(targetInfo, true);
			docAddress.IDCodeType = "";
			AssertListValidationInvalidCodeError(targetInfo, false);
			foreach (var codeType in OrgHeaderHelper.IDCodeTypesWithCustomCode)
			{
				docAddress.IDCodeType = codeType;
				AssertListValidationInvalidCodeError(targetInfo, false);
			}
		}

		public void TestCheckCBPCodeType()
		{
			var docAddress = Factory.New<TWJobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.CBPCodeType = "XX";
			AssertListValidationInvalidCodeError(docAddress.CBPCodeTypeInfo, true);
			docAddress.CBPCodeType = "";
			AssertListValidationInvalidCodeError(docAddress.CBPCodeTypeInfo, false);
			foreach (var codeType in OrgHeaderHelper.CBPCodeTypes)
			{
				docAddress.CBPCodeType = codeType;
				AssertListValidationInvalidCodeError(docAddress.CBPCodeTypeInfo, false);
			}
		}

		public void TestCheckFRICodeType()
		{
			var manufacturerAddress = Factory.New<TWJobDocAddress>();
			manufacturerAddress.DocAddressType = DocAddressType.Manufacturer;
			manufacturerAddress.E2_AddressOverride = true;
			manufacturerAddress.FRICodeType = "XX";
			AssertListValidationInvalidCodeError(manufacturerAddress.FRICodeTypeInfo, true);
			manufacturerAddress.FRICodeType = "";
			AssertListValidationInvalidCodeError(manufacturerAddress.FRICodeTypeInfo, false);
			manufacturerAddress.FRICodeType = "FRI";
			AssertListValidationInvalidCodeError(manufacturerAddress.FRICodeTypeInfo, false);
			manufacturerAddress.FRICodeType = "ZZZ";
			AssertListValidationInvalidCodeError(manufacturerAddress.FRICodeTypeInfo, false);
		}

		public void TestCheckAEOCode()
		{
			var docAddress = Factory.New<TWJobDocAddress>();
			var targetInfo = docAddress.AEOCodeInfo;
			docAddress.E2_AddressOverride = true;
			docAddress.AEOCode = "111111111111111";
			AssertHasWarningContaining(targetInfo, "Only the first 14 characters will be sent to the customs.");
			docAddress.AEOCode = "11111111111111";
			AssertNoWarningContaining(targetInfo, "Only the first 14 characters will be sent to the customs.");
		}

		public void TestCheckE2_RN_NKCountryCode()
		{
			var docAddress = Factory.New<TWJobDocAddress>();
			var targetInfo = docAddress.E2_RN_NKCountryCodeInfo;
			docAddress.E2_AddressOverride = true;
			docAddress.AEOCode = "111111111111111";
			docAddress.E2_RN_NKCountryCode = ZString.Empty;
			AssertHasErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			docAddress.E2_RN_NKCountryCode = "TW";
			docAddress.Validation.ValidateE2_RN_NKCountryCode();
			AssertNoErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckTPCCode()
		{
			var docAddress = Factory.New<TWJobDocAddress>();
			var targetInfo = docAddress.TPCCodeInfo;
			docAddress.E2_AddressOverride = true;
			docAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			docAddress.TPCCode = "123456789";
			AssertHasErrorContaining(targetInfo, "The length of TPC shouldn't be more than 8.");
			docAddress.TPCCode = "12345678";
			AssertNoErrors(targetInfo);
			docAddress.TPCCode = "123";
			AssertNoErrors(targetInfo);
		}

		public void TestCheckIDCode()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "OrgTest1";
			header.OH_RL_NKClosestPort = "TW";
			var mainAddress = header.MainAddress;
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";
			mainAddress.OA_RN_NKCountryCode = "TW";
			header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "NO123465789", "TW");
			Factory.Save();

			var docAddress = Factory.New<TWJobDocAddress>();
			var targetInfo = docAddress.IDCodeInfo;
			docAddress.E2_AddressOverride = true;
			docAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			docAddress.IDCode = "12";
			AssertHasMessageErrorContaining(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			docAddress.IDCode = "12345678";
			AssertNoMessageErrorContaining(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
			AssertHasMessageErrorContaining(targetInfo, "The Taiwan VAT number is invalid.");
			docAddress.IDCode = "12345675";
			AssertNoMessageErrorContaining(targetInfo, "The Taiwan VAT number is invalid.");

			docAddress.IDCodeType = OrgCusCode.CodeTypes.PassportID;
			docAddress.IDCode = "012345678901234";
			AssertHasMessageErrorContaining(targetInfo, "The length of PAS (Passport Number) shouldn't be more than 14.");
			docAddress.IDCode = "01234567890123";
			AssertNoMessageErrorContaining(targetInfo, "The length of PAS (Passport Number) shouldn't be more than 14.");
			AssertNoMessageErrorContaining(targetInfo, "PAS number is formatted as NOxxxxxxxxx, but you only need to enter the suffix component of the number here.");

			docAddress.IDCode = "NO1234";
			AssertHasMessageErrorContaining(targetInfo, "PAS number is formatted as NOxxxxxxxxx, but you only need to enter the suffix component of the number here.");

			docAddress.IDCodeType = OrgCusCode.TaiwanCodeTypes.PID;
			docAddress.IDCode = "01234567891";
			AssertHasMessageError(targetInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");
			docAddress.IDCode = "0123456789";
			AssertNoMessageError(targetInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");

			docAddress.IDCodeType = Constants.OrgCusCodeType.CustomCode;
			docAddress.IDCode = "11111111";
			AssertNoError(targetInfo, "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10.");

			docAddress.OrganisationPK = header.PK;
			docAddress.E2_OA_Address = mainAddress.PK;
			docAddress.E2_AddressOverride = false;
			docAddress.Validation.ValidateIDCode();
			AssertHasMessageErrorContaining(targetInfo, "PAS number is formatted as NOxxxxxxxxx, but you only need to enter the suffix component of the number here.");
		}

		public void TestCheckIDCodeWhenCountryCodeChange()
		{
			var docAddress = Factory.New<TWJobDocAddress>();
			var targetInfo = docAddress.IDCodeInfo;
			docAddress.E2_AddressOverride = true;
			docAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			docAddress.E2_RN_NKCountryCode = "";
			docAddress.IDCode = "012345678912345";
			AssertNoMessageErrors(targetInfo);

			docAddress.E2_RN_NKCountryCode = "TW";
			AssertHasMessageError(targetInfo, "The Taiwan VAT number must be an 8-digit number.");
		}

		public void TestCheckCBPCode()
		{
			var docAddress = Factory.New<TWJobDocAddress>();
			var targetInfo = docAddress.CBPCodeInfo;
			docAddress.E2_AddressOverride = true;
			docAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.CBF;
			docAddress.CBPCode = "1234";
			AssertHasMessageErrorContaining(targetInfo, "A Bonded Factory Customs Controlling Premises Code must be 5 characters long.");
			docAddress.CBPCode = "1234+";
			AssertHasMessageErrorContaining(targetInfo, "A CBF code can only have alphanumeric characters.");
			docAddress.CBPCode = "12345";
			AssertNoMessageErrors(targetInfo);

			docAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
			docAddress.CBPCode = "1234";
			AssertHasMessageErrorContaining(targetInfo, "An Export Processing Zone Customs Controlling Premises Code must be 5 characters long.");
			docAddress.CBPCode = "123-5";
			AssertHasMessageErrorContaining(targetInfo, "The 4th and the 5th characters must be numbers.");
			docAddress.CBPCode = "12345";
			AssertNoMessageErrors(targetInfo);

			docAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			docAddress.CBPCode = "AB12";
			AssertHasMessageErrorContaining(targetInfo, "A Free Trade Zone Customs Controlling Premises Code must be 5 characters long.");
			docAddress.CBPCode = "ABC123";
			AssertHasMessageErrorContaining(targetInfo, "A Free Trade Zone Customs Controlling Premises Code must be 5 characters long.");
			docAddress.CBPCode = "ABCDE";
			AssertHasMessageErrorContaining(targetInfo, "The first character of FTZ code should be one of \"W\", \"X\", \"Y\", \"Z\", \"P\", \"Q\", \"R\", \"S\".");
			docAddress.CBPCode = "WBCDE";
			AssertNoMessageErrors(targetInfo);

			docAddress.CBPCodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			docAddress.CBPCode = "AB12";
			AssertHasMessageErrorContaining(targetInfo, "A Customs Bonded Warehouse Customs Controlling Premises Code must be 5 or 8 characters long.");
			docAddress.CBPCode = "AB123";
			AssertHasMessageErrorContaining(targetInfo, "The second character a CPW code should be 'G' for Non-Personal Warehouses and 'D' for Personal Warehouses.");
			docAddress.CBPCode = "1DEEE";
			AssertHasMessageErrorContaining(targetInfo, "This Registration Number is invalid.");
			docAddress.CBPCode = "ADEE+";
			AssertHasMessageErrorContaining(targetInfo, "A CPW code can only have alphanumeric characters.");
			docAddress.CBPCode = "ADEEE";
			AssertNoMessageErrors(targetInfo);
			docAddress.CBPCode = "ED1AD123";
			AssertHasMessageErrorContaining(targetInfo, "The first three characters of a CPW code must be numeric digits.");
			docAddress.CBPCode = "121AD123";
			AssertNoMessageErrors(targetInfo);

			docAddress.CBPCodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			docAddress.CBPCode = "ABC123456";
			AssertHasMessageErrorContaining(targetInfo, "A Customs Logistic Center Customs Controlling Premises Code must be 5 or 8 characters long.");
			docAddress.CBPCode = "ABCD1234";
			AssertHasMessageErrorContaining(targetInfo, "The fifth character a CCP code should be 'L'.");
			docAddress.CBPCode = "EEEELEEE";
			AssertHasMessageErrorContaining(targetInfo, "This Registration Number is invalid.");
			docAddress.CBPCode = "EEAALEE+";
			AssertHasMessageErrorContaining(targetInfo, "A CCP code can only have alphanumeric characters.");
			docAddress.CBPCode = "EEAALEEE";
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckFRICode()
		{
			var manufacturerAddress = Factory.New<TWJobDocAddress>();
			var targetInfo = manufacturerAddress.FRICodeInfo;
			manufacturerAddress.DocAddressType = DocAddressType.Manufacturer;
			manufacturerAddress.E2_AddressOverride = true;
			manufacturerAddress.FRICodeType = "FRI";
			manufacturerAddress.FRICode = "1234567";
			AssertHasMessageErrorContaining(targetInfo, "The length of FRI must be 8.");
			manufacturerAddress.FRICode = "123456789";
			AssertHasMessageErrorContaining(targetInfo, "The length of FRI must be 8.");
			manufacturerAddress.FRICode = "12345678";
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckE2_ValidationStatus()
		{
			var docAddress = Factory.New<TWJobDocAddress>();
			var targetInfo = docAddress.E2_ValidationStatusInfo;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = "TW";
			docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;

			Env.Instance.Registry.EnableAddressValidationWebService = true;
			var collection = new AddressValidationDisabledCountryItemCollection();
			var item = collection.AddNew();
			item.CountryPK = docAddress.Country.PK;
			item.DisabledForOverrideAddress = false;

			docAddress.Validation.ValidateE2_ValidationStatus();
			AssertNoErrors(targetInfo);
		}

		public void TestCheckE2_Address1()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.OrganisationPK = header.PK;
			importerDocumentaryAddress.E2_AddressOverride = true;
			var localAddress = importerDocumentaryAddress.LocalAddress;
			var localAddressValidation = localAddress.Validation;
			var targetInfo = localAddress.E2_Address1Info;
			var warningMessage = "Chinese Address Only the first 100 characters will be sent to the customs.";
			localAddressValidation.ValidateE2_Address1();
			AssertNoWarningContaining(targetInfo, warningMessage);

			localAddress.AdditionalAddressInformation = new string('A', 50);
			localAddress.Address2 = new string('A', 50);
			localAddress.Address1 = new string('A', 50);
			AssertHasWarningContaining(targetInfo, warningMessage);

			localAddress.E2_AddressType = DocAddressTypes.Codes.SupplierTranslatedDocumentaryAddress;
			localAddressValidation.ValidateE2_Address1();
			AssertHasWarningContaining(targetInfo, warningMessage);

			localAddress.E2_AddressType = DocAddressTypes.Codes.BuyerTranslatedDocumentaryAddress;
			localAddressValidation.ValidateE2_Address1();
			AssertHasWarningContaining(targetInfo, warningMessage);

			localAddress.E2_AddressType = DocAddressTypes.Codes.LocalProcessorTranslatedDocAddress;
			localAddressValidation.ValidateE2_Address1();
			AssertHasWarningContaining(targetInfo, warningMessage);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			localAddressValidation.ValidateE2_Address1();
			AssertNoWarningContaining(targetInfo, warningMessage);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			localAddress.E2_AddressOverride = false;
			localAddressValidation.ValidateE2_Address1();
			AssertNoWarningContaining(targetInfo, warningMessage);

			localAddress.E2_AddressOverride = true;
			localAddress.E2_OA_Address = ZGuid.Empty;
			localAddressValidation.ValidateE2_Address1();
			AssertNoWarningContaining(targetInfo, warningMessage);
		}

		public void TestCheckE2_Address1_ManufacturerTranslatedDocumentaryAddress()
		{
			var message = "You have not entered a Manufacturer Local Address.";
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var cMHeader = instruction.ControllingMessageHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoicelineLinkCMHeader = invoiceLine.InvoiceLineLinkControllingMsgHeaders[0];
			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			manufacturerDocAddress.E2_AddressOverride = true;
			var localAddress = invoiceLine.ManufacturerDocAddress.LocalAddress;
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			localAddress.Validation.ValidateE2_Address1();
			AssertHasMessageError(localAddress.E2_Address1Info, message);

			invoicelineLinkCMHeader.IsLinkedCMHeader = false;
			localAddress.Validation.ValidateE2_Address1();
			AssertNoMessageError("No linked CMHeader", localAddress.E2_Address1Info, message);

			invoicelineLinkCMHeader.IsLinkedCMHeader = true;
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			localAddress.Validation.ValidateE2_Address1();
			AssertNoMessageError("CMHeader message type is not NX101", localAddress.E2_Address1Info, message);

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			manufacturerDocAddress.E2_Address1 = "Company Address";
			localAddress.Validation.ValidateE2_Address1();
			AssertNoMessageError("Company Address has value", localAddress.E2_Address1Info, message);

			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			localAddress.Validation.ValidateE2_Address1();
			AssertHasMessageError("Certificate Type is 15", localAddress.E2_Address1Info, message);

			localAddress.E2_Address1 = "公司地址";
			AssertNoMessageError("Local Company Address has value", localAddress.E2_Address1Info, message);
		}
	}
}
