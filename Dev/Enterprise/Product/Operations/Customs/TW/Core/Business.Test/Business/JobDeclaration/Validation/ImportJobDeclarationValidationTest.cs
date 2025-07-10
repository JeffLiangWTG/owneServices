using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ImportJobDeclarationValidation))]
	sealed class ImportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest<ImportJobDeclarationValidation>
	{
		[TestDate(2020, 3, 30)]
		public override void TestCheckJE_OH_Supplier()
		{
			base.TestCheckJE_OH_Supplier();
			declaration.JE_MessageType = "IMP";
			var targetInfo = declaration.JE_OH_SupplierInfo;
			var errorMessage = ValidationConstants.Declaration.TheOrganizationShouldHaveEnglishAddress;
			var supplier = Factory.New<OrgHeader>();
			supplier.Addresses.RemoveAndDeleteAll();
			supplier.OH_Code = "supplier";
			var mainAddress = supplier.Addresses[0];
			mainAddress.OA_Language = Core.SharedConstants.Languages.English;
			mainAddress.OA_Address1 = "ADDRESS 1";
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			declaration.JE_OH_Supplier = supplier.PK;
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			mainAddress.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
			var entranslatedAddress = mainAddress.TranslatedAddresses.AddNew();
			entranslatedAddress.OTA_Language = Core.SharedConstants.Languages.English;
			entranslatedAddress.OTA_Address1 = "EN OTA ADDRESS 1";
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
		}

		[TestDate(2020, 3, 30)]
		public override void TestCheckJE_OH_Importer()
		{
			base.TestCheckJE_OH_Importer();
			var importer = Factory.New<OrgHeader>();
			var cusCode = importer.CustomsCodes.AddNew();
			var errorMessage = ValidationConstants.Declaration.ShouldHasGovernmentVATCodeOrRodIdCardOrPassportNumber;
			var targetInfo = declaration.JE_OH_ImporterInfo;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.BrokerageRegistration;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			declaration.JE_OH_Importer = importer.PK;
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.PID;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);
		}

		public override void TestCheckJE_PaymentMethod()
		{
			base.TestCheckJE_PaymentMethod();
			var targetInfo = declaration.JE_PaymentMethodInfo;
			declaration.JE_PaymentMethod = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_PaymentMethod = IMPPaymentMethod.Codes._1;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_PaymentMethod = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_ExportDate()
		{
			var targetInfo = declaration.JE_ExportDateInfo;
			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_ExportDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public override void TestCheckJE_DateOfArrival()
		{
			base.TestCheckJE_DateOfArrival();
			var decl = declaration as JobDeclaration;
			var targetInfo = decl.JE_DateOfArrivalInfo;
			decl.JE_DateOfArrival = ZDateTime.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			decl.JE_DateOfArrival = new ZDateTime(2024, 03, 11);
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			decl.JE_DateOfArrivalInfo.ClearValue();

			var message = "Arrival Date of Shipment should be the same as Declaration Date.";
			var entryInstruction = decl.CusEntryInstruction;
			CombineAssertions("D2, D7", () =>
			{
				entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 03, 11);
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
				declaration.JE_DateOfArrival = new ZDateTime(2024, 03, 11);
				AssertNoMessageErrorContaining(targetInfo, message);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
				decl.Validation.ValidateJE_DateOfArrival();
				AssertNoMessageErrorContaining(targetInfo, message);

				entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 03, 09);
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
				decl.Validation.ValidateJE_DateOfArrival();
				AssertHasMessageErrorContaining(targetInfo, message);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
				decl.Validation.ValidateJE_DateOfArrival();
				AssertHasMessageErrorContaining(targetInfo, message);
			});

			CombineAssertions("F2, F3", () =>
			{
				entryInstruction.CEI_WHSMonth = "5";
				entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 03, 09);
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F2;
				decl.Validation.ValidateJE_DateOfArrival();
				AssertNoMessageErrorContaining(targetInfo, message);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F3;
				decl.Validation.ValidateJE_DateOfArrival();
				AssertNoMessageErrorContaining(targetInfo, message);

				entryInstruction.CEI_WHSMonthInfo.ClearValue();
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F2;
				decl.Validation.ValidateJE_DateOfArrival();
				AssertHasMessageErrorContaining(targetInfo, message);

				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F3;
				decl.Validation.ValidateJE_DateOfArrival();
				AssertHasMessageErrorContaining(targetInfo, message);
			});

			CombineAssertions("B6, D8", () =>
			{
				entryInstruction.CEI_WHSMonth = "5";
				entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 03, 10);
				var supplierDocumentaryAddress = decl.SupplierDocumentaryAddress;
				supplierDocumentaryAddress.E2_AddressOverride = true;
				supplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
				decl.Validation.ValidateJE_DateOfArrival();
				AssertNoMessageErrorContaining(targetInfo, message);

				entryInstruction.CEI_WHSMonthInfo.ClearValue();
				decl.Validation.ValidateJE_DateOfArrival();
				AssertHasMessageErrorContaining(targetInfo, message);

				decl.SupplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
				decl.Validation.ValidateJE_DateOfArrival();
				AssertNoMessageErrorContaining(targetInfo, message);

				decl.SupplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
				entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 03, 11);
				decl.Validation.ValidateJE_DateOfArrival();
				AssertNoMessageErrorContaining(targetInfo, message);

				entryInstruction.CEI_WHSMonth = "5";
				entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 03, 10);
				decl.SupplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
				decl.Validation.ValidateJE_DateOfArrival();
				AssertNoMessageErrorContaining(targetInfo, message);

				entryInstruction.CEI_WHSMonthInfo.ClearValue();
				decl.Validation.ValidateJE_DateOfArrival();
				AssertHasMessageErrorContaining(targetInfo, message);

				decl.SupplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
				decl.Validation.ValidateJE_DateOfArrival();
				AssertNoMessageErrorContaining(targetInfo, message);

				decl.SupplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
				entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 03, 11);
				decl.Validation.ValidateJE_DateOfArrival();
				AssertNoMessageErrorContaining(targetInfo, message);
			});

			CombineAssertions("G2", () =>
			{
				var message2 = "Arrival Date of Shipment should be greater or equal to Declaration Date.";
				entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 03, 12);
				entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
				decl.Validation.ValidateJE_DateOfArrival();
				AssertHasMessageErrorContaining(targetInfo, message2);

				entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 03, 11);
				decl.Validation.ValidateJE_DateOfArrival();
				AssertNoMessageErrorContaining(targetInfo, message2);

				entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 03, 10);
				decl.Validation.ValidateJE_DateOfArrival();
				AssertNoMessageErrorContaining(targetInfo, message2);
			});
		}

		public override void TestCheckJE_RL_NKFinalDestination()
		{
			var newFactory = new BusinessObjectFactory();
			var airUNLOCO = newFactory.NewWithValidTestData<RefUNLOCO>();
			airUNLOCO.RL_Code = "TWAIR";
			airUNLOCO.RL_HasAirport = true;

			var airUNLOCO2 = newFactory.NewWithValidTestData<RefUNLOCO>();
			airUNLOCO2.RL_Code = "USAI1";
			airUNLOCO2.RL_HasAirport = true;

			var seaUNLOCO = newFactory.NewWithValidTestData<RefUNLOCO>();
			seaUNLOCO.RL_Code = "TWSEA";
			seaUNLOCO.RL_HasSeaport = true;
			newFactory.Save();

			var declaration = this.declaration as JobDeclaration;
			var targetInfo = declaration.JE_RL_NKFinalDestinationInfo;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_RL_NKOrigin = "CNSHA";
			declaration.JE_RL_NKFinalDestination = "TWSEA";
			AssertHasMessageError(targetInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);
			declaration.JE_RL_NKFinalDestination = "TWAIR";
			AssertNoMessageError(targetInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_RL_NKFinalDestination = "TWSEA";
			AssertNoMessageError(targetInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);
			declaration.JE_RL_NKFinalDestination = "TWAIR";
			AssertHasMessageError(targetInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);
			declaration.JE_RL_NKFinalDestination = "TWZ99";
			AssertNoMessageError(targetInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_RL_NKFinalDestination = "USAI1";
			AssertHasMessageError(targetInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);
		}

		public override void TestCheckJE_RL_NKOrigin()
		{
			var declaration = this.declaration as JobDeclaration;
			var targetInfo = declaration.JE_RL_NKOriginInfo;
			AssertCheckJE_RL_NKOrigin(declaration, targetInfo, "", "ZDSD@", "USLAX", JobDeclarationValidation.MessageErrorPortCodeInvalid);
			AssertCheckJE_RL_NKOrigin(declaration, targetInfo, "G1", "TWTPE", "USTPE", ValidationConstants.Declaration.EnterForeignPortCodeMessage);
			AssertCheckJE_RL_NKOrigin(declaration, targetInfo, "G7", "TWTPE", "USTPE", ValidationConstants.Declaration.EnterForeignPortCodeMessage);
			AssertCheckJE_RL_NKOrigin(declaration, targetInfo, "F1", "TWTPE", "USTPE", ValidationConstants.Declaration.EnterForeignPortCodeMessage);
			AssertCheckJE_RL_NKOrigin(declaration, targetInfo, "G2", "USTPE", "TWTPE", ValidationConstants.Declaration.EnterTWPortCodeMessage);
			AssertCheckJE_RL_NKOrigin(declaration, targetInfo, "D2", "CNSHA", "TWTPE", ValidationConstants.Declaration.EnterTWPortCodeMessage);
			AssertCheckJE_RL_NKOrigin(declaration, targetInfo, "D7", "USTPE", "TWTPE", ValidationConstants.Declaration.EnterTWPortCodeMessage);
			AssertCheckJE_RL_NKOrigin(declaration, targetInfo, "F2", "USTPE", "TWTPE", ValidationConstants.Declaration.EnterTWPortCodeMessage);
			AssertCheckJE_RL_NKOrigin(declaration, targetInfo, "F3", "CNSHA", "TWTPE", ValidationConstants.Declaration.EnterTWPortCodeMessage);

			CombineAssertions("CheckIsNotEmpty", () =>
			{
				declaration.JE_RL_NKOrigin = ZString.Empty;
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
				declaration.JE_RL_NKOrigin = "AUSYD";
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		void AssertCheckJE_RL_NKOrigin(JobDeclaration declaration, ZPropertyInfo targetInfo, string declarationType, string invalidOrigin, string validOrigin, string expectedMessage)
		{
			declaration.JE_RL_NKOrigin = ZString.Empty;
			AssertNoMessageError(targetInfo, expectedMessage);
			declaration.CusEntryInstruction.CEI_Style = declarationType;
			declaration.JE_RL_NKOrigin = invalidOrigin;
			AssertHasMessageError(targetInfo, expectedMessage);
			declaration.JE_RL_NKOrigin = validOrigin;
			AssertNoMessageError(targetInfo, expectedMessage);
		}

		public void TestCheckMasterBillWhenImporterOrSupplierIsFreeTradeZone()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			AssertCheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(header, Constants.DeclarationTypes.Import.D8);
			AssertCheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(header, Constants.DeclarationTypes.Import.B6);
			AssertCheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(header, Constants.DeclarationTypes.Import.F2);
		}

		void AssertCheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(OrgHeader header, ZString declartionType)
		{
			var address = header.MainAddress;
			var declaration = this.declaration as JobDeclaration;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MasterBill = "010-9999999";
			declaration.CusEntryInstruction.CEI_Style = declartionType;
			var documentaryAddress = declaration.SupplierDocumentaryAddress;
			documentaryAddress.OrganisationPK = header.PK;
			documentaryAddress.E2_OA_Address = address.PK;
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
			Assert(declaration.IsFreeTradeZoneDocumentaryAddress);

			var targetInfo = declaration.JE_MasterBillInfo;
			var warningMessage = "System will automatically declare 'Entry Number' into";
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, warningMessage);

			declaration.JE_TransportMode = "AIR";
			declaration.JE_MasterBill = ZString.Empty;
			AssertNoWarningContaining(targetInfo, warningMessage);

			declaration.JE_TransportMode = "SEA";
			address.CustomsCodes.DeleteAll();
			Assert(!declaration.IsFreeTradeZoneDocumentaryAddress);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			declaration.JE_TransportMode = "AIR";
			declaration.JE_MasterBill = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		}
	}
}
