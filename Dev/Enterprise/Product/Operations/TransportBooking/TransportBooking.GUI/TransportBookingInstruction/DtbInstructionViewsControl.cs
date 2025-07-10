using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	public partial class DtbInstructionViewsControl : ZUserControl
	{
		public DtbInstructionViewsControl()
		{
			InitializeComponent();

			HookEvents();
		}

		public DtbBooking SelectedBooking
		{
			get { return (DtbBooking)CurrentDataItem; }
		}

		DtbBookingPackage_PackageView SelectedPackage
		{
			get { return (DtbBookingPackage_PackageView)PackageViewUserControl.PackageGrid.GetCurrent(); }
		}

		DtbBookingConfirmation SelectedInstructionConfirmation
		{
			get { return (DtbBookingConfirmation)InstructionViewDatesGrid.GetCurrent(); }
		}

		DtbBookingConfirmation SelectedPackageConfirmation
		{
			get
			{
				return ((Confirmation_PackageView)PackageViewDatesGrid.GetCurrent()).Confirmation;
			}
		}

		CurrencyManager PackageListManager
		{
			get { return PackageViewUserControl.PackageGrid.ListManager; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var booking = CurrentDataItem as DtbBooking;
			if (booking != null)
			{
				UpdateStandardTabPage();
			}

			UpdatePackagesTabPage_GridsVisibility();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				AddMenuItems();
			}
			AdditionalReferencesTabPage.TabVisible = IsAdditionalReferencesVisible;
			TransportBookingsTabPage.TabVisible = SelectedBooking != null && SelectedBooking.KM_IsMaster;
		}

		void AddMenuItems()
		{
			AddMenusToInstructionsGrid();
			AddMenusToDatesGrid();
		}

		void AddMenusToInstructionsGrid()
		{
			// #warning - Can we use ITemplateCopy here? Dave says no. Geoff says yes. Investigate as low-priority task.
			InstructionConfirmationCopyMenuItem = new ZMenuItem(CopyConfirmationMenuItemText, InstructionConfirmationCopyMenuItem_Click);
			InstructionViewDatesGrid.ContextMenu.MenuItems.Add(InstructionConfirmationCopyMenuItem);
			InstructionViewDatesGrid.ContextMenu.Popup += new EventHandler(InstructionConfirmationContextMenu_Popup);
		}

		void AddMenusToDatesGrid()
		{
			PackageConfirmationCopyMenuItem = new ZMenuItem(CopyConfirmationMenuItemText, PackageConfirmationCopyMenuItem_Click);
			PackageViewDatesGrid.ContextMenu.MenuItems.Add(PackageConfirmationCopyMenuItem);
			PackageViewDatesGrid.ContextMenu.Popup += new EventHandler(PackageConfirmationContextMenu_Popup);
		}

		static MultilingualString CopyConfirmationMenuItemText
		{
			get { return ResString.GetMultilingualString("EE7F25AE-1FFB-4FBB-8C5B-AA2667517ED5", "Copy to New Row"); }
		}

		public void SetUnAssignPackageMessage()
		{
			View = TransportBookingInstructionView.Package;
			NoPackagesAssignedLabel.Text = Res.GetString("694800cc-9cab-48b6-be22-4fc44c15fd68", "Deactivating this Booking will un-assign its packages. If you reactivate this Booking you will need to reassign the packages.");
			NoPackagesAssignedLabel.ForeColor = Color.Red;
			NoPackagesAssignedLabel.Font = new Font(NoPackagesAssignedLabel.Font, FontStyle.Bold);
		}

		void HookEvents()
		{
			PackageViewUserControl.PackageGrid.AfterBind += new EventHandler(PackageGrid_AfterBind);
			ViewTabControl.SelectedIndexChanged += new EventHandler(InstructionsTabControl_SelectedIndexChanged);

			//BookingsGrid.AfterBind += delegate
			//{
			//    BookingsGrid.ListManager.CurrentChanged += new EventHandler(BookingsGrid_ListManager_CurrentChanged);
			//};

			//BookingsGrid.MouseDown += new MouseEventHandler(BookingsGrid_MouseDown);
		}

		void UnhookEvents()
		{
			if (ViewTabControl != null)
			{
				ViewTabControl.SelectedIndexChanged -= new EventHandler(InstructionsTabControl_SelectedIndexChanged);
			}

			if (PackageViewUserControl != null)
			{
				PackageViewUserControl.PackageGrid.AfterBind -= new EventHandler(PackageGrid_AfterBind);
			}

			if (InstructionViewDatesGrid != null && InstructionViewDatesGrid.ContextMenu != null)
			{
				InstructionViewDatesGrid.ContextMenu.Popup -= new EventHandler(InstructionConfirmationContextMenu_Popup);
			}

			if (PackageViewDatesGrid != null && PackageViewDatesGrid.ContextMenu != null)
			{
				PackageViewDatesGrid.ContextMenu.Popup -= new EventHandler(PackageConfirmationContextMenu_Popup);
			}
		}

		void PackageGrid_AfterBind(object sender, EventArgs e)
		{
			if (PackageListManager != null)
			{
				PackageListManager.ListChanged -= new ListChangedEventHandler(PackagesListOrCurrentChanged);
				PackageListManager.CurrentChanged -= new EventHandler(PackagesListOrCurrentChanged);

				PackageListManager.ListChanged += new ListChangedEventHandler(PackagesListOrCurrentChanged);
				PackageListManager.CurrentChanged += new EventHandler(PackagesListOrCurrentChanged);

				SetViewBasedOnCurrentValues();
			}
		}

		void PackagesListOrCurrentChanged(object sender, EventArgs e)
		{
			SetViewBasedOnCurrentValues();
		}

		void SetViewBasedOnCurrentValues()
		{
			if (SelectedBooking != null)
			{
				SelectedBooking.SetInstructionViewAndSelectedPackage(View, SelectedPackage);
			}
		}

		void InstructionConfirmationCopyMenuItem_Click(object sender, EventArgs e)
		{
			CopyInstructionConfirmation();
		}

		void CopyInstructionConfirmation()
		{
			if (SelectedInstructionConfirmation != null)
			{
				SelectedInstructionConfirmation.Clone();
			}
		}

		void InstructionConfirmationContextMenu_Popup(object sender, EventArgs e)
		{
			UpdateInstructionConfirmationContextMenuItems();
		}

		void UpdateInstructionConfirmationContextMenuItems()
		{
			bool confirmationIsSelected = (SelectedInstructionConfirmation != null);
			bool bookingIsNotReadOnly = SelectedBooking != null && !SelectedBooking.ReadOnly;
			InstructionConfirmationCopyMenuItem.Enabled = confirmationIsSelected && bookingIsNotReadOnly;
		}

		KMenuItem InstructionConfirmationCopyMenuItem;

		void PackageConfirmationCopyMenuItem_Click(object sender, EventArgs e)
		{
			CopyPackageConfirmation();
		}

		void CopyPackageConfirmation()
		{
			if (SelectedPackageConfirmation != null)
			{
				SelectedPackageConfirmation.Clone();
			}
		}

		void PackageConfirmationContextMenu_Popup(object sender, EventArgs e)
		{
			UpdatePackageConfirmationContextMenuItems();
		}

		void UpdatePackageConfirmationContextMenuItems()
		{
			bool confirmationIsSelected = (SelectedInstructionConfirmation != null);
			PackageConfirmationCopyMenuItem.Enabled = confirmationIsSelected;
		}

		KMenuItem PackageConfirmationCopyMenuItem;

		internal TransportBookingInstructionView View
		{
			get
			{
				TransportBookingInstructionView result;

				if (ViewTabControl.SelectedTab == TransportBookingsTabPage)
				{
					result = TransportBookingInstructionView.TransportBookings;
				}
				else if (ViewTabControl.SelectedTab == StandardViewTabPage)
				{
					result = TransportBookingInstructionView.Standard;
				}
				else if (ViewTabControl.SelectedTab == InstructionViewTabPage)
				{
					result = TransportBookingInstructionView.Instruction;
				}
				else if (ViewTabControl.SelectedTab == PackageViewTabPage)
				{
					result = TransportBookingInstructionView.Package;
				}
				else if (ViewTabControl.SelectedTab == CustomFieldsViewTabPage)
				{
					result = TransportBookingInstructionView.CustomFields;
				}
				else if (ViewTabControl.SelectedTab == AdditionalReferencesTabPage)
				{
					result = TransportBookingInstructionView.AdditionalReferences;
				}
				else
				{
					throw new InvalidOperationException("The selected tab page has no corresponding TransportBookingView entry.");
				}

				return result;
			}
			set
			{
				switch (value)
				{
					case TransportBookingInstructionView.TransportBookings:
						ViewTabControl.SelectedTab = TransportBookingsTabPage;
						break;
					case TransportBookingInstructionView.Standard:
						ViewTabControl.SelectedTab = StandardViewTabPage;
						break;
					case TransportBookingInstructionView.Instruction:
						ViewTabControl.SelectedTab = InstructionViewTabPage;
						break;
					case TransportBookingInstructionView.Package:
						ViewTabControl.SelectedTab = PackageViewTabPage;
						break;
					case TransportBookingInstructionView.CustomFields:
						ViewTabControl.SelectedTab = CustomFieldsViewTabPage;
						break;
					case TransportBookingInstructionView.AdditionalReferences:
						ViewTabControl.SelectedTab = AdditionalReferencesTabPage;
						break;
					default:
						throw new InvalidOperationException("The selected tab page has no corresponding TransportBookingView entry.");
				}
			}
		}

		void InstructionsTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetViewBasedOnCurrentValues();
			UpdateTabPages();
		}

