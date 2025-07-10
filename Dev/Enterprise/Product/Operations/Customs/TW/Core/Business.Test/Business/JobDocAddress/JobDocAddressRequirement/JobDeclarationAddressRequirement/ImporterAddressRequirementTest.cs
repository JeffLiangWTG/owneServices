using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Testing.TWJobDocAddressTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ImporterAddressRequirement))]
	sealed class ImporterAddressRequirementTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOrganisationPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			var targetInfo = importerDocumentaryAddress.OrganisationPKInfo;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OrgTest1";
			org1.OH_RL_NKClosestPort = "TW";
			org1.OH_Code = "testCode";

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			importerDocumentaryAddress.E2_AddressOverride = false;
			var importerMessage = "A valid TW-VAT or TW-PAS or TW-PID number is required for Importer. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration.";
			importerDocumentaryAddress.OrganisationPK = org1.PK;
			AssertNoMessageErrorContaining(targetInfo, importerMessage);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining(targetInfo, importerMessage);

			var orgHeader = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "123465788", "TW");

			importerDocumentaryAddress.OrganisationPK = orgHeader.PK;
			AssertNoMessageErrorContaining(targetInfo, importerMessage);
		}

		public void TestCheckE2_CompanyName()
		{
			var targetInfo = importerDocumentaryAddress.E2_CompanyNameInfo;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_CompanyName = "A";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			importerDocumentaryAddress.E2_CompanyName = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckE2_CompanyNameMaxLength()
		{
			var warningMessage = "Only the first 80 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			var targetInfo = importerDocumentaryAddress.E2_CompanyNameInfo;
			importerDocumentaryAddress.E2_CompanyName = new ZString('A', 90);
			AssertHasWarning(targetInfo, warningMessage);

			importerDocumentaryAddress.E2_CompanyName = new ZString('A', 79);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_OA_AddressCompanyNameLength()
		{
			var foreignWarning = "Foreign Company Name Only the first 80 characters will be sent to the customs.";
			var chineseWarning = "Chinese Company Name Only the first 70 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = false;
			var targetInfo = importerDocumentaryAddress.E2_OA_AddressInfo;

			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.CompanyName = new ZString('A', 80);
			var transAddress = mainAddress.TranslatedAddresses.AddNew();
			transAddress.Language = "ZH-TW";
			transAddress.CompanyName = new ZString('A', 70);

			importerDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			importerDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			transAddress.CompanyName = new ZString('A', 71);
			importerDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			importerDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			mainAddress.CompanyName = new ZString('A', 81);
			importerDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			importerDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
		}

		public void TestCheckE2_OA_AddressAddressLength()
		{
			var foreignWarning = "Foreign Address Only the first 120 characters will be sent to the customs.";

			var chineseWarning = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = false;
			var targetInfo = importerDocumentaryAddress.E2_OA_AddressInfo;

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

			importerDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			importerDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			transAddress.Address2 = new ZString('A', 43);
			importerDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			importerDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			mainAddress.Address2 = new ZString('A', 47);
			importerDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertNoWarning(targetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			importerDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertHasWarning(targetInfo, foreignWarning);
			AssertHasWarning(targetInfo, chineseWarning);
		}

		public void TestCheckE2_Address1AddressLength()
		{
			var foreignWarning = "Foreign Address Only the first 120 characters will be sent to the customs.";
			var chineseWarning = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			var foreignTargetInfo = importerDocumentaryAddress.E2_Address1Info;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_City = "Taipei";
			importerDocumentaryAddress.E2_Postcode = "239";
			importerDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			importerDocumentaryAddress.AdditionalAddressInformation = "Add";
			importerDocumentaryAddress.E2_Address2 = new ZString('A', 49);
			importerDocumentaryAddress.E2_Address1 = new ZString('B', 48);

			var localAddress = importerDocumentaryAddress.LocalAddress;
			var localTargetInfo = localAddress.E2_Address1Info;
			localAddress.E2_City = "Taipei";
			localAddress.E2_Postcode = "239";
			localAddress.E2_RN_NKCountryCode = "TW";
			localAddress.AdditionalAddressInformation = "Add";
			localAddress.Address2 = new ZString('D', 40);
			localAddress.Address1 = new ZString('C', 48);

			AssertNoWarning(foreignTargetInfo, foreignWarning);
			AssertNoWarning(localTargetInfo, chineseWarning);

			importerDocumentaryAddress.Validation.ValidateE2_Address1();
			localAddress.Validation.ValidateE2_Address1();
			AssertNoWarning(foreignTargetInfo, foreignWarning);
			AssertNoWarning(localTargetInfo, chineseWarning);

			localAddress.Address1 = new ZString('C', 49);
			importerDocumentaryAddress.Validation.ValidateE2_Address1();
			localAddress.Validation.ValidateE2_Address1();
			AssertNoWarning(foreignTargetInfo, foreignWarning);
			AssertHasWarning(localTargetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Export;
			setLocalAddress();
			importerDocumentaryAddress.Validation.ValidateE2_Address1();
			localAddress.Validation.ValidateE2_Address1();
			AssertNoWarning(foreignTargetInfo, foreignWarning);
			AssertNoWarning(localTargetInfo, chineseWarning);

			importerDocumentaryAddress.E2_Address1 = new ZString('B', 49);
			importerDocumentaryAddress.Validation.ValidateE2_Address1();
			localAddress.Validation.ValidateE2_Address1();
			AssertHasWarning(foreignTargetInfo, foreignWarning);
			AssertNoWarning(localTargetInfo, chineseWarning);

			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			setLocalAddress();
			importerDocumentaryAddress.E2_Address1 = new ZString('B', 49);
			importerDocumentaryAddress.Validation.ValidateE2_Address1();
			localAddress.Validation.ValidateE2_Address1();
			AssertHasWarning(foreignTargetInfo, foreignWarning);
			AssertHasWarning(localTargetInfo, chineseWarning);

			void setLocalAddress()
			{
				importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
				foreignTargetInfo = importerDocumentaryAddress.E2_Address1Info;
				importerDocumentaryAddress.E2_AddressOverride = true;
				importerDocumentaryAddress.E2_City = "Taipei";
				importerDocumentaryAddress.E2_Postcode = "239";
				importerDocumentaryAddress.E2_RN_NKCountryCode = "TW";
				importerDocumentaryAddress.AdditionalAddressInformation = "Add";
				importerDocumentaryAddress.E2_Address2 = new ZString('A', 49);
				importerDocumentaryAddress.E2_Address1 = new ZString('B', 48);

				localAddress = importerDocumentaryAddress.LocalAddress;
				localTargetInfo = localAddress.E2_Address1Info;
				localAddress.E2_City = "Taipei";
				localAddress.E2_Postcode = "239";
				localAddress.E2_RN_NKCountryCode = "TW";
				localAddress.AdditionalAddressInformation = "Add";
				localAddress.Address2 = new ZString('D', 40);
				localAddress.Address1 = new ZString('C', 49);
			}
		}

		public void TestCheckE2_OA_Address()
		{
			var targetInfo = importerDocumentaryAddress.E2_OA_AddressInfo;
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			importerDocumentaryAddress.OrganisationPK = header.PK;

			var message1 = "Please select an English address.";
			var message2 = "You have not entered an Importer Documentary Address: Address.";
			var addresse = header.Addresses[2];
			addresse.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			importerDocumentaryAddress.E2_OA_Address = addresse.PK;
			importerDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(targetInfo, message1);
			AssertNoMessageError(targetInfo, message2);

			addresse.OA_Language = Core.SharedConstants.Languages.English;
			importerDocumentaryAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(targetInfo, message1);
			AssertNoMessageError(targetInfo, message2);

			importerDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			AssertNoMessageError(targetInfo, message1);
			AssertHasMessageError(targetInfo, message2);
		}

		public void TestCheckE2_RN_NKCountryCode()
		{
			var targetInfo = importerDocumentaryAddress.E2_RN_NKCountryCodeInfo;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_RN_NKCountryCode = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.Validation.ValidateE2_RN_NKCountryCode();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			importerDocumentaryAddress.E2_RN_NKCountryCode = "TW";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckIDCodeType()
		{
			var targetInfo = importerDocumentaryAddress.IDCodeTypeInfo;
			var validation = importerDocumentaryAddress.Validation;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.IDCodeType = "XX";
			AssertListValidationInvalidCodeError(targetInfo, true);
			importerDocumentaryAddress.IDCodeType = "";
			AssertListValidationInvalidCodeError(targetInfo, false);
			foreach (var codeType in OrgHeaderHelper.IDCodeTypesWithCustomCode)
			{
				importerDocumentaryAddress.IDCodeType = codeType;
				AssertListValidationInvalidCodeError(targetInfo, codeType == Constants.OrgCusCodeType.CustomCode);
			}

			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.IDCodeType = ZString.Empty;
			targetInfo = importerDocumentaryAddress.IDCodeTypeInfo;

			validation.ValidateIDCodeType();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			validation.ValidateIDCodeType();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCBPCodeType()
		{
			var targetInfo = importerDocumentaryAddress.CBPCodeTypeInfo;
			var validation = importerDocumentaryAddress.Validation;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.CBPCodeType = "XX";
			AssertListValidationInvalidCodeError(targetInfo, true);
			importerDocumentaryAddress.CBPCodeType = "";
			AssertListValidationInvalidCodeError(targetInfo, false);
			foreach (var codeType in OrgHeaderHelper.CBPCodeTypes)
			{
				importerDocumentaryAddress.CBPCodeType = codeType;
				AssertListValidationInvalidCodeError(targetInfo, false);
			}

			CombineAssertions("Testing Importer Bonded Type Is Correct", () =>
			{
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.B1, "Importer Bonded ID should be of type EPZ or CBF or ATP or SPK for Declaration Type B1.", new ZString[] { "FTZ", "CCP" }, new ZString[] { "EPZ", "CBF", "ATP", "SPK" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.B2, "Importer Bonded ID should be of type EPZ or CBF or ATP or SPK for Declaration Type B2.", new ZString[] { "FTZ", "CCP" }, new ZString[] { "EPZ", "CBF", "ATP", "SPK" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.B6, "Importer Bonded ID should be of type EPZ or CBF or ATP or SPK for Declaration Type B6.", new ZString[] { "FTZ", "CCP" }, new ZString[] { "EPZ", "CBF", "ATP", "SPK" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.B8, "Importer Bonded ID should be of type FTZ for Declaration Type B8.", new ZString[] { "EPZ", "CBF", "CCP", "ATP", "SPK" }, new ZString[] { "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.B9, "Importer Bonded ID should be of type FTZ for Declaration Type B9.", new ZString[] { "EPZ", "CBF", "CCP", "ATP", "SPK" }, new ZString[] { "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.D5, "Importer Bonded ID should be of type FTZ for Declaration Type D5.", new ZString[] { "EPZ", "CBF", "CCP", "ATP", "SPK" }, new ZString[] { "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.D7, "Importer Bonded ID should be of type EPZ or CBF or ATP or SPK for Declaration Type D7.", new ZString[] { "FTZ", "CCP" }, new ZString[] { "EPZ", "CBF", "ATP", "SPK" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.L1, "Importer Bonded ID should be of type CCP for Declaration Type L1.", new ZString[] { "EPZ", "CBF", "FTZ", "ATP", "SPK" }, new ZString[] { "CCP" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.F1, "Importer Bonded ID should be of type FTZ for Declaration Type F1.", new ZString[] { "EPZ", "CBF", "CCP", "ATP", "SPK" }, new ZString[] { "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.F3, "Importer Bonded ID should be of type FTZ for Declaration Type F3.", new ZString[] { "EPZ", "CBF", "CCP", "ATP", "SPK" }, new ZString[] { "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.F4, "Importer Bonded ID should be of type FTZ for Declaration Type F4.", new ZString[] { "EPZ", "CBF", "CCP", "ATP", "SPK" }, new ZString[] { "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.G1, "Importer Bonded ID should be of type EPZ for Declaration Type G1.", new ZString[] { "CBF", "FTZ", "CCP", "ATP", "SPK" }, new ZString[] { "EPZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.G7, "Importer Bonded ID should be of type EPZ or CBF or ATP or SPK for Declaration Type G7.", new ZString[] { "FTZ", "CCP" }, new ZString[] { "EPZ", "CBF", "ATP", "SPK" });
			});

			void AssertImporterBondedTypeIsCorrect(ZString ceiStyle, ZString error, ZString[] hasErrorCodeTypes, ZString[] noErrorCodeTypes)
			{
				cusEntryInstruction.CEI_Style = ceiStyle;
				foreach (ZString hasErrorCodeType in hasErrorCodeTypes)
				{
					importerDocumentaryAddress.CBPCodeType = hasErrorCodeType;
					importerDocumentaryAddress.CBPCode = "12345678";
					validation.ValidateCBPCodeType();
					AssertHasMessageError(targetInfo, error);
				}

				foreach (ZString noErrorCodeType in noErrorCodeTypes)
				{
					importerDocumentaryAddress.CBPCodeType = noErrorCodeType;
					importerDocumentaryAddress.CBPCode = "12345678";
					validation.ValidateCBPCodeType();
					AssertNoMessageError(targetInfo, error);
				}
			}
		}

		public void TestCheckIDCode()
		{
			var targetInfo = importerDocumentaryAddress.IDCodeInfo;
			var validation = importerDocumentaryAddress.Validation;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;

			importerDocumentaryAddress.IDCode = ZString.Empty;
			validation.ValidateIDCode();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			validation.ValidateIDCode();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCBPCodeShouldNotEnter()
		{
			var formatMessage = "Importer Bonded ID is not required for Declaration Type {0}.";
			var targetInfo = importerDocumentaryAddress.CBPCodeInfo;

			importerDocumentaryAddress.E2_AddressOverride = true;
			CombineAssertions("Testing Importer Bonded ID Should Not Enter", () =>
			{
				AssertImporterBondedIDShouldNotEnter(Constants.DeclarationTypes.Export.D1);
				AssertImporterBondedIDShouldNotEnter(Constants.DeclarationTypes.Import.D2);
				AssertImporterBondedIDShouldNotEnter(Constants.DeclarationTypes.Import.D8);
				AssertImporterBondedIDShouldNotEnter(Constants.DeclarationTypes.Import.F2);
				AssertImporterBondedIDShouldNotEnter(Constants.DeclarationTypes.Export.F5);
				AssertImporterBondedIDShouldNotEnter(Constants.DeclarationTypes.Import.G2);
			});

			importerDocumentaryAddress.E2_AddressOverride = false;
			CombineAssertions("Testing Importer Bonded ID Should Not Show Message When Not overriden", () =>
			{
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Export.D1);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Import.D2);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Import.D8);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Import.F2);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Export.F5);
				AssertNoMessageIsShownWhenNotOverride(Constants.DeclarationTypes.Import.G2);
			});

			void AssertImporterBondedIDShouldNotEnter(ZString ceiStyle)
			{
				var error = string.Format(formatMessage, ceiStyle);
				cusEntryInstruction.CEI_Style = ceiStyle;
				importerDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
				importerDocumentaryAddress.CBPCode = "12345678";
				AssertHasMessageError(targetInfo, error);

				importerDocumentaryAddress.CBPCodeType = ZString.Empty;
				importerDocumentaryAddress.CBPCode = ZString.Empty;
				AssertNoMessageError(targetInfo, error);
			}

			void AssertNoMessageIsShownWhenNotOverride(ZString ceiStyle)
			{
				var error = string.Format(formatMessage, ceiStyle);
				cusEntryInstruction.CEI_Style = ceiStyle;
				importerDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
				importerDocumentaryAddress.CBPCode = "12345678";
				AssertNoMessageError(targetInfo, error);
			}
		}

		public void TestCheckCBPCodeIsRequired()
		{
			var targetInfo = importerDocumentaryAddress.CBPCodeInfo;
			importerDocumentaryAddress.E2_AddressOverride = true;

			CombineAssertions("Importer Bonded ID might be required when goods are being imported into a Free Trade Zone.", () =>
			{
				AssertImporterBondedIDIsRequiredFreeTradeZone(Constants.DeclarationTypes.Export.B8);
				AssertImporterBondedIDIsRequiredFreeTradeZone(Constants.DeclarationTypes.Export.B9);
				AssertImporterBondedIDIsRequiredFreeTradeZone(Constants.DeclarationTypes.Export.D5);
			});

			void AssertImporterBondedIDIsRequiredFreeTradeZone(ZString ceiStyle)
			{
				declaration.JE_RL_NKFinalDestination = "TWTPE";
				var warning = string.Format("Importer Bonded ID might be required for Declaration Type {0} when goods are being imported into a Free Trade Zone.", ceiStyle);
				cusEntryInstruction.CEI_Style = ceiStyle;
				importerDocumentaryAddress.CBPCodeType = "EPZ";
				importerDocumentaryAddress.CBPCode = ZString.Empty;
				AssertHasWarning(targetInfo, warning);

				importerDocumentaryAddress.CBPCode = "12345678";
				AssertNoWarning(targetInfo, warning);

				declaration.JE_RL_NKFinalDestination = "USLAX";
				importerDocumentaryAddress.CBPCode = ZString.Empty;
				AssertNoWarning(targetInfo, warning);
			}

			CombineAssertions("Importer Bonded ID is required.", () =>
			{
				AssertImporterBondedIDIsRequired(targetInfo, Constants.DeclarationTypes.Export.B1);
				AssertImporterBondedIDIsRequired(targetInfo, Constants.DeclarationTypes.Import.B6);
				AssertImporterBondedIDIsRequired(targetInfo, Constants.DeclarationTypes.Export.F4);
				AssertImporterBondedIDIsRequired(targetInfo, Constants.DeclarationTypes.Import.F1);
				AssertImporterBondedIDIsRequired(targetInfo, Constants.DeclarationTypes.Import.F3);
			});
		}

		void AssertImporterBondedIDIsRequired(ZPropertyInfo targetInfo, ZString ceiStyle)
		{
			declaration.JE_RL_NKFinalDestination = "USLAX";
			var error = string.Format("Importer Bonded ID is required for Declaration Type {0}.", ceiStyle);
			cusEntryInstruction.CEI_Style = ceiStyle;
			importerDocumentaryAddress.CBPCodeType = "EPZ";
			importerDocumentaryAddress.CBPCode = ZString.Empty;
			AssertHasMessageError(targetInfo, error);

			importerDocumentaryAddress.CBPCode = "12345678";
			AssertNoMessageError(targetInfo, error);
		}

		public void TestCheckCBPCodeCanNotCoExistOrEmpty()
		{
			var targetInfo = importerDocumentaryAddress.CBPCodeInfo;
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.CBPCodeType = "EPZ";

			AssertImporterBondedIDCanNotCoExistOrEmptyWithToBondedWarehouse(Constants.DeclarationTypes.Export.B2);
			AssertImporterBondedIDCanNotCoExistOrEmptyWithToBondedWarehouse(Constants.DeclarationTypes.Import.D7);
			AssertImporterBondedIDCanNotCoExistWithBondedFactories(Constants.DeclarationTypes.Import.G7);

			void AssertImporterBondedIDCanNotCoExistOrEmptyWithToBondedWarehouse(ZString ceiStyle)
			{
				var error = ValidationConstants.TWJobDocAddress.ImporterBondedIdCanNotCoExistOrEmptyWithToBondedWarehouse;
				cusEntryInstruction.CEI_Style = ceiStyle;
				cusEntryInstruction.CEI_OA_Warehouse2 = testOrg1.MainAddress.PK;
				importerDocumentaryAddress.CBPCode = ZString.Empty;
				AssertNoMessageError(targetInfo, error);

				importerDocumentaryAddress.CBPCode = "12345678";
				AssertHasMessageError(targetInfo, error);

				cusEntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				importerDocumentaryAddress.Validation.ValidateCBPCode();
				AssertNoMessageError(targetInfo, error);

				importerDocumentaryAddress.CBPCode = ZString.Empty;
				AssertHasMessageError(targetInfo, error);
			}

			void AssertImporterBondedIDCanNotCoExistWithBondedFactories(ZString ceiStyle)
			{
				var error = ValidationConstants.TWJobDocAddress.ImporterBondedIdCanNotCoExistWithBondedFactories;
				cusEntryInstruction.CEI_Style = ceiStyle;
				var bondedFactory = declaration.BondedFactories.AddNew();
				bondedFactory.OrganisationPK = testOrg1.PK;
				bondedFactory.E2_OA_Address = testOrg1.MainAddress.PK;
				importerDocumentaryAddress.CBPCode = ZString.Empty;
				AssertNoMessageError(targetInfo, error);

				importerDocumentaryAddress.CBPCode = "12345678";
				AssertHasMessageError(targetInfo, error);

				declaration.BondedFactories.DeleteAll();
				importerDocumentaryAddress.Validation.ValidateCBPCode();
				AssertNoMessageError(targetInfo, error);

				importerDocumentaryAddress.CBPCode = ZString.Empty;
				AssertNoMessageError(targetInfo, error);
			}
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

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			cusEntryInstruction = declaration.CusEntryInstruction;
			importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
		}

		#endregion

		JobDeclarationForJobDocAddressTest declaration;
		CusEntryInstruction cusEntryInstruction;
		TWJobDocAddress importerDocumentaryAddress;
	}
}
