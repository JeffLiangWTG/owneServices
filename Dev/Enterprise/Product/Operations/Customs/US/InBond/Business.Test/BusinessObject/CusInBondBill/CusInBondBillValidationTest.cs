using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB0_MasterBillNumber()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB12334";
			AssertNoMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_MasterBillNumber = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			CusInBondBill bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "MB12334";
			bill.B0_MasterBillNumber = "MB12334";
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillNumberIsDuplicated.ToString());
			bill.B0_MasterBillNumber = "MB12332";
			AssertNoError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillNumberIsDuplicated);
			bill.B0_MasterBillNumber = "MB01234567890";
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillNumberLengthExceeded.ToString());
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
				bill.B0_MasterBillNumber = ZString.Empty;
				AssertHasMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
				bill2.B0_MasterBillNumber = "MB12334";
				bill.B0_MasterBillNumber = "MB12334";
				AssertHasMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillUniqueCodeIsDuplicated.ToString());
				bill2.B0_MasterBillNumber = "MB12334";
				bill2.B0_HouseBillNumber = "HB1234";
				bill.B0_MasterBillNumber = "MB12334";
				AssertNoMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillUniqueCodeIsDuplicated.ToString());
				bill2.B0_MasterBillNumber = "MB12334";
				bill2.B0_HouseBillNumber = "HB1234";
				bill.B0_HouseBillNumber = "HB1234";
				bill.B0_MasterBillNumber = "MB12334";
				AssertHasMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillUniqueCodeIsDuplicated.ToString());
			}

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			bill2.B0_MasterBillNumber = "MB12335";
			bill2.B0_HouseBillNumber = "HB1235";
			bill.B0_HouseBillNumber = "HB1235";
			bill.B0_MasterBillNumber = "MB12335";
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillUniqueCodeIsDuplicated.ToString());
			bill.B0_MasterBillNumber = "MB01234567890";
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, CusInBondAirWayBillValidator.MasterBillLengthWarningMessage);
			bill.B0_MasterBillNumber = "123-456789";
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillNumberContainsSpecialCharacters.ToString());
			header.BH_FTZMove = ZBool.True;
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			bill.B0_MasterBillNumber = "123-456789";
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillNumberContainsSpecialCharacters.ToString());
			AssertNoMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.OrignalEntryAdmissionNumberContainsSpecialCharacters.ToString());
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselNonContainer;
			bill.B0_MasterBillNumber = "123-456789";
			AssertNoMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillNumberContainsSpecialCharacters.ToString());
			AssertNoMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.OrignalEntryAdmissionNumberContainsSpecialCharacters.ToString());
			bill.B0_MasterBillNumber = "123-!456789";
			AssertNoMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillNumberContainsSpecialCharacters.ToString());
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.OrignalEntryAdmissionNumberContainsSpecialCharacters.ToString());
		}

		public void TestMasterBillFormatForMigrationDateOfAirInbondToAC()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var bill1 = header.Bills.AddNew();
			var usCarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(Enterprise.ZArchitecture.Schema.USCarrierCombinedSchema.UI_Code, "A8"));
			if (usCarrier == null)
			{
				usCarrier = Factory.New<USCarrierCombined>();
				usCarrier.UI_Code = "A8";
				usCarrier.UI_ModeOfTransportation = "40";
				usCarrier.UI_Name = "Test Carrier";
			}

			usCarrier.UI_AirwayBillPrefix = "785";
			bill1.B0_IssuerCode = usCarrier.UI_Code;
			bill1.B0_MasterBillNumber = "ABC123456478";
			AssertHasMessageError("Warnings for Air Master Bill number", bill1.B0_MasterBillNumberInfo, "The MAWB should contain 11 digits.");
			bill1.B0_MasterBillNumber = "78512345678";
			AssertNoMessageError(bill1.B0_MasterBillNumberInfo, "The MAWB should contain 11 digits.");
			usCarrier.UI_AirwayBillPrefix = "AMF";
			bill1.B0_MasterBillNumber = "ABC123456478";
			AssertNoMessageError(bill1.B0_MasterBillNumberInfo, "The MAWB should contain 11 digits.");
			bill1.B0_MasterBillNumber = "123";
			AssertHasMessageError("Warnings for Air Master Bill number", bill1.B0_MasterBillNumberInfo, CusInBondAirWayBillValidator.MasterBillLengthWarningMessage);
			bill1.B0_MasterBillNumber = "4781234A678";
			AssertHasMessageError("Warnings for Air Master Bill number", bill1.B0_MasterBillNumberInfo, CusInBondAirWayBillValidator.MasterBillFormatWarningMessage);
		}

		public void TestCheckB0_HouseBillNumber()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var header = Factory.New<CusInBondHeader>();
				var bill = header.Bills.AddNew();
				var bill2 = header.Bills.AddNew();
				header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
				bill.B0_MasterBillNumber = "MB12344";
				bill.B0_HouseBillNumber = "HB12344";
				bill2.B0_MasterBillNumber = "MB12344";
				bill2.B0_HouseBillNumber = "HB12344";
				AssertHasMessageError(bill2.B0_HouseBillNumberInfo, ValidationConstants.Bill.BillUniqueCodeIsDuplicated.ToString());
				header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
				bill.B0_MasterBillNumber = "MB12345";
				bill.B0_HouseBillNumber = "HB12345";
				bill2.B0_MasterBillNumber = "MB12345";
				bill2.B0_HouseBillNumber = "HB12345";
				AssertHasMessageError(bill2.B0_HouseBillNumberInfo, ValidationConstants.Bill.BillUniqueCodeIsDuplicated.ToString());
				bill2.B0_HouseBillNumber = "1234567890123456";
				AssertHasMessageError(bill2.B0_HouseBillNumberInfo, ValidationConstants.Bill.BillNumberLengthExceeded.ToString());
				bill2.B0_HouseBillNumber = "123456789012";
				AssertNoMessageError(bill2.B0_HouseBillNumberInfo, ValidationConstants.Bill.BillNumberLengthExceeded.ToString());
				AssertNoMessageError(bill2.B0_HouseBillNumberInfo, ValidationConstants.Bill.BillNumberContainsSpecialCharacters.ToString());
				bill2.B0_HouseBillNumber = "MB0-123456";
				AssertHasMessageError(bill2.B0_HouseBillNumberInfo, ValidationConstants.Bill.BillNumberContainsSpecialCharacters.ToString());
				header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
				bill.B0_HouseBillIssuerCode = "ABCD";
				bill.B0_HouseBillNumber = "123";
				AssertNoMessageErrorContaining(bill.B0_HouseBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
				bill.B0_HouseBillNumber = "";
				AssertHasMessageErrorContaining(bill.B0_HouseBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckB0_HouseBillIssuerCode()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var header = Factory.New<CusInBondHeader>();
				var bill = header.Bills.AddNew();
				header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
				bill.B0_HouseBillNumber = "123";
				bill.B0_HouseBillIssuerCode = "ABCD";
				AssertNoMessageErrorContaining(bill.B0_HouseBillIssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
				bill.B0_HouseBillIssuerCode = "";
				AssertHasMessageErrorContaining(bill.B0_HouseBillIssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckB0_PlaceOfReceiptDCode()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "123!", "Test Name", startDate, endDate);
			newFactory.Save();

			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			bill.B0_PlaceOfReceiptDCode = "23!2";
			AssertHasMessageErrorContaining(bill.B0_PlaceOfReceiptDCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_PlaceOfReceiptDCode = "123!";
			AssertNoMessageErrorContaining(bill.B0_PlaceOfReceiptDCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_PlaceOfReceiptDCode = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_PlaceOfReceiptDCodeInfo, ListValidation.InvalidCodeMessageError);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			bill.B0_PlaceOfReceiptDCode = "23!2";
			AssertHasWarningContaining(bill.B0_PlaceOfReceiptDCodeInfo, ListValidation.InvalidCodeMessage);
			bill.B0_PlaceOfReceiptDCode = "123!";
			AssertNoWarningContaining(bill.B0_PlaceOfReceiptDCodeInfo, ListValidation.InvalidCodeMessage);
			bill.B0_PlaceOfReceiptDCode = ZString.Empty;
			AssertNoWarningContaining(bill.B0_PlaceOfReceiptDCodeInfo, ListValidation.InvalidCodeMessage);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			bill.B0_PlaceOfReceiptDCode = "23!2";
			AssertNoMessageErrorContaining(bill.B0_PlaceOfReceiptDCodeInfo, ListValidation.InvalidCodeMessageError);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			bill.B0_PlaceOfReceiptDCode = "23!2";
			AssertNoWarningContaining(bill.B0_PlaceOfReceiptDCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckB0_IssuerCode()
		{
			USCarrierCombined carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "SC1Z";
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "Z!Z";
			AssertHasMessageErrorContaining(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_IssuerCode = "SC1Z";
			AssertNoMessageErrorContaining(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessageError);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			bill.B0_IssuerCode = "Z!Z";
			AssertHasWarningContaining(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessage);
			bill.B0_IssuerCode = "SC1Z";
			AssertNoWarningContaining(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessage);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			bill.B0_IssuerCode = "Z!Z";
			AssertNoMessageErrorContaining(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_IssuerCode = "";
			AssertHasMessageErrorContaining(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckB0_IssuerCodeWithFirms()
		{
			var messageError = ValidationConstants.Bill.IssuerCodeShouldBeTheSameAsFirms.ToString();
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = false;
			header.BH_CarrierSCAC = "SCAC";
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "";
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_InBondCarrierSCAC = "ABCD";
			header.BH_FTZMove = false;
			bill1.B0_IssuerCode = ZString.Empty;
			AssertNoMessageErrorContaining(bill1.B0_IssuerCodeInfo, messageError);
			header.BH_FIRMS = "W256";
			header.BH_FTZMove = true;
			AssertEquals("ABCD", moveHeader1.BM_InBondCarrierSCAC);
			AssertEquals("should be same as SCAC", "ABCD", bill1.B0_IssuerCode);
			AssertNoMessageErrorContaining(bill1.B0_IssuerCodeInfo, messageError);
			bill1.B0_IssuerCode = "TEST";
			bill1.Validation.ValidateB0_IssuerCode();
			AssertNoMessageErrorContaining(bill1.B0_IssuerCodeInfo, messageError);
			bill1.B0_IssuerCode = "W200";
			bill1.Validation.ValidateB0_IssuerCode();
			AssertHasMessageErrorContaining(bill1.B0_IssuerCodeInfo, messageError);
		}

		public void TestCheckB0_PortOfLadingKCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Z!123", "Z!123", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			var foreignPort1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "R!456", "R!456", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			header.BH_ImportTransportMode = ZString.Empty;
			var bill = header.Bills.AddNew();
			bill.B0_PortOfLadingKCode = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.B0_PortOfLadingKCode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_PortOfLadingKCode = "Z!Z11";
			AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_PortOfLadingKCode = "Z!123";
			AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, ListValidation.InvalidCodeMessageError);
			foreach (ZString transportMode in new ZString[] {
				InBondTransportModeCodes.Codes.RailContainer,
				InBondTransportModeCodes.Codes.RailNonContainer,
				InBondTransportModeCodes.Codes.TruckContainer,
				InBondTransportModeCodes.Codes.TruckNonContainer
			})
			{
				header.BH_ImportTransportMode = transportMode;
				bill.B0_PortOfLadingKCode = "Z!123";
				AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, ListValidation.InvalidCodeMessageError);
				bill.B0_PortOfLadingKCode = "R!456";
				AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, ListValidation.InvalidCodeMessageError);
			}

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			bill.B0_PortOfLadingKCode = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			bill.B0_PortOfLadingKCode = "Z!123";
			AssertNoErrors("No B0_PortOfLadingKCodeInfo error for Air Mode", bill.B0_PortOfLadingKCodeInfo);
			bill.B0_PortOfLadingKCode = "R!456";
			AssertNoErrors("No B0_PortOfLadingKCodeInfo error for Air Mode", bill.B0_PortOfLadingKCodeInfo);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselNonContainer;
			header.BH_FTZMove = true;
			bill.B0_PortOfLadingKCode = ZString.Empty;
			AssertNoMessageError(bill.B0_PortOfLadingKCodeInfo, ValidationConstants.Bill.FTZForeignPortOfLading.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.Validation.ValidateB0_PortOfLadingKCode();
			AssertHasMessageError(bill.B0_PortOfLadingKCodeInfo, ValidationConstants.Bill.FTZForeignPortOfLading.ToString());
			bill.B0_PortOfLadingKCode = "62000";
			AssertHasMessageError(bill.B0_PortOfLadingKCodeInfo, ValidationConstants.Bill.FTZForeignPortOfLading.ToString());
			bill.B0_PortOfLadingKCode = CusInBondBill.FTZForeignPortOfLading;
			AssertNoMessageError(bill.B0_PortOfLadingKCodeInfo, ValidationConstants.Bill.FTZForeignPortOfLading.ToString());
		}

		public void TestCheckB0_ManifestQty()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondBill bill = header.Bills.AddNew();
			bill.B0_ManifestQty = ZInt.Zero;
			AssertNoMessageError(bill.B0_ManifestQtyInfo, ValidationConstants.Bill.ManifestQuantityMustBeGreaterThanZero.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.B0_ManifestQty = ZInt.Zero;
			AssertHasMessageError(bill.B0_ManifestQtyInfo, ValidationConstants.Bill.ManifestQuantityMustBeGreaterThanZero.ToString());
			bill.B0_ManifestQty = 10;
			AssertNoMessageError(bill.B0_ManifestQtyInfo, ValidationConstants.Bill.ManifestQuantityMustBeGreaterThanZero.ToString());
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			bill.B0_ManifestQty = ZInt.Zero;
			AssertNoMessageError(bill.B0_ManifestQtyInfo, ValidationConstants.Bill.ManifestQuantityMustBeGreaterThanZero.ToString());
		}

		public void TestCheckB0_ManifestUQ()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondBill bill = header.Bills.AddNew();
			bill.B0_ManifestUQ = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.B0_ManifestUQ = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_ManifestUQInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_ManifestUQ = "Z!";
			AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_ManifestUQInfo, ListValidation.InvalidCodeMessageError);
			foreach (CodeDescriptionPair pair in Factory.GetCachedValue<InBondManifestUQList>())
			{
				bill.B0_ManifestUQ = pair.Code;
				AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, ListValidation.InvalidCodeMessageError);
			}

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.B0_ManifestUQ = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			header.RecalculateValidationModesOnHeader(InBondMessageType.DiversionRequest);
			bill.B0_ManifestUQ = "!A";
			AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_VolumeUQ = "^A";
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			header.RecalculateValidationModesOnHeader(InBondMessageType.DepartureAdd);
			bill.Validation.ValidateB0_VolumeUQ();
			AssertHasMessageErrorContaining(bill.B0_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckB0_Weight()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondBill bill = header.Bills.AddNew();
			bill.B0_WeightUQ = Core.Constants.Weight.Pounds;
			bill.B0_Weight = ZDecimal.Zero;
			AssertNoMessageError(bill.B0_WeightInfo, US.Messaging.Business.ValidationConstants.Weight.WeightMustBeGreaterThanZero);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.B0_Weight = ZDecimal.Zero;
			AssertHasMessageError(bill.B0_WeightInfo, US.Messaging.Business.ValidationConstants.Weight.WeightMustBeGreaterThanZero);
			bill.B0_Weight = 10m;
			AssertNoMessageError(bill.B0_WeightInfo, US.Messaging.Business.ValidationConstants.Weight.WeightMustBeGreaterThanZero);
			bill.B0_Weight = 99999999999m;
			AssertHasMessageErrorContaining(bill.B0_WeightInfo, "is greater than the maximum allowed pounds '9,999,999,999'.");
			bill.B0_Weight = 425.56m;
			AssertNoMessageErrorContaining(bill.B0_WeightInfo, "is greater than the maximum allowed pounds '9,999,999,999'.");
			AssertHasWarning(bill.B0_WeightInfo, US.Messaging.Business.ValidationConstants.Weight.WeightUQInWholePounds);
			bill.B0_Weight = 8987544m;
			AssertNoWarning(bill.B0_WeightInfo, US.Messaging.Business.ValidationConstants.Weight.WeightUQInWholePounds);
			bill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
			bill.B0_Weight = 99999999999m;
			AssertHasMessageErrorContaining(bill.B0_WeightInfo, "is greater than the maximum allowed kilograms '9,999,999,999'.");
			bill.B0_Weight = 425.56m;
			AssertNoMessageErrorContaining(bill.B0_WeightInfo, "is greater than the maximum allowed kilograms '9,999,999,999'.");
			AssertHasWarning(bill.B0_WeightInfo, US.Messaging.Business.ValidationConstants.Weight.WeightUQInWholeKilograms);
			bill.B0_Weight = 8987544m;
			AssertNoWarning(bill.B0_WeightInfo, US.Messaging.Business.ValidationConstants.Weight.WeightUQInWholeKilograms);
			bill.B0_WeightUQ = Core.Constants.Weight.Kilotonnes;
			bill.B0_Weight = 999999999m;
			AssertHasMessageErrorContaining(bill.B0_WeightInfo, "is greater than the maximum allowed kilograms '9,999,999,999'.");
			bill.B0_Weight = 425.56m;
			AssertNoMessageErrorContaining(bill.B0_WeightInfo, "is greater than the maximum allowed kilograms '9,999,999,999'.");
			AssertNoWarning(bill.B0_WeightInfo, US.Messaging.Business.ValidationConstants.Weight.WeightUQInWholeKilograms);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.B0_Weight = ZDecimal.Zero;
			AssertNoMessageError(bill.B0_WeightInfo, US.Messaging.Business.ValidationConstants.Weight.WeightMustBeGreaterThanZero);
		}

		public void TestCheckB0_WeightUQ()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondBill bill = header.Bills.AddNew();
			bill.B0_Weight = 1m;
			bill.B0_WeightUQ = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.B0_WeightUQ = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_WeightUQ = "Z!";
			AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_WeightUQInfo, ListValidation.InvalidCodeMessageError);
			foreach (CodeDescriptionPair pair in new CodeDescriptionPairList(OLookUpEditType.Weight))
			{
				bill.B0_WeightUQ = pair.Code;
				AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, ListValidation.InvalidCodeMessageError);
			}

			bill.B0_WeightUQ = Core.Constants.Weight.Hectograms;
			bill.B0_Weight = 425.56m;
			AssertHasWarning(bill.B0_WeightUQInfo, US.Messaging.Business.ValidationConstants.Weight.WeightUQTypeAllowed);
			foreach (string code in new string[] { Core.Constants.Weight.Kilograms, Core.Constants.Weight.Pounds })
			{
				bill.B0_WeightUQ = code;
				AssertNoWarning(bill.B0_WeightUQInfo, US.Messaging.Business.ValidationConstants.Weight.WeightUQTypeAllowed);
			}

			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Weight);
			list.RemoveCode(Core.Constants.Weight.Kilograms);
			list.RemoveCode(Core.Constants.Weight.Pounds);
			foreach (CodeDescriptionPair pair in list)
			{
				bill.B0_WeightUQ = pair.Code;
				AssertHasWarning(bill.B0_WeightUQInfo, US.Messaging.Business.ValidationConstants.Weight.WeightUQTypeAllowed);
			}

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.B0_WeightUQ = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			header.RecalculateValidationModesOnHeader(InBondMessageType.DiversionRequest);
			bill.B0_WeightUQ = "!A";
			bill.Validation.ValidateB0_WeightUQ();
			AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckB0_Volume()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondBill bill = header.Bills.AddNew();
			bill.B0_VolumeUQ = Core.Constants.Volume.CubicFeet;
			bill.B0_Volume = -10m;
			AssertNoMessageError(bill.B0_VolumeInfo, US.Messaging.Business.ValidationConstants.Volume.VolumeMustBeGreaterThanZero);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.B0_Volume = -10m;
			AssertHasMessageError(bill.B0_VolumeInfo, US.Messaging.Business.ValidationConstants.Volume.VolumeMustBeGreaterThanZero);
			bill.B0_Volume = ZDecimal.Zero;
			AssertNoMessageError(bill.B0_VolumeInfo, US.Messaging.Business.ValidationConstants.Volume.VolumeMustBeGreaterThanZero);
			bill.B0_Volume = 10m;
			AssertNoMessageError(bill.B0_VolumeInfo, US.Messaging.Business.ValidationConstants.Volume.VolumeMustBeGreaterThanZero);
			bill.B0_Volume = 99999999999m;
			AssertHasMessageErrorContaining(bill.B0_VolumeInfo, "is greater than the maximum allowed cubic feet '9,999,999,999'.");
			bill.B0_Volume = 425.56m;
			AssertNoMessageErrorContaining(bill.B0_VolumeInfo, "is greater than the maximum allowed cubic feet '9,999,999,999'.");
			AssertHasWarning(bill.B0_VolumeInfo, US.Messaging.Business.ValidationConstants.Volume.VolumeUQInWholeCubicFeet);
			bill.B0_Volume = 8987544m;
			AssertNoWarning(bill.B0_VolumeInfo, US.Messaging.Business.ValidationConstants.Volume.VolumeUQInWholeCubicFeet);
			bill.B0_VolumeUQ = Core.Constants.Volume.CubicMetres;
			bill.B0_Volume = 99999999999m;
			AssertHasMessageErrorContaining(bill.B0_VolumeInfo, "is greater than the maximum allowed cubic meters '9,999,999,999'.");
			bill.B0_Volume = 425.56m;
			AssertNoMessageErrorContaining(bill.B0_VolumeInfo, "is greater than the maximum allowed cubic meters '9,999,999,999'.");
			AssertHasWarning(bill.B0_VolumeInfo, US.Messaging.Business.ValidationConstants.Volume.VolumeUQInWholeCubicMetres);
			bill.B0_Volume = 8987544m;
			AssertNoWarning(bill.B0_VolumeInfo, US.Messaging.Business.ValidationConstants.Volume.VolumeUQInWholeCubicMetres);
			bill.B0_VolumeUQ = Core.Constants.Volume.MegaLitre;
			bill.B0_Volume = 999999999m;
			AssertHasMessageErrorContaining(bill.B0_VolumeInfo, "is greater than the maximum allowed cubic meters '9,999,999,999'.");
			bill.B0_Volume = 425.56m;
			AssertNoMessageErrorContaining(bill.B0_VolumeInfo, "is greater than the maximum allowed cubic meters '9,999,999,999'.");
			AssertNoWarning(bill.B0_VolumeInfo, US.Messaging.Business.ValidationConstants.Volume.VolumeUQInWholeCubicMetres);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.B0_Volume = -10m;
			AssertNoMessageError(bill.B0_VolumeInfo, US.Messaging.Business.ValidationConstants.Volume.VolumeMustBeGreaterThanZero);
		}

		public void TestCheckB0_VolumeUQ()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondBill bill = header.Bills.AddNew();
			bill.B0_Volume = 1m;
			bill.B0_VolumeUQ = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.B0_VolumeUQ = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_VolumeUQ = "Z!";
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			foreach (CodeDescriptionPair pair in Factory.GetCachedValue<VolumeUnitList>())
			{
				bill.B0_VolumeUQ = pair.Code;
				AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			}

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			bill.B0_VolumeUQ = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
