using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Testing.TWJobDocAddressTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(SupplierAddressRequirement))]
	sealed class SupplierAddressRequirementTest : BusinessObjectValidationTestCase
	{
		public void TestCheckE2_CompanyName()
		{
			var targetInfo = supplierDocumentaryAddress.E2_CompanyNameInfo;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_CompanyName = "A";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			supplierDocumentaryAddress.E2_CompanyName = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckE2_CompanyNameMaxLength()
		{
			var warningMessage = "Only the first 80 characters will be sent to the customs.";
			var targetInfo = supplierDocumentaryAddress.E2_CompanyNameInfo;
			supplierDocumentaryAddress.E2_CompanyName = new ZString('A', 90);
			AssertHasWarning(targetInfo, warningMessage);

			supplierDocumentaryAddress.E2_CompanyName = new ZString('A', 79);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_OA_AddressCompanyNameLength()
		{
			var foreignWarning = "Foreign Company Name Only the first 80 characters will be sent to the customs.";
			var chineseWarning = "Chinese Company Name Only the first 70 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = false;
			var targetInfo = supplierDocumentaryAddress.E2_OA_AddressInfo;

			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.CompanyName = new ZString('A', 80);
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = new ZString('A', 70);

			supplierDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			transAddress.CompanyName = new ZString('A', 71);
			supplierDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			supplierDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			mainAddress.CompanyName = new ZString('A', 81);
			supplierDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
		}

		public void TestCheckE2_OA_AddressAddressLength()
		{
			var foreignWarning = "Foreign Address Only the first 120 characters will be sent to the customs.";
			var chineseWarning = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = false;
			var targetInfo = supplierDocumentaryAddress.E2_OA_AddressInfo;

			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.Language = "EN";
			mainAddress.CompanyName = "CompanyName";
			mainAddress.City = "Taipei City";
			mainAddress.Postcode = "239";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.Address1 = new ZString('A', 50);
			mainAddress.Address2 = new ZString('B', 46);
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = "CompanyName";
			transAddress.City = "Taipei";
			transAddress.Postcode = "239";
			transAddress.Address1 = new ZString('C', 49);
			transAddress.Address2 = new ZString('D', 42);

			supplierDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			transAddress.Address2 = new ZString('A', 43);
			supplierDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			supplierDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			mainAddress.Address2 = new ZString('A', 47);
			supplierDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
		}

		public void TestCheckE2_Address1AddressLength()
		{
			var foreignWarning = "Foreign Address Only the first 120 characters will be sent to the customs.";
			var chineseWarning = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			var foreignTargetInfo = supplierDocumentaryAddress.E2_Address1Info;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_City = "Taipei";
			supplierDocumentaryAddress.E2_Postcode = "239";
			supplierDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			supplierDocumentaryAddress.AdditionalAddressInformation = "Add";
			supplierDocumentaryAddress.E2_Address2 = new ZString('A', 49);
			supplierDocumentaryAddress.E2_Address1 = new ZString('B', 48);

			var localAddress = supplierDocumentaryAddress.LocalAddress;
			var localTargetInfo = localAddress.E2_Address1Info;
			localAddress.E2_City = "Taipei";
			localAddress.E2_Postcode = "239";
			localAddress.E2_RN_NKCountryCode = "TW";
			localAddress.AdditionalAddressInformation = "Add";
			localAddress.Address2 = new ZString('D', 40);
			localAddress.Address1 = new ZString('C', 48);

			AssertNoWarning(foreignTargetInfo, foreignWarning);
			AssertNoWarning(localTargetInfo, chineseWarning);

			supplierDocumentaryAddress.Validation.ValidateE2_Address1();
			localAddress.Validation.ValidateE2_Address1();
			AssertNoWarning(foreignTargetInfo, foreignWarning);
			AssertNoWarning(localTargetInfo, chineseWarning);

			localAddress.Address1 = new ZString('C', 49);
			supplierDocumentaryAddress.Validation.ValidateE2_Address1();
			localAddress.Validation.ValidateE2_Address1();
			AssertNoWarning(foreignTargetInfo, foreignWarning);
			AssertHasWarning(localTargetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			setLocalAddress();
			supplierDocumentaryAddress.Validation.ValidateE2_Address1();
			localAddress.Validation.ValidateE2_Address1();
			AssertNoWarning(foreignTargetInfo, foreignWarning);
			AssertNoWarning(localTargetInfo, chineseWarning);

			supplierDocumentaryAddress.E2_Address1 = new ZString('B', 49);
			supplierDocumentaryAddress.Validation.ValidateE2_Address1();
			localAddress.Validation.ValidateE2_Address1();
			AssertHasWarning(foreignTargetInfo, foreignWarning);
			AssertNoWarning(localTargetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			setLocalAddress();
			supplierDocumentaryAddress.E2_Address1 = new ZString('B', 49);
			supplierDocumentaryAddress.Validation.ValidateE2_Address1();
			localAddress.Validation.ValidateE2_Address1();
			AssertHasWarning(foreignTargetInfo, foreignWarning);
			AssertHasWarning(localTargetInfo, chineseWarning);

			void setLocalAddress()
			{
				supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
				foreignTargetInfo = supplierDocumentaryAddress.E2_Address1Info;
				supplierDocumentaryAddress.E2_AddressOverride = true;
				supplierDocumentaryAddress.E2_City = "Taipei";
				supplierDocumentaryAddress.E2_Postcode = "239";
				supplierDocumentaryAddress.E2_RN_NKCountryCode = "TW";
				supplierDocumentaryAddress.AdditionalAddressInformation = "Add";
				supplierDocumentaryAddress.E2_Address2 = new ZString('A', 49);
				supplierDocumentaryAddress.E2_Address1 = new ZString('B', 48);

				localAddress = supplierDocumentaryAddress.LocalAddress;
				localTargetInfo = localAddress.E2_Address1Info;
				localAddress.E2_City = "Taipei";
				localAddress.E2_Postcode = "239";
				localAddress.E2_RN_NKCountryCode = "TW";
				localAddress.AdditionalAddressInformation = "Add";
				localAddress.Address2 = new ZString('D', 40);
				localAddress.Address1 = new ZString('C', 49);
			}
		}

		public void TestCheckOrganisationPK()
		{
			var targetInfo = supplierDocumentaryAddress.OrganisationPKInfo;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OrgTest1";
			org1.OH_RL_NKClosestPort = "TW";
			org1.OH_Code = "testCode";

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.E2_AddressOverride = false;

			var supplierMessage = "A valid TW-VAT or TW-PAS or TW-PID number is required for Supplier. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration.";
			supplierDocumentaryAddress.OrganisationPK = org1.PK;
			AssertNoMessageErrorContaining(targetInfo, supplierMessage);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining(targetInfo, supplierMessage);

			var orgHeader = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "123465788", "TW");

			supplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			AssertNoMessageErrorContaining(targetInfo, supplierMessage);
		}

		public void TestCheckE2_OA_Address()
		{
			var targetInfo = supplierDocumentaryAddress.E2_OA_AddressInfo;
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			supplierDocumentaryAddress.OrganisationPK = header.PK;

			var message1 = "Please select an English address.";
			var message2 = "You have not entered a Supplier Documentary Address: Address.";
			var addresse = header.Addresses[2];
			addresse.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			supplierDocumentaryAddress.E2_OA_Address = addresse.PK;
			supplierDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(targetInfo, message1);
			AssertNoMessageError(targetInfo, message2);

			addresse.OA_Language = Core.SharedConstants.Languages.English;
			supplierDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(targetInfo, message1);
			AssertNoMessageError(targetInfo, message2);

			supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			AssertNoMessageError(targetInfo, message1);
			AssertHasMessageError(targetInfo, message2);
		}

		public void TestCheckE2_Address1()
		{
			var targetInfo = supplierDocumentaryAddress.E2_Address1Info;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_Address1 = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.Validation.ValidateE2_Address1();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			supplierDocumentaryAddress.E2_Address1 = "Address1";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckE2_RN_NKCountryCode()
		{
			var targetInfo = supplierDocumentaryAddress.E2_RN_NKCountryCodeInfo;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_RN_NKCountryCode = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_RN_NKCountryCode = ZString.Empty;
			supplierDocumentaryAddress.Validation.ValidateE2_RN_NKCountryCode();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			supplierDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckIDCodeType()
		{
			var targetInfo = supplierDocumentaryAddress.IDCodeTypeInfo;
			var validation = supplierDocumentaryAddress.Validation;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.IDCodeType = "XX";
			AssertListValidationInvalidCodeError(targetInfo, true);
			supplierDocumentaryAddress.IDCodeType = "";
			AssertListValidationInvalidCodeError(targetInfo, false);
			foreach (var codeType in OrgHeaderHelper.IDCodeTypesWithCustomCode)
			{
				supplierDocumentaryAddress.IDCodeType = codeType;
				AssertListValidationInvalidCodeError(targetInfo, false);
			}

			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.IDCodeType = ZString.Empty;
			targetInfo = supplierDocumentaryAddress.IDCodeTypeInfo;

			validation.ValidateIDCodeType();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			validation.ValidateIDCodeType();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCBPCodeType()
		{
			var targetInfo = supplierDocumentaryAddress.CBPCodeTypeInfo;
			var validation = supplierDocumentaryAddress.Validation;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.CBPCodeType = "XX";
			AssertListValidationInvalidCodeError(targetInfo, true);
			supplierDocumentaryAddress.CBPCodeType = "";
			AssertListValidationInvalidCodeError(targetInfo, false);
			foreach (var codeType in OrgHeaderHelper.CBPCodeTypes)
			{
				supplierDocumentaryAddress.CBPCodeType = codeType;
				AssertListValidationInvalidCodeError(targetInfo, false);
			}

			CombineAssertions("Testing Supplier Bonded Type Is Correct", () =>
			{
				AssertSupplierBondedTypeIsCorrect(Enterprise.Customs.TW.Business.Constants.DeclarationTypes.Export.B2, "Supplier Bonded ID should be of type EPZ or CBF or ATP or SPK for Declaration Type B2.", new ZString[] { "FTZ", "CCP" }, new ZString[] { "EPZ", "CBF", "ATP", "SPK" });
				AssertSupplierBondedTypeIsCorrect(Enterprise.Customs.TW.Business.Constants.DeclarationTypes.Import.B6, "Supplier Bonded ID should be of type FTZ for Declaration Type B6.", new ZString[] { "EPZ", "CBF", "CCP", "ATP", "SPK" }, new ZString[] { "FTZ" });
				AssertSupplierBondedTypeIsCorrect(Enterprise.Customs.TW.Business.Constants.DeclarationTypes.Export.B8, "Supplier Bonded ID should be of type EPZ or CBF or ATP or SPK for Declaration Type B8.", new ZString[] { "FTZ", "CCP" }, new ZString[] { "EPZ", "CBF", "ATP", "SPK" });
				AssertSupplierBondedTypeIsCorrect(Enterprise.Customs.TW.Business.Constants.DeclarationTypes.Export.B9, "Supplier Bonded ID should be of type EPZ or CBF or ATP or SPK for Declaration Type B9.", new ZString[] { "FTZ", "CCP" }, new ZString[] { "EPZ", "CBF", "ATP", "SPK" });
				AssertSupplierBondedTypeIsCorrect(Enterprise.Customs.TW.Business.Constants.DeclarationTypes.Import.D8, "Supplier Bonded ID should be of type FTZ for Declaration Type D8.", new ZString[] { "EPZ", "CBF", "CCP", "ATP", "SPK" }, new ZString[] { "FTZ" });
				AssertSupplierBondedTypeIsCorrect(Enterprise.Customs.TW.Business.Constants.DeclarationTypes.Import.F2, "Supplier Bonded ID should be of type FTZ for Declaration Type F2.", new ZString[] { "EPZ", "CBF", "CCP", "ATP", "SPK" }, new ZString[] { "FTZ" });
				AssertSupplierBondedTypeIsCorrect(Enterprise.Customs.TW.Business.Constants.DeclarationTypes.Import.F3, "Supplier Bonded ID should be of type FTZ for Declaration Type F3.", new ZString[] { "EPZ", "CBF", "CCP", "ATP", "SPK" }, new ZString[] { "FTZ" });
				AssertSupplierBondedTypeIsCorrect(Enterprise.Customs.TW.Business.Constants.DeclarationTypes.Export.F4, "Supplier Bonded ID should be of type FTZ for Declaration Type F4.", new ZString[] { "EPZ", "CBF", "CCP", "ATP", "SPK" }, new ZString[] { "FTZ" });
				AssertSupplierBondedTypeIsCorrect(Enterprise.Customs.TW.Business.Constants.DeclarationTypes.Export.F5, "Supplier Bonded ID should be of type FTZ for Declaration Type F5.", new ZString[] { "EPZ", "CBF", "CCP", "ATP", "SPK" }, new ZString[] { "FTZ" });
				AssertSupplierBondedTypeIsCorrect(Enterprise.Customs.TW.Business.Constants.DeclarationTypes.Import.G2, "Supplier Bonded ID should be of type EPZ or CBF or ATP or SPK for Declaration Type G2.", new ZString[] { "FTZ", "CCP" }, new ZString[] { "EPZ", "CBF", "ATP", "SPK" });
			});

			void AssertSupplierBondedTypeIsCorrect(ZString ceiStyle, ZString error, ZString[] hasErrorCodeTypes, ZString[] noErrorCodeTypes)
			{
				cusEntryInstruction.CEI_Style = ceiStyle;
				foreach (ZString hasErrorCodeType in hasErrorCodeTypes)
				{
					supplierDocumentaryAddress.CBPCodeType = hasErrorCodeType;
					supplierDocumentaryAddress.CBPCode = "12345678";
					validation.ValidateCBPCodeType();
					AssertHasMessageError(targetInfo, error);
				}

				foreach (ZString noErrorCodeType in noErrorCodeTypes)
				{
					supplierDocumentaryAddress.CBPCodeType = noErrorCodeType;
					supplierDocumentaryAddress.CBPCode = "12345678";
					validation.ValidateCBPCodeType();
					AssertNoMessageError(targetInfo, error);
				}
			}
		}

		public void TestCheckIDCode()
		{
			var targetInfo = supplierDocumentaryAddress.IDCodeInfo;
			var validation = supplierDocumentaryAddress.Validation;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;

			supplierDocumentaryAddress.IDCode = ZString.Empty;
			validation.ValidateIDCode();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			validation.ValidateIDCode();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCBPCodeShouldNotEnter()
		{
			var formatMessage = "Supplier Bonded ID is not required for Declaration Type {0}.";
			var targetInfo = supplierDocumentaryAddress.CBPCodeInfo;

			supplierDocumentaryAddress.E2_AddressOverride = true;
			CombineAssertions("Testing Supplier Bonded ID Should Not Enter", () =>
			{
				AssertSupplierBondedIDShouldNotEnter(Constants.DeclarationTypes.Export.B1);
				AssertSupplierBondedIDShouldNotEnter(Constants.DeclarationTypes.Export.D1);
				AssertSupplierBondedIDShouldNotEnter(Constants.DeclarationTypes.Import.D2);
				AssertSupplierBondedIDShouldNotEnter(Constants.DeclarationTypes.Export.D5);
				AssertSupplierBondedIDShouldNotEnter(Constants.DeclarationTypes.Import.D7);
				AssertSupplierBondedIDShouldNotEnter(Constants.DeclarationTypes.Import.L1);
				AssertSupplierBondedIDShouldNotEnter(Constants.DeclarationTypes.Import.F1);
				AssertSupplierBondedIDShouldNotEnter(Constants.DeclarationTypes.Import.G1);
				AssertSupplierBondedIDShouldNotEnter(Constants.DeclarationTypes.Import.G7);
			});

			supplierDocumentaryAddress.E2_AddressOverride = false;
			CombineAssertions("Testing Supplier Bonded ID Should Not Show Message When Not overriden", () =>
			{
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Export.B1);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Export.D1);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Import.D2);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Export.D5);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Import.D7);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Import.L1);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Import.F1);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Import.G1);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Import.G7);
			});

			void AssertSupplierBondedIDShouldNotEnter(ZString ceiStyle)
			{
				var error = string.Format(formatMessage, ceiStyle);
				cusEntryInstruction.CEI_Style = ceiStyle;
				supplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
				supplierDocumentaryAddress.CBPCode = "12345678";
				AssertHasMessageError(targetInfo, error);

				supplierDocumentaryAddress.CBPCodeType = ZString.Empty;
				supplierDocumentaryAddress.CBPCode = ZString.Empty;
				AssertNoMessageError(targetInfo, error);
			}

			void AssertNoMessageIsShownWhenNotOverride(ZString ceiStyle)
			{
				var error = string.Format(formatMessage, ceiStyle);
				cusEntryInstruction.CEI_Style = ceiStyle;
				supplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
				supplierDocumentaryAddress.CBPCode = "12345678";
				AssertNoMessageError(targetInfo, error);
			}
		}

		public void TestCheckCBPCodeIsRequired()
		{
			var targetInfo = supplierDocumentaryAddress.CBPCodeInfo;
			supplierDocumentaryAddress.E2_AddressOverride = true;

			CombineAssertions("Testing Supplier Bonded ID is required when goods are coming from a Free Trade Zone", () =>
			{
				AssertSupplierBondedIDIsRequiredFreeTradeZone(Constants.DeclarationTypes.Import.F3);
				AssertSupplierBondedIDIsRequiredFreeTradeZone(Constants.DeclarationTypes.Export.F4);
			});

			void AssertSupplierBondedIDIsRequiredFreeTradeZone(ZString ceiStyle)
			{
				declaration.JE_RL_NKOrigin = "TWTPE";
				var error = string.Format("Supplier Bonded ID is required for Declaration Type {0} when goods are coming from a Free Trade Zone.", ceiStyle);
				cusEntryInstruction.CEI_Style = ceiStyle;
				supplierDocumentaryAddress.CBPCodeType = "EPZ";
				supplierDocumentaryAddress.CBPCode = ZString.Empty;
				AssertHasMessageError(targetInfo, error);

				supplierDocumentaryAddress.CBPCode = "12345678";
				AssertNoMessageError(targetInfo, error);

				declaration.JE_RL_NKOrigin = "USLAX";
				supplierDocumentaryAddress.CBPCode = ZString.Empty;
				AssertNoMessageError(targetInfo, error);
			}

			CombineAssertions("Testing Supplier Bonded ID is required", () =>
			{
				AssertSupplierBondedIDIsRequired(targetInfo, Constants.DeclarationTypes.Import.G2);
				AssertSupplierBondedIDIsRequired(targetInfo, Constants.DeclarationTypes.Export.B2);
				AssertSupplierBondedIDIsRequired(targetInfo, Constants.DeclarationTypes.Import.F2);
				AssertSupplierBondedIDIsRequired(targetInfo, Constants.DeclarationTypes.Import.F3);
			});

			CombineAssertions("Testing Supplier Boned ID might be required when goods are coming from a Free Trade Zone.", () =>
			{
				AssertSupplierBondedIDMightBeRequiredFreeTradeZone(Constants.DeclarationTypes.Import.D8);
				AssertSupplierBondedIDMightBeRequiredFreeTradeZone(Constants.DeclarationTypes.Import.B6);
			});

			void AssertSupplierBondedIDMightBeRequiredFreeTradeZone(ZString ceiStyle)
			{
				declaration.JE_RL_NKOrigin = "TWTPE";
				var error = string.Format("Supplier Boned ID might be required for Declaration Type {0} when goods are coming from a Free Trade Zone.", ceiStyle);
				cusEntryInstruction.CEI_Style = ceiStyle;
				supplierDocumentaryAddress.CBPCodeType = "EPZ";
				supplierDocumentaryAddress.CBPCode = ZString.Empty;
				AssertHasWarning(targetInfo, error);

				supplierDocumentaryAddress.CBPCode = "12345678";
				AssertNoWarning(targetInfo, error);

				declaration.JE_RL_NKOrigin = "USLAX";
				supplierDocumentaryAddress.CBPCode = ZString.Empty;
				AssertNoWarning(targetInfo, error);
			}
		}

		void AssertSupplierBondedIDIsRequired(ZPropertyInfo targetInfo, ZString ceiStyle)
		{
			declaration.JE_RL_NKFinalDestination = "USLAX";
			var error = string.Format("Supplier Bonded ID is required for Declaration Type {0}.", ceiStyle);
			cusEntryInstruction.CEI_Style = ceiStyle;
			supplierDocumentaryAddress.CBPCodeType = "EPZ";
			supplierDocumentaryAddress.CBPCode = ZString.Empty;
			AssertHasMessageError(targetInfo, error);

			supplierDocumentaryAddress.CBPCode = "12345678";
			AssertNoMessageError(targetInfo, error);
		}

		public void TestCheckCBPCodeCanNotCoExistOrEmpty()
		{
			var targetInfo = supplierDocumentaryAddress.CBPCodeInfo;
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.CBPCodeType = "EPZ";
			AssertSupplierBondedIDCanNotCoExistOrEmptyWithBondedFactories(Constants.DeclarationTypes.Export.B8);
			AssertSupplierBondedIDCanNotCoExistOrEmptyWithBondedFactories(Constants.DeclarationTypes.Export.B9);
			AssertSupplierBondedIDCanNotCoExistOrEmptyWithBondedFactories(Constants.DeclarationTypes.Export.F5);

			void AssertSupplierBondedIDCanNotCoExistOrEmptyWithBondedFactories(ZString ceiStyle)
			{
				var error = ValidationConstants.TWJobDocAddress.SupplierBondedIdCanNotCoExistOrEmptyWithBondedFactories;
				cusEntryInstruction.CEI_Style = ceiStyle;
				var bondedFactory = declaration.BondedFactories.AddNew();
				bondedFactory.OrganisationPK = testOrg1.PK;
				bondedFactory.E2_OA_Address = testOrg1.MainAddress.PK;
				supplierDocumentaryAddress.CBPCode = ZString.Empty;
				AssertNoMessageError(targetInfo, error);

				supplierDocumentaryAddress.CBPCode = "12345678";
				AssertHasMessageError(targetInfo, error);

				declaration.BondedFactories.DeleteAll();
				supplierDocumentaryAddress.Validation.ValidateCBPCode();
				AssertNoMessageError(targetInfo, error);

				supplierDocumentaryAddress.CBPCode = ZString.Empty;
				AssertHasMessageError(targetInfo, error);
			}
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

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			cusEntryInstruction = declaration.CusEntryInstruction;
			supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
		}

		#endregion

		JobDeclarationForJobDocAddressTest declaration;
		CusEntryInstruction cusEntryInstruction;
		TWJobDocAddress supplierDocumentaryAddress;
	}
}
