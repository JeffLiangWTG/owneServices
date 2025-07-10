using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Windows.UI;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Scanning;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Scanning;

namespace Enterprise.Packing.GUI
{
	public partial class PackingUserControl : ZUserControl, INotifications, INotificationSubscriberQueryUser
	{
		#region Construction

		public PackingUserControl()
		{
			InitializeComponent();
			AddReadOnlyAttributes();

			this.SplitContainer.Panel1MinSize = 50;
			this.SplitContainer.Panel2MinSize = 176;
			// see http://social.msdn.microsoft.com/Forums/en/winformsdesigner/thread/ee6abc76-f35a-41a4-a1ff-5be942ae3425

			EmptyPanelLabel.Text = Res.GetString("06584366-74ec-4438-8cfd-c241be001f77", "This Packing is not yet attached to a Job.");
			Dock = DockStyle.Fill;
			ControlExtensions.SetBindingMember(this, ".");

			HookEvents();
		}

		void AddReadOnlyAttributes()
		{
			foreach (var toolStripItem in EditingControls)
			{
				TypeDescriptor.AddAttributes(toolStripItem, new CanBeReadOnlyUIAttribute());
			}
		}

		PkgPackageJob PackageJob => (PkgPackageJob)DataSource;

		public new ZForm ParentForm => (ZForm)base.ParentForm;

#if DEBUG
		internal
#endif
		PackingTreeView Tree => TreeUserControl.Tree;

		#endregion

		//

		#region Hook / Unhook Events

		void HookEvents()
		{
			Grid.AllowDrop = true;
			Grid.AfterBind += Grid_AfterBind;
			Grid.DragEnter += Grid_DragEnter;
			Grid.DragDrop += Grid_DragDrop;

			TreeUserControl.PackageDetailMenuItem.CheckedChanged += PackageDetailMenuItem_CheckedChanged;
			TreeUserControl.Remove += TreeUserControl_Remove;
			TreeUserControl.PackageDetailsBound += TreeUserControl_PackageDetailsBound;
			TreeUserControl.PackageIsClosedChanged += TreeUserControl_PackageIsClosedChanged;
			TreeUserControl.Tree.PackOrUnpackSucceeded += Tree_PackOrUnpackSucceeded;
		}

		void UnhookEvents()
		{
			// business

			if (PackageJob != null)
			{
				PackageJob.Packages.CountChanged -= Packages_CountChanged;
				PackageJob.MassPackageProcessFinished -= PackageJob_MassPackageProcessFinished;
			}

			// gui

			if (Grid != null)
			{
				Grid.AfterBind -= Grid_AfterBind;
				Grid.DragEnter -= Grid_DragEnter;
				Grid.DragDrop -= Grid_DragDrop;
			}

			if (TreeUserControl != null)
			{
				if (TreeUserControl.PackageDetailMenuItem != null)
				{
					TreeUserControl.PackageDetailMenuItem.CheckedChanged -= PackageDetailMenuItem_CheckedChanged;
				}
				if (Tree != null)
				{
					Tree.PackOrUnpackSucceeded -= Tree_PackOrUnpackSucceeded;
				}

				TreeUserControl.Remove -= TreeUserControl_Remove;
				TreeUserControl.PackageDetailsBound -= TreeUserControl_PackageDetailsBound;
				TreeUserControl.PackageIsClosedChanged -= TreeUserControl_PackageIsClosedChanged;
			}

			if (barcodes != null)
			{
				barcodes.NonSystemBarcodeScanned -= NonSystemBarcodeScanned;
			}
		}

		// PackageJobs Events are hooked during binding if visible OR on visible

