using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.OnlineSailingSchedules;
using Enterprise.Freight.OnlineSailingSchedules.Testing;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.GUI.Testing
{
	public class ScheduleChooserControlTest : TestCaseWithFactory
	{
		public void TestClickButtonSelectFromConsortium()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			using (var form = new QuotedBookingForm(quotedBooking))
			using (var control = new ScheduleChooserControl())
			{
				form.Controls.Add(control);

				var buttonSelectFromConsortium = GetPrivateButton(control, "ButtonSelectFromConsortium");
				form.Show();
				Assert("Button SelectFromConsortium should hook", buttonSelectFromConsortium.ClickHasBeenHooked);
			}
		}

		public void TestTextBoxUpToDateWhenNoChangesMade()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.Mode = Core.Constants.RateMode.LSE;
			using (var form = new QuotedBookingForm(quotedBooking))
			using (var control = new ScheduleChooserControl())
			{
				form.Controls.Add(control);
				var viewSailingButton = GetPrivateButton(control, "ViewSailingButton");
				var addNewSailingButton = GetPrivateButton(control, "AddNewSailingButton");
				var clearSailingButton = GetPrivateButton(control, "ClearSailingButton");
				form.Show();
				CombineAssertions(delegate
				{
					AssertEquals("Button caption should contain 'flight'", "View Flights", viewSailingButton.Text);
					AssertEquals("Button caption should contain 'flight'", "Add New Flight", addNewSailingButton.Text);
					AssertEquals("Button caption should contain 'flight'", "Clear Flight", clearSailingButton.Text);
				}

				);
			}
		}

		public void TestBindingWhenParentFormIsNull()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			using (var control = new ScheduleChooserControl())
			{
				AssertEquals("Precondition: Parent Form is null", null, control.ParentForm);
				AssertNoExceptionThrown("No parent should not cause exception on binding.", () => control.SetDataBinding(quotedBooking, ""));
			}
		}

		public void TestLinkSailingSchedule_WithNoAllocationRoutes()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var voyage = CreateVoyage();

			var contract = CreateCarrierContract();
			var allocationRoute = contract.Allocations.AddNew();
			var carrierContractNumber = contract.RCT_ContractNumber;
			var allocationLinePK = allocationRoute.PK;
			var sailing = voyage.Sailings.First();

			quotedBooking.CarrierContractNumber = carrierContractNumber;
			quotedBooking.AllocationLinePK = allocationLinePK;

			var routeDepart = new DateTime(2016, 8, 10);
			var routeArrive = new DateTime(2016, 9, 18);

			var leg1 = RouteTestHelper.CreateServiceLeg("100", "OLGA MAERSK", "SHMAERSK", "AUSYD", routeDepart, "NZAKL", routeArrive, "LISA01");
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				var additionalDetailsControl = form.QuotedBookingAdditionalDetailsControl;
				var scheduleChooserControl = additionalDetailsControl.FindSingle<ScheduleChooserControl>();
				scheduleChooserControl.UpdateCarrierContractAndAllocationDetails(route);

				AssertEquals(booking.JS_JX, sailing.PK);
				AssertEquals(quotedBooking.CarrierContractNumber, carrierContractNumber);
				AssertEquals(quotedBooking.AllocationLinePK, allocationLinePK);
			}
		}

		public void TestLinkSailingSchedule_WithSingleAllocationRoute_EmptyDetails()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);

			var voyage = CreateVoyage();
			var sailing = voyage.Sailings.First();

			var contract = CreateCarrierContract();
			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_JX_SailingSchedule = sailing.PK;

			var routeDepart = new DateTime(2016, 8, 10);
			var routeArrive = new DateTime(2016, 9, 18);
			var leg1 = RouteTestHelper.CreateServiceLeg("100", "OLGA MAERSK", "SHMAERSK", "AUSYD", routeDepart, "NZAKL", routeArrive, "LISA01");
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				var additionalDetailsControl = form.QuotedBookingAdditionalDetailsControl;
				var scheduleChooserControl = additionalDetailsControl.FindSingle<ScheduleChooserControl>();
				scheduleChooserControl.UpdateCarrierContractAndAllocationDetails(route);

				AssertEquals(quotedBooking.Booking.JS_JX, sailing.PK);
				AssertEquals(quotedBooking.CarrierContractNumber, contract.RCT_ContractNumber);
				AssertEquals(quotedBooking.AllocationLinePK, allocationRoute.PK);
			}
		}

		public void TestLinkSailingSchedule_WithSingleAllocationRoute_Override()
		{
			var voyage = CreateVoyage();
			var sailing = voyage.Sailings.First();

			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			quotedBooking.CarrierContractNumber = "ABC";
			quotedBooking.AllocationLinePK = new ZGuid("20ce66c3-a584-4598-8e4f-554b537f90e9");

			var contract = CreateCarrierContract();
			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_JX_SailingSchedule = sailing.PK;

			var routeDepart = new DateTime(2016, 8, 10);
			var routeArrive = new DateTime(2016, 9, 18);

			var leg1 = RouteTestHelper.CreateServiceLeg("100", "OLGA MAERSK", "SHMAERSK", "AUSYD", routeDepart, "NZAKL", routeArrive, "LISA01");
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				var additionalDetailsControl = form.QuotedBookingAdditionalDetailsControl;
				var scheduleChooserControl = additionalDetailsControl.FindSingle<ScheduleChooserControl>();

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				scheduleChooserControl.UpdateCarrierContractAndAllocationDetails(route);

				AssertEquals(booking.JS_JX, sailing.PK);

				AssertEquals(quotedBooking.CarrierContractNumber, contract.RCT_ContractNumber);
				AssertEquals(quotedBooking.AllocationLinePK, allocationRoute.PK);
			}
		}

		public void TestLinkSailingSchedule_WithSingleAllocationRoute_NoOverride()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var voyage = CreateVoyage();
			var sailing = voyage.Sailings.First();

			var contract = CreateCarrierContract();
			var allocation = contract.Allocations.AddNew();
			quotedBooking.CarrierContractNumber = contract.RCT_ContractNumber;
			quotedBooking.AllocationLinePK = allocation.PK;

			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_JX_SailingSchedule = sailing.PK;

			var routeDepart = new DateTime(2016, 8, 10);
			var routeArrive = new DateTime(2016, 9, 18);

			var leg1 = RouteTestHelper.CreateServiceLeg("100", "OLGA MAERSK", "SHMAERSK", "AUSYD", routeDepart, "NZAKL", routeArrive, "LISA01");
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				var additionalDetailsControl = form.QuotedBookingAdditionalDetailsControl;
				var scheduleChooserControl = additionalDetailsControl.FindSingle<ScheduleChooserControl>();

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				scheduleChooserControl.UpdateCarrierContractAndAllocationDetails(route);

				AssertEquals(booking.JS_JX, sailing.PK);

				AssertEquals(quotedBooking.CarrierContractNumber, contract.RCT_ContractNumber);
				AssertEquals(quotedBooking.AllocationLinePK, allocation.PK);
			}
		}

		public void TestLinkSailingSchedule_WithMultipleAllocationRoute_Override()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var voyage = CreateVoyage();
			var sailing = voyage.Sailings.First();

			quotedBooking.CarrierContractNumber = new ZString("ABC");
			quotedBooking.AllocationLinePK = new ZGuid("d546cdc0-530a-4577-b9e6-f62427e0f677");

			var contract = CreateCarrierContract();
			var allocationRoute1 = Factory.New<IRatingContractAllocationLine>();
			allocationRoute1.RCA_RCT_RatingContract = contract.PK;
			allocationRoute1.RCA_JX_SailingSchedule = sailing.PK;

			var allocationRoute2 = Factory.New<IRatingContractAllocationLine>();
			allocationRoute2.RCA_RCT_RatingContract = contract.PK;
			allocationRoute2.RCA_JX_SailingSchedule = sailing.PK;

			var allocationRoute3 = Factory.New<IRatingContractAllocationLine>();
			allocationRoute3.RCA_RCT_RatingContract = contract.PK;
			allocationRoute3.RCA_JX_SailingSchedule = sailing.PK;

			var allocationRoutes = new IRatingContractAllocationLine[] { allocationRoute1, allocationRoute2, allocationRoute3 };

			var routeDepart = new DateTime(2016, 8, 10);
			var routeArrive = new DateTime(2016, 9, 18);

			var leg1 = RouteTestHelper.CreateServiceLeg("100", "OLGA MAERSK", "SHMAERSK", "AUSYD", routeDepart, "NZAKL", routeArrive, "LISA01");
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingForm(quotedBooking))
			using (var control = new ScheduleChooserControl())
			{
				form.Controls.Add(control);
				control.UpdateCarrierContractAndAllocationDetails(route);

				AssertEquals(booking.JS_JX, sailing.PK);

				var lastShownDialog = ZFormModaliser.LastFormShownDialogForTest;
				AssertNotNull(lastShownDialog);
			}
		}

		public void TestLinkSailingSchedule_WithMultipleAllocationRoute_NoOverride()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);

			var voyage = CreateVoyage();
			var sailing = voyage.Sailings.First();

			var contract = CreateCarrierContract();
			var allocationRoute1 = Factory.New<IRatingContractAllocationLine>();
			allocationRoute1.RCA_RCT_RatingContract = contract.PK;
			allocationRoute1.RCA_JX_SailingSchedule = sailing.PK;

			var allocationRoute2 = Factory.New<IRatingContractAllocationLine>();
			allocationRoute2.RCA_RCT_RatingContract = contract.PK;
			allocationRoute2.RCA_JX_SailingSchedule = sailing.PK;

			var allocationRoute3 = Factory.New<IRatingContractAllocationLine>();
			allocationRoute3.RCA_RCT_RatingContract = contract.PK;
			allocationRoute3.RCA_JX_SailingSchedule = sailing.PK;

			quotedBooking.CarrierContractNumber = contract.RCT_ContractNumber;
			quotedBooking.AllocationLinePK = allocationRoute2.PK;

			var routeDepart = new DateTime(2016, 8, 10);
			var routeArrive = new DateTime(2016, 9, 18);
			var leg1 = RouteTestHelper.CreateServiceLeg("100", "OLGA MAERSK", "SHMAERSK", "AUSYD", routeDepart, "NZAKL", routeArrive, "LISA01");
			var serviceRoute = RouteTestHelper.CreateServiceRoute(leg1);
			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new QuotedBookingForm(quotedBooking))
			{
				var additionalDetailsControl = form.QuotedBookingAdditionalDetailsControl;
				var scheduleChooserControl = additionalDetailsControl.FindSingle<ScheduleChooserControl>();
				scheduleChooserControl.UpdateCarrierContractAndAllocationDetails(route);

				AssertEquals(booking.JS_JX, sailing.PK);

				AssertEquals(quotedBooking.CarrierContractNumber, contract.RCT_ContractNumber);
				AssertEquals(quotedBooking.AllocationLinePK, allocationRoute2.PK);
			}
		}

		ZButton GetPrivateButton(ScheduleChooserControl control, string buttonName)
		{
			var button = (ZButton)control.GetType().GetField(buttonName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(control);
			return button;
		}

		IRatingContract CreateCarrierContract()
		{
			var contract = Factory.New<IRatingContract>();
			contract.RCT_ContractNumber = "ABC";
			contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
			contract.RCT_StartDate = ZDate.Today;
			contract.RCT_GS_NKContractOwner = "DS2";

			var carrierOrganization = Factory.NewWithValidTestData<OrgHeader>();
			contract.RCT_OH = carrierOrganization.PK;

			return contract;
		}

		JobVoyage CreateVoyage()
		{
			var carrierOrganization = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganization.OH_Code = "SHMAERSK";
			carrierOrganization.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SHMA",
				Core.Constants.CountryCodes.UnitedStates);

			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			var vessel = (BusinessObject)Factory.New<IRefVessel>();
			vessel[RefVesselSchema.RV_Code] = "OLGA MAERSK";
			vessel[RefVesselSchema.RV_LloydsNumber] = "LISA01";

			voyage.JV_RV_NKVessel = "OLGA MAERSK";
			voyage.JV_OH_Line = carrierOrganization.PK;
			voyage.JV_VoyageFlight = "100";
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();

			origin.JA_RL_NKPortOfLoading = "AUSYD";
			destination.JB_RL_NKPortOfDischarge = "NZAKL";

			voyage.GenerateSailings();

			Factory.Save();

			return voyage;
		}
	}
}
