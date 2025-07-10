using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI.Testing
{
	public class TransportBookingsControlTest : DtbBookingTestCaseWithFactory
	{
		public void TestOpenBooking()
		{
			ConsolidationViewModeService.SetViewMode(Factory, ConsolidationViewMode.MultiJob);

			var booking = Helper.CreateBooking();
			var consolidation = Helper.CreateConsolidationMultiJob();
			consolidation.Bookings.Add(booking);

			using (var form = new TransportBookingMultiForm(booking.ConsolidationMultiJob))
			{
				form.Show();
				var grid = form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid;
				grid.Select(0);
				grid.ContextMenu.MenuItems.FindByText("OpenBooking").PerformClick();
				AssertNull("No form should be shown because the Booking is not saved.", form.transportBookingsControl.ControllerForTest);

				Factory.Save();
				grid.ContextMenu.MenuItems.FindByText("OpenBooking").PerformClick();
				var lastShownForm = (TransportBookingForm)form.transportBookingsControl.ControllerForTest.LastShownForm;
				AssertEquals(ConsolidationViewMode.SingleJob, ConsolidationViewModeService.GetViewMode(lastShownForm.BusinessEntity.Factory));
				AssertEquals(ODisplayMode.Browse, lastShownForm.DisplayMode);
				lastShownForm.Dispose();
			}
		}

		public void TestOpenConsolidation()
		{
			ConsolidationViewModeService.SetViewMode(Factory, ConsolidationViewMode.SingleJob);

			var booking = Helper.CreateBooking();
			Factory.Save();

			using (var form = new TransportBookingMultiForm(booking.ConsolidationSingleJob))
			{
				form.Show();
				var grid = form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid;
				grid.Select(0);

				// booking with no multi-job consolidation
				grid.ContextMenu.MenuItems.FindByText("OpenConsolidation").PerformClick();
				AssertNull("No form should be shown because the Booking is not on a Multi-Job Consolidation.", form.transportBookingsControl.ControllerForTest);

				// booking with a multi-job consolidation
				Helper.CreateConsolidationMultiJob().Bookings.Add(booking);
				grid.ContextMenu.MenuItems.FindByText("OpenConsolidation").PerformClick();
				AssertNull("No form should be shown because the Booking is not saved.", form.transportBookingsControl.ControllerForTest);

				Factory.Save();
				grid.ContextMenu.MenuItems.FindByText("OpenConsolidation").PerformClick();
				var lastShownForm = (TransportBookingMultiForm)form.transportBookingsControl.ControllerForTest.LastShownForm;
				AssertEquals(ConsolidationViewMode.MultiJob, ConsolidationViewModeService.GetViewMode(lastShownForm.BusinessEntity.Factory));
				AssertEquals(ODisplayMode.Browse, lastShownForm.DisplayMode);
				lastShownForm.Dispose();
			}
		}

		public void TestTransportCoAndBookingPartVisibility()
		{
			var consolidationMultiJob = Helper.CreateConsolidationMultiJob();
			var consolidationSingleJob = Helper.CreateConsolidation();
			var bookingForConsolidationMultiJob = Helper.CreateBooking(consolidationMultiJob);
			var bookingForConsolidationSingleJob = Helper.CreateBooking(consolidationSingleJob);

			using (var form = new TransportBookingMultiForm(consolidationMultiJob))
			{
				form.Show();
				AssertEquals(true, form.transportBookingsControl.TransportCoAndBookingPartyPanelForTest.Visible);
				AssertEquals(true, form.transportBookingsControl.TransportCoPanelForTest.Visible);
				AssertEquals(false, form.transportBookingsControl.BookingPartyPanelControlForTest.Visible);
			}

			using (var form = new TransportBookingMultiForm(consolidationSingleJob))
			{
				form.Show();
				AssertEquals(true, form.transportBookingsControl.TransportCoAndBookingPartyPanelForTest.Visible);
				AssertEquals(false, form.transportBookingsControl.TransportCoPanelForTest.Visible);
				AssertEquals(true, form.transportBookingsControl.BookingPartyPanelControlForTest.Visible);
			}

			// Consolidation single job has a parent
			var shipment = Factory.New<Integration.Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			consolidationSingleJob.KB_ParentID = shipment.PK;
			consolidationSingleJob.KB_ParentTableCode = "JS";
			using (var form = new TransportBookingMultiForm(consolidationSingleJob))
			{
				form.Show();
				AssertEquals(false, form.transportBookingsControl.TransportCoAndBookingPartyPanelForTest.Visible);
				AssertEquals(false, form.transportBookingsControl.TransportCoPanelForTest.Visible);
				AssertEquals(false, form.transportBookingsControl.BookingPartyPanelControlForTest.Visible);
			}
		}

		public void TestBookingsGrid_DoubleClick_OpenModalSingleBookingForm()
		{
			ConsolidationViewModeService.SetViewMode(Factory, ConsolidationViewMode.SingleJob);

			var booking = Helper.CreateBooking();
			Factory.Save();

			using (var form = new TransportBookingMultiForm(booking.ConsolidationSingleJob))
			{
				form.Show();
				var grid = form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid;
				grid.Select(0);

				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 2, 50, 30, 0) });
				var singleBookingForm = ZFormModaliser.ActiveForm;
				AssertNotNull("Should Open a single Transport Booking form.", singleBookingForm);
				AssertEquals(typeof(TransportBookingForm), singleBookingForm.GetType());
				AssertEquals(form, singleBookingForm.Owner);

				singleBookingForm.Dispose();
			}
		}

		public void TestBookingsModuleButtonGridColumns()
		{
			ConsolidationViewModeService.SetViewMode(Factory, ConsolidationViewMode.SingleJob);

			var booking = Helper.CreateBooking();
			Factory.Save();

			using (var form = new TransportBookingMultiForm(booking.ConsolidationSingleJob))
			{
				form.Show();
				var gridColumns = form.transportBookingsControl.BookingsModuleButtonGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var gridColumnNames = gridColumns.Select(l => l.ColumnName);

				var expectedGridColumnNames = new[]
				{
					"KM_JobID",
					"ConsolidationSingleJob+Parent+JobNumber",
					"ConsolidationMultiJob+KB_JobID",
					"KM_KT_NKBookingTemplate",
					"BookingTemplate+KT_DescriptionMultilingual",
					"Address+OrganisationNameOrPK",
					"Address+E2_OA_Address",
					"KM_TransportReference",
					"StatusDescription",
					"KM_Direction",
					"KM_IsActive",
					"WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent",
					"WorkflowItems+Milestones+LastMilestone+DescriptionWithReference",
					"WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding",
					"WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent",
					"WorkflowItems+Milestones+NextMilestone+DescriptionWithReference",
					"WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding",
					"ConsolidationSingleJob+KB_GoodsDescription",
					"WayBillNumber",
					"KM_RS_NKServiceLevel",
					"KM_PL_NKCarrierServiceLevel",
					"KM_RatingFreightMode",
					"KM_IsHazardous",
					"KM_RequiresRefrigeration",
					"BillingPartyOrLocalClientPK_ZAddress+OrgPK",
					"BillingPartyOrLocalClientPK",
					"CarrierAccount+OAN_AccountNumber",
					"TotalPackages",
					"TotalPackagesType",
					"TotalWeightExcludingDuplicatePackages+Amount",
					"TotalWeightExcludingDuplicatePackages+Unit",
					"TotalVolumeExcludingDuplicatePackages+Amount",
					"TotalVolumeExcludingDuplicatePackages+Unit",
					"KM_Calc_ActualVolumeWeight",
					"KM_Calc_ActualVolumeWeightUnit",
					"KM_Calc_RoundedChargeable",
					"ChargeableUnits",
					"KM_OverrideChargeable",
					"CarrierBookingAgentDocAddress+OrganisationNameOrPK",
					"KM_GB_Branch",
					"MasterBooking+KM_JobID",
					"Address+E2_CompanyName",
					"CarrierBookingAgentDocAddress+E2_CompanyName",
					"KM_TransportMode",
					"BillingPartyAddress+CompanyName",
					"KM_Direction",
					"KM_SystemCreateTimeUtc",
					"KM_SystemLastEditTimeUtc",
					"KM_SystemCreateUser",
					"KM_SystemLastEditUser",
					"CarrierBookingAgentDocAddress+E2_OA_Address",
				};

				AssertContainsExactElementsInAnyOrder(expectedGridColumnNames, gridColumnNames);
			}
		}

		protected override BusinessObjectFactory NewFactory()
		{
			var factory = base.NewFactory();
			DtbChildEditableService.SetState(factory, DtbChildEditableServiceState.Consolidation);
			return factory;
		}
	}
}
