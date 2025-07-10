using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ExportJobDeclarationValidation))]
	sealed class ExportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest<ExportJobDeclarationValidation>
	{
		[TestDate(2020, 3, 30)]
		public override void TestCheckJE_OH_Supplier()
		{
			base.TestCheckJE_OH_Supplier();
			var supplier = Factory.New<OrgHeader>();
			var cusCode = supplier.CustomsCodes.AddNew();
			var errorMessage = ValidationConstants.Declaration.ShouldHasGovernmentVATCodeOrRodIdCardOrPassportNumber;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.BrokerageRegistration;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			var targetInfo = declaration.JE_OH_SupplierInfo;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			declaration.JE_OH_Supplier = supplier.PK;
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.PID;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
		}

		public override void TestCheckJE_RL_NKOrigin()
		{
			base.TestCheckJE_RL_NKOrigin();
			var targetInfo = declaration.JE_RL_NKOriginInfo;
			declaration.JE_RL_NKOrigin = ZString.Empty;
			AssertNoMessageError(targetInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);
			declaration.JE_RL_NKOrigin = "TWSSS";
			AssertHasMessageError(targetInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);
			declaration.JE_RL_NKOrigin = "TWZ99";
			AssertNoMessageError(targetInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);
		}

		public override void TestCheckJE_RL_NKFinalDestination()
		{
			base.TestCheckJE_RL_NKFinalDestination();
			var targetInfo = declaration.JE_RL_NKFinalDestinationInfo;
			CombineAssertions("CheckPortCodeIsInvalid", () =>
			{
				var declaration = this.declaration as JobDeclaration;
				AssertCheckJE_RL_NKFinalDestination(declaration, targetInfo, "", "ZDSD@", "USLAX", JobDeclarationValidation.MessageErrorPortCodeInvalid);
				AssertCheckJE_RL_NKFinalDestination(declaration, targetInfo, "G3", "TWTPE", "USTPE", ValidationConstants.Declaration.EnterForeignPortCodeMessage);
				AssertCheckJE_RL_NKFinalDestination(declaration, targetInfo, "G5", "TWTXG", "USTPE", ValidationConstants.Declaration.EnterForeignPortCodeMessage);
				AssertCheckJE_RL_NKFinalDestination(declaration, targetInfo, "F5", "TWKHH", "USTPE", ValidationConstants.Declaration.EnterForeignPortCodeMessage);
				AssertCheckJE_RL_NKFinalDestination(declaration, targetInfo, "D1", "USTPE", "TWTPE", ValidationConstants.Declaration.EnterTWPortCodeMessage);
				AssertCheckJE_RL_NKFinalDestination(declaration, targetInfo, "B1", "CNSHA", "TWTPE", ValidationConstants.Declaration.EnterTWPortCodeMessage);
				AssertCheckJE_RL_NKFinalDestination(declaration, targetInfo, "B2", "USTPE", "TWTPE", ValidationConstants.Declaration.EnterTWPortCodeMessage);
				AssertCheckJE_RL_NKFinalDestination(declaration, targetInfo, "F4", "CNSHA", "TWTPE", ValidationConstants.Declaration.EnterTWPortCodeMessage);
			}

			);
			CombineAssertions("CheckIsNotEmpty", () =>
			{
				declaration.JE_RL_NKFinalDestination = ZString.Empty;
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
				declaration.JE_RL_NKFinalDestination = "AUSYD";
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			}

			);
		}

		void AssertCheckJE_RL_NKFinalDestination(JobDeclaration declaration, ZPropertyInfo targetInfo, string declarationType, string invalidFinalDestination, string validFinalDestination, string expectedMessage)
		{
			declaration.JE_RL_NKFinalDestination = ZString.Empty;
			AssertNoMessageError(targetInfo, expectedMessage);
			declaration.CusEntryInstruction.CEI_Style = declarationType;
			declaration.JE_RL_NKFinalDestination = invalidFinalDestination;
			AssertHasMessageError(targetInfo, expectedMessage);
			declaration.JE_RL_NKFinalDestination = validFinalDestination;
			AssertNoMessageError(targetInfo, expectedMessage);
		}

		public void TestCheckMasterBillWhenImporterOrSupplierIsFreeTradeZone()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			AssertCheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(header, Constants.DeclarationTypes.Export.D5);
			AssertCheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(header, Constants.DeclarationTypes.Export.B8);
			AssertCheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(header, Constants.DeclarationTypes.Export.B9);
			AssertCheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(header, Constants.DeclarationTypes.Export.F4);
		}

		void AssertCheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(OrgHeader header, ZString declartionType)
		{
			var declaration = this.declaration as JobDeclaration;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MasterBill = "010-9999999";
			var address = header.MainAddress;
			var documentaryAddress = declaration.ImporterDocumentaryAddress;
			documentaryAddress.OrganisationPK = header.PK;
			documentaryAddress.E2_OA_Address = address.PK;
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
			Assert(declaration.IsFreeTradeZoneDocumentaryAddress);

			var autoImportDeclareEntryNumMessage = "System will automatically declare 'Entry Number' into ";
			var autoDeclareNilMessage = "System will automatically declare 'NIL' when ";

			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_WHSMonth = ZString.Empty;
			entryInstruction.CEI_Style = declartionType;
			var targetInfo = declaration.JE_MasterBillInfo;
			declaration.JE_MasterBill = ZString.Empty;
			AssertHasWarningContaining(targetInfo, autoImportDeclareEntryNumMessage);
			AssertNoWarningContaining(targetInfo, autoDeclareNilMessage);
			declaration.JE_MasterBill = "010-9999999";
			AssertNoWarningContaining(targetInfo, autoImportDeclareEntryNumMessage);

			declaration.JE_TransportMode = "AIR";
			declaration.JE_MasterBill = ZString.Empty;
			AssertNoWarningContaining(targetInfo, autoImportDeclareEntryNumMessage);

			declaration.JE_TransportMode = "SEA";
			entryInstruction.CEI_WHSMonth = "2";
			declaration.JE_MasterBill = ZString.Empty;
			AssertNoWarningContaining(targetInfo, autoImportDeclareEntryNumMessage);
			AssertHasWarningContaining(targetInfo, autoDeclareNilMessage);
			declaration.JE_MasterBill = "010-9999999";
			AssertNoWarningContaining(targetInfo, autoDeclareNilMessage);

			declaration.JE_TransportMode = "AIR";
			declaration.JE_MasterBill = ZString.Empty;
			AssertNoWarningContaining(targetInfo, autoDeclareNilMessage);

			declaration.JE_TransportMode = "SEA";
			address.CustomsCodes.DeleteAll();
			declaration.JE_MasterBill = ZString.Empty;
			Assert(!declaration.IsFreeTradeZoneDocumentaryAddress);
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MasterBill = "010-9999999";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = "AIR";
			declaration.JE_MasterBill = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_OH_Consignee()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_RL_NKClosestPort = "TWKEL";
			testOrg1.MainAddress.OA_RN_NKCountryCode = "TW";

			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_RL_NKClosestPort = "USLAX";
			testOrg2.MainAddress.OA_RN_NKCountryCode = "US";

			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.OH_RL_NKClosestPort = "USLAX";
			testOrg3.MainAddress.OA_RN_NKCountryCode = "US";
			testOrg3.MainAddress.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;

			var testOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg4.OH_RL_NKClosestPort = "USLAX";
			testOrg4.MainAddress.OA_RN_NKCountryCode = "US";
			testOrg4.MainAddress.OA_Language = Core.SharedConstants.Languages.French;

			var testOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg5.OH_RL_NKClosestPort = "USLAX";
			testOrg5.MainAddress.OA_RN_NKCountryCode = "US";
			testOrg5.MainAddress.OA_Language = Core.SharedConstants.Languages.French;
			var translatedAddress = testOrg5.MainAddress.AddNewTranslatedAddress();
			translatedAddress.OTA_Language = Core.SharedConstants.Languages.English;
			translatedAddress.OTA_Address1 = "ADD1";

			var testOrg6 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg6.OH_RL_NKClosestPort = "USLAX";
			testOrg6.MainAddress.OA_RN_NKCountryCode = "US";
			testOrg6.MainAddress.OA_Language = Core.SharedConstants.Languages.EnglishBritish;
			testOrg6.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);

			var testOrg7 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg7.OH_RL_NKClosestPort = "USLAX";
			testOrg7.MainAddress.OA_RN_NKCountryCode = "US";
			testOrg7.MainAddress.OA_Language = Core.SharedConstants.Languages.EnglishBritish;
			testOrg7.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS12345", Core.Constants.CountryCodes.Taiwan);

			var testOrg8 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg8.OH_RL_NKClosestPort = "USLAX";
			testOrg8.MainAddress.OA_RN_NKCountryCode = "US";
			testOrg8.MainAddress.OA_Language = Core.SharedConstants.Languages.EnglishBritish;
			testOrg8.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "PID12345", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();

			var decl = declaration as JobDeclaration;
			var entryInstruction = decl.CusEntryInstruction;
			var message1 = "You have not selected a Consignee.";
			var message2 = "The selected Consignee does not have English address.";
			var message3 = "A valid TW-VAT or TW-PID or TW-PAS number is required for Consignee. To create a valid TW-VAT or TW-PID or TW-PAS, visit Organization > Details > Config > Registration.";
			CombineAssertions("G3", () =>
			{
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G3;
				decl.JE_OH_Importer = testOrg1.PK;
				decl.JE_MessageType = JobMessageTypeList.Codes.Export;
				decl.Validation.ValidateJE_OH_Consignee();
				AssertHasMessageError(decl.JE_OH_ConsigneeInfo, message1);

				decl.JE_OH_Importer = testOrg2.PK;
				decl.JE_MessageType = JobMessageTypeList.Codes.Export;
				decl.Validation.ValidateJE_OH_Consignee();
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message1);

				decl.JE_OH_Importer = testOrg1.PK;
				decl.JE_MessageType = JobMessageTypeList.Codes.Export;
				decl.JE_OH_Consignee = testOrg4.PK;
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message1);
				AssertHasMessageError(decl.JE_OH_ConsigneeInfo, message2);
				AssertHasMessageError(decl.JE_OH_ConsigneeInfo, message3);

				decl.JE_OH_Importer = testOrg2.PK;
				decl.JE_MessageType = JobMessageTypeList.Codes.Export;
				decl.Validation.ValidateJE_OH_Consignee();
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message2);
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message3);

				decl.JE_OH_Importer = testOrg1.PK;
				decl.JE_MessageType = JobMessageTypeList.Codes.Export;
				decl.JE_OH_Consignee = testOrg3.PK;
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message2);
				AssertHasMessageError(decl.JE_OH_ConsigneeInfo, message3);

				decl.JE_OH_Consignee = testOrg5.PK;
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message2);

				decl.JE_OH_Consignee = testOrg6.PK;
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message3);

				decl.JE_OH_Consignee = testOrg7.PK;
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message3);

				decl.JE_OH_Consignee = testOrg8.PK;
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message3);
			});

			CombineAssertions("G5", () =>
			{
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G5;
				decl.JE_OH_Importer = testOrg1.PK;
				decl.JE_MessageType = JobMessageTypeList.Codes.Export;
				decl.JE_OH_Consignee = ZGuid.Empty;
				AssertHasMessageError(decl.JE_OH_ConsigneeInfo, message1);

				decl.JE_OH_Importer = testOrg2.PK;
				decl.JE_MessageType = JobMessageTypeList.Codes.Export;
				decl.Validation.ValidateJE_OH_Consignee();
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message1);

				decl.JE_OH_Importer = testOrg1.PK;
				decl.JE_MessageType = JobMessageTypeList.Codes.Export;
				decl.JE_OH_Consignee = testOrg4.PK;
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message1);
				AssertHasMessageError(decl.JE_OH_ConsigneeInfo, message2);
				AssertHasMessageError(decl.JE_OH_ConsigneeInfo, message3);

				decl.JE_OH_Importer = testOrg2.PK;
				decl.JE_MessageType = JobMessageTypeList.Codes.Export;
				decl.Validation.ValidateJE_OH_Consignee();
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message2);
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message3);

				decl.JE_OH_Importer = testOrg1.PK;
				decl.JE_MessageType = JobMessageTypeList.Codes.Export;
				decl.JE_OH_Consignee = testOrg3.PK;
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message2);
				AssertHasMessageError(decl.JE_OH_ConsigneeInfo, message3);

				decl.JE_OH_Consignee = testOrg5.PK;
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message2);

				decl.JE_OH_Consignee = testOrg6.PK;
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message3);

				decl.JE_OH_Consignee = testOrg7.PK;
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message3);

				decl.JE_OH_Consignee = testOrg8.PK;
				AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message3);
			});

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			decl.JE_OH_Importer = testOrg1.PK;
			decl.JE_MessageType = JobMessageTypeList.Codes.Export;
			decl.JE_OH_Consignee = ZGuid.Empty;
			AssertNoMessageError(decl.JE_OH_ConsigneeInfo, message1);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		}
	}
}
