using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.GUI.Testing
{
	public abstract class TransportBookingMultiFormTest : DtbBookingZFormBasherTest
	{
		protected abstract DtbBookingConsolidation GetNewConsolidation();
		protected abstract bool NewActionEnabled { get; }
		protected abstract ControllerID ControllerID { get; }

		public void TestConstruction_PlugIns()
		{
			var consolidation = GetNewConsolidation();
			using (var form = new TransportBookingMultiForm(consolidation))
			{
				AssertEquals(true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.IsAdditionalReferencesVisible);
				if (consolidation.IsMultiBooking)
				{
					AssertNull(form.PlugIns.GetPlugIn(ControllerIDs.PackingPlugIn));
					AssertNull(form.PlugIns.GetPlugIn(ControllerIDs.Routing));
					AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.Apportionment));
				}
				else
				{
					AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.PackingPlugIn));
					AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.Routing));
					AssertNull(form.PlugIns.GetPlugIn(ControllerIDs.Apportionment));
				}
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		public void TestDelete()
		{
			// blank, but cancel
			var consolidation = GetNewConsolidation();
			using (var form = new TransportBookingMultiForm(consolidation))
			{
				var bookingButCancel = Helper.CreateBooking(consolidation);

				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				bookingButCancel.Delete();
				AssertEquals("Should have no affect", true, bookingButCancel.IsDeleted);
			}

			// with package,  but cancel
			consolidation = Helper.CreateConsolidation();
			using (var form = new TransportBookingMultiForm(consolidation))
			{
				var bookingWithPackagesButCancel = consolidation.Bookings.AddNew();

				form.Show();

				var pickupInstruction = bookingWithPackagesButCancel.Instructions.AddNew(InstructionTypes.Codes.PickUp);
				var box = consolidation.PackageJob.Packages.AddNew("BOX");
				var divot = Helper.CreatePackageDivot(pickupInstruction, box, 1);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				bookingWithPackagesButCancel.Delete();
				AssertEquals(false, bookingWithPackagesButCancel.IsDeleted);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				bookingWithPackagesButCancel.Delete();
				AssertEquals(true, bookingWithPackagesButCancel.IsDeleted);
			}

			// with dates and reference, but cancel
			consolidation = Helper.CreateConsolidation();
			using (var form = new TransportBookingMultiForm(consolidation))
			{
				var bookingWithDatesAndReferencesButCancel = consolidation.Bookings.AddNew();

				form.Show();

				var pickupInstruction = bookingWithDatesAndReferencesButCancel.Instructions.AddNew(InstructionTypes.Codes.PickUp);
				pickupInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = ZDateTime.Now.AddDays(1);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				bookingWithDatesAndReferencesButCancel.Delete();
				AssertEquals(false, bookingWithDatesAndReferencesButCancel.IsDeleted);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				bookingWithDatesAndReferencesButCancel.Delete();
				AssertEquals(true, bookingWithDatesAndReferencesButCancel.IsDeleted);
			}
		}

		public void TestReadOnlyStatusOfAttachedBookings_IsUpdatedOnFormLoad()
		{
			var consolidation = Helper.CreateConsolidation();

			var booking1 = Helper.CreateBooking(consolidation);
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking1);
			booking1.KM_Status = TransportStatuses.Codes.ServiceCommenced;

			var booking2 = Helper.CreateBooking(consolidation);
			Helper.AssignBookingToAnAuthorisedCarrierBookingAgent(booking2);
			booking2.KM_Status = TransportStatuses.Codes.ServiceCommenced;

			AssertEquals("Precondition: booking1 should be being managed by an authorised Carrier Booking Agent.", true, booking1.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);
			AssertEquals("Precondition: booking2 should be being managed by an authorised Carrier Booking Agent.", true, booking2.BookingIsBeingManagedByAuthorisedCarrierBookingAgent);

			AssertEquals("Precondition: booking1 should not have been updated to be ReadOnly yet.", false, booking1.ReadOnly);
			AssertEquals("Precondition: booking2 should not have been updated to be ReadOnly yet.", false, booking2.ReadOnly);

			using (new TransportBookingMultiForm(consolidation))
			{
				CombineAssertions(() =>
				{
					AssertEquals("booking1 should have been updated to be ReadOnly.", true, booking1.ReadOnly);
					AssertEquals("booking2 should have been updated to be ReadOnly.", true, booking2.ReadOnly);
				});
			}
		}

		public void TestSetView()
		{
			var consolidation = Helper.CreateConsolidation();
			var packingJob = Helper.CreatePackageJob(consolidation);
			var container1 = packingJob.Packages.AddNew("CNT");
			var container2 = packingJob.Packages.AddNew("CNT");
			var pallet1 = container1.Packages.AddNew("PLT");
			var pallet2 = container2.Packages.AddNew("PLT");

			var booking = Helper.CreateBooking(consolidation);
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Show();
				form.SetView(TransportBookingInstructionView.Standard);
				AssertEquals(form.transportBookingsControl.DtbBookingInstructionsViewsPanel.StandardViewTabPage, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.ViewTabControl.SelectedTab);

				form.SetView(TransportBookingInstructionView.Instruction);
				AssertEquals(form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewTabPage, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.ViewTabControl.SelectedTab);
			}
		}

		public void TestNotifications_Add()
		{
			var consolidation = Helper.CreateConsolidation();

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Add(CargoWise.ComponentModel.NotificationType.Warning, "You're doing something crazy!");

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("You're doing something crazy!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNotifyReceived_WhenChangeBookingTemplate()
		{
			var consolidation = Helper.CreateConsolidation();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;
			var package1 = packageJob.Packages.AddNew();
			package1.KP_PackageID = "PKG00001";
			var package2 = packageJob.Packages.AddNew();
			package2.KP_PackageID = "PKG00002";
			var package3 = packageJob.Packages.AddNew();
			package3.KP_PackageID = "PKG00003";

			var booking = consolidation.Bookings.AddNew();
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			Helper.CreatePackageDivot(picInstruction, package1, 1);
			Helper.CreatePackageDivot(dlvInstruction, package1, 1);
			AssertEquals(1, booking.AssignedPackages.Count);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new TransportBookingMultiForm(consolidation))
			{
				var template = Helper.CreateTransportBookingTemplate("IFFF", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Containerised);
				Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CFS, InstructionTypes.Codes.PickUp, false, true, PackageCategories.Codes.Containers);
				Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery, false, true, PackageCategories.Codes.Containers);

				booking.KM_KT_NKBookingTemplate = "IFFF";
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Could not find Containers/Outer Packages based on Instruction Template Package Type. See Packages Tab."));
			}
		}

		// WI00054972 - Issue 00864570 - Object reference not set to an instance of an object.
		public void TestTabInitialisationNotDelayed()
		{
			var booking = Helper.CreateBooking();
			using (var form = new TransportBookingMultiForm(booking.ConsolidationSingleJob))
			{
				AssertNotNull("Should be initialised on the forms construction.", form.transportBookingsControl);
			}
		}

		public void TestJobNumberChanged()
		{
			var consolidation = Helper.CreateConsolidation();
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;
			packageJob.KJ_ParentTableCode = consolidation.TablePrefix;
			var booking1 = consolidation.Bookings.AddNew();
			Factory.Save();

			var raisedCount = 0;
			packageJob.ParentJobNumberChanged += delegate
			{ raisedCount++; };

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Show();
				AssertEquals("Precondition.", 0, raisedCount);

				var booking2 = consolidation.Bookings.AddNew();
				AssertEquals("Should have been raised so packing can repaint it's PackageJob node.", 1, raisedCount);

				var booking3 = consolidation.Bookings.AddNew();
				AssertEquals("Should NOT have raised a second time, because the number won't change.", 1, raisedCount);
			}
		}

		public void TestSaveToRecentItems_WhenIsMultiBookingIsTrue()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			Factory.Save();

			AssertEquals("The consolidation should be a multi booking", true, consolidation.IsMultiBooking);

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Show();
				form.ControllerID = ControllerID;
				Application.DoEvents();
				var recentItems = RecentItemManager.Instance.GetRecentItems(ModuleIDs.DtbBookingConsolidation.Name);
				AssertContainsExactElementsInAnyOrder("Recent Items should contain link to multi booking consolidation", new List<string>() { consolidation.PK.ToString() }, recentItems.Select(i => i.STL_ItemPK.ToString()));
			}
		}

		public void TestSaveToRecentItems_WhenIsMultiBookingIsFalse()
		{
			var consolidation = Helper.CreateConsolidation();
			Factory.Save();

			AssertEquals("The consolidation should not be a multi booking", false, consolidation.IsMultiBooking);

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Show();
				form.ControllerID = ControllerID;
				Application.DoEvents();
				var recentItems = RecentItemManager.Instance.GetRecentItems(ModuleIDs.DtbBookingConsolidation.Name);
				AssertEquals("Recent Items should be empty", 0, recentItems.Count);
			}
		}

		public void TestSubTransportBookingIsPartiallyEditable()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			var masterInstruction = Helper.CreateInstruction(masterBooking);
			masterInstruction.KN_IsMaster = true;
			masterInstruction.KN_MasterBookingVersion = 1;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = Helper.CreateBooking(subConsolidation);
			subBooking.KM_KM_MasterBooking = masterBooking.PK;
			subBooking.KM_MasterBookingVersion = 1;
			Helper.LoadOrCreateJobHeader(subBooking);
			var subInstruction = Helper.CreateInstruction(subBooking);
			subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			subInstruction.KN_MasterBookingVersion = 1;

			using (var form = new TransportBookingMultiForm(subBooking.ConsolidationSingleJob))
			{
				form.Show();

				CombineAssertions(() =>
					{
						AssertEquals("Booking should be ReadOnly.", true, subBooking.ReadOnly);
						AssertEquals("DocAddress collection should be ReadOnly.", true, subBooking.DocAddresses.ReadOnly);
						AssertEquals("Transport Co Address should be ReadOnly.", true, subBooking.Address.ReadOnly);
						AssertEquals("Instruction collection should be ReadOnly.", true, subBooking.Instructions.ReadOnly);
						AssertEquals("Packages View should be ReadOnly.", true, subBooking.Packages_PackageView.ReadOnly);
						AssertEquals("Custom Business Object should NOT be ReadOnly, due to Partial Lockdown", false, subBooking.CustomBusinessObject_ForTest.ReadOnly);
						AssertEquals("WorkflowItems should be NOT ReadOnly, due to Partial Lockdown", false, subBooking.WorkflowItems.ReadOnly);
						AssertEquals("BillingJob should be NOT ReadOnly, due to Partial Lockdown", false, subBooking.Job.ReadOnly);
						AssertEquals("Instruction Confirmations should be ReadOnly.", true, subInstruction.Confirmations.ReadOnly);
						AssertEquals("Instruction PackageDivots should be ReadOnly.", true, subInstruction.PackageDivots.ReadOnly);
						AssertEquals("Instruction DocAddresses should be ReadOnly.", true, subInstruction.DocAddresses.ReadOnly);
						AssertEquals("Instruction WorkflowItems should be ReadOnly.", true, subInstruction.WorkflowItems.ReadOnly);
					});
			}
		}

		public void TestSubTransportBookingAdditionalReferenceNumbersIsEditable()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;

			var subBooking = Helper.CreateBooking();
			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			using (var form = new TransportBookingMultiForm(subBooking.ConsolidationSingleJob))
			{
				form.Show();
				AssertEquals("AdditionalReferenceNumbers on SubBooking should not be ReadOnly", false, subBooking.AdditionalReferenceNumbers.ReadOnly);
			}
		}

		public void TestNonSubTransportBookingAdditionalReferenceNumbersIsEditable()
		{
			var booking = Helper.CreateBooking();
			AssertEquals("Precondition: Booking should not be sub booking.", false, booking.IsSub);

			using (var form = new TransportBookingMultiForm(booking.ConsolidationSingleJob))
			{
				form.Show();
				AssertEquals("AdditionalReferenceNumbers on Non-SubBooking should be editable", false, booking.AdditionalReferenceNumbers.ReadOnly);
			}
		}

		public void TestNonSubTransportBookingIsEditable()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var instruction = Helper.CreateInstruction(booking);

			AssertEquals("Precondition: Booking should not be a Sub.", false, booking.IsSub);
			AssertEquals("Precondition: Instruction should not be a Sub.", false, instruction.IsSub);

			using (var form = new TransportBookingMultiForm(booking.ConsolidationSingleJob))
			{
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Booking NOT should be ReadOnly.", false, booking.ReadOnly);
					AssertEquals("DocAddress collection should NOT be ReadOnly.", false, booking.DocAddresses.ReadOnly);
					AssertEquals("Transport Co Address should NOT be ReadOnly.", false, booking.Address.ReadOnly);
					AssertEquals("Instruction collection should NOT be ReadOnly.", false, booking.Instructions.ReadOnly);
					AssertEquals("Packages View should NOT be ReadOnly.", false, booking.Packages_PackageView.ReadOnly);
					AssertEquals("Custom Business Object should NOT be ReadOnly", false, booking.CustomBusinessObject_ForTest.ReadOnly);
					AssertEquals("Instruction Confirmations should NOT be ReadOnly.", false, instruction.Confirmations.ReadOnly);
					AssertEquals("Instruction PackageDivots should NOT be ReadOnly.", false, instruction.PackageDivots.ReadOnly);
					AssertEquals("Instruction DocAddresses should NOT be ReadOnly.", false, instruction.DocAddresses.ReadOnly);
				});
			}
		}

		protected override BusinessObjectFactory NewFactory()
		{
			var factory = base.NewFactory();
			DtbChildEditableService.SetState(factory, DtbChildEditableServiceState.Consolidation);
			return factory;
		}

		protected override Form GetFormToBashCore()
		{
			var consolidation = GetNewConsolidation();

			var result = new TransportBookingMultiForm(consolidation);
			result.ControllerID = ControllerID;

			return result;
		}
	}
}
