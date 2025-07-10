using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class ValidationModesCalculatorTest : TestCaseWithFactory
	{
		public void TestRecalculateDiversionRequest()
		{
			var header = Factory.New<CusInBondHeader>();
			header.RecalculateValidationModesOnHeader(InBondMessageType.DiversionRequest);
			Assert(header.IsDiversionRequestMode);
		}

		public void TestRecalculateValidationModesOnHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			header.RecalculateValidationModesOnHeader(InBondMessageType.AirInBondAmend);
			Assert(header.IsAirInBondInitiationAndDeletionMode);
		}

		public void TestUpdatingValidationModesOnLoadedDoesNotMarkAsNeedingValidationIfSame()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2709", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = Enterprise.Customs.US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_CarrierSCAC = "QF";
			header.BH_PortUnladingDCode = "2709";
			header.BH_VoyageNumber = "323";
			CusInBondBill bill = header.Bills.AddNew();
			Factory.Save(); //will have BH_JobReference assigned and it will be marked as invalid
			AssertEquals("pre-condition", false, header.LightValidationIsValid);
			header.BH_ETA = ZDateTime.Today;
			header.RunPreSaveValidation();
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var headerLoaded = factory2.Load<CusInBondHeader>(header.PK);
			AssertEquals(true, headerLoaded.LightValidationIsValid);
		}

		public void TestIsThisValidationOn()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			Factory.Save(); //will have JE_JobReference assigned and it will be marked as invalid
			ValidationModesCalculator calculator = new ValidationModesCalculator(header);
			AssertEquals(ValidationModes.Departure, header.ValidationModes);
			AssertEquals(true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.Departure));
			AssertEquals(false, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.InBondLevelArrival));
			AssertEquals(false, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.InBondLevelExportation));
			AssertEquals(false, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.InBondLevelTransferOfLiability));
			header.ValidationModes = ValidationModes.InBondLevelArrival;
			AssertEquals(true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.InBondLevelArrival));
			header.ValidationModes = ValidationModes.BillOfLadingLevelArrival;
			AssertEquals(true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.BillOfLadingLevelArrival));
			header.ValidationModes = ValidationModes.ContainerLevelArrival;
			AssertEquals(true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.ContainerLevelArrival));
			header.ValidationModes = ValidationModes.InBondLevelExportation;
			AssertEquals(true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.InBondLevelExportation));
			header.ValidationModes = ValidationModes.BillOfLadingLevelExportation;
			AssertEquals(true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.BillOfLadingLevelExportation));
			header.ValidationModes = ValidationModes.ContainerLevelExportation;
			AssertEquals(true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.ContainerLevelExportation));
			header.ValidationModes = ValidationModes.InBondLevelTransferOfLiability;
			AssertEquals(true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.InBondLevelTransferOfLiability));
			header.ValidationModes = ValidationModes.AirInitiationAndDeletion;
			AssertEquals("Expect validation mode is AirInitiationAndDeletion", true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.AirInitiationAndDeletion));
			header.ValidationModes = ValidationModes.AirEntireInBondArrival;
			AssertEquals("Expect validation mode is AirEntireInBondArrival", true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.AirEntireInBondArrival));
			header.ValidationModes = ValidationModes.AirEntireInBondExportation;
			AssertEquals("Expect validation mode is AirEntireInBondExportation", true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.AirEntireInBondExportation));
			header.ValidationModes = ValidationModes.BillOfLadingDelete;
			AssertEquals("Expect validation mode is BillOfLadingDelete", true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.BillOfLadingDelete));
			header.ValidationModes = ValidationModes.BillOfLadingDelete;
			AssertEquals("Expect validation mode is BillOfLadingDelete", true, calculator.IsThisValidationOn(header.ValidationModes, ValidationModes.BillOfLadingDelete));
		}

		public void TestConvertMessageType()
		{
			var header = Factory.New<CusInBondHeader>();
			var calculator = header.ValidationModesCalculator;
			AssertEquals(InBondMessageType.InBondLevelArrival, calculator.ConvertMessageType(InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival));
			AssertEquals(InBondMessageType.InBondLevelExportation, calculator.ConvertMessageType(InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export));
			AssertEquals(InBondMessageType.InBondLevelExportation, calculator.ConvertMessageType(InBondMenuItemMessageData.InBondMenuItemMessageTypes.Pedimento));

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			AssertEquals(InBondMessageType.AirEntireInBondArrival, calculator.ConvertMessageType(InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival));
			AssertEquals(InBondMessageType.AirEntireInBondExportation, calculator.ConvertMessageType(InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export));
			AssertEquals(InBondMessageType.AirEntireInBondExportation, calculator.ConvertMessageType(InBondMenuItemMessageData.InBondMenuItemMessageTypes.Pedimento));
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
		}
	}
}