#if DEBUG
		internal
#endif
		void UpdateTabPages()
		{
			switch (View)
			{
				case TransportBookingInstructionView.TransportBookings:
					UpdateTransportBookingsTabPage();
					break;

				case TransportBookingInstructionView.Standard:
					UpdateStandardTabPage();
					break;

				case TransportBookingInstructionView.Instruction:
					UpdateInstructionsTabPage();
					break;

				case TransportBookingInstructionView.Package:
					UpdatePackagesTabPage();
					break;

				case TransportBookingInstructionView.CustomFields:
					UpdateCustomFieldsTabPage();
					break;
				case TransportBookingInstructionView.AdditionalReferences:
					UpdateAdditionalReferencesTabPage();
					break;

				default:
					break;
			}
		}

		void UpdateTransportBookingsTabPage()
		{
			ViewGroupBox.Text = Res.GetString("8ebb9bd8-8d6c-4e86-9e6f-446c8457d80e", "Transport Bookings");
		}

		void UpdateStandardTabPage()
		{
			ViewGroupBox.Text = InstructionsLabel;
		}

		void UpdateInstructionsTabPage()
		{
			ViewGroupBox.Text = InstructionsLabel;

			InstructionViewSplitContainer.SplitterDistance = PackageViewSplitContainer.SplitterDistance;
			InstructionViewUserControl.GridsSplitContainer.SplitterDistance =
				InstructionViewUserControl.GridsSplitContainer.Width - PackageViewUserControl.TopLevelSplitContainer.SplitterDistance;
		}

		string InstructionsLabel
		{
			get
			{
				var instructionsLabel = Res.GetString("f2c5d928-5c00-4d65-88ce-e0060263faae", "Instructions");

				var booking = CurrentDataItem as DtbBooking;
				if (booking != null && booking.Instructions.Count == 1)
				{
					instructionsLabel = Res.GetString("85583006-9283-4555-b903-9924828b5e52", "Instruction");
				}
				return instructionsLabel;
			}
		}

		void UpdatePackagesTabPage()
		{
			ViewGroupBox.Text = Res.GetString("d61ec384-f7e8-4b7a-82de-23584e460798", "Packages");

			UpdatePackagesTabPage_GridsVisibility();

			PackageViewSplitContainer.SplitterDistance = InstructionViewSplitContainer.SplitterDistance;
			PackageViewUserControl.TopLevelSplitContainer.SplitterDistance = InstructionViewUserControl.GridsSplitContainer.Panel2.Width;
		}

		void UpdatePackagesTabPage_GridsVisibility()
		{
			if (View == TransportBookingInstructionView.Package)
			{
				var hasPackagesAssigned = (SelectedBooking != null && SelectedBooking.HasPackages);
				PackageViewSplitContainer.Visible = hasPackagesAssigned;
				NoPackagesAssignedGroupBox.Visible = !hasPackagesAssigned;
			}
		}

		void UpdateCustomFieldsTabPage()
		{
			ViewGroupBox.Text = Res.GetString("2700b554-adf7-4a24-9b71-0e325284bc41", "Custom Fields");
		}

		void UpdateAdditionalReferencesTabPage()
		{
			ViewGroupBox.Text = Res.GetString("e3834032-34c0-4dc3-9020-9131c27b297c", "Additional References");
		}

		public bool IsAdditionalReferencesVisible { get; set; }

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
