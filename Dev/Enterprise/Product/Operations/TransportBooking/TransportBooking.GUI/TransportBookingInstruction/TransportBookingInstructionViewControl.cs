using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.GUI;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.GUI;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.GUI
{
	public partial class TransportBookingInstructionViewControl : ZUserControl
	{
		public static MultilingualString AddressMenuItemText
		{
			get { return ResString.GetMultilingualString("064d2269-73d4-448b-b7ff-ab5847232300", "Override Address"); }
		}

		public static ResourceStringData AssignPackagesToSelectedInstructionsMenuText
		{
			get { return Res.GetData("9aaa17a2-d5b9-4409-bec5-0d13eed43a86", "...to Selected Instructions"); }
		}

		public static ResourceStringData AssignPackagesToAllInstructionsMenuText
		{
			get { return Res.GetData("31e92f33-13f5-4d25-a5fe-b3e0db130858", "...to All Instructions"); }
		}

		public static ResourceStringData AssignAllOuterPackagesToAllInstructionsMenuText
		{
			get { return Res.GetData("a564ec0e-6b1d-4457-9246-39e7a2e0719b", "Assign All Outer Packages to All Instructions"); }
		}

		public static ResourceStringData AssignAllContainersToAllInstructionsMenuText
		{
			get { return Res.GetData("f691a550-6bcb-4ccc-bec3-a51e65726b15", "Assign All Containers to All Instructions"); }
		}

		public static string AssignPackagesButtonText
		{
			get { return Res.GetString("cbc58bea-0492-40bb-8736-e98a70bb8269", "Assign Packages"); }
		}

		public static string UnassignPackageButtonText
		{
			get { return Res.GetString("44e2f27b-1426-46b6-8bda-779abea518a5", "Un-assign Packages"); }
		}

		public TransportBookingInstructionViewControl()
		{
			InitializeComponent();
			AddReadOnlyAttributes();
			HookEvents();
			InstructionsGrid.AllowSorting = false;

			InstructionsGroupBox.AllowOutsideOfParent();
		}

		void AddReadOnlyAttributes()
		{
			foreach (var item in PackagesToolStrip.Items)
			{
				TypeDescriptor.AddAttributes(item, new CanBeReadOnlyUIAttribute());
			}
		}

		public new ZForm ParentForm
		{
			get { return (ZForm)base.ParentForm; }
		}

		void HookEvents()
		{
			InstructionsGrid.AfterBind += delegate
			{
				InstructionsGrid.ListManager.CurrentChanged += new EventHandler(InstructionsGrid_ListManager_CurrentChanged);
			};
		}

		void UnhookEvents()
		{
			if (InstructionsGrid != null && InstructionsGrid.ListManager != null)
			{
				InstructionsGrid.ListManager.CurrentChanged -= new EventHandler(InstructionsGrid_ListManager_CurrentChanged);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AddMenuItems();
			SetButtonAndMenuTexts();
		}

		void AddMenuItems()
		{
			InstructionsGrid.ContextMenu.MenuItems.Add(new ZMenuItem(AddressMenuItemText, AddressMenuItem_Click));
		}

		void SetButtonAndMenuTexts()
		{
			AssignPackagesButton.Text = AssignPackagesButtonText;
			UnassignPackagesButton.Text = UnassignPackageButtonText;

			AssignToSelectedInstructionMenuItem.CaptionResourceString = AssignPackagesToSelectedInstructionsMenuText;
			AssignToAllInstructionsMenuItem.CaptionResourceString = AssignPackagesToAllInstructionsMenuText;
			AssignAllOuterPackagesToAllInstructionsMenuItem.CaptionResourceString = AssignAllOuterPackagesToAllInstructionsMenuText;
			AssignAllContainersToAllInstructionsMenuItem.CaptionResourceString = AssignAllContainersToAllInstructionsMenuText;
		}

		DtbBooking GetSelectedBooking()
		{
			var currentDataItem = CurrentDataItem as DtbBooking;
			return currentDataItem == null || currentDataItem.IsDeleted ? null : currentDataItem;
		}

		DtbBookingInstruction SelectedInstruction
		{
			get { return (DtbBookingInstruction)InstructionsGrid.GetCurrent(); }
		}

		bool ReadOnly
		{
			get
			{
				var booking = GetSelectedBooking();
				return booking == null || booking.IsHeld || booking.ReadOnly;
			}
		}

		void UpdateToolStripReadOnly()
		{
			var readOnly = ReadOnly;
			foreach (ToolStripItem item in PackagesToolStrip.Items)
			{
				item.Enabled = !readOnly;
			}

			UpdateToolStripUpDownButtonsReadOnly();
		}

		void UpdateToolStripUpDownButtonsReadOnly()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var readOnly = ReadOnly;

				var instruction = ZGridExtensions.GetCurrent(InstructionsGrid);
				MoveUpButton.Enabled = !readOnly && InstructionsGrid.ListManager != null && InstructionsGrid.ListManager.Position > 0 && instruction != null;
				MoveDownButton.Enabled = !readOnly && InstructionsGrid.ListManager != null && InstructionsGrid.ListManager.Position < InstructionsGrid.ListManager.Count - 1 && instruction != null;
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (!IsDisplayModeChangedHooked && ParentForm != null)
			{
				IsDisplayModeChangedHooked = true;
				ParentForm.DisplayModeChanged += new DisplayModeChangedEventHandler(ParentForm_DisplayModeChanged);
				UpdateToolStripReadOnly();
			}
		}

		void ParentForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			UpdateToolStripReadOnly();
		}

		bool IsDisplayModeChangedHooked;

		void InstructionsGrid_ListManager_CurrentChanged(object sender, EventArgs e)
		{
			UpdateToolStripUpDownButtonsReadOnly();
		}

		void MoveUpButton_Click(object sender, EventArgs e)
		{
			MoveSelectedInstruction(MoveDirection.Up);
		}

		void MoveDownButton_Click(object sender, EventArgs e)
		{
			MoveSelectedInstruction(MoveDirection.Down);
		}

		void MoveSelectedInstruction(MoveDirection direction)
		{
			var selectedInstruction = SelectedInstruction;
			bool isMovingSingleInstruction = (selectedInstruction != null && InstructionsGrid.SelectedRowCount <= 1);

			if (isMovingSingleInstruction)
			{
				InstructionsGrid.ListManager.EndCurrentEdit(); // handle case when editing a new row

				if (direction == MoveDirection.Up)
				{
					GetSelectedBooking().Instructions.MoveUp(selectedInstruction);
				}
				else
				{
					GetSelectedBooking().Instructions.MoveDown(selectedInstruction);
				}

				InstructionsGrid.SelectSingleElement(selectedInstruction);
			}
		}

		enum MoveDirection
		{
			Up,
			Down
		}

		public enum AssignPackagesStrategy
		{
			SelectedPackagesFromUser,
			OuterPackages,
			Containers
		}

		void AssignToSelectedInstructionMenuItem_Click(object sender, EventArgs e)
		{
			const bool AssignToSelectedInstructions = true;
			AssignPackages(AssignPackagesStrategy.SelectedPackagesFromUser, AssignToSelectedInstructions);
		}

		void AssignToAllInstructionsMenuItem_Click(object sender, EventArgs e)
		{
			AssignPackages(AssignPackagesStrategy.SelectedPackagesFromUser);
		}

		void AssignAllOuterPackagesToAllInstructionsMenuItem_Click(object sender, EventArgs e)
		{
			AssignPackages(AssignPackagesStrategy.OuterPackages);
		}

		void AssignAllContainersToAllInstructionsMenuItem_Click(object sender, EventArgs e)
		{
			AssignPackages(AssignPackagesStrategy.Containers);
		}

		void AssignPackages(AssignPackagesStrategy assignStrategy, bool assignToSelectedInstructions = false)
		{
			var hasInstructions = InstructionsGrid.ListManager.Count > 0;
			if (hasInstructions)
			{
				var booking = GetSelectedBooking();
				var bookingConsolidation = booking.ConsolidationSingleJob;
				var packageJob = bookingConsolidation.PackageJob;

				if (packageJob != null && packageJob.Packages.Count > 0)
				{
					if (assignStrategy == AssignPackagesStrategy.SelectedPackagesFromUser)
					{
						using (var popup = new PackingPopupDialog(packageJob))
						{
							var dialog = ZFormModaliser.ShowDialogWithoutDispose(popup);

							if (dialog == DialogResult.OK)
							{
								var instructionsToAssignTo = GetInstructionsToAssignPackagesTo(assignToSelectedInstructions);
								var packagesToAssign = popup.SelectedPackages;

								AssignPackages(instructionsToAssignTo, packagesToAssign);
							}
						}
					}
					else
					{
						var instructionsToAssignTo = GetInstructionsToAssignPackagesTo(assignToSelectedInstructions);
						var packagesToAssign = (assignStrategy == AssignPackagesStrategy.OuterPackages)
							? packageJob.GetAllNonContainerOutersAndFirstLevelPackagesOnContainers()
							: packageJob.Packages.Where(p => p.IsContainer); // AssignPackagesStrategy.Containers

						AssignPackages(instructionsToAssignTo, packagesToAssign);
					}
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("30a7b859-b2e3-4e38-815b-90081f28a999",
						"No Packages exist. You should first setup the Booking's Packages on the Packing Tab."));
				}
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("15e77bbe-3a46-47e3-95a2-7cb2c75ed9c6",
					"No Instructions exist. You should first add your Booking Instructions and then Assign packages to them."));
			}
		}

		void AssignPackages(IEnumerable<DtbBookingInstruction> instructionsToAssignTo, IEnumerable<PkgPackage> packagesToAssign)
		{
			foreach (var instruction in instructionsToAssignTo)
			{
				foreach (var package in packagesToAssign)
				{
					instruction.DivotsWithPackages.AddPackage(package);
				}
			}
		}

		IEnumerable<DtbBookingInstruction> GetInstructionsToAssignPackagesTo(bool assignToSelectedPackages = false)
		{
			IEnumerable<DtbBookingInstruction> result = null;

			if (assignToSelectedPackages)
			{
				result = InstructionsGrid.GetSelectedElements<DtbBookingInstruction>();

				// if nothing is selected, get the current row instead..
				if (!result.Any() && SelectedInstruction != null)
				{
					result = new DtbBookingInstruction[] { SelectedInstruction };
				}
			}
			else
			{
				result = GetSelectedBooking().Instructions;
			}

			return result;
		}

		void UnassignPackagesButton_Click(object sender, EventArgs e)
		{
			UnassignPackages();
		}

		void UnassignPackages()
		{
			if (PackageDivotGrid.SelectedElements.Length > 0)
			{
				foreach (var divot in PackageDivotGrid.GetSelectedElements<DtbBookingInstructionPkgDivot>())
				{
					divot.Delete();
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("7686683a-e930-44f0-8b7f-17e8d72ad1ca", "No Packages are Selected."));
			}
		}

		void AddressMenuItem_Click(object sender, EventArgs e)
		{
			ShowPopupAddress();
		}

		void ShowPopupAddress()
		{
			addressPopup = new AddressPopup();
			addressPopup.ShowPopupAddress(InstructionsGrid, Res.GetString("18290a08-3914-4f0b-8f2a-6206013a9e71", "Instruction"), DocAddressType.TransportCompanyDocumentaryAddress);
		}

		AddressPopup addressPopup;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();

				if (ParentForm != null)
				{
					ParentForm.DisplayModeChanged -= new DisplayModeChangedEventHandler(ParentForm_DisplayModeChanged);
				}
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

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			Unhook();

			base.OnCurrentDataItemChanged(e);
			UpdateToolStripReadOnly();

			Hook();
		}

		void Hook()
		{
			var booking = GetSelectedBooking();
			if (booking != null)
			{
				booking.KM_RatingFreightModeInfo.ValueChanged += new EventHandler(KM_RatingFreightModeInfo_ValueChanged);
				booking.ReadOnlyChanged += new EventHandler(KB_IsOverriddenInfo_ValueChanged);
			}

			UpdateRatingVisibility();
		}

		void Unhook()
		{
			var booking = GetSelectedBooking();
			if (booking != null)
			{
				booking.KM_RatingFreightModeInfo.ValueChanged -= new EventHandler(KM_RatingFreightModeInfo_ValueChanged);
				booking.ReadOnlyChanged -= new EventHandler(KB_IsOverriddenInfo_ValueChanged);
			}
		}

		void KM_RatingFreightModeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateRatingVisibility();
		}

		void UpdateRatingVisibility()
		{
			var booking = GetSelectedBooking();
			if (booking != null)
			{
				var freightMode = booking.KM_RatingFreightMode;
				var showContainerRateable = freightMode == RatingFreightModes.Codes.Containerised || freightMode == RatingFreightModes.Codes.Both;
				var showLooseRateable = freightMode == RatingFreightModes.Codes.Loose || freightMode == RatingFreightModes.Codes.Both;

				InstructionsGrid.RunAfterBind(delegate
				{
					InstructionsGrid.SetColumnVisible(showContainerRateable, DtbBookingInstructionSchema.Constants.KN_IsContainerRateable);
					InstructionsGrid.SetColumnVisible(showLooseRateable, DtbBookingInstructionSchema.Constants.KN_IsLooseRateable);
				});
			}
		}

		void KB_IsOverriddenInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateToolStripReadOnly();
		}
	}
}
