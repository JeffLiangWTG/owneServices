using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateJC_RH_NKContainerCommodityCode()
		{
			var code1 = Factory.New<RefCommodityCode>();
			code1.RH_Code = "AAA";
			code1.RH_IsShipping = true;
			var code2 = Factory.New<RefCommodityCode>();
			code2.RH_Code = "BBB";
			code2.RH_IsShipping = false;
			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.RealContainers.AddNew();
			container.JC_RH_NKContainerCommodityCode = code1.RH_Code;
			AssertNoWarnings(container.JC_RH_NKContainerCommodityCodeInfo);
			container.JC_RH_NKContainerCommodityCode = code2.RH_Code;
			AssertHasError(container.JC_RH_NKContainerCommodityCodeInfo, "Enter a valid Commodity.");
			container.JC_RH_NKContainerCommodityCode = "ZZZ";
			AssertHasError(container.JC_RH_NKContainerCommodityCodeInfo, "Enter a valid Commodity.");
		}

		public void TestJC_ContainerNumDuplicated()
		{
			const string error = "Duplicate Container Number is entered.";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.BookedContainers.AddNew().JC_ContainerNum = "FAKE4100011";
			shipment.RealContainers.AddNew().JC_ContainerNum = "FAKE4100027";
			shipment.RealContainers.AddNew().JC_ContainerNum = "";
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			AssertNoError(container.JC_ContainerNumInfo, error);
			container.JC_ContainerNum = "FAKE4100027";
			AssertHasError(container.JC_ContainerNumInfo, error);
			container.JC_ContainerNum = "";
			AssertNoError(container.JC_ContainerNumInfo, error);
		}

		public void TestJC_EIDOStatus()
		{
			const string error = "The last message sent for this container was rejected.";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			EDIMessage message = container.Messages.AddNew();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = EIDOMessageTypes.Codes.Original;
			container.Validation.ValidateAll();
			AssertEquals("precondition:", ReleaseImportOrderMessageStatusList.Codes.OriginalSent, container.JC_ImportReleaseOrderStatus);
			AssertNoNotifications(container.JC_ImportReleaseOrderStatusInfo);
			message.EM_Status = EDIMessage.Status.Rejected;
			container.Validation.ValidateAll();
			AssertEquals("precondition:", ReleaseImportOrderMessageStatusList.Codes.Rejected, container.JC_ImportReleaseOrderStatus);
			AssertHasWarning(container.JC_ImportReleaseOrderStatusInfo, error);
			message.EM_Status = EDIMessage.Status.Acknowledged;
			container.Validation.ValidateAll();
			AssertEquals("precondition:", ReleaseImportOrderMessageStatusList.Codes.Acknowledged, container.JC_ImportReleaseOrderStatus);
			AssertNoNotifications(container.JC_ImportReleaseOrderStatusInfo);
			message.EM_Status = EDIMessage.Status.Received;
			container.Validation.ValidateAll();
			AssertEquals("precondition:", ReleaseImportOrderMessageStatusList.Codes.Accepted, container.JC_ImportReleaseOrderStatus);
			AssertNoNotifications(container.JC_ImportReleaseOrderStatusInfo);
		}

		public void TestJC_SetPointTemp()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_IsNonOperativeReefer = false;
			container.JC_RC = RC_20GP_PK;
			container.JC_SetPointTemp = 0;
			container.Validation.ValidateJC_SetPointTemp();
			AssertNoWarnings("should not have any warnings", container.JC_SetPointTempInfo);
			container.JC_RC = RC_20RE_PK;
			container.Validation.ValidateJC_SetPointTemp();
			AssertHasWarnings("should have a warning", container.JC_SetPointTempInfo);
			container.JC_SetPointTemp = -5;
			AssertNoWarnings("should not have a warning anymore", container.JC_SetPointTempInfo);
			container.JC_RC = RC_20RE_PK;
			container.JC_IsNonOperativeReefer = true;
			container.JC_SetPointTemp = 0;
			container.Validation.ValidateJC_SetPointTemp();
			AssertNoWarnings("should not have any warnings", container.JC_SetPointTempInfo);
		}

		public void TestJC_HumidityPercent()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_IsNonOperativeReefer = false;
			container.JC_RC = RC_20GP_PK;
			container.JC_HumidityPercent = 0;
			container.Validation.ValidateJC_HumidityPercent();
			AssertNoWarnings("should not have any warnings", container.JC_HumidityPercentInfo);
			container.JC_RC = RC_20RE_PK;
			container.Validation.ValidateJC_HumidityPercent();
			AssertHasWarnings("should have a warning", container.JC_HumidityPercentInfo);
			container.JC_HumidityPercent = 20;
			AssertNoWarnings("should not have a warning anymore", container.JC_HumidityPercentInfo);
			container.JC_RC = RC_20RE_PK;
			container.JC_HumidityPercent = 0;
			container.JC_IsNonOperativeReefer = true;
			container.Validation.ValidateJC_HumidityPercent();
			AssertNoWarnings("should not have any warnings", container.JC_HumidityPercentInfo);
		}

		public void TestJC_AirVentFlow()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_IsNonOperativeReefer = false;
			container.JC_RC = RC_20GP_PK;
			container.JC_AirVentFlow = 0;
			container.Validation.ValidateJC_AirVentFlow();
			AssertNoWarnings("should not have any warnings", container.JC_AirVentFlowInfo);
			container.JC_RC = RC_20RE_PK;
			container.Validation.ValidateJC_AirVentFlow();
			AssertHasWarnings("should have a warning", container.JC_AirVentFlowInfo);
			container.JC_AirVentFlow = 20;
			AssertNoWarnings("should not have a warning anymore", container.JC_AirVentFlowInfo);
			container.JC_RC = RC_20RE_PK;
			container.JC_AirVentFlow = 0;
			container.JC_IsNonOperativeReefer = true;
			container.Validation.ValidateJC_AirVentFlow();
			AssertNoWarnings("should not have any warnings", container.JC_AirVentFlowInfo);
		}

		public void TestJC_IsEmptyContainer()
		{
			const string error = "This container has pack lines packed into it and yet is marked as empty";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AgencyShipmentPackLine packLine1 = shipment.OuterPackLines.AddNew();
			AgencyShipmentPackLine packLine2 = shipment.OuterPackLines.AddNew();
			AgencyShipmentContainer realContainer = shipment.RealContainers.AddNew();
			realContainer.JC_ContainerNum = "FAKE4100027";
			AgencyShipmentContainer bookedContainer = shipment.BookedContainers.AddNew();
			bookedContainer.JC_ContainerNum = "FAKE4100011";
			realContainer.JC_IsEmptyContainer = false;
			bookedContainer.JC_IsEmptyContainer = false;
			AssertNoError("RealContainer should has no errors", realContainer.JC_IsEmptyContainerInfo, error);
			AssertNoError("BookedContainer should has no errors", bookedContainer.JC_IsEmptyContainerInfo, error);
			realContainer.JC_IsEmptyContainer = true;
			bookedContainer.JC_IsEmptyContainer = true;
			AssertNoError("RealContainer should has no errors", realContainer.JC_IsEmptyContainerInfo, error);
			AssertNoError("BookedContainer should has no errors", bookedContainer.JC_IsEmptyContainerInfo, error);
			packLine1.JL_JC = realContainer.PK;
			realContainer.JC_IsEmptyContainer = false;
			packLine2.JL_JC = bookedContainer.PK;
			bookedContainer.JC_IsEmptyContainer = false;
			AssertNoError("RealContainer should has no errors", realContainer.JC_IsEmptyContainerInfo, error);
			AssertNoError("BookedContainer should has no errors", bookedContainer.JC_IsEmptyContainerInfo, error);
			realContainer.JC_IsEmptyContainer = true;
			AssertHasError("RealContainer should has error", realContainer.JC_IsEmptyContainerInfo, error);
			AssertNoError("BookedContainer should has no errors", bookedContainer.JC_IsEmptyContainerInfo, error);
			bookedContainer.JC_IsEmptyContainer = true;
			AssertHasError("RealContainer should has error", realContainer.JC_IsEmptyContainerInfo, error);
			AssertHasError("BookedContainer should has error", bookedContainer.JC_IsEmptyContainerInfo, error);
			realContainer.JC_IsEmptyContainer = false;
			AssertNoError("RealContainer should has no errors", realContainer.JC_IsEmptyContainerInfo, error);
			AssertHasError("BookedContainer should has error", bookedContainer.JC_IsEmptyContainerInfo, error);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			realContainer = shipment.RealContainers.AddNew();
			realContainer.JC_ContainerNum = "FAKE4100027";
			bookedContainer = shipment.BookedContainers.AddNew();
			bookedContainer.JC_ContainerNum = "FAKE4100011";
			bookedContainer.JC_IsEmptyContainer = true;
			realContainer.JC_IsEmptyContainer = true;
			AssertNoError("RealContainer should has no errors", realContainer.JC_IsEmptyContainerInfo, error);
			AssertNoError("BookedContainer should has no errors", bookedContainer.JC_IsEmptyContainerInfo, error);
			bookedContainer.JC_IsEmptyContainer = false;
			realContainer.JC_IsEmptyContainer = false;
			AssertNoError("RealContainer should has no errors", realContainer.JC_IsEmptyContainerInfo, error);
			AssertNoError("BookedContainer should has no errors", bookedContainer.JC_IsEmptyContainerInfo, error);
		}

		#region TestValidateJC_SetPointTemp
		public void TestValidateJC_SetPointTemp()
		{
			AgencyShipmentContainer container = Factory.New<AgencyShipmentContainer>();
			container.JC_RC = RC_20GP_PK;
			container.Validation.ValidateJC_SetPointTemp();
			AssertNoNotifications(container.JC_SetPointTempInfo);
			container.JC_RC = RC_20RE_PK;
			container.Validation.ValidateJC_SetPointTemp();
			AssertHasWarnings(container.JC_SetPointTempInfo);
			AssertNoErrors(container.JC_SetPointTempInfo);
			container.JC_SetPointTemp = 5;
			AssertNoNotifications(container.JC_SetPointTempInfo);
		}

		protected ZGuid RC_20GP_PK
		{
			get
			{
				return Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			}
		}

		protected ZGuid RC_20RE_PK
		{
			get
			{
				return Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			}
		}

		#endregion
		public void TestJC_ContainerMode()
		{
			var expected = "Please enter a Container Mode.";
			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.BookedContainers.AddNew();
			var codes = typeof(Constants.ContainerModes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => (string)field.GetValue(null));
			CombineAssertions(delegate
			{
				var assert = false;
				foreach (var code in codes)
				{
					container.JC_ContainerMode = code;
					if (container.Validation.GetType() != typeof(AgencyShipmentContainerValidation))
					{
						continue;
					}

					if (container.JC_ContainerMode_List.ContainsCode(code))
					{
						AssertNoErrors(string.Format("No Errors - {0}", code), container.JC_ContainerModeInfo);
					}
					else
					{
						AssertHasError(string.Format("Has Error - {0}", code), container.JC_ContainerModeInfo, expected);
					}

					assert = true;
				}

				Assert(assert);
			});
		}

		public void TestJC_ContainerNum()
		{
			var expected = "Container number FAKE4100011 is flagged as FCL, but it is also registered against the following shipment(s) on the same vessel-voyage:" + System.Environment.NewLine + "Bill Of Lading BOL_002, Container Mode LCL" + System.Environment.NewLine + "Bill Of Lading BOL_003, Container Mode GRP" + System.Environment.NewLine + "Bill Of Lading BOL_004, Container Mode FCL" + System.Environment.NewLine + "Bill Of Lading BOL_005, Container Mode BCN" + System.Environment.NewLine + "Bill Of Lading BOL_006, Container Mode LCL" + System.Environment.NewLine + "Bill Of Lading BOL_007, Container Mode GRP" + System.Environment.NewLine + "Bill Of Lading BOL_008, Container Mode FCL" + System.Environment.NewLine + "Bill Of Lading BOL_009, Container Mode BCN" + System.Environment.NewLine + "Booking BKD_001, Container Mode BCN" + System.Environment.NewLine + "Booking BKD_002, Container Mode FCL" + System.Environment.NewLine + "Booking BKD_003, Container Mode GRP" + System.Environment.NewLine + "Booking BKD_004, Container Mode LCL" + System.Environment.NewLine + "Booking BKD_005, Container Mode BCN" + System.Environment.NewLine + "Booking BKD_006, Container Mode FCL" + System.Environment.NewLine + "Booking BKD_007, Container Mode GRP" + System.Environment.NewLine + "Booking BKD_008, Container Mode LCL" + System.Environment.NewLine + "Booking BKD_009, Container Mode BCN" + System.Environment.NewLine + "Booking BKD_010, Container Mode FCL" + System.Environment.NewLine + "Booking BKD_011, Container Mode GRP" + System.Environment.NewLine + "Booking BKD_012, Container Mode LCL" + System.Environment.NewLine;
			var expectedBOL = expected + "The Container Mode on this Bill Of Lading V00001001 should not be FCL.";
			var expectedBKD = expected + "The Container Mode on this Booking V00001002 should not be FCL.";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = Factory.NewWithValidTestData<RefVessel>().RV_FK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUADL";
			var sailings = voyage.Sailings;
			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = Factory.NewWithValidTestData<RefVessel>().RV_FK;
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
			SetNewContainer("BOL_009", sailings[0], "FAKE4100011", ShipmentStatusList.Codes.Confirmed, Constants.ContainerModes.BuyersConsol);
			SetNewContainer("BOL_008", sailings[1], "FAKE4100011", ShipmentStatusList.Codes.Confirmed, Constants.ContainerModes.FCL);
			SetNewContainer("BOL_007", sailings[2], "FAKE4100011", ShipmentStatusList.Codes.Confirmed, Constants.ContainerModes.Groupage);
			SetNewContainer("BOL_006", sailings[3], "FAKE4100011", ShipmentStatusList.Codes.Confirmed, Constants.ContainerModes.LCL);
			SetNewContainer("BOL_005", sailings[4], "FAKE4100011", ShipmentStatusList.Codes.WebFwdInstruction, Constants.ContainerModes.BuyersConsol);
			SetNewContainer("BOL_004", sailings[5], "FAKE4100011", ShipmentStatusList.Codes.WebFwdInstruction, Constants.ContainerModes.FCL);
			SetNewContainer("BOL_003", sailings[6], "FAKE4100011", ShipmentStatusList.Codes.WebFwdInstruction, Constants.ContainerModes.Groupage);
			SetNewContainer("BOL_002", sailings[0], "FAKE4100011", ShipmentStatusList.Codes.WebFwdInstruction, Constants.ContainerModes.LCL);
			SetNewContainer("BOL_001", voyage2.Sailings[0], "FAKE4100011", ShipmentStatusList.Codes.Confirmed, Constants.ContainerModes.FCL);
			SetNewContainer("BKD_001", sailings[1], "FAKE4100011", ShipmentStatusList.Codes.Booked, Constants.ContainerModes.BuyersConsol);
			SetNewContainer("BKD_002", sailings[2], "FAKE4100011", ShipmentStatusList.Codes.Booked, Constants.ContainerModes.FCL);
			SetNewContainer("BKD_003", sailings[3], "FAKE4100011", ShipmentStatusList.Codes.Booked, Constants.ContainerModes.Groupage);
			SetNewContainer("BKD_004", sailings[4], "FAKE4100011", ShipmentStatusList.Codes.Booked, Constants.ContainerModes.LCL);
			SetNewContainer("BKD_005", sailings[5], "FAKE4100011", ShipmentStatusList.Codes.WebBooking, Constants.ContainerModes.BuyersConsol);
			SetNewContainer("BKD_006", sailings[6], "FAKE4100011", ShipmentStatusList.Codes.WebBooking, Constants.ContainerModes.FCL);
			SetNewContainer("BKD_007", sailings[0], "FAKE4100011", ShipmentStatusList.Codes.WebBooking, Constants.ContainerModes.Groupage);
			SetNewContainer("BKD_008", sailings[1], "FAKE4100011", ShipmentStatusList.Codes.WebBooking, Constants.ContainerModes.LCL);
			SetNewContainer("BKD_009", sailings[2], "FAKE4100011", ShipmentStatusList.Codes.WaitListed, Constants.ContainerModes.BuyersConsol);
			SetNewContainer("BKD_010", sailings[3], "FAKE4100011", ShipmentStatusList.Codes.WaitListed, Constants.ContainerModes.FCL);
			SetNewContainer("BKD_011", sailings[4], "FAKE4100011", ShipmentStatusList.Codes.WaitListed, Constants.ContainerModes.Groupage);
			SetNewContainer("BKD_012", sailings[5], "FAKE4100011", ShipmentStatusList.Codes.WaitListed, Constants.ContainerModes.LCL);
			SetNewContainer("BKD_013", voyage2.Sailings[0], "FAKE4100011", ShipmentStatusList.Codes.Booked, Constants.ContainerModes.FCL);
			Factory.Save();
			var bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00001001";
			bill.JS_JX = sailings[2].PK;
			var billContainer = bill.RealContainers.AddNew();
			AssertEquals(Constants.ContainerModes.FCL, billContainer.JC_ContainerMode);
			billContainer.JC_ContainerNum = "FAKE4100011";
			AssertHasWarning(billContainer.JC_ContainerNumInfo, expectedBOL);
			billContainer.JC_ContainerNum = "bob";
			AssertNoWarning(billContainer.JC_ContainerNumInfo, expectedBOL);
			billContainer.JC_ContainerNum = "FAKE4100011";
			AssertHasWarning(billContainer.JC_ContainerNumInfo, expectedBOL);
			billContainer.JC_ContainerMode = Constants.ContainerModes.Groupage;
			AssertNoWarning(billContainer.JC_ContainerNumInfo, expectedBOL);
			var booking = Factory.New<AgencyBooking>();
			booking.JS_UniqueConsignRef = "V00001002";
			booking.JS_JX = sailings[2].PK;
			var bookingContainer = booking.BookedContainers.AddNew();
			AssertEquals(Constants.ContainerModes.FCL, bookingContainer.JC_ContainerMode);
			bookingContainer.JC_ContainerNum = "FAKE4100011";
			AssertHasWarning(bookingContainer.JC_ContainerNumInfo, expectedBKD);
			bookingContainer.JC_ContainerNum = "bob";
			AssertNoWarning(bookingContainer.JC_ContainerNumInfo, expectedBKD);
			bookingContainer.JC_ContainerNum = "FAKE4100011";
			AssertHasWarning(bookingContainer.JC_ContainerNumInfo, expectedBKD);
			bookingContainer.JC_ContainerMode = Constants.ContainerModes.BuyersConsol;
			AssertNoWarning(bookingContainer.JC_ContainerNumInfo, expectedBKD);
		}

		public void TestJC_ContainerNum_Mandatory()
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			var realContainer = booking.RealContainers.AddNew();
			var bookingContainer = booking.BookedContainers.AddNew();
			AssertEquals("Pre-condition: expected container to have defaulted a valid container code", Constants.ContainerModes.FCL, realContainer.JC_ContainerMode);
			AssertEquals("Pre-condition: expected container to have defaulted a valid container code", Constants.ContainerModes.FCL, bookingContainer.JC_ContainerMode);
			AssertNoErrors("Pre-condition", realContainer.JC_ContainerModeInfo);
			AssertNoErrors("Pre-condition", bookingContainer.JC_ContainerModeInfo);
			realContainer.JC_ContainerMode = ZString.Empty;
			bookingContainer.JC_ContainerMode = ZString.Empty;
			AssertHasErrors("Expected an error as container mode is mandatory", bookingContainer.JC_ContainerModeInfo);
			AssertHasErrors("Expected an error as container mode is mandatory", bookingContainer.JC_ContainerModeInfo);
			realContainer.JC_ContainerMode = Constants.ContainerModes.LCL;
			bookingContainer.JC_ContainerMode = Constants.ContainerModes.FCL;
			AssertNoErrors("Expected no error as a valid container mode has been entered", realContainer.JC_ContainerModeInfo);
			AssertNoErrors("Expected no error as a valid container mode has been entered", bookingContainer.JC_ContainerModeInfo);
		}

		#region JC_HarmonisedCode
		public void TestCheckJC_HarmonisedCode()
		{
			var errorMessage = "Invalid Harmonized Code. Only numeric characters and dots are allowed.";
			var container1 = Factory.New<AgencyShipmentContainer>();
			container1.JC_HarmonisedCode = "123ABC";
			AssertHasError(container1.JC_HarmonisedCodeInfo, errorMessage);
			container1.JC_HarmonisedCode = "123A$#";
			AssertHasError(container1.JC_HarmonisedCodeInfo, errorMessage);
			container1.JC_HarmonisedCode = "......";
			AssertHasError(container1.JC_HarmonisedCodeInfo, errorMessage);
			container1.JC_HarmonisedCode = ".123";
			AssertHasError(container1.JC_HarmonisedCodeInfo, errorMessage);
			container1.JC_HarmonisedCode = string.Empty;
			AssertNoErrors(container1.JC_HarmonisedCodeInfo);
			container1.JC_HarmonisedCode = "13.3.1";
			AssertNoErrors(container1.JC_HarmonisedCodeInfo);
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			var container2 = booking.BookedContainers.AddNew();
			using (container2.SuspendValidationTesting())
			{
				container2.JC_HarmonisedCode = "123ABC";
				Factory.Save();
			}

			container2.Validation.ValidateJC_HarmonisedCode();
			AssertHasWarning(container2.JC_HarmonisedCodeInfo, errorMessage);
		}

		#endregion
		void SetNewContainer(ZString shipmentNum, JobSailing sailing, ZString containerNum, ZString shipmentStatus, ZString containerMode)
		{
			var shipment = ShipmentStatusHelperMethods.IsBillOfLadingStage(shipmentStatus) ? Factory.New<BillOfLading>() : (AgencyShipment)Factory.New<AgencyBooking>();
			shipment.JS_ShipmentStatus = shipmentStatus;
			shipment.JS_UniqueConsignRef = shipmentNum;
			shipment.JS_JX = sailing.PK;
			var container = ShipmentStatusHelperMethods.IsBillOfLadingStage(shipmentStatus) ? shipment.RealContainers.AddNew() : shipment.BookedContainers.AddNew();
			container.JC_ContainerMode = containerMode;
			container.JC_ContainerNum = containerNum;
		}

		public void TestJC_StowagePosition()
		{
			var bill = Factory.New<BillOfLading>();
			BillOfLadingContainer container;
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				bill.JS_PackingMode = mode;
				container = bill.RealContainers.AddNew();
				container.JC_ContainerMode = mode;
				container.JC_StowagePosition = "";
				container.JC_StowagePosition = "ABC12";
				AssertNoWarnings(container.JC_StowagePositionInfo);
			}

			container = bill.RealContainers.AddNew();
			container.JC_ContainerMode = Constants.ContainerModes.FCL;
			ZString warning = "Stowage Position format should be BBBRRTT where BBB is Bay, RR is Row and TT is Tier. All values should be numeric.";
			container.JC_StowagePosition = "1234";
			AssertHasWarning(container.JC_StowagePositionInfo, warning);
			container.JC_StowagePosition = "12345678";
			AssertHasWarning(container.JC_StowagePositionInfo, warning);
			container.JC_StowagePosition = "A123456";
			AssertHasWarning(container.JC_StowagePositionInfo, warning);
			container.JC_StowagePosition = "0123456";
			AssertNoWarnings(container.JC_StowagePositionInfo);
		}
	}
}