		void HookPackageJobEvents()
		{
			if (Visible && PackageJob != null && PackageJob != PackageJobThatWasHooked)
			{
				#region Test
#if DEBUG
				HookHitCountForTesting++;
#endif
				#endregion

				PackageJob.AutoPrinting += PackageJob_AutoPrinting;
				PackageJob.AutoPrinted += PackageJob_AutoPrinted;
				PackageJob.AutoPrintFailed += PackageJob_AutoPrintFailed;
				PackageJob.PackageDeleteCanceled += PackageJob_PackageDeleteCancelled;
				PackageJob.Packages.CountChanged += Packages_CountChanged;
				PackageJob.MassPackageProcessFinished += PackageJob_MassPackageProcessFinished;
				PackageJob.OuterPackageAdded += PackageJob_OuterPackageAdded;
				PackageJob.IsFinalisedChanged += PackageJob_IsFinalisedChanged;
				PackageJob.ParentIsReadOnlyChanged += PackageJob_ParentIsReadOnlyChanged;
				PackageJob.Deleted += PackageJob_Deleted;
				PackageJob.Factory.Saving += Factory_Saving;
				PackageJob.Factory.Saved += Factory_Saved;

				PackageJobThatWasHooked = PackageJob;
			}
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			// We want to suspend Notification Update Events & Package Update Events during the whole save process.
			// There is no straight forward way to do so as Factory.Saved event fires just a bit too early on a
			// successful save. To work around this, we Resume Events on a Failed Factory.Saved and we subscribe
			// an After Committed Service that happens at the actual end to Resume on a successful save.
			var packageJob = PackageJob;
			if (packageJob != null)
			{
				var massPackageProcessSuspension = packageJob.InitiateMassPackageProcess_WithNoDefer();
				var massPackageAction = new DisposableAction(() =>
				{
					massPackageProcessSuspension.Dispose();
					UpdateAutoPackMenuItemAvailability();
				});

				var afterCommitService = factory.ServiceContainer.GetAfterCommittedService<ResumeSuspensionsService>();
				if (afterCommitService == null)
				{
					afterCommitService = new ResumeSuspensionsService();
					factory.ServiceContainer.AddAfterCommittedService(afterCommitService);
				}

				var disposable = new DisposableList(new[]
				{
					new DisposableAction(() =>
					{
						SavingDisposable = null;
						afterCommitService.Clear();
					}),
					Tree.SuspendNotificationsChanged(),
					massPackageAction,
				});
				SavingDisposable = disposable;

				afterCommitService.AddDisposableForCommit(SavingDisposable);
			}
		}

		class ResumeSuspensionsService : IAfterCommittedService
		{
			public void AddDisposableForCommit(IDisposable disposable) => ToDisposeAfterCommit = disposable;
			public void Clear() => ToDisposeAfterCommit = null;

			public void DoAfterCommitted(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				ToDisposeAfterCommit?.Dispose();
			}

			IDisposable ToDisposeAfterCommit;
		}

		protected virtual void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (!savedSuccessfully)
			{
				SavingDisposable?.Dispose();
			}
		}

		IDisposable SavingDisposable;

		void UnhookPackageJobEvents()
		{
			if (PackageJob != null)
			{
				PackageJob.AutoPrinting -= PackageJob_AutoPrinting;
				PackageJob.AutoPrinted -= PackageJob_AutoPrinted;
				PackageJob.AutoPrintFailed -= PackageJob_AutoPrintFailed;
				PackageJob.PackageDeleteCanceled -= PackageJob_PackageDeleteCancelled;
				PackageJob.Packages.CountChanged -= Packages_CountChanged;
				PackageJob.MassPackageProcessFinished -= PackageJob_MassPackageProcessFinished;
				PackageJob.OuterPackageAdded -= PackageJob_OuterPackageAdded;
				PackageJob.IsFinalisedChanged -= PackageJob_IsFinalisedChanged;
				PackageJob.ParentIsReadOnlyChanged -= PackageJob_ParentIsReadOnlyChanged;
				PackageJob.Deleted -= PackageJob_Deleted;
				PackageJob.Factory.Saving -= Factory_Saving;
				PackageJob.Factory.Saved -= Factory_Saved;

				PackageJobThatWasHooked = null;
			}
		}

		PkgPackageJob PackageJobThatWasHooked;

		#region Test
#if DEBUG
		internal int HookHitCountForTesting;
#endif
		#endregion

		#endregion

		#region Binding

