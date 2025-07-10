using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public class DeliveryDueDateWithExceptionsCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2023, 9, 27, 9, 0, 0)]
		public void TestCalculateDeliveryDueDateWithExceptions_MaxDuration()
		{
			AssertCalculateDeliveryDueDateWithExceptions(TimeSpan.FromHours(24),
				new ZDateTime(2023, 10, 4, 9, 0, 0),
				@"Calculating Delivery Due Date With Exceptions...
Latest Added/Changed Exception [Type: Exception-Future Event|Description: Delay7|Time: 27-Sep-23 09:00:00|Duration: 12 Hours]
Original Delivery Due Date: 28-Sep-23 11:00:00
Applying exception Delay1: The next available business day for Revised Delivery Due Date was: 29-Sep-23 07:00:00 which is Friday.
Applying exception Delay3: Weekend Days/Public Holidays of [21/8 Camillo St. WA Cannington 6155 AUCNN]: Saturday 30-Sep-23; Sunday 01-Oct-23;  The next available business day for Revised Delivery Due Date was: 02-Oct-23 02:00:00 which is Monday.
Applying exception Delay4: The next available business day for Revised Delivery Due Date was: 03-Oct-23 02:00:00 which is Tuesday.
Applying exception Delay7: The next available business day for Revised Delivery Due Date was: 04-Oct-23 02:00:00 which is Wednesday.
Finding closest opening hour: 04-Oct-23 02:00:00 adjusted to 04-Oct-23 09:00:00
Calculated revised delivery due date by applying exception delays and finding closest opening hours: 04-Oct-23 09:00:00
");
		}

		[TestDate(2023, 9, 27, 9, 0, 0)]
		public void TestCalculateDeliveryDueDateWithExceptions_UnlimitedDuration()
		{
			AssertCalculateDeliveryDueDateWithExceptions(TimeSpan.FromHours(0),
				new ZDateTime(2023, 10, 10, 9, 0, 0),
				@"Calculating Delivery Due Date With Exceptions...
Latest Added/Changed Exception [Type: Exception-Future Event|Description: Delay7|Time: 27-Sep-23 09:00:00|Duration: 12 Hours]
Original Delivery Due Date: 28-Sep-23 11:00:00
Applying exception Delay1: The next available business day for Revised Delivery Due Date was: 29-Sep-23 07:00:00 which is Friday.
Applying exception Delay3: Weekend Days/Public Holidays of [21/8 Camillo St. WA Cannington 6155 AUCNN]: Saturday 30-Sep-23; Sunday 01-Oct-23;  The next available business day for Revised Delivery Due Date was: 02-Oct-23 08:00:00 which is Monday.
Applying exception Delay4: The next available business day for Revised Delivery Due Date was: 06-Oct-23 12:00:00 which is Friday.
Applying exception Delay6: Weekend Days/Public Holidays of [21/8 Camillo St. WA Cannington 6155 AUCNN]: Saturday 07-Oct-23; Sunday 08-Oct-23;  The next available business day for Revised Delivery Due Date was: 09-Oct-23 09:00:00 which is Monday.
Finding closest opening hour: 09-Oct-23 21:00:00 adjusted to 10-Oct-23 09:00:00
Calculated revised delivery due date by applying exception delays and finding closest opening hours: 10-Oct-23 09:00:00
");
		}

		[TestDate(2023, 9, 27, 9, 0, 0)]
		public void TestCalculateDeliveryDueDateWithExceptions_MaxDuration_MondayIsPublicHoliday()
		{
			var mondayPublicHoliday = new ZDateTime(2023, 10, 02, 0, 0, 0);
			var state = Factory.LoadTop1<RefCountryStates>(new ZQuery(new ZQuery(RefCountryStatesSchema.RW_Code, "WA"),
				new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, Core.Constants.CountryCodes.Australia)));
			var glbHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			glbHoliday.GH_Date = mondayPublicHoliday;
			glbHoliday.GH_IsWorkingDay = false;
			glbHoliday.GH_Recurring = true;
			glbHoliday.GH_ParentID = state.Country.PK;
			glbHoliday.GH_ParentTableCode = "RN";
			glbHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			glbHoliday.GH_IsActive = true;
			Factory.Save();
			AssertCalculateDeliveryDueDateWithExceptions(TimeSpan.FromHours(24),
				new ZDateTime(2023, 10, 5, 9, 0, 0),
				@"Calculating Delivery Due Date With Exceptions...
Latest Added/Changed Exception [Type: Exception-Future Event|Description: Delay7|Time: 27-Sep-23 09:00:00|Duration: 12 Hours]
Original Delivery Due Date: 28-Sep-23 11:00:00
Applying exception Delay1: The next available business day for Revised Delivery Due Date was: 29-Sep-23 07:00:00 which is Friday.
Applying exception Delay3: Weekend Days/Public Holidays of [21/8 Camillo St. WA Cannington 6155 AUCNN]: Saturday 30-Sep-23; Sunday 01-Oct-23; Monday 02-Oct-23;  The next available business day for Revised Delivery Due Date was: 03-Oct-23 02:00:00 which is Tuesday.
Applying exception Delay4: The next available business day for Revised Delivery Due Date was: 04-Oct-23 02:00:00 which is Wednesday.
Applying exception Delay7: The next available business day for Revised Delivery Due Date was: 05-Oct-23 02:00:00 which is Thursday.
Finding closest opening hour: 05-Oct-23 02:00:00 adjusted to 05-Oct-23 09:00:00
Calculated revised delivery due date by applying exception delays and finding closest opening hours: 05-Oct-23 09:00:00
");
		}

		void AssertCalculateDeliveryDueDateWithExceptions(TimeSpan maximumDuration, ZDateTime expectedNewDeliveryDueDate, string expectedExceptionsApplied)
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			using (FreightDataRegistry.Instance.CalculateDeliveryDateWithExceptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDateWithExceptionsOptions { MaximumDurationHours = (int)maximumDuration.TotalHours }))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				Env.Security.MaintainShipmentDeliveryDueDateOverride.IsAllowed = true;
				shipment.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "I want to override";

				shipment.JS_DeliveryDueDate = new ZDateTime(2023, 9, 28, 11, 0, 0);
				var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(Factory.NewWithValidTestData<OrgHeader>(), "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");
				shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;

				var exception1 = shipment.WorkflowItems.Exceptions.AddNew();
				exception1.P9_TaskID = "T001";
				exception1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-3);
				exception1.P9_ExceptionDurationHours = 20;
				exception1.P9_Description = "Delay1";

				var exception2 = shipment.WorkflowItems.Exceptions.AddNew();
				exception2.P9_TaskID = "T002";
				exception2.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-3);
				exception2.P9_ExceptionDurationHours = 10;
				exception2.P9_Description = "Delay2";

				var exception3 = shipment.WorkflowItems.Exceptions.AddNew();
				exception3.P9_TaskID = "T003";
				exception3.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-2);
				exception3.P9_ExceptionDurationHours = 15;
				exception3.P9_Description = "Delay3";

				var exception4 = shipment.WorkflowItems.Exceptions.AddNew();
				exception4.P9_TaskID = "T004";
				exception4.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-1);
				exception4.P9_ExceptionDurationHours = 100;
				exception4.P9_Description = "Delay4";

				var consol = shipment.Consols.AddNew();
				var exception5 = consol.WorkflowItems.Exceptions.AddNew();
				exception5.P9_TaskID = "T005";
				exception5.P9_ActualDateOffset = ZDateTimeOffset.Now;
				exception5.P9_ExceptionDurationHours = 10;
				exception5.P9_Description = "Delay5";

				var container = consol.Containers.AddNew();
				var packLine = shipment.OuterPackLines.AddNew();
				AssertEquals("Precondition", 1, shipment.Containers.Count());
				var exception6 = container.WorkflowItems.Exceptions.AddNew();
				exception6.P9_TaskID = "T006";
				exception6.P9_ActualDateOffset = ZDateTimeOffset.Now;
				exception6.P9_ExceptionDurationHours = 11;
				exception6.P9_Description = "Delay6";

				var exception7 = shipment.WorkflowItems.Exceptions.AddNew();
				exception7.P9_TaskID = "T007";
				exception7.P9_ActualDateOffset = ZDateTimeOffset.Now;
				exception7.P9_ExceptionDurationHours = 12;
				exception7.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionFutureEvent;
				exception7.P9_Description = "Delay7";

				Factory.Save();

				var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
				var result = calculator.CalculateDeliveryDueDateWithExceptions();
				AssertEquals(expectedNewDeliveryDueDate, result.DeliveryDueDate);
				AssertEquals(expectedExceptionsApplied, result.CalculationLog);
			}
		}

		public void TestCalculateDeliveryDueDateWithExceptions_NotAllPropertiesAreValid_ReturnsDeliveryDueDate()
		{
			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2023, 10, 1));
			AssertEquals("Precondition", new ZDateTime(2023, 10, 1), shipment.JS_DeliveryDueDate);
			AssertNotNull("Precondition", shipment.JS_OA_ImportReleaseDepot_ZAddress);

			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;

			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals(new ZDateTime(2023, 10, 2, 9, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Calculating Delivery Due Date With Exceptions...
Original Delivery Due Date: 01-Oct-23 00:00:00
Weekend Days/Public Holidays of [20/8 Camillo St. Cannington WA 6155 AUCNN]: Sunday 01-Oct-23; 
Finding closest opening hour: 01-Oct-23 00:00:00 adjusted to 02-Oct-23 09:00:00
Calculated revised delivery due date by applying exception delays and finding closest opening hours: 02-Oct-23 09:00:00
", result.CalculationLog);

			shipment.JS_DeliveryDueDate = ZDateTime.Empty;
			calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals(ZDateTime.Empty, result.DeliveryDueDate);
			AssertEquals("Checking Previous Delivery Due Date: Failed to calculate [Delivery Due Date with Exceptions] because Delivery Due Date is empty.", result.CalculationLog);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_WhenAgentDeliveryAddressIsNull()
		{
			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2023, 10, 1));
			AssertEquals("Precondition", new ZDateTime(2023, 10, 1), shipment.JS_DeliveryDueDate);
			AssertNotNull("Precondition", shipment.JS_OA_ImportReleaseDepot_ZAddress);

			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;

			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals(ZDateTime.Empty, result.DeliveryDueDate);
			AssertEquals("Finding CFS/Transit Warehouse and Service Level Default Transit Time: Failed to calculate [Delivery Due Date with Exceptions] because Delivery > CFS/Transit Warehouse is empty and Service Level does not have default transit time.", result.CalculationLog);

			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "D2D";
			serviceLevel.RS_DefaultTransitHours = 10;
			shipment.JS_RS_NKServiceLevel = "D2D";
			calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals(new ZDateTime(2023, 10, 1), result.DeliveryDueDate);
			AssertEquals(@"Calculating Delivery Due Date With Exceptions...
Original Delivery Due Date: 01-Oct-23 00:00:00
Calculated revised delivery due date by applying exception delays and finding closest opening hours: 01-Oct-23 00:00:00
", result.CalculationLog);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_WhenDeliveryAgentAddressIsNullAndShipmentIsDirectToCNE()
		{
			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2023, 10, 1));
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			Factory.Save();
			AssertEquals("Precondition", new ZDateTime(2023, 10, 1), shipment.JS_DeliveryDueDate);
			AssertNotNull("Precondition", shipment.JS_OA_ImportReleaseDepot_ZAddress);

			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
			AssertEquals("Precondition", true, shipment.IsDTC);

			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals(ZDateTime.Empty, result.DeliveryDueDate);
			AssertEquals("Finding Delivery Agent Address and Service Level Default Transit Time: Failed to calculate [Delivery Due Date with Exceptions] because Delivery > Delivery Agent Address is empty and Service Level does not have default transit time.", result.CalculationLog);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_ExceptionDeleted()
		{
			var deliveryDueDate = new ZDateTime(2023, 10, 4, 9, 0, 0);
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			using (FreightDataRegistry.Instance.CalculateDeliveryDateWithExceptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDateWithExceptionsOptions { UnlimitedDuration = ZBool.True }))
			{
				var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, deliveryDueDate);

				var exception1 = shipment.WorkflowItems.Exceptions.AddNew();
				exception1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-3);
				exception1.P9_ExceptionDurationHours = 6;
				exception1.P9_Description = "Bad weather";

				Factory.Save();

				var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
				var result = calculator.CalculateDeliveryDueDateWithExceptions();
				AssertEquals("Precondition", deliveryDueDate.AddHours(6), result.DeliveryDueDate);
				AssertEquals($@"Calculating Delivery Due Date With Exceptions...
Latest Added/Changed Exception [Type: |Description: Bad weather|Time: {exception1.P9_ActualDateOffset.ToZDateTime()}|Duration: 6 Hours]
Original Delivery Due Date: 04-Oct-23 09:00:00
Finding closest opening hour: 04-Oct-23 15:00:00 adjusted to 04-Oct-23 15:00:00
Calculated revised delivery due date by applying exception delays and finding closest opening hours: 04-Oct-23 15:00:00
", result.CalculationLog);

				var exceptionLog = shipment.Logs.AddNew();
				using (exceptionLog.LockForUpdatingKeyFieldsForTesting())
				{
					exceptionLog.SL_Reference = $"|JOB={exception1.PK}|CHG=Exception Deleted";
				}

				Factory.Save();
				calculator = new DeliveryDueDateWithExceptionsCalculator(shipment, exceptionLog);
				result = calculator.CalculateDeliveryDueDateWithExceptions();
				AssertEquals(deliveryDueDate, result.DeliveryDueDate);
				AssertEquals(@"Calculating Delivery Due Date With Exceptions...
Latest change: An exception has been deleted
Original Delivery Due Date: 04-Oct-23 09:00:00
Finding closest opening hour: 04-Oct-23 09:00:00 adjusted to 04-Oct-23 09:00:00
Calculated revised delivery due date by applying exception delays and finding closest opening hours: 04-Oct-23 09:00:00
", result.CalculationLog);
			}
		}

		public void TestDefaultTimetablesAreSetWhenCalculationInSaveTransaction()
		{
			var temporaryDefaultOrgTimetable = new DefaultOrgTimetableSettingsCollection();
			var defaultSetting = temporaryDefaultOrgTimetable.AddNew();
			var item1 = defaultSetting.Timetables.AddNew();
			item1.Type = OrgTimetableType.Codes.Deliver;
			item1.From = new ZDateTime(ZDateTime.Now.Year, 1, 1, 9, 0, 0);
			item1.To = new ZDateTime(ZDateTime.Now.Year, 1, 1, 17, 0, 0);
			item1.Day = "MON";

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryDefaultOrgTimetable))
			{
				var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2023, 10, 14, 17, 0, 0));
				Factory.Save();

				Factory.Saving += factory =>
				{
					var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
					var result = calculator.CalculateDeliveryDueDateWithExceptions();
					AssertEquals("Precondition", 1, shipment.ImportReleaseDepot.Timetables.Count);
					AssertEquals(new ZDateTime(2023, 10, 16, 9, 0, 0), result.DeliveryDueDate);
					AssertEquals(@"Calculating Delivery Due Date With Exceptions...
Original Delivery Due Date: 14-Oct-23 17:00:00
Weekend Days/Public Holidays of [21/8 Camillo St. Cannington WA 6155 AUCNN]: Saturday 14-Oct-23; Sunday 15-Oct-23; 
Finding closest opening hour: 14-Oct-23 17:00:00 adjusted to 16-Oct-23 09:00:00
Calculated revised delivery due date by applying exception delays and finding closest opening hours: 16-Oct-23 09:00:00
", result.CalculationLog);
				};

				Factory.Save();
			}
		}

		public void TestCalculateDeliveryDueDateWithExceptions_ShouldIgnoreOpeningHours_WhenDeliveryIsToAirport()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryDueDate = new ZDateTime(2024, 10, 26, 9, 0, 0);
			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, deliveryDueDate);
			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT;
			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals("When HBL Delivery Mode is to Airport, Opening hours should be ignored.", deliveryDueDate, result.DeliveryDueDate);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_ShouldIgnoreOpeningHours_WhenDeliveryIsToAirport_AndThereIsAnException()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryDueDate = new ZDateTime(2024, 10, 26, 9, 0, 0);
			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, deliveryDueDate);
			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT;

			var exception1 = shipment.WorkflowItems.Exceptions.AddNew();
			exception1.P9_TaskID = "T001";
			exception1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-3);
			exception1.P9_ExceptionDurationHours = 24;
			exception1.P9_Description = "Delay";

			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals("When HBL Delivery Mode is to Airport, Opening hours should be ignored.", deliveryDueDate.AddHours(24), result.DeliveryDueDate);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_ShouldApplyDeliveryDueTime_WhenDeliveryIsToDoor()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 7, 0, 0).TimeOfDay, DeliveryDueTimeSource.ZoneItem);
			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			var transportZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 24);
			transportZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.Time;

			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2023, 10, 1));
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals("When HBL Delivery Mode is Door to Door, Delivery Due Time should be applied.", new ZDateTime(2023, 10, 2, 7, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Calculating Delivery Due Date With Exceptions...
Original Delivery Due Date: 01-Oct-23 00:00:00
Delivery Address Weekend Days/Public Holidays: Sunday 01-Oct-23; 
01-Oct-23 07:00:00 adjusted to 02-Oct-23 07:00:00 because of Delivery Address Weekends/Public Holidays
Delivery due time adjusted from 01-Oct-23 00:00:00 to 02-Oct-23 07:00:00 based on ZoneItem's configured delivery due time
", result.CalculationLog);

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;

			calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals("When HBL Delivery Mode is Door to DTC, Delivery Due Time should not be applied and public holidays should not be checked.", new ZDateTime(2023, 10, 1, 0, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Calculating Delivery Due Date With Exceptions...
Original Delivery Due Date: 01-Oct-23 00:00:00
Delivery due time has not been adjusted because it is DTC. Calculated revised delivery due date by applying exception delays: 01-Oct-23 00:00:00
", result.CalculationLog);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_ShouldApplyHoldForPickup_WhenDeliveryIsToCFS()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var holdForPickup = new ZDateTime(1900, 1, 1, 7, 0, 0).TimeOfDay;
			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			var transportZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 24);
			transportZone.TransportProvider.TP_DefaultHoldForPickupTime = holdForPickup;

			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2023, 10, 1));
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS;

			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals(new ZDateTime(2023, 10, 2, 7, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Calculating Delivery Due Date With Exceptions...
Original Delivery Due Date: 01-Oct-23 00:00:00
Hold For Pickup Time: 07:00:00
Set Hold For Pickup Time: 01-Oct-23 00:00:00 adjusted to 01-Oct-23 07:00:00 based on DLV CFS's Hold for pickup time
DLV CFS/Transit Warehouse Weekend Days/Public Holidays: Sunday 01-Oct-23; 
01-Oct-23 07:00:00 adjusted to 02-Oct-23 09:00:00 because of DLV CFS Weekends/Public Holidays/Opening Hours
Set Hold For Pickup Time: 02-Oct-23 09:00:00 adjusted to 02-Oct-23 07:00:00 based on DLV CFS's Hold for pickup time
", result.CalculationLog);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_ShouldNotApplyDeliveryTimeWhenItIsWeekendDelivery()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 7, 0, 0).TimeOfDay, DeliveryDueTimeSource.ZoneItem);
			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			var transportZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 24);
			transportZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.Time;

			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2023, 10, 1));
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.ServiceLevel.RS_DeliverOnSaturday = true;

			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals("When Deliver On Weekend is true, Delivery Due Time should not be applied.", new ZDateTime(2023, 10, 1, 0, 0, 0), result.DeliveryDueDate);
			AssertEquals(@"Calculating Delivery Due Date With Exceptions...
Original Delivery Due Date: 01-Oct-23 00:00:00
Delivery due time adjusted as per the selected weekend service.
", result.CalculationLog);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_ShouldNotApplyDeliveryTimeWhenIsWeekendDelivery_OnSaturday()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 7, 0, 0).TimeOfDay, DeliveryDueTimeSource.ZoneItem);
			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			var transportZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 24);
			transportZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.Time;

			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2024, 05, 25));
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.ServiceLevel.RS_DeliverOnSaturday = true;
			var exception1 = shipment.WorkflowItems.Exceptions.AddNew();
			exception1.P9_TaskID = "T001";
			exception1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-3);
			exception1.P9_ExceptionDurationHours = 24;
			exception1.P9_Description = "Delay";

			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals("When Deliver On Weekend is true, Delivery Due Time should not be applied.", new ZDateTime(2024, 05, 27, 0, 0, 0), result.DeliveryDueDate);
			AssertEquals($@"Calculating Delivery Due Date With Exceptions...
Latest Added/Changed Exception [Type: |Description: {exception1.P9_Description}|Time: {exception1.P9_ActualDateOffset.ToZDateTime()}|Duration: 24 Hours]
Original Delivery Due Date: 25-May-24 00:00:00
Applying exception Delay: Weekend Days/Public Holidays of [21/8 Camillo St. WA Cannington 6155 AUCNN]: Sunday 26-May-24;  Service Level STD, has delivery on weekend ticked. The next available business day for Revised Delivery Due Date was: 27-May-24 00:00:00 which is Monday.
Delivery due time adjusted as per the selected weekend service.
", result.CalculationLog);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_ShouldNotApplyDeliveryTimeWhenIsWeekendDelivery_OnSaturdayAndSunday()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 7, 0, 0).TimeOfDay, DeliveryDueTimeSource.ZoneItem);
			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			var transportZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 24);
			transportZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.Time;

			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2024, 05, 25));
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.ServiceLevel.RS_DeliverOnSaturday = true;
			shipment.ServiceLevel.RS_DeliverOnSunday = true;
			var exception1 = shipment.WorkflowItems.Exceptions.AddNew();
			exception1.P9_TaskID = "T001";
			exception1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-3);
			exception1.P9_ExceptionDurationHours = 24;
			exception1.P9_Description = "Delay";

			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals("When Deliver On Weekend is true, Delivery Due Time should not be applied.", new ZDateTime(2024, 05, 26, 0, 0, 0), result.DeliveryDueDate);
			AssertEquals($@"Calculating Delivery Due Date With Exceptions...
Latest Added/Changed Exception [Type: |Description: {exception1.P9_Description}|Time: {exception1.P9_ActualDateOffset.ToZDateTime()}|Duration: 24 Hours]
Original Delivery Due Date: 25-May-24 00:00:00
Applying exception Delay: The next available business day for Revised Delivery Due Date was: 26-May-24 00:00:00 which is Sunday.
Delivery due time adjusted as per the selected weekend service.
", result.CalculationLog);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_ShouldNotApplyDeliveryTimeWhenIsWeekendDelivery_OnFridayAsFirstWeekendDay()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedArabEmirates));
			country.IsFridayNonWorkingDay = true;
			country.IsSaturdayNonWorkingDay = true;
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.UnitedArabEmirates, "Dubai", "No state", "AEAAN");
			deliveryAddress.Timetables.NotApplicable = true;

			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 7, 0, 0).TimeOfDay, DeliveryDueTimeSource.ZoneItem);
			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AEDXB";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "PLOT NO. S21532, SOUTH FREE ZONE", "6155", Core.Constants.CountryCodes.UnitedArabEmirates, "JEBEL ALI", "No state", "AEDXB");
			deliveryCFSAddress.Timetables.NotApplicable = true;
			var transportZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 24);
			transportZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.Time;

			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2024, 05, 24));
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.ServiceLevel.RS_DeliverOnSaturday = true;
			Factory.Save();

			var exception1 = shipment.WorkflowItems.Exceptions.AddNew();
			exception1.P9_TaskID = "T001";
			exception1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-3);
			exception1.P9_ExceptionDurationHours = 24;
			exception1.P9_Description = "Delay";

			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals("When Deliver On Weekend is true, Delivery Due Time should not be applied.", new ZDateTime(2024, 05, 26, 0, 0, 0), result.DeliveryDueDate);
			AssertEquals($@"Calculating Delivery Due Date With Exceptions...
Latest Added/Changed Exception [Type: |Description: {exception1.P9_Description}|Time: {exception1.P9_ActualDateOffset.ToZDateTime()}|Duration: 24 Hours]
Original Delivery Due Date: 24-May-24 00:00:00
Applying exception Delay: Weekend Days/Public Holidays of [PLOT NO. S21532, SOUTH FREE ZONE No state JEBEL ALI 6155 AEDXB]: Saturday 25-May-24;  Service Level STD, has delivery on weekend ticked. The next available business day for Revised Delivery Due Date was: 26-May-24 00:00:00 which is Sunday.
Delivery due time adjusted as per the selected weekend service.
", result.CalculationLog);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_WhenFirstWeekendTickedAndSecondWeekendUnticked_DeliveryOnSaturday_MondayHoliday_ReturnsNextTuesday()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");

			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 7, 0, 0).TimeOfDay, DeliveryDueTimeSource.ZoneItem);
			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");

			var transportZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 24);
			transportZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.Time;
			var state = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_Code, "WA"));
			var mondayPublicHoliday = new ZDateTime(2024, 05, 27, 0, 0, 0);
			var glbHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			glbHoliday.GH_Date = mondayPublicHoliday;
			glbHoliday.GH_IsWorkingDay = false;
			glbHoliday.GH_Recurring = true;
			glbHoliday.GH_ParentID = state.PK;
			glbHoliday.GH_ParentTableCode = "RW";
			glbHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			glbHoliday.GH_IsActive = true;
			Factory.Save();

			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2024, 05, 25));
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.ServiceLevel.RS_DeliverOnSaturday = true;
			var exception1 = shipment.WorkflowItems.Exceptions.AddNew();
			exception1.P9_TaskID = "T001";
			exception1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-3);
			exception1.P9_ExceptionDurationHours = 24;
			exception1.P9_Description = "Delay";
			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals("When Deliver On Weekend is true, Delivery Due Time should not be applied.", new ZDateTime(2024, 05, 28, 0, 0, 0), result.DeliveryDueDate);
			AssertEquals($@"Calculating Delivery Due Date With Exceptions...
Latest Added/Changed Exception [Type: |Description: {exception1.P9_Description}|Time: {exception1.P9_ActualDateOffset.ToZDateTime()}|Duration: 24 Hours]
Original Delivery Due Date: 25-May-24 00:00:00
Applying exception Delay: Weekend Days/Public Holidays of [21/8 Camillo St. Cannington WA 6155 AUCNN]: Sunday 26-May-24; Monday 27-May-24;  Service Level STD, has delivery on weekend ticked. The next available business day for Revised Delivery Due Date was: 28-May-24 00:00:00 which is Tuesday.
Delivery due time adjusted as per the selected weekend service.
", result.CalculationLog);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_WhenBothWeekendsTicked_SecondWeekendPublicHoliday_DeliveryOnFirstWeekendSaturday_OneDayException_ReturnsMonday()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "NSW", "Sydney", "AUSYD");

			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 7, 0, 0).TimeOfDay, DeliveryDueTimeSource.ZoneItem);
			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "WA", "Cannington", "AUCNN");

			var transportZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 24);
			transportZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.Time;
			var state = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_Code, "WA"));
			var saturdayPublicHoliday = new ZDateTime(2024, 05, 26, 0, 0, 0);
			var glbHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			glbHoliday.GH_Date = saturdayPublicHoliday;
			glbHoliday.GH_IsWorkingDay = false;
			glbHoliday.GH_Recurring = true;
			glbHoliday.GH_ParentID = state.PK;
			glbHoliday.GH_ParentTableCode = "RW";
			glbHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			glbHoliday.GH_IsActive = true;
			Factory.Save();

			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2024, 05, 25));
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.ServiceLevel.RS_DeliverOnSaturday = true;
			shipment.ServiceLevel.RS_DeliverOnSunday = true;
			var exception1 = shipment.WorkflowItems.Exceptions.AddNew();
			exception1.P9_TaskID = "T001";
			exception1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-3);
			exception1.P9_ExceptionDurationHours = 24;
			exception1.P9_Description = "Delay";
			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals("When Deliver On Weekend is true, Delivery Due Time should not be applied.", new ZDateTime(2024, 05, 27, 0, 0, 0), result.DeliveryDueDate);
			AssertEquals($@"Calculating Delivery Due Date With Exceptions...
Latest Added/Changed Exception [Type: |Description: {exception1.P9_Description}|Time: {exception1.P9_ActualDateOffset.ToZDateTime()}|Duration: 24 Hours]
Original Delivery Due Date: 25-May-24 00:00:00
Applying exception Delay: Weekend Days/Public Holidays of [21/8 Camillo St. Cannington WA 6155 AUCNN]: Sunday 26-May-24;  Service Level STD, has delivery on weekend ticked. Delivery Due Date is 25-May-24 00:00:00 which is Saturday. The next available business day for Revised Delivery Due Date was: 27-May-24 00:00:00 which is Monday.
Delivery due time adjusted as per the selected weekend service.
", result.CalculationLog);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_WhenDeliveryTimeAfterDayWorkingTime()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 7, 0, 0).TimeOfDay, DeliveryDueTimeSource.ZoneItem);
			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			var transportZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 24);
			transportZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.Time;

			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2024, 05, 20, 11, 0, 0));
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS;
			var exception1 = shipment.WorkflowItems.Exceptions.AddNew();
			exception1.P9_TaskID = "T001";
			exception1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-3);
			exception1.P9_ExceptionDurationHours = 10;

			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals("When Deliver On Weekend is true, Delivery Due Time should not be applied.", new ZDateTime(2024, 05, 21, 9, 0, 0), result.DeliveryDueDate);
			AssertEquals($@"Calculating Delivery Due Date With Exceptions...
Latest Added/Changed Exception [Type: |Description: |Time: {exception1.P9_ActualDateOffset.ToZDateTime()}|Duration: 10 Hours]
Original Delivery Due Date: 20-May-24 11:00:00
Finding closest opening hour: 20-May-24 21:00:00 adjusted to 21-May-24 09:00:00
Calculated revised delivery due date by applying exception delays and finding closest opening hours: 21-May-24 09:00:00
", result.CalculationLog);
		}

		public void TestCalculateDeliveryDueDateWithExceptions_WhenDeliveryTimeBeforeDayWorkingTime()
		{
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryOrg, "357/4 Bindon Place", "2017", Core.Constants.CountryCodes.Australia, "Sydney", "NSW", "AUSYD");

			var deliveryDueTime = new DeliveryDueTime(new ZDateTime(1900, 1, 1, 7, 0, 0).TimeOfDay, DeliveryDueTimeSource.ZoneItem);
			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.OH_RL_NKClosestPort = "AUCNN";
			var deliveryCFSAddress = DeliveryDueDateCalculationTestHelper.SetupAddress(deliveryCFSOrg, "21/8 Camillo St.", "6155", Core.Constants.CountryCodes.Australia, "Cannington", "WA", "AUCNN");

			var transportZone = DeliveryDueDateCalculationTestHelper.SetupTransportZone(Factory, deliveryCFSOrg, deliveryAddress.Postcode, ZString.Empty, deliveryAddress.OA_RN_NKCountryCode, 24);
			transportZone.Items[0].TQ_DeliveryDueTime = deliveryDueTime.Time;

			var shipment = DeliveryDueDateWithExceptionsTestHelper.CreateShipmentForTest(Factory, new ZDateTime(2024, 05, 20, 11, 0, 0));
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = deliveryCFSAddress.PK;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS;
			var exception1 = shipment.WorkflowItems.Exceptions.AddNew();
			exception1.P9_TaskID = "T001";
			exception1.P9_ActualDateOffset = ZDateTimeOffset.Now.AddDays(-3);
			exception1.P9_ExceptionDurationHours = 2;

			var calculator = new DeliveryDueDateWithExceptionsCalculator(shipment);
			var result = calculator.CalculateDeliveryDueDateWithExceptions();
			AssertEquals("When Deliver On Weekend is true, Delivery Due Time should not be applied.", new ZDateTime(2024, 05, 20, 13, 0, 0), result.DeliveryDueDate);
			AssertEquals($@"Calculating Delivery Due Date With Exceptions...
Latest Added/Changed Exception [Type: |Description: |Time: {exception1.P9_ActualDateOffset.ToZDateTime()}|Duration: 2 Hours]
Original Delivery Due Date: 20-May-24 11:00:00
Finding closest opening hour: 20-May-24 13:00:00 adjusted to 20-May-24 13:00:00
Calculated revised delivery due date by applying exception delays and finding closest opening hours: 20-May-24 13:00:00
", result.CalculationLog);
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}
	}
}
