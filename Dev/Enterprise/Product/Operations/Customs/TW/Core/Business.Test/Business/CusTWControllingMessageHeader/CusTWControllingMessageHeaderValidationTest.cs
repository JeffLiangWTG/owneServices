using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusTWControllingMessageHeaderValidation))]
	sealed class CusTWControllingMessageHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPermitNumber()
		{
			CombineAssertions(() =>
			{
				var header = controllingMessageHeader;
				var propertyInfo = controllingMessageHeader.PermitNumberInfo;

				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
				header.PermitNumber = "";
				header.Validation.ValidatePermitNumber();
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
				header.PermitNumber = "";
				header.Validation.ValidatePermitNumber();
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
				header.PermitNumber = "";
				header.Validation.ValidatePermitNumber();
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
				header.PermitNumber = "";
				header.Validation.ValidatePermitNumber();
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
				header.PermitNumber = "";
				header.Validation.ValidatePermitNumber();
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
				header.PermitNumber = "";
				header.Validation.ValidatePermitNumber();
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
				header.PermitNumber = "";
				header.Validation.ValidatePermitNumber();
				AssertHasMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);

				header.PermitNumber = "123456789_123";
				AssertHasMessageError(propertyInfo, ValidationConstants.CusTWControllingMessageHeader.LengthForPermitNo);
				header.PermitNumber = "123456789_1234";
				AssertNoMessageError(propertyInfo, ValidationConstants.CusTWControllingMessageHeader.LengthForPermitNo);
			});
		}

		public void TestCheckTW1_BusinessType()
		{
			CombineAssertions(() =>
			{
				var header = controllingMessageHeader;
				var propertyInfo = controllingMessageHeader.TW1_BusinessTypeInfo;

				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
				header.TW1_BusinessType = "";
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
				header.TW1_BusinessType = "";
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
				header.TW1_BusinessType = "";
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
				header.TW1_BusinessType = "";
				AssertHasMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
				header.TW1_BusinessType = "";
				AssertHasMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
				header.TW1_BusinessType = "";
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
				header.TW1_BusinessType = "";
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);

				controllingMessageHeader.TW1_BusinessType = "XX";
				AssertHasMessageErrorContaining(propertyInfo, ListValidation.InvalidCodeMessageError);
				controllingMessageHeader.TW1_BusinessType = controllingMessageHeader.Lookups.BusinessTypeList[0].Code;
				AssertNoMessageErrorContaining(propertyInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckTW1_ProcessingUnit()
		{
			CombineAssertions(() =>
			{
				var twca = Factory.New<ZZRefCusCodeListCombined>();
				twca.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
				twca.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWControllingAgency;
				twca.ZZD_Code = "BT";
				twca.ZZD_StartDate = ZDateTime.Today;
				twca.ZZD_EndDate = ZDateTime.Today.AddYears(1);
				var facility1 = Factory.New<ZZRefCusCodeListCombined>();
				facility1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
				facility1.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit;
				facility1.ZZD_Code = "ANP0060N";
				facility1.ZZD_StartDate = ZDateTime.Today;
				facility1.ZZD_EndDate = ZDateTime.Today.AddYears(1);
				facility1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "BT");
				var facility2 = Factory.New<ZZRefCusCodeListCombined>();
				facility2.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
				facility2.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit;
				facility2.ZZD_Code = "BNP0060N";
				facility2.ZZD_StartDate = ZDateTime.Today;
				facility2.ZZD_EndDate = ZDateTime.Today.AddYears(1);
				facility2.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.ControlAgency, "AT");
				Factory.Save();

				var header = controllingMessageHeader;
				var propertyInfo = controllingMessageHeader.TW1_ProcessingUnitInfo;

				header.TW1_ProcessingUnit = "AT";
				AssertHasMessageErrorContaining(propertyInfo, ListValidation.InvalidCodeMessageError);

				header.TW1_ControllingAgency = "BT";
				header.TW1_ProcessingUnit = "BNP0060N";
				AssertNoMessageErrorContaining(propertyInfo, ListValidation.InvalidCodeMessageError);
				AssertHasMessageErrorContaining(propertyInfo, ValidationConstants.CusTWControllingMessageHeader.ProcessingUnitDoesNotBelongToControllingAgency(controllingMessageHeader.TW1_ProcessingUnit, controllingMessageHeader.TW1_ControllingAgency));

				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
				header.Validation.ValidateTW1_ProcessingUnit();
				AssertNoMessageErrorContaining(propertyInfo, ListValidation.InvalidCodeMessageError);
				AssertHasMessageErrorContaining(propertyInfo, ValidationConstants.CusTWControllingMessageHeader.ProcessingUnitDoesNotBelongToControllingAgency(controllingMessageHeader.TW1_ProcessingUnit, controllingMessageHeader.TW1_ControllingAgency));

				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
				header.TW1_ProcessingUnit = "";
				AssertHasMessageErrorContaining("NX101", propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
				header.TW1_ProcessingUnit = "";
				AssertHasMessageErrorContaining("NX301", propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
				header.TW1_ProcessingUnit = "";
				AssertNoMessageErrorContaining("NX301_DN", propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
				header.TW1_ProcessingUnit = "";
				AssertHasMessageErrorContaining("NX401", propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
				header.TW1_ProcessingUnit = "";
				AssertHasMessageErrorContaining("NX601", propertyInfo, MandatoryValidation.YouHaveNotEntered);
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
				header.TW1_ProcessingUnit = "";
				AssertHasMessageErrorContaining("NX603", propertyInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckTW1_RL_NKPortOfLoading()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			ValidationTestHelper.AssertInvalidCodeMessageError(messageHeader.TW1_RL_NKPortOfLoadingInfo, "KKKKK", "USLAX");

			messageHeader.TW1_RL_NKPortOfLoading = "TWZ99";
			AssertNoMessageErrorContaining(messageHeader.TW1_RL_NKPortOfLoadingInfo, ListValidation.InvalidCodeMessageError.ToString());

			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(messageHeader.TW1_RL_NKPortOfLoadingInfo);

			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code4;
			messageHeader.TW1_RL_NKPortOfLoadingInfo.ClearValue();
			AssertNoMessageErrorContaining(messageHeader.TW1_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);

			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			messageHeader.Validation.ValidateTW1_RL_NKPortOfLoading();
			AssertNoMessageErrorContaining(messageHeader.TW1_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTW1_RL_NKPortOfUnloading()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			ValidationTestHelper.AssertInvalidCodeMessageError(messageHeader.TW1_RL_NKPortOfUnloadingInfo, "KKKKK", "USLAX");

			messageHeader.TW1_RL_NKPortOfUnloading = "TWZ99";
			AssertNoMessageErrorContaining(messageHeader.TW1_RL_NKPortOfUnloadingInfo, ListValidation.InvalidCodeMessageError.ToString());

			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(messageHeader.TW1_RL_NKPortOfUnloadingInfo);

			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code4;
			messageHeader.TW1_RL_NKPortOfUnloadingInfo.ClearValue();
			AssertNoMessageErrorContaining(messageHeader.TW1_RL_NKPortOfUnloadingInfo, MandatoryValidation.YouHaveNotEntered);

			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			messageHeader.Validation.ValidateTW1_RL_NKPortOfUnloading();
			AssertNoMessageErrorContaining(messageHeader.TW1_RL_NKPortOfUnloadingInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTW1_PortOfLoadingName()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(messageHeader.TW1_PortOfLoadingNameInfo);

			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code4;
			messageHeader.TW1_PortOfLoadingNameInfo.ClearValue();
			AssertNoMessageErrorContaining(messageHeader.TW1_PortOfLoadingNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTW1_PortOfUnloadingName()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(messageHeader.TW1_PortOfUnloadingNameInfo);

			messageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code4;
			messageHeader.TW1_PortOfUnloadingNameInfo.ClearValue();
			AssertNoMessageErrorContaining(messageHeader.TW1_PortOfUnloadingNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTW1_ProcessingUnitWhenMessageTypeIsNX101()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("COOIU", "Certificate of Origin Issuing Unit");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Type, "The Control Agency to which the Processing Unit belongs", "COOIU", "TW");

			var codeList1 = helper.CreateCusCodeList("TW", "COOIU", "BA", "台灣花卉輸出業同業公會", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("TW", "COOIU", "IG", "臺灣甲魚養殖協會", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("TW", "COOIU", "GB", "經濟部標準檢驗局基隆分局", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			codeList1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Type, "VP");
			Factory.Save();

			var info = controllingMessageHeader.TW1_ProcessingUnitInfo;
			controllingMessageHeader.TW1_ControllingMessageType = "NX101";
			controllingMessageHeader.TW1_CertificateType = "VP";
			controllingMessageHeader.TW1_ProcessingUnit = "GG";
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			controllingMessageHeader.TW1_ProcessingUnit = "BA";
			AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckTW1_PaymentMethod()
		{
			var header = controllingMessageHeader;
			var propertyInfo = controllingMessageHeader.TW1_PaymentMethodInfo;

			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			header.TW1_PaymentMethod = "";
			AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			header.TW1_PaymentMethod = "";
			AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			header.TW1_PaymentMethod = "";
			AssertHasMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			header.TW1_PaymentMethod = "";
			AssertHasMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			header.TW1_PaymentMethod = "";
			AssertHasMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			header.TW1_PaymentMethod = "";
			AssertHasMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
			header.TW1_PaymentMethod = "";
			AssertHasMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);

			controllingMessageHeader.TW1_PaymentMethod = "X";
			AssertHasMessageErrorContaining(propertyInfo, ListValidation.InvalidCodeMessageError);
			controllingMessageHeader.TW1_ControllingAgency = ControllingAgencyList.Codes.VP;
			controllingMessageHeader.TW1_PaymentMethod = controllingMessageHeader.Lookups.CPT_116_PaymentMethodList[0].Code;
			AssertNoMessageErrorContaining(propertyInfo, ListValidation.InvalidCodeMessageError);
			controllingMessageHeader.TW1_PaymentMethod = ZString.Empty;
			AssertHasMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
			controllingMessageHeader.TW1_PaymentMethod = controllingMessageHeader.Lookups.CPT_116_PaymentMethodList[0].Code;
			AssertNoMessageErrors(propertyInfo);
		}

		public void TestCheckTW1_AppointmentPeriod()
		{
			CombineAssertions(() =>
			{
				var header = controllingMessageHeader;
				var propertyInfo = header.TW1_AppointmentPeriodInfo;

				header.TW1_AppointmentDate = new ZDate(2022, 11, 11);
				header.TW1_AppointmentPeriod = "";
				AssertHasMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);

				header.TW1_AppointmentDate = ZDate.Empty;
				header.TW1_AppointmentPeriod = "";
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);

				header.TW1_AppointmentDate = ZDate.Invalid;
				header.TW1_AppointmentPeriod = "";
				AssertNoMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckTW1_AppointmentDateWithNX301_DN()
		{
			var messageError = "Appointment Date should be empty when Business Type is 'C'.";
			var header = controllingMessageHeader;
			var propertyInfo = header.TW1_AppointmentDateInfo;
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			CombineAssertions(() =>
			{
				header.TW1_BusinessType = "C";
				header.TW1_AppointmentDate = new ZDate(2022, 11, 11);
				AssertHasMessageErrorContaining(propertyInfo, messageError);

				header.TW1_AppointmentDate = ZDate.Empty;
				AssertNoMessageErrorContaining(propertyInfo, messageError);

				header.TW1_BusinessType = "B";
				header.TW1_AppointmentDate = new ZDate(2022, 11, 12);
				AssertNoMessageErrorContaining(propertyInfo, messageError);
			});
		}

		public void TestCheckTW1_AppointmentPeriodWithNX301_DN()
		{
			var messageError = "Appointment Period should be empty when Business Type is 'C'.";
			var header = controllingMessageHeader;
			var propertyInfo = header.TW1_AppointmentPeriodInfo;
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			CombineAssertions(() =>
			{
				header.TW1_BusinessType = "C";
				header.TW1_AppointmentPeriod = "A";
				AssertHasMessageErrorContaining(propertyInfo, messageError);

				header.TW1_AppointmentPeriod = ZString.Empty;
				AssertNoMessageErrorContaining(propertyInfo, messageError);

				header.TW1_BusinessType = "B";
				header.TW1_AppointmentPeriod = "B";
				AssertNoMessageErrorContaining(propertyInfo, messageError);
			});
		}

		public void TestCheckTW1_PrePermitNumber()
		{
			var messageErrorPreviousPermitNumberLengthMustbe14 = ValidationConstants.CusTWControllingMessageHeader.PreviousPermitNumberLengthMustbe14;
			var info = controllingMessageHeader.TW1_PrePermitNumberInfo;
			controllingMessageHeader.TW1_PrePermitNumber = ZString.Empty;
			AssertNoMessageError(info, messageErrorPreviousPermitNumberLengthMustbe14);
			controllingMessageHeader.TW1_PrePermitNumber = "A";
			AssertHasMessageError(info, messageErrorPreviousPermitNumberLengthMustbe14);
			controllingMessageHeader.TW1_PrePermitNumber = new ZString('A', 14);
			AssertNoMessageError(info, messageErrorPreviousPermitNumberLengthMustbe14);
			AssertNoMessageErrors(info);
		}

		public void TestCheckTW1_PreWineInspectionStatus()
		{
			CombineAssertions(() =>
			{
				var header = controllingMessageHeader;
				var propertyInfo = header.TW1_PreWineInspectionStatusInfo;
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
				AssertNotEntered(propertyInfo);
				AssertInvalidCode(propertyInfo, "X", new string[] { "1", "2", "3" });
				header.TW1_PreWineInspectionStatus = "1";
				AssertNoMessageErrors(propertyInfo);
			});
		}

		public void TestCheckTW1_Purpose()
		{
			CombineAssertions(() =>
			{
				var header = controllingMessageHeader;
				var propertyInfo = header.TW1_PurposeInfo;
				header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
				header.TW1_BusinessType = "A";
				AssertNotEntered(propertyInfo);
				AssertInvalidCode(propertyInfo, "X", new string[] { "1", "2", "3" });
				header.TW1_BusinessType = "B";
				AssertNotEntered(propertyInfo);
				AssertInvalidCode(propertyInfo, "X", new string[] { "1", "2", "3" });
				header.TW1_BusinessType = "C";
				AssertNotEntered(propertyInfo);
				AssertInvalidCode(propertyInfo, "X", new string[] { "51", "52", "53", "54", "99" });
			});
		}

		public void TestCheckTW1_ControllingMessageType()
		{
			var info = controllingMessageHeader.TW1_ControllingMessageTypeInfo;
			var declaration = controllingMessageHeader.Declaration;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertInvalidCode(info, "X", new string[] { ControllingMessageTypeList.Codes.NX101, ControllingMessageTypeList.Codes.X101, ControllingMessageTypeList.Codes.NX201_01, ControllingMessageTypeList.Codes.NX401 });

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertInvalidCode(info, "X", new string[] { ControllingMessageTypeList.Codes.NX201_01, ControllingMessageTypeList.Codes.NX201_07, ControllingMessageTypeList.Codes.NX301, ControllingMessageTypeList.Codes.NX301_AX, ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX401, ControllingMessageTypeList.Codes.NX601, ControllingMessageTypeList.Codes.NX603 });
		}

		public void TestCheckTW1_CertificateType()
		{
			var info = controllingMessageHeader.TW1_CertificateTypeInfo;
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			controllingMessageHeader.TW1_CertificateType = ZString.Empty;
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
			controllingMessageHeader.TW1_CertificateType = "A";
			AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			controllingMessageHeader.TW1_CertificateType = ZString.Empty;
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
			controllingMessageHeader.TW1_CertificateType = "A";
			AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code0;
			AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
		}

		[ExpectNoExceptions]
		public void TestCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber()
		{
			AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(true, true, CertificateTypeList.Codes.Code8, ControllingMessageTypeList.Codes.NX101, true, true);
			AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(true, true, CertificateTypeList.Codes.Code16, ControllingMessageTypeList.Codes.NX101, true, true);
			AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(false, false, CertificateTypeList.Codes.Code8, ControllingMessageTypeList.Codes.X101, true, true);
			AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(false, false, CertificateTypeList.Codes.Code1, ControllingMessageTypeList.Codes.NX101, true, true);
			AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(false, false, CertificateTypeList.Codes.Code8, ControllingMessageTypeList.Codes.NX101, false, true);
			AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(false, false, CertificateTypeList.Codes.Code8, ControllingMessageTypeList.Codes.NX101, true, false);
		}
		[ExpectNoExceptions]
		public void TestCheckTW1_CertificateType_PreviousDocumentNumber()
		{
			AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(true, false, CertificateTypeList.Codes.Code17, ControllingMessageTypeList.Codes.NX101, true, false);
			AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(false, false, CertificateTypeList.Codes.Code17, ControllingMessageTypeList.Codes.NX101, false, false);
			AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(false, false, CertificateTypeList.Codes.Code1, ControllingMessageTypeList.Codes.NX101, true, false);
		}

		[ExpectNoExceptions]
		public void TestCheckTW1_CertificateType_CertificateofOriginNumber()
		{
			AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(false, true, CertificateTypeList.Codes.Code17, ControllingMessageTypeList.Codes.NX101, false, true);
			AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(false, false, CertificateTypeList.Codes.Code17, ControllingMessageTypeList.Codes.NX101, false, false);
			AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(false, false, CertificateTypeList.Codes.Code1, ControllingMessageTypeList.Codes.NX101, false, true);
		}

		[ExpectNoExceptions]
		void AssertCheckTW1_CertificateType_PreviousDocumentNumberAndCertificateofOriginNumber(bool expectShowErrorPreviousDocumentNumber, bool expectShowErrorCertificateofOriginNumber, ZString certificateType, ZString messageType, bool previousDocumentNumberEmpty, bool certificateofOriginNumberEmpty)
		{
			controllingMessageHeader.TW1_ControllingMessageType = messageType;
			controllingMessageHeader.TW1_CertificateType = certificateType;
			controllingMessageHeader.PreviousDocumentNumbers.RemoveAndDeleteAll();
			controllingMessageHeader.CertificateOfOrigins.RemoveAndDeleteAll();
			if (!previousDocumentNumberEmpty)
			{
				controllingMessageHeader.PreviousDocumentNumbers.AddNew().CSI_ReferenceNumber = "TEST1";
			}
			if (!certificateofOriginNumberEmpty)
			{
				controllingMessageHeader.CertificateOfOrigins.AddNew().CSI_ReferenceNumber = "TEST2";
			}
			controllingMessageHeader.Validation.ValidateTW1_CertificateType();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(controllingMessageHeader.TW1_CertificateTypeInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Previous Document Number")), NUnit.Framework.Is.EqualTo(expectShowErrorPreviousDocumentNumber), "Previous Document Number");
				NUnit.Framework.Assert.That(controllingMessageHeader.TW1_CertificateTypeInfo.HasMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Certificate of Origin Number")), NUnit.Framework.Is.EqualTo(expectShowErrorCertificateofOriginNumber), "Certificate of Origin Number");
			});
		}

		public void TestCheckTW1_IsSpecialApplication()
		{
			var info = controllingMessageHeader.TW1_IsSpecialApplicationInfo;
			var validation = controllingMessageHeader.Validation;
			controllingMessageHeader.TW1_OriginalQuantity = 5;
			controllingMessageHeader.TW1_CopyQuantity = 10;
			controllingMessageHeader.TW1_IsSpecialApplication = false;
			validation.ValidateTW1_IsSpecialApplication();
			AssertNoMessageErrorContaining(info, ValidationConstants.CusTWControllingMessageHeader.SpecialApplicationMustBeTickedForCopyQuantity);
			AssertNoMessageErrorContaining(info, ValidationConstants.CusTWControllingMessageHeader.SpecialApplicationMustBeTickedForOriginalQuantity);

			controllingMessageHeader.TW1_CopyQuantity = 11;
			validation.ValidateTW1_IsSpecialApplication();
			AssertHasMessageErrorContaining(info, ValidationConstants.CusTWControllingMessageHeader.SpecialApplicationMustBeTickedForCopyQuantity);
			AssertNoMessageErrorContaining(info, ValidationConstants.CusTWControllingMessageHeader.SpecialApplicationMustBeTickedForOriginalQuantity);

			controllingMessageHeader.TW1_OriginalQuantity = 6;
			validation.ValidateTW1_IsSpecialApplication();
			AssertNoMessageErrorContaining(info, ValidationConstants.CusTWControllingMessageHeader.SpecialApplicationMustBeTickedForCopyQuantity);
			AssertHasMessageErrorContaining(info, ValidationConstants.CusTWControllingMessageHeader.SpecialApplicationMustBeTickedForOriginalQuantity);

			controllingMessageHeader.TW1_IsSpecialApplication = true;
			AssertNoMessageErrorContaining(info, ValidationConstants.CusTWControllingMessageHeader.SpecialApplicationMustBeTickedForCopyQuantity);
			AssertNoMessageErrorContaining(info, ValidationConstants.CusTWControllingMessageHeader.SpecialApplicationMustBeTickedForOriginalQuantity);
		}

		public void TestCheckTW1_OriginalQuantity()
		{
			var info = controllingMessageHeader.TW1_OriginalQuantityInfo;
			controllingMessageHeader.TW1_OriginalQuantity = 3;
			AssertNoMessageErrorContaining(info, ValidationConstants.CusTWControllingMessageHeader.OriginalQuantityCannotMoreThanOne);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			controllingMessageHeader.TW1_OriginalQuantity = 2;
			AssertHasMessageErrorContaining(info, ValidationConstants.CusTWControllingMessageHeader.OriginalQuantityCannotMoreThanOne);

			controllingMessageHeader.TW1_OriginalQuantity = 1;
			AssertNoMessageErrorContaining(info, ValidationConstants.CusTWControllingMessageHeader.OriginalQuantityCannotMoreThanOne);
		}

		public void TestCheckTW1_EUSteelProductNo()
		{
			var info = controllingMessageHeader.TW1_EUSteelProductNoInfo;
			AssertInvalidCode(info, "X", new string[] { "01", "02" });
		}

		public void TestCheckTW1_EUSteelProductPhase()
		{
			var info = controllingMessageHeader.TW1_EUSteelProductPhaseInfo;
			AssertInvalidCode(info, "X", new string[] { "1", "2", "3", "4", "5" });
		}

		public void TestCheckTW1_ManufacturerPrintingCode()
		{
			var info = controllingMessageHeader.TW1_ManufacturerPrintingCodeInfo;
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code9;
			AssertInvalidCode(info, "X", new string[] { "1", "2", "3" });

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code11;
			AssertInvalidCode(info, "X", new string[] { "1", "2", "3" });

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			AssertInvalidCode(info, "X", new string[] { "1", "2", "3" });

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code14;
			AssertInvalidCode(info, "X", new string[] { "1", "2", "3" });

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			AssertInvalidCode(info, "X", new string[] { "1", "2", "3" });

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code5;
			CombineAssertions(() =>
			{
				AssertInvalidCode(info, "1", System.Array.Empty<string>());
				AssertInvalidCode(info, "2", System.Array.Empty<string>());
				AssertInvalidCode(info, "3", System.Array.Empty<string>());
			});
		}

		public void TestCheckTW1_PrintingCode()
		{
			var info = controllingMessageHeader.TW1_PrintingCodeInfo;
			AssertInvalidCode(info, "X", new string[] { "01", "02", "03", "04" });

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(info);
		}

		public void TestCheckTW1_OH_Applicant()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "OrgTest1";
			header.OH_RL_NKClosestPort = "TW";
			header.OH_IsConsignee = true;
			var mainAddress = header.MainAddress;
			mainAddress.OA_CompanyNameOverride = "HAPPY CO., LTD.";
			mainAddress.OA_Language = "EN";
			mainAddress.UnrestrictedAdditionalAddressInformation = "addinfo address";
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Address2 = "ORANGE DISTRICT";
			mainAddress.OA_RN_NKCountryCode = "TW";
			mainAddress.OA_City = "APPLE CITY";
			mainAddress.Postcode = "12345";
			mainAddress.OA_State = "TPE";
			mainAddress.OA_Phone = "+1 (273) 5495200";
			mainAddress.OA_Email = "001@xx.com";

			var messageHeader = Factory.New<CusTWControllingMessageHeader>();
			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var address = messageHeader.ApplicantDocumentaryAddress;
			address.OrganisationPK = header.PK;
			var message = "You have not entered an Applicant Local Company Name.";
			AssertHasMessageError(messageHeader.TW1_OH_ApplicantInfo, message);

			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			AssertNoMessageError(messageHeader.TW1_OH_ApplicantInfo, message);

			messageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			AssertHasMessageError(messageHeader.TW1_OH_ApplicantInfo, message);

			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "OrgTest1";
			header2.OH_RL_NKClosestPort = "TW";
			header2.OH_IsConsignee = true;
			var mainAddress2 = header2.MainAddress;
			var zhTWtranslatedAddress1 = mainAddress2.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = "ZH-TW";
			zhTWtranslatedAddress1.UnrestrictedAdditionalAddressInformation = "附加信息2";
			zhTWtranslatedAddress1.OTA_CompanyName = "綠晃科技股份有限公司";
			zhTWtranslatedAddress1.OTA_Address1 = "臺北加工出口區園東街6號";
			zhTWtranslatedAddress1.OTA_Address2 = string.Empty;
			zhTWtranslatedAddress1.OTA_City = "臺北巿";
			zhTWtranslatedAddress1.OTA_PostCode = "90093";
			zhTWtranslatedAddress1.OTA_State = "TPE";
			zhTWtranslatedAddress1.ClosestPort = "TW";

			address.OrganisationPK = header2.PK;
			AssertNoMessageError(messageHeader.TW1_OH_ApplicantInfo, message);
		}

		public void TestCheckTW1_OH_Supplier()
		{
			controllingMessageHeader.RunPreSaveValidation();
			AssertHasMessageErrorContaining(controllingMessageHeader.TW1_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

			var header = Factory.NewWithValidTestData<OrgHeader>();
			controllingMessageHeader.SupplierDocumentaryAddress.OrganisationPK = header.PK;
			AssertNoMessageErrorContaining(controllingMessageHeader.TW1_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTW1_OH_Importer()
		{
			controllingMessageHeader.RunPreSaveValidation();
			AssertHasMessageErrorContaining(controllingMessageHeader.TW1_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			var header = Factory.NewWithValidTestData<OrgHeader>();
			controllingMessageHeader.ImporterDocumentaryAddress.OrganisationPK = header.PK;
			AssertNoMessageErrorContaining(controllingMessageHeader.TW1_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTW1_BeforeClearanceApplicationReason()
		{
			var message = "You have not selected a Goods Release Reason.";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var controllingMessageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code18;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var targetInfo = controllingMessageHeader.TW1_BeforeClearanceApplicationReasonInfo;

			var cusNum = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = entryHeader.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = Customs.Business.JobMessageTypeList.Codes.Import;
			cusNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum.CE_EntryStatus = ZString.Empty;

			controllingMessageHeader.TW1_BeforeClearanceApplicationReason = CPT_127_GoodsReleaseReasonCodeList.Codes._01;
			AssertNoMessageError(targetInfo, message);

			controllingMessageHeader.TW1_BeforeClearanceApplicationReason = ZString.Empty;
			AssertHasMessageError(targetInfo, message);

			cusNum.CE_EntryStatus = "C1";
			controllingMessageHeader.TW1_BeforeClearanceApplicationReason = CPT_127_GoodsReleaseReasonCodeList.Codes._01;
			AssertNoMessageError(targetInfo, message);

			controllingMessageHeader.TW1_BeforeClearanceApplicationReason = ZString.Empty;
			AssertNoMessageError(targetInfo, message);

			cusNum.CE_EntryStatus = ZString.Empty;
			controllingMessageHeader.Validation.ValidateTW1_BeforeClearanceApplicationReason();
			AssertHasMessageError(targetInfo, message);

			controllingMessageHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			controllingMessageHeader.Validation.ValidateTW1_BeforeClearanceApplicationReason();
			AssertNoMessageError(targetInfo, message);

			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "07", CPT_127_GoodsReleaseReasonCodeList.Codes._01);
		}

		public void TestCheckBulkApplicationID()
		{
			var messageError = "Bulk Application ID must consist 14 alphanumeric characters.";
			var targetInfo = controllingMessageHeader.BulkApplicationIDInfo;
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_AX;
			controllingMessageHeader.BulkApplicationID = new ZString('A', 13);
			AssertHasMessageError(targetInfo, messageError);
			controllingMessageHeader.BulkApplicationID = new ZString('A', 14);
			AssertNoMessageError(targetInfo, messageError);
			controllingMessageHeader.BulkApplicationID = ZString.Empty;
			AssertNoMessageError(targetInfo, messageError);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			controllingMessageHeader.BulkApplicationID = new ZString('A', 13);
			AssertNoMessageError(targetInfo, messageError);
		}

		public void TestCheckTW1_PortOfBulkCommodity()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(controllingMessageHeader.TW1_PortOfBulkCommodityInfo, "7", "1");
		}

		public void TestCheckBulkPaymentID()
		{
			var messageError = "Bulk Payment ID must consist 14 alphanumeric characters.";
			var targetInfo = controllingMessageHeader.BulkPaymentIDInfo;
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_AX;
			controllingMessageHeader.BulkPaymentID = new ZString('A', 13);
			AssertHasMessageError(targetInfo, messageError);
			controllingMessageHeader.BulkPaymentID = new ZString('A', 14);
			AssertNoMessageError(targetInfo, messageError);
			controllingMessageHeader.BulkPaymentID = ZString.Empty;
			AssertNoMessageError(targetInfo, messageError);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			controllingMessageHeader.BulkPaymentID = new ZString('A', 13);
			AssertNoMessageError(targetInfo, messageError);
		}

		public void TestCheckCustomsMessageIdentifier()
		{
			var messageError = "Customs Message Identifier must consist 19 alphanumeric characters";
			var targetInfo = controllingMessageHeader.CustomsMessageIdentifierInfo;
			controllingMessageHeader.CustomsMessageIdentifier = new ZString('A', 18);
			AssertHasMessageError(targetInfo, messageError);
			controllingMessageHeader.CustomsMessageIdentifier = new ZString('A', 19);
			AssertNoMessageError(targetInfo, messageError);
			controllingMessageHeader.CustomsMessageIdentifier = ZString.Empty;
			AssertNoMessageError(targetInfo, messageError);
		}

		void AssertNotEntered(ZPropertyInfo info)
		{
			var messageErrorYouHaveNotEntered = MandatoryValidation.YouHaveNotEntered;
			info.Value = ZString.Empty;
			AssertHasMessageErrorContaining(info, messageErrorYouHaveNotEntered);
			info.Value = new ZString("1");
			AssertNoMessageErrorContaining(info, messageErrorYouHaveNotEntered);
		}

		void AssertInvalidCode(ZPropertyInfo info, string invalidCode, params string[] validCodes)
		{
			info.Value = new ZString(invalidCode);
			var messageErrorInvalidCode = ListValidation.InvalidCodeMessageError;
			AssertHasMessageErrorContaining(info, messageErrorInvalidCode);
			foreach (var code in validCodes)
			{
				info.Value = new ZString(code);
				AssertNoMessageErrorContaining(info, messageErrorInvalidCode);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateRefCusCodeForControllingMessageType();
			controllingMessageHeader = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
		}

		CusTWControllingMessageHeader controllingMessageHeader;
	}
}