		#region Bind

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (IsValidDataSource(dataSource))
			{
				var packageJob = GetPackageJobForBind(dataSource);

				if (ParentJobInCaseOfDelete == null || HasDataSourceChanged(packageJob))
				{
					ParentJobInCaseOfDelete = dataSource as IPackingParent;
					UnhookPackageJobEvents();
					base.SetDataBinding(null, dataMember); // to remove old bindings

					if (packageJob != null)
					{
						base.SetDataBinding(packageJob, dataMember);
						HookPackageJobEvents();
						UpdateAutoPackMenuItemAvailability();
						SetViewMode();
						SetLooseIDsButton(packageJob);
					}
					else
					{
						TreeUserControl.Bind();
					}
				}
			}
		}

		void SetLooseIDsButton(PkgPackageJob packageJob)
		{
			LooseIDsButton.Visible = TreeUserControl.IsLoosePackageIDsSupported(packageJob);
		}

		void TreeUserControl_PackageDetailsBound(object sender, EventArgs e)
		{
			UpdateReadOnly();
			UpdateCloseButton();
		}

		bool IsValidDataSource(object dataSource)
		{
			return dataSource == null || dataSource is IPackingParent || dataSource is PkgPackageJob;
		}

		bool HasDataSourceChanged(object dataSource)
		{
			return DataSource != dataSource;
		}

		PkgPackageJob GetPackageJobForBind(object dataSource)
		{
			var result = dataSource as PkgPackageJob;
			if (result == null)
			{
				var packingParent = dataSource as IPackingParent;
				if (packingParent != null)
				{
					result = PkgPackageJob.LoadOrCreatePackageJobWithNoChanges(packingParent);
				}
			}

			return result;
		}

		#endregion

		#region Grid After Bind

		void Grid_AfterBind(object sender, EventArgs e)
		{
			AddCustomPropertyColumnsToGrid();

			UpdatePackButtonReadOnly();
			Grid.ListManager.ListChanged += delegate
			{
				UpdatePackButtonReadOnly();
			};
		}

		void AddCustomPropertyColumnsToGrid()
		{
			var dataSource = ((PkgPackageJob)Grid.DataSource).PackableItemParents;
			if (dataSource != null)
			{
				var isVisible = true;
				var isReadOnly = true;

				var customColumnsInitializer = new ZActiveGridCustomColumnsInitializer(
					Grid, dataSource, Res.GetData("f41db3c1-f1e6-4d71-96bb-3ae9aa1c6ba9", "Extra Details"), isVisible, isReadOnly);

				customColumnsInitializer.HookCollection();
			}
		}

		#endregion

		#region ViewMode

		void SetViewMode()
		{
			var parentJob = PackageJob.ParentJob as IPackingParentWithPackableItems;
			if (parentJob != null)
			{
				SetDefaultScanMode(parentJob);
			}
			else
			{
				ViewMode = PackingViewMode.PackagesOnly;
			}
		}

		void SetDefaultScanMode(IPackingParentWithPackableItems parentJob)
		{
			if (parentJob.IsScanQtyAllowed && !IsScanModeQty)
			{
				ScanQtyModeButton.PerformClick();
			}
		}

		/// <summary>
		/// PackingViewMode.Default shows both the packable items grid and the tree, as well as all packable item actions.
		/// PackingViewMode.PackagesOnly shows only the tree and disables all packable item actions.
		/// </summary>
		PackingViewMode ViewMode
		{
			set
			{
				bool isFullView = (value == PackingViewMode.Default);

				SplitContainer.Panel1.Enabled = isFullView;
				SplitContainer.Panel1Collapsed = !isFullView;
				PackButton.Visible = isFullView;
				RemoveButton.Visible = true;
				ToolStripSeparator1.Visible = isFullView;

				// move the toolbar down slightly because it looks odd pressed up to the top of the parent tab control
				int splitterTopPadding = isFullView ? 0 : 2;
				SplitContainer.Panel2.Padding = ControlDpiScalingHelper.NewScaledPadding(0, splitterTopPadding, 0, 0);

				if (!isFullView)
				{
					if (Globals.IsTest)
					{
						// the Form Basher complains about Grid Column Captions and seeing as we don't need the grid, remove the binding info
						Grid.BindTo = "";
					}

					// scanning should not be visible if the consumer does not support packing
					HideScanningGUI();
				}

				TreeUserControl.ViewMode = value;
			}
		}

		#endregion

		public bool IsBound => PackageJob != null && !PackageJob.IsDeleted; // pjob might be deleted on another form (eg whs rls)

		#endregion

		#region ReadOnly

		public bool ReadOnly
		{
			get
			{
				var parent = new Lazy<IPackingParent>(() => PackageJob.ParentJob);
				return ParentForm == null
					|| ParentForm.DisplayMode == ODisplayMode.ReadOnly
					|| ParentForm.DisplayMode == ODisplayMode.Delete
					|| PackageJob == null
					|| PackageJob.IsDeleted
					|| PackageJob.KJ_IsFinalized
					|| PackageJob.ReadOnly
					|| parent.Value == null
					|| parent.Value.IsPackingJobReadOnly;
			}
		}

		#region UpdateReadOnly

		void UpdateReadOnly()
		{
			var readOnly = ReadOnly;

			foreach (var toolStripItem in EditingControls)
			{
				toolStripItem.Enabled = !readOnly;
			}

			if (!readOnly)
			{
				GenerateIDsButton.Enabled = Tree.IsPackageJobSelected || Tree.IsPackageSelected;
				ClosePackageButton.Enabled = Tree.IsSinglePackageSelected;
				RemoveButton.Enabled = !Tree.IsPackageJobSelected && Tree.IsAnyNodeSelected;

				PackIntoSelectedPackageMenuItem.Enabled = Tree.IsPackageSelected;
				UpdatePackButtonReadOnly();
			}

			PackIntoSelectedPackageMenuItem.Text = Tree.IsSinglePackageSelected
				? Res.GetString("2cf8369a-f188-455b-aa11-b69c0ce5dd76", "...into Selected {0}", Tree.SelectedPackage.Description)
				: Res.GetString("b82def6e-323d-4d45-a3e4-879dcdf5b7ba", "...into Selected Packages");

			// these controls will cause blowups if clicked on with no DataSource
			Grid.Enabled = IsBound;
			CustomizeViewButton.Enabled = IsBound;
		}

		void UpdatePackButtonReadOnly()
		{
			PackButton.Enabled = !ReadOnly && Grid.ListManager != null && Grid.ListManager.Count > 0;
		}

		IEnumerable<ToolStripItem> EditingControls
		{
			get
			{
				return new ToolStripItem[]
				{
					AddPackageButton,
					ClosePackageButton,
					GenerateIDsButton,
					PackButton,
					RemoveButton,
					ScanPackModeButton,
					ScanQtyModeButton
				};
			}
		}

		#endregion

		#region PackageJob IsFinalised Change

		void PackageJob_IsFinalisedChanged(object sender, EventArgs e)
		{
			UpdateReadOnly();
		}

		#endregion

		#region PackageJob Parent IsReadOnly Change

		void PackageJob_ParentIsReadOnlyChanged(object sender, EventArgs e)
		{
			UpdateReadOnly();
		}

		#endregion

		#region Package Count Change

		void Packages_CountChanged(object sender, EventArgs e)
		{
			if (!PackageJob.IsMassPackageProcessRunning)
			{
				UpdateAutoPackMenuItemAvailability();
			}
		}

		void UpdateAutoPackMenuItemAvailability()
		{
			var parentJobWithPackableItems = PackageJob.ParentJob as IPackingParentWithPackableItems;
			var isAutoPackAllowed = parentJobWithPackableItems != null ? parentJobWithPackableItems.IsAutoPackAllowed : YesNoWithReasonForNo.No(PkgPackageJob.NothingToAutoPackMessage);
			var isAutoPackAllowedAndNoPackagesExist = isAutoPackAllowed && PackageJob.Packages.Count == 0;

			string caption;

			if (isAutoPackAllowedAndNoPackagesExist)
			{
				caption = Res.GetString("f575a489-d053-4df1-9930-df7daa052b24", "Auto-Pack");
			}
			else if (!isAutoPackAllowed)
			{
				caption = isAutoPackAllowed.ReasonForNotAllowed;
			}
			else // packages already exist
			{
				caption = Res.GetString("bf96f0e0-820a-47a8-a9d6-f8ae25a72662", "Remove Packages to enable Auto-Pack");
			}

			AutoPackToolStripMenuItem.Text = caption;
			AutoPackToolStripMenuItem.Enabled = isAutoPackAllowedAndNoPackagesExist;
		}

		#endregion

		#region PackageJob_MassPackageProcessFinished

		void PackageJob_MassPackageProcessFinished(object sender, EventArgs e)
		{
			UpdateAutoPackMenuItemAvailability();
		}

		#endregion

		#endregion

		#region Outer Package Added

		void PackageJob_OuterPackageAdded(object sender, PackageEventArgs e)
		{
			if (!AddingNodesSuspender.IsAddingNodesSuspended(e.Package.Factory))
			{
				var topNode = Tree.TopNode;
				Tree.PopulateFromPackage(e.Package, topNode);

				if (!topNode.IsExpanded)
				{
					topNode.Expand();
				}

				topNode.Paint();
			}
		}

		#endregion

		#region Scanning

		#region OnVisible - Enable Scanning

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				//SetLooseIDsButton();
				OnVisible();
			}
		}

		void OnVisible()
		{
			HookPackageJobEvents();

			if (!IsScanningEnabled && ParentForm != null && IsBound)
			{
				IsScanningEnabled = true;

				// don't handle scan if packing tab is not visible or packing is finalised
				Func<bool> canScan = () => TreeUserControl.ViewMode == PackingViewMode.Default && Visible && !PackageJob.KJ_IsFinalized && !ReadOnly;

				// enable scanning
				ParentForm.EnableScanning(Barcodes, canScan, TreeUserControl.TreeSplitContainer.Panel1);
				Tree.ActionsHelper.ScanningNotification += (sender, e) => ParentForm.ShowScanningMessage(e.Message, e.NotifyType, e.ShowOkButton);
			}
		}

		bool IsScanningEnabled;

		#endregion

		#region HideScanningGUI

		void HideScanningGUI()
		{
			ScanQtyModeButton.Visible = false;
			ScanPackModeButton.Visible = false;
			ClosePackageButton.Visible = false;
		}

		#endregion

		#region Barcodes

		BarcodeManager Barcodes
		{
			get
			{
				if (barcodes == null)
				{
					barcodes = new BarcodeManager();
					barcodes.AddBarcode(PackingBarcodes.AddOuter, HandleAddOuter);
					barcodes.AddBarcode(PackingBarcodes.AddInner, HandleAddInnerViaScan);
					barcodes.AddBarcode(PackingBarcodes.ClosePackage, ClosePackageViaScan);
					barcodes.AddBarcode(PackingBarcodes.OpenPackage, OpenPackage);
					barcodes.AddBarcode(PackingBarcodes.ChangePackMode, ChangePackModeViaScan);
					barcodes.AddBarcode(PackingBarcodes.ChangeScanMode, ChangeScanModeViaScan);
					barcodes.NonSystemBarcodeScanned += NonSystemBarcodeScanned;
				}
				return barcodes;
			}
		}

		BarcodeManager barcodes;

		#endregion

		void NonSystemBarcodeScanned(object sender, BarcodeScanEventArgs e)
		{
			Tree.ActionsHelper.NonSystemBarcodeScanned(e, TreeUserControl.PackageDetailControl.IsPackageIDBoxFocused, IsScanModePacking, IsScanModeQty);
		}

		#endregion

		#region Printing

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void PackageJob_AutoPrinting(object sender, AutoPrintingEventArgs e)
		{
			if (e.Package != null && ParentForm != null)
			{
				ParentForm.ShowScanningMessage(Res.GetString("39e6a6f6-6441-408b-a815-6dd6bda6c3a4", "Printing label(s) for {0} {1}.",
					e.Package.Description, e.Package.KP_PackageID), NotificationTypes.None, showOkButton: false);

				Application.DoEvents(); // this is crap but there's no other solution as we are single-threaded.
			}
		}

		void PackageJob_AutoPrinted(object sender, EventArgs e)
		{
			if (ParentForm != null)
			{
				ParentForm.HideScanningMessage();
			}
		}

		void PackageJob_AutoPrintFailed(object sender, AutoPrintFailedEventArgs e)
		{
			// we want this to popup and interrupt even when scanning so don't use ShowScanningMessage() here.
			if (e.CanContinueWithManualPrint)
			{
				ShowMessageBox(e.Message, Res.GetString("PackingUserControl|Warning", "Warning"), MessageBoxIcon.Warning);
			}
			else
			{
				ShowMessageBox(e.Message, Res.GetString("PackingUserControl|Error", "Error"), MessageBoxIcon.Error);
			}
		}

		DialogResult ShowMessageBox(string message, string caption, MessageBoxIcon icon)
		{
			var result = DialogResult.None;
			using (var msgBox = new MessageBoxForPacking(message, caption, icon))
			{
				result = ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
			}

			return result;
		}

		#endregion

		#region Actions

		#region Add Package

		void AddPackageButton_Click(object sender, EventArgs e)
		{
			HandleAddOuter();
		}

		void HandleAddOuter()
		{
			TreeUserControl.AddNewPackage();
		}

		void HandleAddInnerViaScan()
		{
			// when scanning we want to add multiple inners to a package with each ADD_INNER scan, hence nestNewPackage: false
			TreeUserControl.AddNewInner(nestNewPackage: false);
		}

		#endregion

		#region Generate IDs

		void GenerateIDsButton_Click(object sender, EventArgs e)
		{
			TreeUserControl.GenerateIDsMenuItem.PerformClickEnableFirst();
		}

		#endregion

		#region Open / Close Package

		void OpenPackage()
		{
			TreeUserControl.OpenPackage();
		}

		void ClosePackageButton_Click(object sender, EventArgs e)
		{
			TreeUserControl.TogglePackageOpenClose();
		}

		void ClosePackageViaScan()
		{
			TreeUserControl.ClosePackage(isScanning: true);
		}

		void TreeUserControl_PackageIsClosedChanged(object sender, EventArgs e)
		{
			UpdateCloseButton();
		}

		void UpdateCloseButton()
		{
			if (Tree.IsSinglePackageSelected)
			{
				if (Tree.SelectedPackage.IsClosed)
				{
					ClosePackageButton.Checked = true;
					ClosePackageButton.Image = Properties.Resources.ClosePack.ToBitmap();
					ClosePackageButton.CaptionResourceString = Res.GetData("22acd523-4bee-4cf8-9a00-d9691f0defc4", "", "Package is Closed. Click to Open.");
				}
				else
				{
					ClosePackageButton.Checked = false;
					ClosePackageButton.Image = Properties.Resources.OpenPack.ToBitmap();
					ClosePackageButton.CaptionResourceString = Res.GetData("db74d1fa-9e83-4ecc-94c2-b52e03a8772d", "", "Package is Open. Click to Close.");
				}
			}
		}

		#endregion

		#region Pack

		// pack

		void PackIntoSelectedPackageMenuItem_Click(object sender, EventArgs e)
		{
			Tree.ActionsHelper.Pack(Grid.SelectedElements.Cast<PackableItemParentWrapper>().Select(w => w.PackableItemParent), Tree.SelectedPackages.ToArray());
		}

		void PackIntoNewPackagesMenuItem_Click(object sender, EventArgs e)
		{
			Tree.ActionsHelper.Pack(Grid.SelectedElements.Cast<PackableItemParentWrapper>().Select(w => w.PackableItemParent), Array.Empty<PkgPackage>());
		}

		void Tree_PackOrUnpackSucceeded(object sender, EventArgs e)
		{
			Grid.Refresh();
		}

		// auto-pack

		void AutoPackToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Tree.ActionsHelper.AutoPack(this);
			Grid.Refresh();
		}

		#endregion

		#region Unpack

		void RemoveButton_Click(object sender, EventArgs e)
		{
			TreeUserControl.RemoveMenuItem.PerformClickEnableFirst();
		}

		// drag/drop remove

		void Grid_DragEnter(object sender, DragEventArgs e)
		{
			if (!ReadOnly)
			{
				e.Effect = DragDropEffects.Move;
			}
		}

		void Grid_DragDrop(object sender, DragEventArgs e)
		{
			HandleGridDrop(e);
		}

		protected void HandleGridDrop(DragEventArgs e)
		{
			// if we don't check for Tree.IsDragging, we may be dragging from the grid to itself (bad).
			if (!ReadOnly && Tree.IsDragging && e.Data.GetDataPresent(typeof(ArrayList)))
			{
				RemoveButton.PerformClick();
			}
		}

		// updating the grid when the tree has nodes removed

		void TreeUserControl_Remove(object sender, EventArgs e)
		{
			Grid.Refresh();
		}

		#endregion

		#region Loose IDs

		void LooseIDsButton_Click(object sender, EventArgs e)
		{
			TreeUserControl.ShowLoosePackageIDsGrid(!LooseIDsButton.Checked);
			LooseIDsButton.Checked = !LooseIDsButton.Checked;
		}

		#endregion

		#region Package Detail

		void PackageDetailButton_Click(object sender, EventArgs e)
		{
			TreeUserControl.PackageDetailMenuItem.PerformClickEnableFirst();
		}

		void PackageDetailMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			PackageDetailButton.Checked = TreeUserControl.PackageDetailMenuItem.Checked;
		}

		#endregion

		#region Customize View

		void CustomizeViewButton_Click(object sender, EventArgs e)
		{
			TreeUserControl.CustomizeViewMenuItem.PerformClickEnableFirst();
		}

		#endregion

		#region Change Pack Mode

		void ScanPackModeButton_Click(object sender, EventArgs e)
		{
			ChangePackMode();
		}

		void ChangePackModeViaScan()
		{
			ScanPackModeButton.PerformClick();
		}

		void ChangePackMode()
		{
			if (ScanPackModeButton.Checked)
			{
				ScanPackModeButton.Text = Res.GetString("4c50c8b7-3f53-4d9e-bb38-80ed224ceae7", "Mode: Unpacking");
				ScanPackModeButton.ForeColor = Color.Red;
			}
			else
			{
				ScanPackModeButton.Text = Res.GetString("f4dbe61c-9b8e-4ece-9a9f-1ecb628bef03", "Mode: Packing");
				ScanPackModeButton.ForeColor = SystemColors.WindowText;
			}
		}

		bool IsScanModePacking => !ScanPackModeButton.Checked;

		#endregion

		#region Change Scan Mode

		bool IsChangingScanModeViaScanner;

		void ChangeScanModeViaScan()
		{
			IsChangingScanModeViaScanner = true;

			try
			{ ScanQtyModeButton.PerformClick(); }
			finally { IsChangingScanModeViaScanner = false; }
		}

		void ScanQtyModeButton_Click(object sender, EventArgs e)
		{
			var isScanQtyAllowed = ((IPackingParentWithPackableItems)PackageJob.ParentJob).IsScanQtyAllowed;
			if (isScanQtyAllowed)
			{
				ScanQtyModeButton.Text = ScanQtyModeButton.Checked
					? Res.GetString("aa76e7df-97fe-4d5e-8fb5-e56d52f3e489", "Scan: Qty")
					: Res.GetString("f70e950c-2104-44c4-b088-df9a596cfb11", "Scan: All");
			}
			else
			{
				ScanQtyModeButton.Checked = false;
				ScanQtyModeButton.Text = Res.GetString("f70e950c-2104-44c4-b088-df9a596cfb11", "Scan: All"); // in case mode was Qty then user changed security

				if (IsChangingScanModeViaScanner)
				{
					ParentForm.ShowScanningMessage(isScanQtyAllowed.ReasonForNotAllowed, NotificationTypes.Error);
				}
				else
				{
					Globals.Message.ShowError(isScanQtyAllowed.ReasonForNotAllowed);
				}
			}
		}

		bool IsScanModeQty => ScanQtyModeButton.Checked;

		#endregion

		#region IsGridDragging

		public bool IsGridDragging => Grid != null && Grid.IsDragging;

		#endregion

		#endregion

		#region Delete

		#region Package Delete Cancelled by Consumer

		void PackageJob_PackageDeleteCancelled(object sender, PackageCanceledActionEventArgs e)
		{
			this.AddError(e.ReasonForNotAllowingAction);
		}

		#endregion

		#region PackageJob Deleted by DataRefreshBus

		void PackageJob_Deleted(object sender, EventArgs e)
		{
			// If the packagejob is deleted, rebind to create a new packagejob without changes.
			// This allows the gui to continue to function, and will show packable items (whereas binding
			// via collection would make the gui readonly and not be able to show the packable items).
			SetDataBinding(ParentJobInCaseOfDelete, "");
			OnPackageJobDelete();
		}

		void OnPackageJobDelete()
		{
			PackageJobDelete?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler PackageJobDelete;
		IPackingParent ParentJobInCaseOfDelete;

		#endregion

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				UnhookEvents();
			}
		}

		#endregion

		//

		#region INotifications

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion

		#region INotificationSubscriberQueryUser

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			var args = (QueryUserMsgBoxEventArgs)e;
			args.Response = Globals.Message.Show(args.Message, args.Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		#endregion
	}

	public enum PackingViewMode
	{
		Default,
		PackagesOnly,
		NoEditing
	}
}

