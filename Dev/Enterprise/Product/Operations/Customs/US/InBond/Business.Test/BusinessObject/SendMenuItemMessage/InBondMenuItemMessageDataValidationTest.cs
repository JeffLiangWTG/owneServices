using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class InBondMenuItemMessageDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUSDestinationPortCode()
		{
			var inBondMenuItemMessageSendingObject = inBondMenuItemMessageData_Arrival.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject.InBondNumber = "111111114";
			inBondMenuItemMessageData_Arrival.Validation.ValidateUSDestinationPortCode();
			AssertHasMessageErrorContaining(inBondMenuItemMessageData_Arrival.USDestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageSendingObject.USDestinationPortCode = "1111";
			inBondMenuItemMessageData_Arrival.Validation.ValidateUSDestinationPortCode();
			AssertNoMessageErrorContaining(inBondMenuItemMessageData_Arrival.USDestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Arrival.USDestinationPortCode = "1111";
			inBondMenuItemMessageSendingObject.USDestinationPortCode = "";
			inBondMenuItemMessageData_Arrival.Validation.ValidateUSDestinationPortCode();
			AssertNoMessageErrorContaining(inBondMenuItemMessageData_Arrival.USDestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var validCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "Test Name", startDate, endDate);
			newFactory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(inBondMenuItemMessageData_Arrival.USDestinationPortCodeInfo, "????", validCode.ZZD_Code);
		}

		public void TestCheckArrivalDate()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			var movementHeader = inBondHeader.MovementHeaders.AddNew();
			movementHeader.InBondNumber = "111111114";
			movementHeader.BM_ArrivalDate = ZDateTime.Today;
			Factory.Save();
			var inBondMenuItemMessageSendingObject = inBondMenuItemMessageData_Arrival.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageData_Arrival.Validation.ValidateArrivalDate();
			AssertHasMessageErrorContaining(inBondMenuItemMessageData_Arrival.ArrivalDateInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Arrival.ArrivalDate = ZDateTime.Today;
			inBondMenuItemMessageData_Arrival.Validation.ValidateArrivalDate();
			AssertNoMessageErrorContaining(inBondMenuItemMessageData_Arrival.ArrivalDateInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Arrival.ArrivalDate = ZDateTime.Empty;
			inBondMenuItemMessageSendingObject.InBondNumber = "111111114";
			inBondMenuItemMessageData_Arrival.Validation.ValidateArrivalDate();
			AssertNoMessageErrorContaining(inBondMenuItemMessageData_Arrival.ArrivalDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckFIRMSCode()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			var movementHeader = inBondHeader.MovementHeaders.AddNew();
			movementHeader.InBondNumber = "111111114";
			movementHeader.BM_FIRMS = "1111";
			Factory.Save();
			var inBondMenuItemMessageSendingObject = inBondMenuItemMessageData_Arrival.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageData_Arrival.Validation.ValidateFIRMSCode();
			AssertHasMessageErrorContaining(inBondMenuItemMessageData_Arrival.FIRMSCodeInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Arrival.FIRMSCode = "1111";
			inBondMenuItemMessageData_Arrival.Validation.ValidateFIRMSCode();
			AssertNoMessageErrorContaining(inBondMenuItemMessageData_Arrival.FIRMSCodeInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Arrival.FIRMSCode = "";
			inBondMenuItemMessageSendingObject.InBondNumber = "111111114";
			inBondMenuItemMessageData_Arrival.Validation.ValidateFIRMSCode();
			AssertNoMessageErrorContaining(inBondMenuItemMessageData_Arrival.FIRMSCodeInfo, MandatoryValidation.YouHaveNotEntered);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "HXU~", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(inBondMenuItemMessageData_Arrival.FIRMSCodeInfo, "????", "HXU~");
			var inbondHeader = Factory.New<CusInBondHeader>();
			inbondHeader.BH_ImportTransportMode = US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			var movementHeader1 = inbondHeader.MovementHeaders.AddNew();
			inBondMenuItemMessageData_Arrival = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			AssertNoMessageErrorContaining(inBondMenuItemMessageData_Arrival.FIRMSCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckExportDate()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			var movementHeader = inBondHeader.MovementHeaders.AddNew();
			movementHeader.InBondNumber = "111111114";
			movementHeader.BM_ExportDate = ZDateTime.Today;
			Factory.Save();
			var inBondMenuItemMessageData_Export = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			var inBondMenuItemMessageSendingObject = inBondMenuItemMessageData_Export.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageData_Export.Validation.ValidateExportDate();
			AssertHasMessageErrorContaining(inBondMenuItemMessageData_Export.ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.ExportDate = ZDateTime.Today;
			inBondMenuItemMessageData_Export.Validation.ValidateExportDate();
			AssertNoMessageErrorContaining(inBondMenuItemMessageData_Export.ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.ExportDate = ZDateTime.Empty;
			inBondMenuItemMessageSendingObject.InBondNumber = "111111114";
			inBondMenuItemMessageData_Export.Validation.ValidateExportDate();
			AssertNoMessageErrorContaining(inBondMenuItemMessageData_Export.ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckExportMOT()
		{
			var inBondMenuItemMessageData_Export = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			inBondMenuItemMessageData_Export.ExportConveyance = "A";
			inBondMenuItemMessageData_Export.Validation.ValidateExportMOT();
			AssertNoMessageErrorContaining("There is no MessageSendingBO in MessageData", inBondMenuItemMessageData_Export.ExportMOTInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.ExportMOT = "A";
			AssertHasMessageErrorContaining(inBondMenuItemMessageData_Export.ExportMOTInfo, ListValidation.InvalidCodeMessageError);
			inBondMenuItemMessageData_Export.ExportMOT = US.Business.InBondTransportModeCodes.Codes.VesselNonContainer;
			AssertNoMessageErrorContaining(inBondMenuItemMessageData_Export.ExportMOTInfo, ListValidation.InvalidCodeMessageError);
			inBondMenuItemMessageData_Export.ExportMOT = US.Business.InBondTransportModeCodes.Codes.VesselContainer;
			AssertNoMessageErrorContaining(inBondMenuItemMessageData_Export.ExportMOTInfo, ListValidation.InvalidCodeMessageError);
			var inbondHeader = Factory.New<CusInBondHeader>();
			var movementHeader1 = inbondHeader.MovementHeaders.AddNew();
			movementHeader1.BM_ExportTransportMode = "A";
			movementHeader1.BM_ExportLadenOn = "A";
			inBondMenuItemMessageData_Export = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			inBondMenuItemMessageData_Export.Validation.ValidateExportMOT();
			AssertNoMessageErrorContaining("Both ExportTransportMode and ExportLadenOn of MovementHeader1 are not empty.", inBondMenuItemMessageData_Export.ExportMOTInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageData_Export.Validation.ValidateExportMOT();
			AssertNoMessageErrorContaining("Both ExportTransportMode and ExportLadenOn of MovementHeader1 are not empty, and the both of the new MovementHeader are empty.", inBondMenuItemMessageData_Export.ExportMOTInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.ExportConveyance = "A";
			inBondMenuItemMessageData_Export.Validation.ValidateExportMOT();
			AssertHasMessageErrorContaining("The ExportLadenOn of the new MovementHeader is not empty.", inBondMenuItemMessageData_Export.ExportMOTInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.ExportMOT = "A";
			inBondMenuItemMessageData_Export.Validation.ValidateExportMOT();
			AssertNoMessageErrorContaining("All ExportTransportMode and ExportLadenOn are not empty.", inBondMenuItemMessageData_Export.ExportMOTInfo, MandatoryValidation.YouHaveNotEntered);
			var movementHeader2 = inbondHeader.MovementHeaders.AddNew();
			movementHeader2.BM_ExportLadenOn = "A";
			inBondMenuItemMessageData_Export = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			inBondMenuItemMessageData_Export.Validation.ValidateExportMOT();
			AssertHasMessageErrorContaining("The ExportLadenOn of the MovementHeader2 is not empty.", inBondMenuItemMessageData_Export.ExportMOTInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageData_Export.Validation.ValidateExportMOT();
			AssertHasMessageErrorContaining("The ExportLadenOn of the MovementHeader2 is not empty.", inBondMenuItemMessageData_Export.ExportMOTInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.ExportMOT = "A";
			inBondMenuItemMessageData_Export.Validation.ValidateExportMOT();
			AssertNoMessageErrorContaining("All ExportTransportMode and ExportLadenOn are not empty.", inBondMenuItemMessageData_Export.ExportMOTInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckExportConveyance()
		{
			var inBondMenuItemMessageData_Export = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			inBondMenuItemMessageData_Export.ExportMOT = "A";
			inBondMenuItemMessageData_Export.Validation.ValidateExportConveyance();
			AssertNoMessageErrorContaining("There is no MessageSendingBO in MessageData", inBondMenuItemMessageData_Export.ExportConveyanceInfo, MandatoryValidation.YouHaveNotEntered);
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "1234";
			ValidationTestHelper.AssertInvalidCodeMessageError(inBondMenuItemMessageData_Export.ExportConveyanceInfo, "????", "1234");
			var inbondHeader = Factory.New<CusInBondHeader>();
			var movementHeader1 = inbondHeader.MovementHeaders.AddNew();
			movementHeader1.BM_ExportTransportMode = "A";
			movementHeader1.BM_ExportLadenOn = "A";
			inBondMenuItemMessageData_Export = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			inBondMenuItemMessageData_Export.Validation.ValidateExportConveyance();
			AssertNoMessageErrorContaining("Both ExportTransportMode and ExportLadenOn of MovementHeader1 are not empty.", inBondMenuItemMessageData_Export.ExportConveyanceInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageData_Export.Validation.ValidateExportConveyance();
			AssertNoMessageErrorContaining("Both ExportTransportMode and ExportLadenOn of MovementHeader1 are not empty, and the both of the new MovementHeader are empty.", inBondMenuItemMessageData_Export.ExportConveyanceInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.ExportMOT = "A";
			inBondMenuItemMessageData_Export.Validation.ValidateExportConveyance();
			AssertHasMessageErrorContaining("The ExportTransportMode of the new MovementHeader is not empty.", inBondMenuItemMessageData_Export.ExportConveyanceInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.ExportConveyance = "A";
			inBondMenuItemMessageData_Export.Validation.ValidateExportConveyance();
			AssertNoMessageErrorContaining("All ExportTransportMode and ExportLadenOn are not empty.", inBondMenuItemMessageData_Export.ExportConveyanceInfo, MandatoryValidation.YouHaveNotEntered);
			var movementHeader2 = inbondHeader.MovementHeaders.AddNew();
			movementHeader2.BM_ExportTransportMode = "A";
			inBondMenuItemMessageData_Export = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			inBondMenuItemMessageData_Export.Validation.ValidateExportConveyance();
			AssertHasMessageErrorContaining("The ExportTransportMode of the MovementHeader2 is not empty.", inBondMenuItemMessageData_Export.ExportConveyanceInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageData_Export.Validation.ValidateExportConveyance();
			AssertHasMessageErrorContaining("The ExportTransportMode of the MovementHeader2 is not empty.", inBondMenuItemMessageData_Export.ExportConveyanceInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMenuItemMessageData_Export.ExportConveyance = "A";
			inBondMenuItemMessageData_Export.Validation.ValidateExportConveyance();
			AssertNoMessageErrorContaining("All ExportTransportMode and ExportLadenOn are not empty.", inBondMenuItemMessageData_Export.ExportConveyanceInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			inBondMenuItemMessageData_Arrival = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
		}

		InBondMenuItemMessageData inBondMenuItemMessageData_Arrival;
	}
}
