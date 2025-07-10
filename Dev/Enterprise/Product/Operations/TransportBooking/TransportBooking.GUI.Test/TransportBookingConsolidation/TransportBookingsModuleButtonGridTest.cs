using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.GUI.Testing
{
	public class TransportBookingsModuleButtonGridTest : DtbBookingTestCaseWithFactory
	{
		public void TestUpdateBookingsRemoveAction()
		{
			var booking1 = Helper.CreateBooking();
			var booking2 = Helper.CreateBooking();
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			var singleJobConsolidation = Helper.CreateConsolidation();
			multiJobConsolidation.Bookings.Add(booking1);
			singleJobConsolidation.Bookings.Add(booking2);

			using (var form = new TransportBookingMultiForm(multiJobConsolidation))
			{
				form.Show();

				var moduleButtonGridControl = form.transportBookingsControl.BookingsModuleButtonGrid;
				var innerGrid = moduleButtonGridControl.InnerGrid;
				innerGrid.SetCurrentHitTestForTest(0, 0);
				innerGrid.ContextMenu.DoPopup();
				CombineAssertions("Check detach button and caption", () =>
				{
					AssertEquals(RemoveAction.Remove, innerGrid.RemoveAction);
					AssertEquals("Detach Booking", innerGrid.DeleteMenuItem.Text);
					AssertEquals("Menu item for Detach Booking is enabled", true, innerGrid.DeleteMenuItem.Enabled);
				});
			}

			using (var form = new TransportBookingMultiForm(singleJobConsolidation))
			{
				form.Show();

				var moduleButtonGridControl = form.transportBookingsControl.BookingsModuleButtonGrid;
				var innerGrid = moduleButtonGridControl.InnerGrid;
				innerGrid.SetCurrentHitTestForTest(0, 0);
				innerGrid.ContextMenu.DoPopup();
				CombineAssertions("Check delete button and caption", () =>
				{
					AssertEquals(RemoveAction.RemoveAndDelete, innerGrid.RemoveAction);
					AssertEquals("Delete Booking", innerGrid.DeleteMenuItem.Text);
					AssertEquals("Menu item for Delete Booking is disabled", false, innerGrid.DeleteMenuItem.Enabled);
				});
			}
		}

		public void TestUpdateMasterBookingRemoveAction()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var masterBooking = helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			setupFactory.Save();
			var subBooking = helper.CreateBooking();
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subBooking.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBooking.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			setupFactory.Save();
			var masterBookingPk = masterBooking.PK;
			masterBooking = Factory.Load<DtbBooking>(masterBookingPk);
			var subBookingPk = subBooking.PK;
			subBooking = Factory.Load<DtbBooking>(subBookingPk);
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			using (var form = new TransportBookingForm(masterBooking))
			{
				form.Show();
				var moduleButtonGridControl = form.InstructionViewsControl.SubTransportBookingsControl.BookingsModuleButtonGrid;
				var innerGrid = moduleButtonGridControl.InnerGrid;
				innerGrid.SetCurrentHitTestForTest(0, 0);
				moduleButtonGridControl.InnerGrid.ContextMenu.DoPopup();
				CombineAssertions("Check detach action and caption are correct", () =>
				{
					AssertEquals(RemoveAction.Remove, innerGrid.RemoveAction);
					AssertEquals("Detach Booking", innerGrid.DeleteMenuItem.Text);
					AssertEquals("Menu item for Detach Booking is disabled", false, innerGrid.DeleteMenuItem.Enabled);
				});
			}
		}

		public void TestButtonsAreHidden()
		{
			using (var control = new TransportBookingsModuleButtonGrid())
			{
				AssertEquals(false, control.ShowAttachButton);
				AssertEquals(false, control.ShowDetachButton);
				AssertEquals(false, control.ShowEditButton);
				AssertEquals(false, control.ShowNewButton);
			}
		}

		public void TestPerformClickOnAttachButton()
		{
			using (var control = new TransportBookingsModuleButtonGrid())
			{
				control.Attaching += delegate(object sender, ModuleButtonGridOperationCancelEventArgs args)
				{
					args.Cancel = true;
					Assert("Attaching was fired.", true);
				};

				control.PerformClickOnAttachButton();
			}

			// Not worth unit testing Focus(), difficult to do and if it failed the impliciation is minimal.
		}

		public void TestPerformClickOnDetachButton()
		{
			using (var control = new TransportBookingsModuleButtonGrid())
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				control.PerformClickOnDetachButton();
				AssertEquals("Please select an item in the grid by first clicking in the left-hand gutter to highlight the whole row.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}

			// Not worth unit testing Focus(), difficult to do and if it failed the impliciation is minimal.
		}

		public void TestMessageBoxButtons()
		{
			using (var control = new TransportBookingsModuleButtonGrid())
			{
				AssertEquals("When confirming detach, message box buttons should be yes/no buttons", MessageBoxButtons.YesNo, control.MessageBoxButtons);
			}
		}

		public void TestDetachMessage()
		{
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			using (var form = new TransportBookingMultiForm(multiJobConsolidation))
			{
				form.Show();

				var detachMessage = form.transportBookingsControl.BookingsModuleButtonGrid.DetachMessage;
				CombineAssertions("WhenTransportBookingsModule is being used for a booking consolidation, the prompt message for detaching is not empty because of resource strings changing a caption into a full description", () =>
				{
					AssertEquals("Precondition: GridMode is TransportBookingsModuleButtonGridMode.ConsolidationMultiJob", TransportBookingsModuleButtonGridMode.ConsolidationMultiJob, form.transportBookingsControl.BookingsModuleButtonGrid.GridMode);
					AssertNotEquals("DetachMessage does not fail StringContentTest test TestFullDescriptionDiffersFromCaption because they're identical", detachMessage.FullDescription.ToUpperInvariant(), detachMessage.Caption.ToUpperInvariant());
					AssertNotEquals("DetachMessage does not fail StringContentTest test TestFullDescriptionDiffersFromCaption because FullDescription has an extra full stop", detachMessage.FullDescription.ToUpperInvariant(), detachMessage.Caption.ToUpperInvariant() + ".");
					AssertEquals("DetachMessage.Caption is informative", "Are you sure you want to detach the selected records? New records will be deleted.\r\nAll unsaved changes in detached items will be canceled.", detachMessage.Caption);
					AssertEquals("DetachMessage.FullDescription is informative, even though it has a trivial difference from Caption", "Are you sure you want to detach the selected records? New records will be deleted.\r\nAll unsaved changes in detached items will be canceled. ", detachMessage.FullDescription);
				});
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestDetachBookingFromMultiJobConsolidation()
		{
			var booking1 = Helper.CreateBooking();
			var booking2 = Helper.CreateBooking();
			var booking3 = Helper.CreateBooking();
			var booking4 = Helper.CreateBooking();
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking1);
			multiJobConsolidation.Bookings.Add(booking2);
			multiJobConsolidation.Bookings.Add(booking3);
			multiJobConsolidation.Bookings.Add(booking4);

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(multiJobConsolidation, accChargeCode, 250, 250);
			var jobHeader1 = new JobHeader.Loader(Factory, booking1).TryCreateWithoutMutexForTestOnly();
			var jobHeader2 = new JobHeader.Loader(Factory, booking2).TryCreateWithoutMutexForTestOnly();

			var charge1 = Helper.CreateCharge(jobHeader1, accChargeCode, consolCost.PK, 100, 100);
			var charge2 = Helper.CreateCharge(jobHeader2, accChargeCode, consolCost.PK, 150, 150);

			Factory.Save();

			using (var form = new TransportBookingMultiForm(multiJobConsolidation))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking3);
				form.transportBookingsControl.BookingsModuleButtonGrid.DetachSelectedElement();
				AssertContains(@"Are you sure you want to detach the selected records? New records will be deleted.
All unsaved changes in detached items will be canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have detached booking3.", 3, multiJobConsolidation.Bookings.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectAllElements();
				form.transportBookingsControl.BookingsModuleButtonGrid.DetachSelectedElement();
				AssertContains("Mentions booking1, booking2, but not booking4 in error message", $"Cannot detach the {booking1.HumanReadableName}, {booking2.HumanReadableName} from the {multiJobConsolidation.HumanReadableName} as posted/unposted apportionments exist on the consolidation.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have detached any bookings, even booking4.", 3, multiJobConsolidation.Bookings.Count);

				// currently, booking4 can be detached. Create an apportioned charge for it in another company, so it can't be.
				CreateConsolCostAndApportionToBookingInAnotherCompany(booking4.PK);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectAllElements();
				form.transportBookingsControl.BookingsModuleButtonGrid.DetachSelectedElement();
				AssertContains("Mentions booking4 as well as booking1 and booking2 in error message", $"Cannot detach the {booking1.HumanReadableName}, {booking2.HumanReadableName}, {booking4.HumanReadableName} from the {multiJobConsolidation.HumanReadableName} as posted/unposted apportionments exist on the consolidation.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have detached any bookings.", 3, multiJobConsolidation.Bookings.Count);
			}
		}

		public void TestDetachBookingFromMultiJobConsolidation_UsingDeleteMenuItem()
		{
			var booking1 = Helper.CreateBooking();
			var booking2 = Helper.CreateBooking();
			var booking3 = Helper.CreateBooking();
			var booking4 = Helper.CreateBooking();
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking1);
			multiJobConsolidation.Bookings.Add(booking2);
			multiJobConsolidation.Bookings.Add(booking3);
			multiJobConsolidation.Bookings.Add(booking4);

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(multiJobConsolidation, accChargeCode, 250, 250);
			var jobHeader1 = new JobHeader.Loader(Factory, booking1).TryCreateWithoutMutexForTestOnly();
			var jobHeader2 = new JobHeader.Loader(Factory, booking2).TryCreateWithoutMutexForTestOnly();

			var charge1 = Helper.CreateCharge(jobHeader1, accChargeCode, consolCost.PK, 100, 100);
			var charge2 = Helper.CreateCharge(jobHeader2, accChargeCode, consolCost.PK, 150, 150);

			Factory.Save();

			using (var form = new TransportBookingMultiForm(multiJobConsolidation))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking3);
				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.DeleteMenuItem.PerformClick();
				AssertContains(@"Are you sure you want to detach the selected records? New records will be deleted.
All unsaved changes in detached items will be canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have detached booking3.", 3, multiJobConsolidation.Bookings.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectAllElements();
				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.DeleteMenuItem.PerformClick();
				AssertContains("Mentions booking1, booking2, but not booking4 in error message", $"Cannot detach the {booking1.HumanReadableName}, {booking2.HumanReadableName} from the {multiJobConsolidation.HumanReadableName} as posted/unposted apportionments exist on the consolidation.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have detached any bookings, even booking4.", 3, multiJobConsolidation.Bookings.Count);
			}
		}

		public void TestDetachBookingFromMultiJobConsolidation_JobHeadersReferToShipment_ConsolCostingTabNotVisited_ConsolCostsApportioned_DoesNotAllowDetach()
		{
			var shipment = Helper.CreateForwardingShipment("S01", "HSB", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment); // Use this consolidation solely for creating the booking, and don't get it confused with multiJobConsolidation
			var booking = Helper.CreateBooking(consolidation);
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking);

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(multiJobConsolidation, accChargeCode, 100, 100);
			var jobHeader = new JobHeader.Loader(Factory, (IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();

			var charge = Helper.CreateCharge(jobHeader, accChargeCode, consolCost.PK, 100, 100);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			DtbChildEditableService.SetState(otherFactory, DtbChildEditableServiceState.Consolidation);

			var multiJobConsolidationOtherFactory = otherFactory.Load<DtbBookingConsolidation>(multiJobConsolidation.PK);
			var bookingOtherFactory = otherFactory.Load<DtbBooking>(booking.PK);

			using (var form = new TransportBookingMultiForm(multiJobConsolidationOtherFactory))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking);
				form.transportBookingsControl.BookingsModuleButtonGrid.DetachSelectedElement();
				AssertContains("Mentions booking in error message", $"Cannot detach the {bookingOtherFactory.HumanReadableName} from the {multiJobConsolidationOtherFactory.HumanReadableName} as posted/unposted apportionments exist on the consolidation.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have detached any bookings", 1, multiJobConsolidationOtherFactory.Bookings.Count);
			}
		}

		public void TestDetachBookingFromMultiJobConsolidation_JobHeaderRefersToBooking_NoJobConsolCost_AllowsDetach()
		{
			var booking1 = Helper.CreateBooking();
			var booking2 = Helper.CreateBooking();
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();
			multiJobConsolidation.Bookings.Add(booking1);
			multiJobConsolidation.Bookings.Add(booking2);

			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var consolCost = (BusinessObject)Helper.CreateConsolCost(multiJobConsolidation, accChargeCode, 100, 100);
			var jobHeader1 = new JobHeader.Loader(Factory, booking1).TryCreateWithoutMutexForTestOnly();
			var jobHeader2 = new JobHeader.Loader(Factory, booking2).TryCreateWithoutMutexForTestOnly();

			var charge1 = Helper.CreateCharge(jobHeader1, accChargeCode, consolCost.PK, 100, 100);
			// Don't create a charge against jobHeader2, which is associated with booking2

			Factory.Save();

			using (var form = new TransportBookingMultiForm(multiJobConsolidation))
			{
				form.Show();
				AssertEquals("Precondition", 2, multiJobConsolidation.Bookings.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking2);
				form.transportBookingsControl.BookingsModuleButtonGrid.DetachSelectedElement();
				AssertContains(@"Are you sure you want to detach the selected records? New records will be deleted.
All unsaved changes in detached items will be canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have detached booking", 1, multiJobConsolidation.Bookings.Count);
			}
		}

		public void TestDetachSubBookingFromMasterBooking()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var masterBooking = helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			setupFactory.Save();
			var subBooking1 = helper.CreateBooking();
			subBooking1.KM_KM_MasterBooking = masterBooking.PK;
			subBooking1.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBooking1.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking1.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;

			var subBooking2 = helper.CreateBooking();
			subBooking2.KM_KM_MasterBooking = masterBooking.PK;
			subBooking2.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBooking2.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking2.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;

			var subBooking3 = helper.CreateBooking();
			subBooking3.KM_KM_MasterBooking = masterBooking.PK;
			subBooking3.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBooking3.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking3.ConsolidationSingleJob.KB_MasterBookingVersion = new ZShort(32767);

			var subBooking4 = helper.CreateBooking();
			subBooking4.KM_KM_MasterBooking = masterBooking.PK;
			subBooking4.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBooking4.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking4.ConsolidationSingleJob.KB_MasterBookingVersion = new ZShort(32767);

			setupFactory.Save();
			var masterBookingPk = masterBooking.PK;
			masterBooking = Factory.Load<DtbBooking>(masterBookingPk);
			var subBooking1Pk = subBooking1.PK;
			subBooking1 = Factory.Load<DtbBooking>(subBooking1Pk);
			var subBooking2Pk = subBooking2.PK;
			subBooking2 = Factory.Load<DtbBooking>(subBooking2Pk);
			var subBooking3Pk = subBooking3.PK;
			subBooking3 = Factory.Load<DtbBooking>(subBooking3Pk);
			var subBooking4Pk = subBooking4.PK;
			subBooking4 = Factory.Load<DtbBooking>(subBooking4Pk);

			AssertEquals("Precondition: masterBooking starts with 4 sub bookings.", 4, masterBooking.SubBookings.Count);

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			using (var form = new TransportBookingForm(masterBooking))
			{
				form.Show();
				var moduleButtonGrid = form.InstructionViewsControl.SubTransportBookingsControl.BookingsModuleButtonGrid;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleButtonGrid.InnerGrid.SelectSingleElement(subBooking2);
				moduleButtonGrid.DetachSelectedElement();
				CombineAssertions("Check detach messages and that detach worked", () =>
				{
					AssertContains(@"All records selected will be detached from this Master Transport Booking. Please confirm this action.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should have detached booking2.", 3, masterBooking.SubBookings.Count);
					AssertEquals("Detached booking should not be deactivated", true, subBooking2.KM_IsActive);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				moduleButtonGrid.InnerGrid.SelectAllElements();
				moduleButtonGrid.DetachSelectedElement();
				CombineAssertions("Check detach messages and that failed detach did not make any changes", () =>
				{
					AssertContains("Mentions subBooking3 and subBooking4, but not booking1 in error message", FormattableString.Invariant($"Cannot detach the transport booking(s) {subBooking3.KM_JobID}, {subBooking4.KM_JobID} from the Master Transport Booking {masterBooking.KM_JobID} as they are not yet in sync.\r\nPlease reload the form and try again."), UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should not have detached any more bookings, even subBooking1.", 3, masterBooking.SubBookings.Count);
				});
			}
		}

		public void TestDetachSubBookingToMasterBookingWillUpdatePackageDivotsOnMaster()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);

			var masterBooking = helper.CreateBooking(isMaster: true);
			masterBooking.KM_KT_NKBookingTemplate = "ILDV";

			var subBooking1 = helper.CreateBooking();
			subBooking1.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking1.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			var subBookingPackage1 = helper.CreatePackage(ZString.Empty, weight: 100.0M, quantity: 1);
			subBooking1.PackageJob.Packages.Add(subBookingPackage1);

			var subBooking2 = helper.CreateBooking();
			var subBookingPackage2 = helper.CreatePackage(ZString.Empty, weight: 400.0M, quantity: 2);
			subBooking2.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking2.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			subBooking2.PackageJob.Packages.Add(subBookingPackage2);

			var subBooking3 = helper.CreateBooking();
			var subBookingPackage3 = helper.CreatePackage(ZString.Empty, weight: 900.0M, quantity: 3);
			subBooking3.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking3.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			subBooking3.PackageJob.Packages.Add(subBookingPackage3);

			var subBookings = new List<DtbBooking>()
			{
				subBooking1,
				subBooking2,
				subBooking3
			};

			masterBooking.SubBookings.AddRange(subBookings);

			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage1);
			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage2);
			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage3);
			setupFactory.Save();

			PerformQuickMasterBookingReplicationOrAlignment(
				masterBooking,
				new DtbBooking[] { subBooking1, subBooking2, subBooking3 },
				updateMasterFields: true);

			masterBooking.DefaultPackagesIfMasterFromSubBookingsForAllInstructions(subBookings);
			setupFactory.Save();

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking1 = Factory.Load<DtbBooking>(subBooking1.PK);
			subBooking2 = Factory.Load<DtbBooking>(subBooking2.PK);
			subBooking3 = Factory.Load<DtbBooking>(subBooking3.PK);

			var versionChecker = new DtbMasterBookingVersionChecker(masterBooking);
			Assert("Precondition: booking about to be detached is in sync with masterBooking", versionChecker.IsSubInSyncWithMaster(subBooking1));

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			using (var form = new TransportBookingForm(masterBooking))
			{
				form.Show();
				var moduleButtonGrid = form.InstructionViewsControl.SubTransportBookingsControl.BookingsModuleButtonGrid;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleButtonGrid.InnerGrid.SelectSingleElement(subBooking1);
				moduleButtonGrid.DetachSelectedElement();

				AssertContains(@"All records selected will be detached from this Master Transport Booking. Please confirm this action.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertSubBookingDetachedFromMasterBookingWithDivotsRestored(nameof(subBooking1), subBooking1, subBookingPackage1);
				AssertSubBookingStillAttachedToMasterBookingWithCorrectProxyDivots(nameof(subBooking2), subBooking2, masterBooking, subBookingPackage2);
				AssertSubBookingStillAttachedToMasterBookingWithCorrectProxyDivots(nameof(subBooking3), subBooking3, masterBooking, subBookingPackage3);
				AssertMasterBookingNoLongerHasDivots(nameof(subBooking1), masterBooking, subBookingPackage1);

				CombineAssertions("Master booking's sub bookings are incorrect", () =>
				{
					AssertCollectionNotContains($"Master booking should not have {nameof(subBooking1)} attached to it.", subBooking1, masterBooking.SubBookings);
					AssertCollectionContains($"Master booking is missing {nameof(subBooking2)}", subBooking2, masterBooking.SubBookings);
					AssertCollectionContains($"Master booking is missing {nameof(subBooking3)}", subBooking3, masterBooking.SubBookings);
				});
			}
		}

		public void TestDetachSubBookingToMasterBookingWillUpdatePackageDivotsOnMaster_ForAttachAndDetachWithoutSaving()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);

			var masterBooking = helper.CreateBooking(isMaster: true);
			masterBooking.KM_KT_NKBookingTemplate = "ILDV";

			var subBooking1 = helper.CreateBooking();
			subBooking1.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking1.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			var subBookingPackage1 = helper.CreatePackage(ZString.Empty, weight: 100.0M, quantity: 1);
			subBooking1.PackageJob.Packages.Add(subBookingPackage1);

			var subBooking2 = helper.CreateBooking();
			var subBookingPackage2 = helper.CreatePackage(ZString.Empty, weight: 400.0M, quantity: 2);
			subBooking2.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking2.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			subBooking2.PackageJob.Packages.Add(subBookingPackage2);

			var otherBooking = helper.CreateBooking();
			otherBooking.KM_KT_NKBookingTemplate = "ILDV";
			var otherBookingPackage = helper.CreatePackage(ZString.Empty, weight: 900.0M, quantity: 3);
			otherBooking.PackageJob.Packages.Add(otherBookingPackage);

			var alreadyAttachedSubBookings = new List<DtbBooking>()
			{
				subBooking1,
				subBooking2,
				// Do not attach otherBooking
			};

			masterBooking.SubBookings.AddRange(alreadyAttachedSubBookings);

			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage1);
			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage2);
			AssignDivotsForPackageOnEachInstructionInBooking(otherBooking, otherBookingPackage);
			setupFactory.Save();

			PerformQuickMasterBookingReplicationOrAlignment(
				masterBooking,
				new DtbBooking[] { subBooking1, subBooking2 },
				updateMasterFields: true);

			masterBooking.DefaultPackagesIfMasterFromSubBookingsForAllInstructions(alreadyAttachedSubBookings);
			setupFactory.Save();

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking1 = Factory.Load<DtbBooking>(subBooking1.PK);
			subBooking2 = Factory.Load<DtbBooking>(subBooking2.PK);
			otherBooking = Factory.Load<DtbBooking>(otherBooking.PK);

			AssertEquals("Precondition: booking about to be attached and detached is not a sub booking", false, otherBooking.IsSub);

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			using (var form = new TransportBookingForm(masterBooking))
			{
				form.Show();
				var moduleButtonGrid = form.InstructionViewsControl.SubTransportBookingsControl.BookingsModuleButtonGrid;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleButtonGrid.PerformClickOnAttachButton();
				using (var popup = moduleButtonGrid.LastShownAttachPopupForTesting)
				{
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new BusinessObject[] { otherBooking });
				}
				AssertEquals("Precondition: Should have attached otherBooking to masterBooking", masterBooking.PK, otherBooking.KM_KM_MasterBooking);
				AssertEquals("Precondition: otherBooking is not in sync with masterBooking", (short)0, otherBooking.KM_MasterBookingVersion);

				moduleButtonGrid.InnerGrid.SelectSingleElement(otherBooking);
				moduleButtonGrid.DetachSelectedElement();
				CombineAssertions("Precondition: Check detach messages and that detach worked", () =>
				{
					AssertContains("Should allow detach of otherBooking even though it's not in sync with masterBooking, as its attachment has not yet been saved", @"All records selected will be detached from this Master Transport Booking. Please confirm this action.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should have detached otherBooking.", 2, masterBooking.SubBookings.Count);
				});

				AssertSubBookingStillAttachedToMasterBookingWithCorrectProxyDivots(nameof(subBooking1), subBooking1, masterBooking, subBookingPackage1);
				AssertSubBookingStillAttachedToMasterBookingWithCorrectProxyDivots(nameof(subBooking2), subBooking2, masterBooking, subBookingPackage2);
				AssertSubBookingDetachedFromMasterBookingWithDivotsRestored(nameof(otherBooking), otherBooking, otherBookingPackage);
				AssertMasterBookingNoLongerHasDivots(nameof(otherBooking), masterBooking, otherBookingPackage);

				CombineAssertions("Master booking's sub bookings are incorrect", () =>
				{
					AssertCollectionContains($"Master booking is missing {nameof(subBooking1)}", subBooking1, masterBooking.SubBookings);
					AssertCollectionContains($"Master booking is missing {nameof(subBooking2)}", subBooking2, masterBooking.SubBookings);
					AssertCollectionNotContains($"Master booking should not have {nameof(otherBooking)} attached to it.", otherBooking, masterBooking.SubBookings);
				});
			}
		}

		public void TestDetachSubBookingToMasterBookingDoesNotUpdatePackageDivotsOnMasterWhenCancellationIsNotConfirmed()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);

			var masterBooking = helper.CreateBooking(isMaster: true);
			masterBooking.KM_KT_NKBookingTemplate = "ILDV";

			var subBooking1 = helper.CreateBooking();
			subBooking1.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking1.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			var subBookingPackage1 = helper.CreatePackage(ZString.Empty, weight: 100.0M, quantity: 1);
			subBooking1.PackageJob.Packages.Add(subBookingPackage1);

			var subBooking2 = helper.CreateBooking();
			var subBookingPackage2 = helper.CreatePackage(ZString.Empty, weight: 400.0M, quantity: 2);
			subBooking2.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking2.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			subBooking2.PackageJob.Packages.Add(subBookingPackage2);

			var subBooking3 = helper.CreateBooking();
			var subBookingPackage3 = helper.CreatePackage(ZString.Empty, weight: 900.0M, quantity: 3);
			subBooking3.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking3.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			subBooking3.PackageJob.Packages.Add(subBookingPackage3);

			var subBookings = new List<DtbBooking>()
			{
				subBooking1,
				subBooking2,
				subBooking3
			};

			masterBooking.SubBookings.AddRange(subBookings);

			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage1);
			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage2);
			AssignDivotsForPackageOnEachInstructionInBooking(masterBooking, subBookingPackage3);
			setupFactory.Save();

			PerformQuickMasterBookingReplicationOrAlignment(
				masterBooking,
				new DtbBooking[] { subBooking1, subBooking2, subBooking3 },
				updateMasterFields: true);

			masterBooking.DefaultPackagesIfMasterFromSubBookingsForAllInstructions(subBookings);
			setupFactory.Save();

			masterBooking = Factory.Load<DtbBooking>(masterBooking.PK);
			subBooking1 = Factory.Load<DtbBooking>(subBooking1.PK);
			subBooking2 = Factory.Load<DtbBooking>(subBooking2.PK);
			subBooking3 = Factory.Load<DtbBooking>(subBooking3.PK);

			var versionChecker = new DtbMasterBookingVersionChecker(masterBooking);
			Assert("Precondition: subBooking2 is in sync with masterBooking", versionChecker.IsSubInSyncWithMaster(subBooking1));

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			using (var form = new TransportBookingForm(masterBooking))
			{
				form.Show();
				var moduleButtonGrid = form.InstructionViewsControl.SubTransportBookingsControl.BookingsModuleButtonGrid;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				moduleButtonGrid.InnerGrid.SelectSingleElement(subBooking1);
				moduleButtonGrid.DetachSelectedElement();

				AssertContains(@"All records selected will be detached from this Master Transport Booking. Please confirm this action.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertSubBookingStillAttachedToMasterBookingWithCorrectProxyDivots(nameof(subBooking1), subBooking1, masterBooking, subBookingPackage1);
				AssertSubBookingStillAttachedToMasterBookingWithCorrectProxyDivots(nameof(subBooking2), subBooking2, masterBooking, subBookingPackage2);
				AssertSubBookingStillAttachedToMasterBookingWithCorrectProxyDivots(nameof(subBooking3), subBooking3, masterBooking, subBookingPackage3);

				CombineAssertions("Master booking's sub bookings are incorrect", () =>
				{
					AssertCollectionContains($"Master booking is missing {nameof(subBooking1)}", subBooking1, masterBooking.SubBookings);
					AssertCollectionContains($"Master booking is missing {nameof(subBooking2)}", subBooking2, masterBooking.SubBookings);
					AssertCollectionContains($"Master booking is missing {nameof(subBooking3)}", subBooking3, masterBooking.SubBookings);
				});
			}
		}

		void AssignDivotsForPackageOnEachInstructionInBooking(DtbBooking booking, PkgPackage package)
		{
			foreach (var instruction in booking.Instructions)
			{
				var packageDivot = instruction.PackageDivots.AddNew();
				packageDivot.KD_KP_Package = package.PK;
				packageDivot.KD_Quantity = package.KP_PackageQty;
			}
		}

		void AssignDivotsToInstructions(DtbBookingInstruction instruction, (ZGuid packagePK, ZInt packageQty)[] divotDetails)
		{
			instruction.PackageDivots.DeleteAll();
			foreach (var divotDetail in divotDetails)
			{
				var packageDivot = instruction.PackageDivots.AddNew();
				packageDivot.KD_KP_Package = divotDetail.packagePK;
				packageDivot.KD_Quantity = divotDetail.packageQty;
			}
		}

		void AssertSubBookingDetachedFromMasterBookingWithDivotsRestored(string subBookingName, DtbBooking subBooking, PkgPackage package)
		{
			CombineAssertions($"{subBookingName} appears to be invalid", () => {
				Assert($"{subBookingName} no longer has its package", subBooking.PackageJob.Packages.Contains(package));
				AssertEquals($"{subBookingName} should only have one package", 1, subBooking.PackageJob.Packages.Count);
				Assert($"{subBookingName} no longer has its divots", subBooking.Instructions.All(i => i.PackageDivots.Any(pv => pv.Package.PK == package.PK)));
				Assert($"{subBookingName} should only have divots from its package", subBooking.Instructions.All(i => i.PackageDivots.All(pv => pv.Package.PK == package.PK)));
				Assert($"{subBookingName} should only have one package divot per instruction", subBooking.Instructions.All(i => i.PackageDivots.Count == 1));
				Assert($"{subBookingName} Should only have two instruction", subBooking.Instructions.Count == 2);
				Assert($"{subBookingName} should not be associated with a master booking", subBooking.MasterBooking == null);
			});
		}

		void AssertMasterBookingNoLongerHasDivots(string subBookingName, DtbBooking masterBooking, PkgPackage package)
		{
			CombineAssertions($"{subBookingName} appears to be invalid", () =>
			{
				AssertEquals($"{subBookingName}'s package should not be on the master booking", false, masterBooking.PackageJob.Packages.Contains(package));
				AssertEquals($"{subBookingName}'s divots should not be on the master booking", false, masterBooking.Instructions.Any(i => i.PackageDivots.Any(pv => pv.Package.PK == package.PK)));
			});
		}

		void AssertSubBookingStillAttachedToMasterBookingWithCorrectProxyDivots(string subBookingName, DtbBooking subBooking, DtbBooking masterBooking, PkgPackage package)
		{
			CombineAssertions($"{subBookingName} appears to be invalid", () =>
			{
				Assert($"{subBookingName} no longer has its package", subBooking.PackageJob.Packages.Contains(package));
				Assert($"{subBookingName} no longer has its proxy divots", subBooking.Instructions.All(i => i.DivotsWithPackages.Any(pv => pv.Package.PK == package.PK)));
				Assert($"{subBookingName} should only have two instruction", subBooking.Instructions.Count == 2);
				Assert($"{subBookingName} is no longer associated with the master booking", subBooking.MasterBooking == masterBooking);
			});
		}

		public void TestAttachSubBookingToMasterBookingWillUpdatePackageDivotsOnMaster()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;

			masterBooking.KM_KT_NKBookingTemplate = "EFPL";
			Helper.AssertEFPLTemplateHasFourInstructionsWithSpecificPackageCategories(isPreConditionAssert: true);
			AssertEquals("Precondition: Should now have four instructions from template", 4, masterBooking.Instructions.Count);
			var extraInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			extraInstruction.KN_Sequence = 5;

			var subBooking1 = Helper.CreateBooking();
			var subBooking1Container1 = Helper.CreatePackageContainer("CONT1");
			var subBooking1Container1InnerLoosePackage = Helper.CreatePackage("CONT1_IN");
			var subBooking1Container1InnerInnerLoosePackage = Helper.CreatePackage("CONT1_IN_IN");
			subBooking1.PackageJob.Packages.Add(subBooking1Container1);
			subBooking1Container1.Packages.Add(subBooking1Container1InnerLoosePackage);
			subBooking1Container1InnerLoosePackage.Packages.Add(subBooking1Container1InnerInnerLoosePackage);
			var subBooking1Container2 = Helper.CreatePackageContainer("CONT2");
			var subBooking1Container2InnerLoosePackage = Helper.CreatePackage("CONT2_IN");
			var subBooking1Container2InnerInnerLoosePackage = Helper.CreatePackage("CONT2_IN_IN");
			subBooking1.PackageJob.Packages.Add(subBooking1Container2);
			subBooking1Container2.Packages.Add(subBooking1Container2InnerLoosePackage);
			subBooking1Container2InnerLoosePackage.Packages.Add(subBooking1Container2InnerInnerLoosePackage);

			var subBooking2 = Helper.CreateBooking();
			var subBooking2LoosePackage = Helper.CreatePackage("LOOSE3", 1);
			subBooking2.PackageJob.Packages.Add(subBooking2LoosePackage);

			PerformQuickMasterBookingReplicationOrAlignment(
				masterBooking,
				new DtbBooking[] { subBooking1, subBooking2 },
				updateMasterFields: false);

			var masterInstructionPic1 = masterBooking.Instructions.First(i => i.KN_Sequence == 1);
			var masterInstructionPic2 = masterBooking.Instructions.First(i => i.KN_Sequence == 2);
			var masterInstructionMlt3 = masterBooking.Instructions.First(i => i.KN_Sequence == 3);
			var masterInstructionDlv4 = masterBooking.Instructions.First(i => i.KN_Sequence == 4);
			var masterInstructionDlv5 = masterBooking.Instructions.First(i => i.KN_Sequence == 5);

			var allSubBookings = new BusinessObject[] { subBooking1, subBooking2 };
			var expectedSubBookingPKs = allSubBookings.Select(x => x.PK);

			var expectedLoosePackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container1InnerLoosePackage.PK, packageQty: subBooking1Container1InnerLoosePackage.KP_PackageQty),
					//intentionally missing subBooking1Container2InnerLoosePackage to check unassigned packages on a sub are not assigned onto the master
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
			};

			var expectedContainerPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container1.PK, packageQty: subBooking1Container1.KP_PackageQty),
					//intentionally missing subBooking1Container2 to check unassigned packages on a sub are not assigned onto the master
			};

			var expectedBothPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container1.PK, packageQty: subBooking1Container1.KP_PackageQty),
					(packagePK: subBooking1Container1InnerLoosePackage.PK, packageQty: subBooking1Container1InnerLoosePackage.KP_PackageQty),
					//intentionally missing subBooking1Container2 and subBooking1Container2InnerLoosePackage to check unassigned packages on a sub are not assigned onto the master
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
			};

			var expectedOuterPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
					(packagePK: subBooking1Container1.PK, packageQty: subBooking1Container1.KP_PackageQty),
					(packagePK: subBooking1Container1InnerLoosePackage.PK, packageQty: subBooking1Container1InnerLoosePackage.KP_PackageQty), // DefaultPackagesCore code for addTopLevelLoose true doesn't check if a package is top level
					//intentionally missing subBooking1Container2 and subBooking1Container2InnerLoosePackage to check unassigned packages on a sub are not assigned onto the master
					(packagePK: subBooking2LoosePackage.PK, packageQty: subBooking2LoosePackage.KP_PackageQty),
			};

			var subBooking1LoosePackages = expectedLoosePackages.Where(lp => lp.packagePK != subBooking2LoosePackage.PK).ToArray();
			var subBooking1ContainerPackages = expectedContainerPackages.Where(lp => lp.packagePK != subBooking2LoosePackage.PK).ToArray();
			var subBooking1BothPackages = expectedBothPackages.Where(lp => lp.packagePK != subBooking2LoosePackage.PK).ToArray();
			var subBooking1OuterPackages = expectedOuterPackages.Where(lp => lp.packagePK != subBooking2LoosePackage.PK && lp.packagePK != subBooking1Container1InnerLoosePackage.PK).ToArray();

			var subBooking2LoosePackages = expectedLoosePackages.Where(lp => lp.packagePK != subBooking1Container1.PK && lp.packagePK != subBooking1Container1InnerLoosePackage.PK).ToArray();
			var subBooking2ContainerPackages = expectedLoosePackages.Where(lp => lp.packagePK != subBooking1Container1.PK && lp.packagePK != subBooking1Container1InnerLoosePackage.PK).ToArray();
			var subBooking2BothPackages = expectedLoosePackages.Where(lp => lp.packagePK != subBooking1Container1.PK && lp.packagePK != subBooking1Container1InnerLoosePackage.PK).ToArray();
			var subBooking2OuterPackages = expectedLoosePackages.Where(lp => lp.packagePK != subBooking1Container1.PK && lp.packagePK != subBooking1Container1InnerLoosePackage.PK).ToArray();

			var subBooking1InstructionPic1 = subBooking1.Instructions.First(i => i.KN_Sequence == 1);
			var subBooking1InstructionPic2 = subBooking1.Instructions.First(i => i.KN_Sequence == 2);
			var subBooking1InstructionMlt3 = subBooking1.Instructions.First(i => i.KN_Sequence == 3);
			var subBooking1InstructionDlv4 = subBooking1.Instructions.First(i => i.KN_Sequence == 4);
			var subBooking1InstructionDlv5 = subBooking1.Instructions.First(i => i.KN_Sequence == 5);

			var subBooking2InstructionPic1 = subBooking2.Instructions.First(i => i.KN_Sequence == 1);
			var subBooking2InstructionPic2 = subBooking2.Instructions.First(i => i.KN_Sequence == 2);
			var subBooking2InstructionMlt3 = subBooking2.Instructions.First(i => i.KN_Sequence == 3);
			var subBooking2InstructionDlv4 = subBooking2.Instructions.First(i => i.KN_Sequence == 4);
			var subBooking2InstructionDlv5 = subBooking2.Instructions.First(i => i.KN_Sequence == 5);

			AssignDivotsToInstructions(subBooking1InstructionPic1, subBooking1LoosePackages);
			AssignDivotsToInstructions(subBooking1InstructionPic2, subBooking1ContainerPackages);
			AssignDivotsToInstructions(subBooking1InstructionMlt3, subBooking1BothPackages);
			AssignDivotsToInstructions(subBooking1InstructionDlv4, subBooking1ContainerPackages);
			AssignDivotsToInstructions(subBooking1InstructionDlv5, subBooking1OuterPackages);

			AssignDivotsToInstructions(subBooking2InstructionPic1, subBooking2LoosePackages);
			AssignDivotsToInstructions(subBooking2InstructionPic2, subBooking2ContainerPackages);
			AssignDivotsToInstructions(subBooking2InstructionMlt3, subBooking2BothPackages);
			AssignDivotsToInstructions(subBooking2InstructionDlv4, subBooking2ContainerPackages);
			AssignDivotsToInstructions(subBooking2InstructionDlv5, subBooking2OuterPackages);

			Factory.Save();

			AssertEquals("Precondition: No subs attached to master yet", 0, masterBooking.SubBookings.Count);

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			using (var form = new TransportBookingForm(masterBooking))
			{
				form.Show();
				var moduleButtonGrid = form.InstructionViewsControl.SubTransportBookingsControl.BookingsModuleButtonGrid;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				moduleButtonGrid.PerformClickOnAttachButton();
				using (var popup = moduleButtonGrid.LastShownAttachPopupForTesting)
				{
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(allSubBookings);
				}
				CombineAssertions("Should have attached subBooking1 and subBooking2 correctly", () =>
				{
					AssertContainsExactElementsInAnyOrder("Should have attached subBooking1 and subBooking2 to masterBooking", expectedSubBookingPKs, masterBooking.SubBookings.Select(b => b.PK));
					AssertMasterInstructionHasCorrectPackageCategoryAndDivots(masterInstructionPic1, PackageCategories.Codes.Loose, PackageCategories.Descriptions.Loose, expectedLoosePackages);
					AssertMasterInstructionHasCorrectPackageCategoryAndDivots(masterInstructionPic2, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
					AssertMasterInstructionHasCorrectPackageCategoryAndDivots(masterInstructionMlt3, PackageCategories.Codes.Both, PackageCategories.Descriptions.Both, expectedBothPackages);
					AssertMasterInstructionHasCorrectPackageCategoryAndDivots(masterInstructionDlv4, PackageCategories.Codes.Containers, PackageCategories.Descriptions.Containers, expectedContainerPackages);
					AssertMasterInstructionHasCorrectPackageCategoryAndDivots(masterInstructionDlv5, PackageCategories.Codes.Outers, PackageCategories.Descriptions.Outers, expectedOuterPackages);
				});
			}
		}

		void AssertMasterInstructionHasCorrectPackageCategoryAndDivots(
			DtbBookingInstruction instruction,
			string expectedPackageCategoryCode,
			string expectedPackageCategoryDescription,
			(ZGuid packagePK, ZInt packageQty)[] expectedDivotDetails)
		{
			AssertEquals(
				FormattableString.Invariant($"Should have Instruction {instruction.KN_InstructionType} Sequence {instruction.KN_Sequence} PackageCategory '{expectedPackageCategoryCode}'"),
				expectedPackageCategoryCode,
				instruction.PackageCategory);
			AssertContainsExactElementsInAnyOrder(FormattableString.Invariant($"Should have {expectedPackageCategoryDescription} packages on Instruction {instruction.KN_InstructionType} Sequence {instruction.KN_Sequence}"),
				expectedDivotDetails,
				instruction.DivotsWithPackages.Select(d => (packagePK: d.KD_KP_Package, packageQty: d.KD_Quantity)).ToArray());
		}

		void PerformQuickMasterBookingReplicationOrAlignment(DtbBooking masterBooking, DtbBooking[] subBookings, bool updateMasterFields)
		{
			foreach (var subBooking in subBookings)
			{
				PerformQuickMasterBookingReplicationOrAlignment(masterBooking, subBooking, updateMasterFields);
			}
		}

		void PerformQuickMasterBookingReplicationOrAlignment(DtbBooking masterBooking, DtbBooking subBooking, bool updateMasterFields)
		{
			subBooking.KM_KT_NKBookingTemplate = masterBooking.KM_KT_NKBookingTemplate;
			Array.ForEach(subBooking.Instructions.ToArray(), i => i.Delete());
			foreach (var masterInstruction in masterBooking.Instructions)
			{
				var subInstruction = subBooking.Instructions.AddNew(masterInstruction.KN_InstructionType, masterInstruction.OrganisationType);
				subInstruction.KN_Sequence = masterInstruction.KN_Sequence;
				subInstruction.KN_DropMode = masterInstruction.KN_DropMode;
				subInstruction.KN_Status = masterInstruction.KN_Status;
				subInstruction.KN_IsContainerRateable = masterInstruction.KN_IsContainerRateable;
				subInstruction.KN_IsLooseRateable = masterInstruction.KN_IsLooseRateable;
				subInstruction.KN_ServiceInstruction = masterInstruction.KN_ServiceInstruction;
				subInstruction.KN_IsAuthorisedToLeave = masterInstruction.KN_IsAuthorisedToLeave;
				subInstruction.KN_RQ_Equipment = masterInstruction.KN_RQ_Equipment;
				subInstruction.KN_TZ_DomesticZone = masterInstruction.KN_TZ_DomesticZone;
				if (updateMasterFields)
				{
					subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
					subInstruction.KN_MasterBookingVersion = masterInstruction.KN_MasterBookingVersion;
				}

				foreach (var masterConfirmation in masterInstruction.Confirmations)
				{
					var subConfirmation = subInstruction.Confirmations.AddNew(masterConfirmation.KK_ConfirmationType);
					if (updateMasterFields)
					{
						subConfirmation.KK_KK_MasterBookingConfirmation = masterConfirmation.PK;
						subConfirmation.KK_MasterBookingVersion = masterConfirmation.KK_MasterBookingVersion;
					}
				}
			}
			subBooking.KM_Direction = masterBooking.KM_Direction;
			subBooking.KM_RatingFreightMode = masterBooking.KM_RatingFreightMode;
			if (updateMasterFields)
			{
				subBooking.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
				subBooking.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			}
			Factory.Save();
		}

		public void TestDetachSubBooking_BookingStatusNotAvailableOrHeld()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var masterBooking = helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			setupFactory.Save();
			var subBooking1 = helper.CreateBooking();
			subBooking1.KM_KM_MasterBooking = masterBooking.PK;
			subBooking1.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBooking1.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking1.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;

			setupFactory.Save();

			masterBooking.KM_Status = subBooking1.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			setupFactory.Save();

			subBooking1.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			setupFactory.Save();

			var masterBookingPk = masterBooking.PK;
			masterBooking = Factory.Load<DtbBooking>(masterBookingPk);
			var subBooking1Pk = subBooking1.PK;
			subBooking1 = Factory.Load<DtbBooking>(subBooking1Pk);

			AssertEquals("Precondition: masterBooking starts with 1 sub bookings.", 1, masterBooking.SubBookings.Count);

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			using (var form = new TransportBookingForm(masterBooking))
			{
				form.Show();
				var moduleButtonGrid = form.InstructionViewsControl.SubTransportBookingsControl.BookingsModuleButtonGrid;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				moduleButtonGrid.InnerGrid.SelectSingleElement(subBooking1);
				moduleButtonGrid.DetachSelectedElement();
				CombineAssertions("Check detach message and behavior when booking status is not available or held", () =>
				{
					AssertContains(@"This Master TB has already commenced. Detaching records now will result in the deactivation of the selected records.
Alternatively, remove Actual Pickup/Delivery dates to reset the status to Available and then detach without deactivating the selected records.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should have detached booking.", 0, masterBooking.SubBookings.Count);
					AssertEquals("Detached booking should be deactivated", false, subBooking1.KM_IsActive);
				});
			}
		}

		public void TestDetachSubBooking_BookingStatusNotAvailableOrHeld_DetachRejected()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var masterBooking = helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			setupFactory.Save();
			var subBooking1 = helper.CreateBooking();
			subBooking1.KM_KM_MasterBooking = masterBooking.PK;
			subBooking1.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBooking1.ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = masterBooking.ConsolidationSingleJob.PK;
			subBooking1.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;

			setupFactory.Save();

			masterBooking.KM_Status = subBooking1.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			setupFactory.Save();

			subBooking1.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			setupFactory.Save();

			var masterBookingPk = masterBooking.PK;
			masterBooking = Factory.Load<DtbBooking>(masterBookingPk);
			var subBooking1Pk = subBooking1.PK;
			subBooking1 = Factory.Load<DtbBooking>(subBooking1Pk);

			AssertEquals("Precondition: masterBooking starts with 1 sub bookings.", 1, masterBooking.SubBookings.Count);

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);

			using (var form = new TransportBookingForm(masterBooking))
			{
				form.Show();
				var moduleButtonGrid = form.InstructionViewsControl.SubTransportBookingsControl.BookingsModuleButtonGrid;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				moduleButtonGrid.InnerGrid.SelectSingleElement(subBooking1);
				moduleButtonGrid.DetachSelectedElement();
				CombineAssertions("Check detach message and behavior when booking status is not available or held, and user decides not to detach", () =>
				{
					AssertContains(@"This Master TB has already commenced. Detaching records now will result in the deactivation of the selected records.
Alternatively, remove Actual Pickup/Delivery dates to reset the status to Available and then detach without deactivating the selected records.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should not have detached booking.", 1, masterBooking.SubBookings.Count);
					AssertEquals("Booking should not be deactivated", true, subBooking1.KM_IsActive);
				});
			}
		}

		public void TestBindingNewDataSourceChangesGridModeAndMasterBookingProperties()
		{
			var multiBookingConsolidation = Helper.CreateConsolidation();
			var masterBooking1 = Helper.CreateBooking();
			masterBooking1.KM_IsMaster = true;
			masterBooking1.KM_KB_BookingConsolidationMultiJob = multiBookingConsolidation.PK;
			Factory.Save();

			using (var multiForm = new TransportBookingMultiForm(multiBookingConsolidation))
			{
				var control = multiForm.transportBookingsControl.BookingsModuleButtonGrid;
				CombineAssertions("Should set GridMode correctly for DataSource of booking consolidation", () =>
				{
					AssertEquals("If DataSource is set to consolidation then should set grid mode to ConsolidationMultiJob", TransportBookingsModuleButtonGridMode.ConsolidationMultiJob, control.GridMode);
					AssertNull("If DataSource is set to consolidation then should set MasterBooking to null", control.MasterBooking);
				});

				control.SetDataBinding(masterBooking1, nameof(DtbBooking.SubBookings));
				CombineAssertions("Should set GridMode correctly for DataSource to masterBooking1 after re-binding DataSource", () =>
				{
					AssertEquals("If DataSource is set to booking then should set grid mode to MasterBooking", TransportBookingsModuleButtonGridMode.MasterBooking, control.GridMode);
					AssertEquals("If DataSource is set to masterBooking1 then should set MasterBooking to masterBooking1", masterBooking1.PK, control.MasterBooking.PK);
				});
			}
		}

		public static void CreateConsolCostAndApportionToBookingInAnotherCompany(ZGuid bookingPK)
		{
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var newHelper = new TransportBookingTestHelper(newFactory);
			var branchInAnotherCompany = newFactory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var anotherCompany = branchInAnotherCompany.Company;
			anotherCompany.SetCurrency(GlbCompany.CurrentCompany.LocalCurrency.Code);

			var booking = newFactory.Load<DtbBooking>(bookingPK);
			var accChargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			accChargeCode.AC_Code = "AAAAA";
			accChargeCode.AC_GC = anotherCompany.PK;
			newFactory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchInAnotherCompany.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var consolCost = (BusinessObject)newHelper.CreateConsolCost(booking.ConsolidationMultiJob, accChargeCode, 250, 250, anotherCompany, booking.KM_JobID);
				var jobHeader = new JobHeader.Loader(newFactory, booking).TryCreateWithoutMutexForTestOnly(branchInAnotherCompany);
				jobHeader.JH_GC = anotherCompany.PK;
				var charge = newHelper.CreateCharge(jobHeader, accChargeCode, consolCost.PK, 250, 250, branchInAnotherCompany);
			}
			newFactory.Save();
		}

		public static void CreateConsolCostAndApportionToBookingAttachedToMasterBookingInAnotherCompany(ZGuid bookingPK)
		{
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var newHelper = new TransportBookingTestHelper(newFactory);
			var branchInAnotherCompany = newFactory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var anotherCompany = branchInAnotherCompany.Company;
			anotherCompany.SetCurrency(GlbCompany.CurrentCompany.LocalCurrency.Code);

			var booking = newFactory.Load<DtbBooking>(bookingPK);
			var accChargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			accChargeCode.AC_Code = "AAAAA";
			accChargeCode.AC_GC = anotherCompany.PK;
			newFactory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchInAnotherCompany.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var consolCost = (BusinessObject)newHelper.CreateConsolCost(booking.MasterBooking, accChargeCode, 250, 250, anotherCompany, booking.KM_JobID);
				var jobHeader = new JobHeader.Loader(newFactory, booking).TryCreateWithoutMutexForTestOnly(branchInAnotherCompany);
				jobHeader.JH_GC = anotherCompany.PK;
				var charge = newHelper.CreateCharge(jobHeader, accChargeCode, consolCost.PK, 250, 250, branchInAnotherCompany);
			}
			newFactory.Save();
		}

		protected override BusinessObjectFactory NewFactory()
		{
			var factory = base.NewFactory();
			DtbChildEditableService.SetState(factory, DtbChildEditableServiceState.Consolidation);
			return factory;
		}
	}
}
