using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.GUI
{
	public partial class TransportBookingsControl : ZUserControl
	{
		public TransportBookingsControl()
		{
			InitializeComponent();
			AddReadOnlyAttributes();
			OverrideParentCheckBox.AllowOverlap(BookingsSplitContainer);
		}

		void AddReadOnlyAttributes()
		{
			TypeDescriptor.AddAttributes(AttachBookingButton, new CanBeReadOnlyUIAttribute());
			TypeDescriptor.AddAttributes(DetachBookingButton, new CanBeReadOnlyUIAttribute());
		}

		public BusinessObjectFactory Factory
		{
			get { return ((IBusiness)DataSource).Factory; }
		}

		public new TransportBookingMultiForm ParentForm
		{
			get { return (TransportBookingMultiForm)base.ParentForm; }
		}

		ZGrid BookingsGrid
		{
			get { return BookingsModuleButtonGrid.InnerGrid; }
		}

		void UnhookEvents()
		{
			if (BookingsGrid != null)
			{
				if (BookingsGrid.ContextMenu != null)
				{
					BookingsGrid.ContextMenu.Popup -= new EventHandler(ContextMenu_Popup);
				}

				UnhookBookingsGridEvents();
			}
		}

		public DtbBooking SelectedBooking
		{
			get { return (DtbBooking)BookingsGrid.GetCurrent(); }
		}

		CurrencyManager BookingsListManager
		{
			get { return BookingsGrid.ListManager; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				AddMenuItems();
				HookBookingsGridEvents();
			}
		}

		void AddMenuItems()
		{
			AddMenusToBookingsGrid();
		}

		void AddMenusToBookingsGrid()
		{
			AddressOverrideMenuItem = new ZMenuItem(TransportBookingInstructionViewControl.AddressMenuItemText, AddressMenuItem_Click);
			HoldBookingMenuItem = new ZMenuItem("", HoldMenuItem_Click);
			DeactivateMenuItem = new ZMenuItem("", DeActivateMenuItem_Click);
			OpenConsolidationMenuItem = new ZMenuItem("OpenConsolidation", OpenConsolidationMenuItem_Click);
			OpenBookingMenuItem = new ZMenuItem("OpenBooking", OpenBookingMenuItem_Click);

			BookingsGrid.ContextMenu.MenuItems.Add("-");
			BookingsGrid.ContextMenu.MenuItems.Add(AddressOverrideMenuItem);
			BookingsGrid.ContextMenu.MenuItems.Add(HoldBookingMenuItem);

			// consolidated bookings only
			if (ConsolidationViewModeService.GetViewMode(Factory) == ConsolidationViewMode.MultiJob)
			{
				BookingsGrid.ContextMenu.MenuItems.Add(OpenBookingMenuItem);
				BookingsGrid.RowsDeleting += BookingsGrid_RowDeleting;
			}
			else // bookings only
			{
				BookingsGrid.ContextMenu.MenuItems.Add(DeactivateMenuItem);
				BookingsGrid.ContextMenu.MenuItems.Add(OpenConsolidationMenuItem);
			}

			BookingsGrid.ContextMenu.Popup += new EventHandler(ContextMenu_Popup);
		}

		public static MultilingualString HoldBookingMenuItemText
		{
			get { return ResString.GetMultilingualString("a9736bc6-a333-492a-a246-7269ff96ffd3", "Hold Booking"); }
		}

		public static MultilingualString UnholdBookingMenuItemText
		{
			get { return ResString.GetMultilingualString("f78e7992-1289-424d-864d-5f9e4ebbe8f3", "Remove Held Status"); }
		}

		public static MultilingualString MakeAvailableStatusMenuItemText
		{
			get { return ResString.GetMultilingualString("TransportBookingForm|MakeAvailable", "Make Available"); }
		}

		// activate / deactivate booking

		static MultilingualString ActivateBookingMenuItemText
		{
			get { return ResString.GetMultilingualString("5aabd2b2-6f56-4c29-bb31-4916c959ff95", "Activate Booking"); }
		}

		static MultilingualString DeactivateBookingMenuItemText
		{
			get { return ResString.GetMultilingualString("accdea90-5377-4b7b-b2d2-871d389e4b21", "Deactivate Booking"); }
		}

		// opening the booking

		static MultilingualString OpenBookingMenuItemText
		{
			get { return ResString.GetMultilingualString("4272f073-3efe-44dd-ace9-3614bb2d2dda", "Open Booking"); }
		}

		static MultilingualString ConsolidationNotSavedMenuItemText
		{
			get { return ResString.GetMultilingualString("a97b7313-ec92-4f9d-a638-9136d6932ba3", "Please save before opening the Booking."); }
		}

		// opening the consolidation

		static MultilingualString OpenConsolidationMenuItemText
		{
			get { return ResString.GetMultilingualString("61756ab8-3c7b-407e-8ea9-a197735e69d9", "Open Consolidation"); }
		}

		static MultilingualString BookingNotConsolidatedMenuItemText
		{
			get { return ResString.GetMultilingualString("5379c5d9-5428-4725-9e2f-4536f2075546", "Booking not Consolidated"); }
		}

		static MultilingualString BookingNotSavedMenuItemText
		{
			get { return ResString.GetMultilingualString("dff053b9-d09d-4edd-a1a7-6fc1b086f333", "Please save before opening the Consolidation."); }
		}

		void HookBookingsGridEvents()
		{
			BookingsGrid.MouseDown += BookingsGrid_MouseDown;
		}

		void BookingsGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks > 1 && SelectedBooking != null)
			{
				var controller = (IDtbBookingController)ZControllerFactory.Create(ControllerIDs.DtbBooking);
				var bookingForm = controller.ShowEditFormForSingleBooking(SelectedBooking);
				if (bookingForm != null)
				{
					ZFormModaliser.Show(bookingForm, ParentForm);
				}
			}
		}

		void UnhookBookingsGridEvents()
		{
			BookingsGrid.MouseDown -= BookingsGrid_MouseDown;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			UpdateConsolidationViewMode((DtbBookingConsolidation)dataSource);
		}

		void UpdateConsolidationViewMode(DtbBookingConsolidation consolidation)
		{
			SuspendLayout();
			try
			{
				TransportCoAndBookingPartyPanel.Visible = false;
				TransportCoPanel.Visible = false;
				BookingPartyPanelControl.Visible = false;

				if (consolidation != null)
				{
					if (consolidation.IsMultiBooking)
					{
						ConsolidationViewModeService.SetViewMode(consolidation.Factory, ConsolidationViewMode.MultiJob);

						TransportCoAndBookingPartyPanel.Visible = true;
						TransportCoPanel.Visible = true;
						HideInstructionsButton.PerformClick(); // minimize the instructions
						OverrideParentCheckBox.Visible = false;
					}
					else
					{
						ConsolidationViewModeService.SetViewMode(consolidation.Factory, ConsolidationViewMode.SingleJob);
						BookingsSplitContainer.Panel2Collapsed = true; // hide the attach/detach toolbar
						OverrideParentCheckBox.Visible = true;
						var hasParent = consolidation.Parent == null;
						TransportCoAndBookingPartyPanel.Visible = hasParent;
						BookingPartyPanelControl.Visible = hasParent;
					}
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			UpdateContextMenuItems();
		}

		void UpdateContextMenuItems()
		{
			bool isBookingSelected = (SelectedBooking != null);
			bool bookingIsNotReadOnly = isBookingSelected && !SelectedBooking.ReadOnly;
			bool bookingIsDeactivated = isBookingSelected && SelectedBooking.IsCancelled;

			AddressOverrideMenuItem.Enabled = isBookingSelected && bookingIsNotReadOnly;
			HoldBookingMenuItem.Enabled = isBookingSelected && ParentForm.DisplayMode != ODisplayMode.ReadOnly && bookingIsNotReadOnly;
			DeactivateMenuItem.Enabled = isBookingSelected && ParentForm.DisplayMode != ODisplayMode.ReadOnly && (bookingIsNotReadOnly || bookingIsDeactivated);
			OpenConsolidationMenuItem.Enabled = isBookingSelected && SelectedBooking.IsOnMultiJobConsolidation;
			BookingsGrid.DeleteMenuItem.Enabled = isBookingSelected && SelectedBooking.IsOnMultiJobConsolidation;
			OpenBookingMenuItem.Enabled = isBookingSelected;

			// show the "Hold Booking" or "Remove Held Status" caption
			HoldBookingMenuItem.Caption = isBookingSelected && SelectedBooking.IsHeld ? UnholdBookingMenuItemText : HoldBookingMenuItemText;

			// show the "Activate Booking" or "Deactivate Booking" caption
			DeactivateMenuItem.Caption = isBookingSelected && SelectedBooking.KM_IsActive ? DeactivateBookingMenuItemText : ActivateBookingMenuItemText;

			// show the "Open Booking" or "Consolidation not saved" caption
			OpenBookingMenuItem.Caption = isBookingSelected && !SelectedBooking.HasChanges ? OpenBookingMenuItemText : ConsolidationNotSavedMenuItemText;

			// show the "Open Consolidation" or "Booking not Consolidated" caption
			MultilingualString openConsolidationMenuText;
			if (!isBookingSelected || !SelectedBooking.IsOnMultiJobConsolidation)
			{
				openConsolidationMenuText = BookingNotConsolidatedMenuItemText;
			}
			else
			{
				openConsolidationMenuText = SelectedBooking.HasChanges ? BookingNotSavedMenuItemText : OpenConsolidationMenuItemText;
			}
			OpenConsolidationMenuItem.Caption = openConsolidationMenuText;
		}

		ZMenuItem HoldBookingMenuItem;
		ZMenuItem AddressOverrideMenuItem;
		ZMenuItem DeactivateMenuItem;
		ZMenuItem OpenConsolidationMenuItem;
		ZMenuItem OpenBookingMenuItem;

		void AddressMenuItem_Click(object sender, EventArgs e)
		{
			ShowPopupAddress();
		}

		void ShowPopupAddress()
		{
			addressPopup = new AddressPopup();
			addressPopup.ShowPopupAddress(BookingsGrid, Res.GetString("04b36dec-f445-4964-aeb4-9301005cc265", "Booking"), DocAddressType.TransportCompanyDocumentaryAddress);
		}

		AddressPopup addressPopup;

		void HoldMenuItem_Click(object sender, EventArgs e)
		{
			ToggleHeldStatus();
		}

		void ToggleHeldStatus()
		{
			if (SelectedBooking != null)
			{
				SelectedBooking.ToggleHeldStatus(ParentForm);
			}
		}

		internal void SelectBooking(DtbBooking booking)
		{
			BookingsGrid.SelectSingleElement(booking);
		}

		internal void SetInstructionView(TransportBookingInstructionView instructionView)
		{
			DtbBookingInstructionsViewsPanel.View = instructionView;
		}

		void parentLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var multiBooking = (DtbBookingConsolidation)CurrentDataItem;
			if (multiBooking != null)
			{
				var parent = multiBooking.Parent;
				var parentBO = parent != null ? parent.ParentWithWorkflow : null;
				if (parentBO != null)
				{
					var controller = ZControllerFactory.Create(parent.ControllerID);
					controller.ShowEditForm(parentBO);
				}
			}
		}

		// bookings only

		void OpenConsolidationMenuItem_Click(object sender, EventArgs e)
		{
			OpenMultiJobConsolidation();
		}

		void OpenMultiJobConsolidation()
		{
			var selectedBooking = SelectedBooking;

			if (selectedBooking != null)
			{
				BookingsListManager.EndCurrentEdit(); // handle case when editing a new row

				if (!selectedBooking.HasChanges && selectedBooking.IsOnMultiJobConsolidation)
				{
					var controller = ZControllerFactory.Create(ControllerIDs.DtbBookingConsolidation);
					controller.ShowEditForm(selectedBooking.ConsolidationMultiJob);

#if DEBUG
					ControllerForTest = controller;
#endif
				}
			}
		}

		void DeActivateMenuItem_Click(object sender, EventArgs e)
		{
			ToggleBookingActiveStatus();
		}

		void ToggleBookingActiveStatus()
		{
			if (SelectedBooking != null)
			{
				if (SelectedBooking.ConsolidationSingleJob.HasChanges)
				{
					ShowHasChangesMessage();
				}
				else
				{
					ShowSingleBookingFormAndClickDeactivate();
				}
			}
			else
			{
				var message = Res.GetString("f61792a9-4987-44a1-bab0-52b94c8de686", "Please select a Movement.");
				var caption = Res.GetString("0c038a4d-bff5-4c84-abd8-3ca3c08a0320", "Warning");

				Globals.Message.ShowWarning(message, caption);
			}
		}

		void ShowHasChangesMessage()
		{
			var activateText = SelectedBooking.IsCancelled
				? Res.GetString("306c20af-bf8f-451a-a0d3-6347d807849d", "activated")
				: Res.GetString("1aa00d51-a082-485c-add5-65b026d3d700", "deactivated");

			Globals.Message.ShowInformation(
				Res.GetString("55ad7245-88f3-4de3-94d6-b03f1303c39e", "The Booking must be saved before it can be {0}.", activateText),
				Res.GetString("3f8b20c3-2b88-4d9b-a161-7923629767a8", "Save Changes"));
		}

		void ShowSingleBookingFormAndClickDeactivate()
		{
			var controller = (IDtbBookingController)ZControllerFactory.Create(ControllerIDs.DtbBooking);
			var bookingForm = controller.ShowEditFormForSingleBooking(SelectedBooking);
			if (bookingForm != null)
			{
				ZFormModaliser.Show(bookingForm, ParentForm);

				var activationMenuItem = bookingForm.FindDeactivateActionsMenuItem();
				if (activationMenuItem != null)
				{
					activationMenuItem.PerformClick();
				}
			}
		}

		// consolidated bookings only

		void OpenBookingMenuItem_Click(object sender, EventArgs e)
		{
			OpenBooking();
		}

		void OpenBooking()
		{
			var selectedBooking = SelectedBooking;

			if (selectedBooking != null)
			{
				BookingsListManager.EndCurrentEdit(); // handle case when editing a new row

				if (!selectedBooking.HasChanges)
				{
					var controller = (IDtbBookingController)ZControllerFactory.Create(ControllerIDs.DtbBooking);
					controller.ShowEditFormForSingleBooking(selectedBooking);

#if DEBUG
					ControllerForTest = (ZController)controller;
#endif
				}
			}
		}

		void AttachBookingButton_Click(object sender, EventArgs e)
		{
			AttachBookings();
		}

		void AttachBookings()
		{
			BookingsModuleButtonGrid.PerformClickOnAttachButton();
		}

		void DetachBookingButton_Click(object sender, EventArgs e)
		{
			DetachBookings();
		}

		void DetachBookings()
		{
			BookingsModuleButtonGrid.PerformClickOnDetachButton();
		}

		void BookingsGrid_RowDeleting(object sender, RowsDeletingEventArgs e)
		{
			var selectedBookings = e.Objects.Cast<DtbBooking>();
			var cannotDetachMessage = ShowBookingMessageHelper.GetCannotDetachMessage(selectedBookings);
			if (!cannotDetachMessage.IsEmpty)
			{
				Globals.Message.Show(cannotDetachMessage);
				e.Cancel = true;
			}
			else
			{
				var result = Globals.Message.Show(Res.GetString("984a3d35-bde6-4b5e-9ded-b1a4dfa1ae0f", "Are you sure you want to detach the selected records? New records will be deleted.\r\nAll unsaved changes in detached items will be canceled."), Res.GetString("9fcd7082-b21e-42c2-9a94-163befd27db2", "Confirm Detach..."), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
				if (result != DialogResult.Yes)
				{
					e.Cancel = true;
				}
			}
		}

		void HideInstructionsButton_Click(object sender, EventArgs e)
		{
			ToggleInstructionsVisibility();
		}

		void ToggleInstructionsVisibility()
		{
			BookingsAndInstructionsSplitContainer.Panel2Collapsed = !BookingsAndInstructionsSplitContainer.Panel2Collapsed;

			if (BookingsAndInstructionsSplitContainer.Panel2Collapsed)
			{
				HideInstructionsButton.Image = Properties.Resources.ChevronUp;
				HideInstructionsButton.CaptionResourceString = Res.GetData("030b93a4-2593-4573-ae9d-c3d6619d266c", "Show Instructions");
			}
			else
			{
				HideInstructionsButton.Image = Properties.Resources.ChevronDown;
				HideInstructionsButton.CaptionResourceString = Res.GetData("2d5136e6-51c6-4998-a29c-c1f92c8d25e3", "Hide Instructions");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();

				if (addressPopup != null)
				{
					addressPopup.Dispose();
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

#if DEBUG
		internal ZController ControllerForTest;
#endif
	}
}

#if DEBUG

namespace Enterprise.TransportBookings.GUI
{
	public partial class TransportBookingsControl
	{
		public ZPanel TransportCoAndBookingPartyPanelForTest
		{
			get { return TransportCoAndBookingPartyPanel; }
		}

		public ZPanel TransportCoPanelForTest
		{
			get { return TransportCoPanel; }
		}

		public ZPanel BookingPartyPanelControlForTest
		{
			get { return BookingPartyPanelControl; }
		}
	}
}

#endif
