using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusInBondHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBH_VoyageNumber()
		{
			var messager = "Vessel REG should be 6 digits.";
			var cusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			var trageInfo = cusInBondHeader.BH_VoyageNumberInfo;
			cusInBondHeader.BH_VoyageNumber = "";
			AssertNoMessageErrorContaining(trageInfo, messager);
			cusInBondHeader.BH_VoyageNumber = "1111";
			AssertHasMessageErrorContaining(trageInfo, messager);
			cusInBondHeader.BH_VoyageNumber = "123456";
			AssertNoMessageErrorContaining(trageInfo, messager);
			cusInBondHeader.BH_VoyageNumber = "\u5176\u4ed6\u904b\u8f38\u65b9\u5f0f";
			AssertHasError(trageInfo, EnglishCharactersValidation.GetNotificationMessage(trageInfo));
		}

		[TestDate(2019, 08, 29)]
		public void TestCheckBH_GS_NKCusAgent()
		{
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_Code = "GS1";
			Factory.Save();
			var cusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			var messager = ListValidation.GetNotificationMessage(cusInBondHeader.BH_GS_NKCusAgentInfo).ToString();
			cusInBondHeader.BH_GS_NKCusAgent = "";
			AssertNoMessageErrorContaining(cusInBondHeader.BH_GS_NKCusAgentInfo, messager);
			cusInBondHeader.BH_GS_NKCusAgent = "GS2";
			AssertHasMessageErrorContaining(cusInBondHeader.BH_GS_NKCusAgentInfo, messager);
			cusInBondHeader.BH_GS_NKCusAgent = "GS1";
			AssertNoMessageErrorContaining(cusInBondHeader.BH_GS_NKCusAgentInfo, messager);
			cusInBondHeader.BH_GS_NKCusAgent = "\u5176";
			AssertHasError(cusInBondHeader.BH_GS_NKCusAgentInfo, EnglishCharactersValidation.GetNotificationMessage(cusInBondHeader.BH_GS_NKCusAgentInfo));
			cusInBondHeader.BH_GS_NKCusAgent = "GS1";
			AssertHasMessageErrorContaining(cusInBondHeader.BH_GS_NKCusAgentInfo, ValidationConstants.CusInBondHeader.BrokerStaffNotHaveValidCertificateNumber);
			AssertNoMessageErrorContaining(cusInBondHeader.BH_GS_NKCusAgentInfo, ValidationConstants.CusInBondHeader.BrokerStaffIsNotEmpty);
			cusInBondHeader.BH_GS_NKCusAgent = "";
			AssertHasMessageErrorContaining(cusInBondHeader.BH_GS_NKCusAgentInfo, ValidationConstants.CusInBondHeader.BrokerStaffIsNotEmpty);
			var certificate = glbStaff.Certificates.AddNew();
			certificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			certificate.XZ_RefNumber = ZString.Empty;
			certificate.XZ_IssueDate = new ZDate(2019, 08, 02);
			certificate.XZ_ExpiryOrDueDate = new ZDate(2020, 08, 02);
			cusInBondHeader.BH_GS_NKCusAgent = "GS2";
			cusInBondHeader.BH_GS_NKCusAgent = "GS1";
			AssertHasMessageErrorContaining(cusInBondHeader.BH_GS_NKCusAgentInfo, ValidationConstants.CusInBondHeader.BrokerStaffNotHaveValidCertificateNumber);
			certificate.XZ_RefNumber = "111";
			cusInBondHeader.BH_GS_NKCusAgent = "GS2";
			cusInBondHeader.BH_GS_NKCusAgent = "GS1";
			AssertNoMessageErrorContaining(cusInBondHeader.BH_GS_NKCusAgentInfo, ValidationConstants.CusInBondHeader.BrokerStaffNotHaveValidCertificateNumber);
		}

		public void TestCheckDeconsolidator()
		{
			var org1 = OrgHeader.New(Factory);
			org1.OH_Code = "OH1";
			org1.OH_FullName = "OH1 Name";
			Factory.Save();
			var cusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			var messager = ListValidation.GetNotificationMessage(cusInBondHeader.DeconsolidatorInfo).ToString();
			cusInBondHeader.Deconsolidator = ZGuid.Empty;
			AssertNoMessageErrorContaining(cusInBondHeader.DeconsolidatorInfo, messager);
			cusInBondHeader.Deconsolidator = ZGuid.NewZGuid();
			AssertHasMessageErrorContaining(cusInBondHeader.DeconsolidatorInfo, messager);
			cusInBondHeader.Deconsolidator = org1.PK;
			AssertNoMessageErrorContaining(cusInBondHeader.DeconsolidatorInfo, messager);
		}

		public void TestCheckBH_OA_Importer()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "OH1 Name";
			org1.MainAddress.Address1 = "Address1";
			var address1 = org1.MainAddress;
			Factory.Save();
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			cusInBondHeader.BH_OA_Importer = address1.PK;
			var targetInfo = cusInBondHeader.BH_OA_ImporterInfo;
			AssertHasMessageError(targetInfo, ValidationConstants.CusInBondHeader.MissingVATorPIDorPAS);
			var cusCode = org1.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusInBondHeader.Validation.ValidateBH_OA_Importer();
			AssertNoMessageErrors(targetInfo);
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusInBondHeader.Validation.ValidateBH_OA_Importer();
			AssertHasMessageError(targetInfo, ValidationConstants.CusInBondHeader.MissingVATorPIDorPAS);
			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.PID;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusInBondHeader.Validation.ValidateBH_OA_Importer();
			AssertNoMessageErrors(targetInfo);
			cusCode.OK_CodeType = OrgCusCode.TaiwanCodeTypes.AEO;
			cusInBondHeader.Validation.ValidateBH_OA_Importer();
			AssertHasMessageError(targetInfo, ValidationConstants.CusInBondHeader.MissingVATorPIDorPAS);
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			cusInBondHeader.Validation.ValidateBH_OA_Importer();
			AssertNoMessageErrors(targetInfo);
			cusInBondHeader.BH_OA_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			cusInBondHeader.BH_OA_Importer = address1.PK;
			AssertNoMessageErrors(targetInfo);
		}

		[TestDate(2019, 08, 29)]
		public void TestCheckBH_CustomsProfile()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "XXX";
			var cusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "GS1";
			AddgenRegCertAccredMaintList(staff1, "BRK", new ZDate(2017, 1, 1), new ZDate(2020, 1, 1));
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "GS2";
			AddgenRegCertAccredMaintList(staff2, "BRK", new ZDate(2017, 1, 1), new ZDate(2020, 1, 1));
			AddMailBoxCredential(cusInBondHeader.Company, staff1, "00000000-2", PasswordTypesList.Codes.TVA);
			AddMailBoxCredential(cusInBondHeader.Company, staff1, "00000000-1", PasswordTypesList.Codes.UVC);
			AddMailBoxCredential(company, staff1, "00000000-3", PasswordTypesList.Codes.TVA);
			var extPswUva = AddMailBoxCredential(cusInBondHeader.Company, staff2, "00000000-4", PasswordTypesList.Codes.TVA);
			Factory.Save();
			var messager = ListValidation.InvalidCodeMessageError.ToString();
			var targetInfo = cusInBondHeader.BH_CustomsProfileInfo;
			cusInBondHeader.BH_GS_NKCusAgent = "GS1";
			cusInBondHeader.BH_CustomsProfile = "";
			AssertNoMessageErrorContaining(targetInfo, messager);
			cusInBondHeader.BH_CustomsProfile = "00000000-3";
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining(targetInfo, messager);
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			});
			cusInBondHeader.BH_CustomsProfile = "00000000-1";
			AssertNoMessageErrorContaining(targetInfo, messager);
			cusInBondHeader.BH_GS_NKCusAgent = "GS2";
			cusInBondHeader.BH_CustomsProfile = "00000000-4";
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(targetInfo, messager);
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			});
			cusInBondHeader.BH_CustomsProfile = "";
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			cusInBondHeader.BH_GS_NKCusAgent = "GS2";
			cusInBondHeader.BH_CustomsProfile = "00000000-4";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			extPswUva.GP_UserID = PasswordTypesList.Codes.TVA;
			extPswUva.GP_UserID = "001TEST";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			cusInBondHeader.Validation.ValidateBH_CustomsProfile();
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			});
			extPswUva.GP_UserID = PasswordTypesList.Codes.UVC;
			extPswUva.GP_UserID = "001TEST";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			cusInBondHeader.Validation.ValidateBH_CustomsProfile();
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			});
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			cusInBondHeader.Validation.ValidateBH_CustomsProfile();
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			});
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			extPswUva.GP_UserID = PasswordTypesList.Codes.TVA;
			extPswUva.GP_UserID = "001";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			cusInBondHeader.Validation.ValidateBH_CustomsProfile();
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(targetInfo, messager);
				AssertHasMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			});
			extPswUva.GP_UserID = PasswordTypesList.Codes.UVC;
			extPswUva.GP_UserID = "001";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			cusInBondHeader.Validation.ValidateBH_CustomsProfile();
			CombineAssertions(() =>
			{
				AssertHasMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			});
			TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			extPswUva.GP_UserID = PasswordTypesList.Codes.TVA;
			extPswUva.GP_UserID = "001TEST";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			cusInBondHeader.Validation.ValidateBH_CustomsProfile();
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
				AssertHasMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			});
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			cusInBondHeader.Validation.ValidateBH_CustomsProfile();
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
				AssertHasMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			});
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			extPswUva.GP_UserID = PasswordTypesList.Codes.UVC;
			extPswUva.GP_UserID = "001TEST";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			cusInBondHeader.Validation.ValidateBH_CustomsProfile();
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
				AssertHasMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			});
			extPswUva.GP_UserID = PasswordTypesList.Codes.TVA;
			extPswUva.GP_UserID = "001";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			cusInBondHeader.Validation.ValidateBH_CustomsProfile();
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			});
			extPswUva.GP_UserID = PasswordTypesList.Codes.UVC;
			extPswUva.GP_UserID = "001";
			extPswUva.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			cusInBondHeader.Validation.ValidateBH_CustomsProfile();
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForTesting);
				AssertNoMessageError(targetInfo, ValidationConstants.Declaration.MailboxNotForProduction);
			});
		}

		public void TestCheckBH_ImportTransportMode()
		{
			var messager = ListValidation.InvalidCodeMessageError.ToString();
			var cusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			var targetInfo = cusInBondHeader.BH_ImportTransportModeInfo;
			cusInBondHeader.BH_ImportTransportMode = "";
			AssertNoMessageErrorContaining(targetInfo, messager);
			cusInBondHeader.BH_ImportTransportMode = "XX";
			AssertHasMessageErrorContaining(targetInfo, messager);
			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			AssertNoMessageErrorContaining(targetInfo, messager);
		}

		public void TestCheckBH_ImportTransportModeWithReceiptOffice()
		{
			var messageDifferentTransportMode = ValidationConstants.CusInBondHeader.DifferentTransportMode;
			var cusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			var targetInfo = cusInBondHeader.BH_ImportTransportModeInfo;

			cusInBondHeader.ReceiptOffice = "OB";
			cusInBondHeader.BH_ImportTransportMode = "";
			AssertHasMessageErrorContaining(targetInfo, messageDifferentTransportMode);
			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			AssertNoMessageErrorContaining(targetInfo, messageDifferentTransportMode);
			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			AssertHasMessageErrorContaining(targetInfo, messageDifferentTransportMode);

			cusInBondHeader.ReceiptOffice = "OC";
			cusInBondHeader.BH_ImportTransportMode = "";
			AssertHasMessageErrorContaining(targetInfo, messageDifferentTransportMode);
			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			AssertHasMessageErrorContaining(targetInfo, messageDifferentTransportMode);
			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			AssertNoMessageErrorContaining(targetInfo, messageDifferentTransportMode);
		}

		public void TestCheckUnladingOffice()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			cusInBondHeader.UnladingOffice = "X2";
			AssertHasMessageErrorContaining(cusInBondHeader.UnladingOfficeInfo, ListValidation.InvalidCodeMessageError);
			cusInBondHeader.UnladingOffice = "OB";
			AssertNoMessageErrorContaining(cusInBondHeader.UnladingOfficeInfo, ListValidation.InvalidCodeMessageError);
			cusInBondHeader.UnladingOffice = "OC";
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(cusInBondHeader.UnladingOfficeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageErrorContaining(cusInBondHeader.UnladingOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			});
			cusInBondHeader.UnladingOffice = ZString.Empty;
			AssertHasMessageErrorContaining(cusInBondHeader.UnladingOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckReceiptOffice()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			cusInBondHeader.ReceiptOffice = "X2";
			AssertHasMessageErrorContaining(cusInBondHeader.ReceiptOfficeInfo, ListValidation.InvalidCodeMessageError);
			cusInBondHeader.ReceiptOffice = "OC";
			AssertNoMessageErrorContaining(cusInBondHeader.ReceiptOfficeInfo, ListValidation.InvalidCodeMessageError);
			cusInBondHeader.UnladingOffice = ZString.Empty;
			AssertNoMessageErrorContaining(cusInBondHeader.ReceiptOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			cusInBondHeader.UnladingOffice = "X1";
			cusInBondHeader.ReceiptOffice = ZString.Empty;
			AssertHasMessageErrorContaining(cusInBondHeader.ReceiptOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBH_UniqueVoyageIdentifier()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			var targetInfo = cusInBondHeader.BH_UniqueVoyageIdentifierInfo;
			cusInBondHeader.BH_UniqueVoyageIdentifier = "Voyage";
			AssertNoWarningContaining(targetInfo, "System will automatically declare 'NIL' when Flight No/Voyage is empty.");

			cusInBondHeader.BH_UniqueVoyageIdentifier = ZString.Empty;
			AssertHasWarningContaining(targetInfo, "System will automatically declare 'NIL' when Flight No/Voyage is empty.");

			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			cusInBondHeader.Validation.ValidateBH_UniqueVoyageIdentifier();
			AssertNoWarningContaining(targetInfo, "System will automatically declare 'NIL' when Flight No/Voyage is empty.");
		}

		public void TestCheckBH_RL_NKImportLoadPort()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			var targetInfo = cusInBondHeader.BH_RL_NKImportLoadPortInfo;
			cusInBondHeader.BH_RL_NKImportLoadPort = "TWTPE";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			cusInBondHeader.BH_RL_NKImportLoadPort = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			cusInBondHeader.Validation.ValidateBH_RL_NKImportLoadPort();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBH_ETA()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			var targetInfo = cusInBondHeader.BH_ETAInfo;
			cusInBondHeader.BH_ETA = ZDateTime.Today;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			cusInBondHeader.BH_ETA = ZDateTime.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			cusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			cusInBondHeader.Validation.ValidateBH_ETA();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTW_BoxNumber()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerageBoxNumber();
			var cusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			var targetInfo = cusInBondHeader.TW_BoxNumberInfo;
			cusInBondHeader.ReceiptOffice = "BF";
			cusInBondHeader.TW_BoxNumber = "XXX";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			cusInBondHeader.ReceiptOffice = "BF";
			cusInBondHeader.TW_BoxNumber = "100";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2022, 06, 30)]
		public void TestCheckEntryNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			header.ReceiptOffice = "AA";
			header.UnladingOffice = "BB";
			header.TW_BoxNumber = "123";
			Factory.Save();

			header.AllocateEntryNumber();
			header.RunPreSaveValidation();
			AssertNoMessageErrorContaining(header.EntryNumberInfo, ValidationConstants.AllocateNumber.KeyComponentValuesChanged);

			header.ReceiptOffice = "CD";
			header.RunPreSaveValidation();
			AssertHasMessageErrorContaining(header.EntryNumberInfo, ValidationConstants.AllocateNumber.KeyComponentValuesChanged);

			header.ReceiptOffice = "AA";
			header.UnladingOffice = "CD";
			header.RunPreSaveValidation();
			AssertHasMessageErrorContaining(header.EntryNumberInfo, ValidationConstants.AllocateNumber.KeyComponentValuesChanged);

			header.UnladingOffice = "BB";
			header.TW_BoxNumber = "321";
			header.RunPreSaveValidation();
			AssertHasMessageErrorContaining(header.EntryNumberInfo, ValidationConstants.AllocateNumber.KeyComponentValuesChanged);
		}

		GlbExternalPassword AddMailBoxCredential(GlbCompany company, GlbStaff staff, ZString mailBoxID, ZString passwordType)
		{
			var extPassword = Factory.NewWithValidTestData<GlbExternalPassword>();
			extPassword.GP_PasswordType = passwordType;
			extPassword.GP_MailBoxID = mailBoxID;
			if (company != null)
			{
				extPassword.GP_GC = company.PK;
			}

			if (staff != null)
			{
				extPassword.GP_GS = staff.PK;
			}

			return extPassword;
		}

		static GenRegCertAccredMaintList AddgenRegCertAccredMaintList(GlbStaff staff, ZString type, ZDate issueDate, ZDate expiryOrDueDate)
		{
			var genRegCertAccredMaintList = staff.Certificates.AddNew();
			genRegCertAccredMaintList.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			genRegCertAccredMaintList.XZ_Type = type;
			genRegCertAccredMaintList.XZ_IssueDate = issueDate;
			genRegCertAccredMaintList.XZ_ExpiryOrDueDate = expiryOrDueDate;
			return genRegCertAccredMaintList;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "OB", "TEST DATA1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList1.PK, TransportTypeList.Codes.Sea);
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "OC", "TEST DATA2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList2.PK, TransportTypeList.Codes.Air);
			newFactory.Save();
		}
	}
}
