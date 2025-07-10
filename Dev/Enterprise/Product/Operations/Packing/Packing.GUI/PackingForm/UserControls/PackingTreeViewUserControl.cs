using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.GUI
{
	partial class PackingTreeViewUserControl : ZUserControl, INotifications, INotificationSubscriberQueryUser
	{
		#region Construction

		public PackingTreeViewUserControl()
		{
			InitializeComponent();
#if DEBUG
			AddAddEventMenuItem();
#endif

			HookEvents();

			BreakDownPackageMenuItem.Text = BreakDownPackage.BreakDownPackageDescription;
			CaptionRenderingEnabled = true;
			BindOnFirstVisible = true;
			LoadInitialTreeLayout = true;
		}

		void HideScanningGUI()
		{
			PackingMenuStrip.Items.Remove(ClosePackageMenuItem);
			PackingMenuStrip.Items.Remove(ReleasePackageMenuItem);
			PackingMenuStrip.Items.Remove(ToolStripSeparator1);
		}

		public new ZForm ParentForm => (ZForm)base.ParentForm;

		PkgPackageJob PackageJob => (PkgPackageJob)DataSource;

		#region AddAddEventMenuItem
#if DEBUG
		void AddAddEventMenuItem()
		{
			if (AddEventMenuItem == null)
			{
				AddEventMenuItem = new ZToolStripMenuItem(Res.GetString("980c37e1-94c7-4bae-917c-74c873b6756f", "Add Event for Testing"), AddEventMenuItem_Click);
				PackingMenuStrip.Items.Add(AddEventMenuItem);
			}
		}

		ZToolStripMenuItem AddEventMenuItem;
#endif
		#endregion

		#endregion

		#region Events

		void HookEvents()
		{
			Tree.AfterSelectOrDeselect += TreeView_AfterSelectOrDeselect;
			Tree.DeleteNodes += new EventHandler(Tree_DeleteNodes);
			Tree.SelectedPackageNodeQtyOrTypeChanged += new EventHandler(TreeView_SelectedPackageNodeQtyOrTypeChanged);

			PackingMenuStrip.Opening += new CancelEventHandler(PackingMenuStrip_Opening);

			PackageDetailControl.PackageIsClosedChanged += new EventHandler(PackageDetailControl_PackageIsClosedChanged);
		}

		void UnhookEvents()
		{
			if (Tree != null)
			{
				Tree.AfterSelectOrDeselect -= TreeView_AfterSelectOrDeselect;
				Tree.DeleteNodes -= new EventHandler(Tree_DeleteNodes);
				Tree.SelectedPackageNodeQtyOrTypeChanged -= new EventHandler(TreeView_SelectedPackageNodeQtyOrTypeChanged);
			}

			if (PackingMenuStrip != null)
			{
				PackingMenuStrip.Opening -= new CancelEventHandler(PackingMenuStrip_Opening);
			}

			if (PackageDetailControl != null)
			{
				PackageDetailControl.PackageIsClosedChanged -= new EventHandler(PackageDetailControl_PackageIsClosedChanged);
			}
		}

		#endregion

		#region Bind

		#region Bind

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (!BindOnFirstVisible)
			{
				Bind();
			}
		}

		public void Bind()
		{
			BindOnFirstVisible = false;

			Tree.Populate(PackageJob);

			if (PackageJob != null)
			{
				Tree.SelectedNode = Tree.TopNode;

				if (PackageJob.ParentJob != null)
				{
					ViewJobMenuItem.Text = Res.GetString("db67bca6-3163-4390-8ecf-8b50dd250a70", "View {0}", PackageJob.ParentJob.JobDescription);

					var isPackageIDsSupported = IsLoosePackageIDsSupported(PackageJob);
					ShowLoosePackageIDsGrid(isPackageIDsSupported);
					if (isPackageIDsSupported)
					{
						PackageIDsGrid.SetDataBinding(DataSource, "LoosePackagePivots");
						PackageIDsGrid.ContextMenu.Popup += (sender, e) =>
						{
							SetCurrentPackageJob((PkgPackageJobPackageHeaderPivot)PackageIDsGrid.SelectedElements.FirstOrDefault());
						};
					}
				}
			}
			else
			{
				Tree.NodeSelector.DeselectAllNodes();
				BindPackageDetails();
			}
		}

		void SetCurrentPackageJob(PkgPackageJobPackageHeaderPivot id)
		{
			if (id != null)
			{
				id.PackageHeader.CurrentPackageJob = PackageJob;
			}
		}

		#endregion

		#region ShowLoosePackageIDsGrid

		public void ShowLoosePackageIDsGrid(bool show = true)
		{
			TreeGridSplitContainer.Panel2Collapsed = !show;
		}

		#endregion

		#region IsLoosePackageIDsSupported

		public bool IsLoosePackageIDsSupported(PkgPackageJob packageJob)
		{
			return (packageJob?.ParentJob?.IsLoosePackageIDsSupported ?? false) && Env.CurrentUser.IsSupportUser;
		}

		#endregion

		#region BindPackageDetails

		/// <summary>
		/// Updates the package details pane by binding to the current (selected) TreeNode.
		/// </summary>
		void BindPackageDetails()
		{
			var resetFocus = false;
			if (Tree.ContainsFocus)
			{
				resetFocus = true;
			}

			PackageDetailControl.Bind(PackageJob, Tree.SelectedPackage);

			// If binding gave focus to the PackageDetailControl's TabControl, this refocuses on the
			// tree to allow navigation to continue. This can happen if for example the Seals and
			// Temperature Tab was selected and then a Package was selected through arrow key movement.
			if (!Tree.ContainsFocus && resetFocus)
			{
				Tree.Focus();
			}
			OnPackageDetailsBound();
		}

		void OnPackageDetailsBound()
		{
			if (PackageDetailsBound != null)
			{
				PackageDetailsBound(this, EventArgs.Empty);
			}
		}

		public event EventHandler PackageDetailsBound;

		#endregion

		#region OnVisible

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				OnVisible();
			}
		}

		void OnVisible()
		{
			if (IsBound) // DataSource is null if we are dependant and the parent job collection is empty (eg Release has no Orders).
			{
				if (ViewMode == PackingViewMode.Default && !(PackageJob.ParentJob is IPackingParentWithPackableItems))
				{
					ViewMode = PackingViewMode.PackagesOnly;
				}

				if (BindOnFirstVisible)
				{
					Bind();
				}

				if (LoadInitialTreeLayout)
				{
					const bool UpdateTree = true;
					Tree.ActionsHelper.LoadTreeSummaryLayout(UpdateTree);
					LoadInitialTreeLayout = false;
				}
			}
		}

		bool LoadInitialTreeLayout;

		#endregion

		bool IsBound => PackageJob != null && !PackageJob.IsDeleted;
		bool BindOnFirstVisible;

		#endregion

		#region ReadOnly

		public bool ReadOnly => IsBound && (ParentForm is PackingPopupDialog || (ParentPackageControl != null && ParentPackageControl.ReadOnly));

		PackingUserControl ParentPackageControl => parentPackageControl ?? (parentPackageControl = this.GetParent<PackingUserControl>());
		PackingUserControl parentPackageControl;

		#region Selected TreeNode Change

		void TreeView_SelectedPackageNodeQtyOrTypeChanged(object sender, EventArgs e)
		{
			BindPackageDetails();
		}

		void TreeView_AfterSelectOrDeselect(object sender, EventArgs e)
		{
			BindPackageDetails();
		}

		#endregion

		#endregion

		#region ViewMode

		public PackingViewMode ViewMode
		{
			get { return viewMode; }
			set
			{
				viewMode = value;
				PackageDetailControl.SetViewMode(value);

				switch (viewMode)
				{
					case PackingViewMode.Default:
					case PackingViewMode.PackagesOnly:

						bool isFullView = (value == PackingViewMode.Default);
						RemoveMenuItem.Visible = isFullView;
						RemoveMenuItemSeparator1.Visible = isFullView;

						if (!isFullView)
						{
							HideScanningGUI();
						}

						foreach (var toolStripItem in EditingControls)
						{
							toolStripItem.Visible = true;
						}

						break;

					case PackingViewMode.NoEditing:

						foreach (var toolStripItem in EditingControls)
						{
							toolStripItem.Visible = false;
						}

						UpdateViewJobMenuItemVisibility();

						break;

					default:
						throw new ArgumentException("");
				}
			}
		}

		PackingViewMode viewMode;

		#endregion

		#region Actions

		#region View Job

		void ViewJobMenuItem_Click(object sender, EventArgs e)
		{
			EditParentJob();
		}

		void EditParentJob()
		{
			var parentJob = PackageJob.ParentJob;

			if (parentJob != null)
			{
				var parentJobAsBizO = parentJob as BusinessObject;
				if (parentJobAsBizO != null)
				{
					var controller = ZControllerFactory.Create(parentJob.ControllerID);
					controller.ShowEditForm(parentJobAsBizO);

					#region Test
#if DEBUG
					ControllerForTest = controller;
#endif
					#endregion
				}
			}
		}

		#region Test
#if DEBUG
		internal ZController ControllerForTest;
#endif
		#endregion

		#endregion

		#region Add Event for Testing
#if DEBUG

		void AddEventMenuItem_Click(object sender, EventArgs e)
		{
			ShowAddEventForm();
		}

		void ShowAddEventForm()
		{
			if (Tree.IsSinglePackageSelected)
			{
				var selectedPackage = Tree.SelectedPackage;
				ZFormModaliser.ShowDialogAndDispose(new ZStmALogAddForm(new StmALogCollectionView(selectedPackage), selectedPackage.HasChanges));
			}
		}

#endif
		#endregion

		#region Add Package

		void AddPackageMenuItem_Click(object sender, EventArgs e)
		{
			AddNewInner();
		}

		public void AddNewInner(bool nestNewPackage = true)
		{
			Tree.ActionsHelper.AddNewPackageAsInner(nestNewPackage);
			FocusOnPackageQty();
		}

		public void AddNewPackage()
		{
			Tree.ActionsHelper.AddNewPackage();
			FocusOnPackageQty();
		}

		#endregion

		#region Generate IDs

		void GenerateIDsMenuItem_Click(object sender, EventArgs e)
		{
			GenerateIDs(SSCCGenerationContext.GeneratingIDsViaUser);
		}

		void GenerateIDs(SSCCGenerationContext context)
		{
			// use non-popup notifications if we are scanning
			INotifications notify = this;
			if (context == SSCCGenerationContext.ScanPacking)
			{
				notify = new ScanningNotifications();
				((ScanningNotifications)notify).NotificationAdded += delegate(object sender, ScanningNotificationsEventArgs e)
				{
					ParentForm.ShowScanningMessage(e.Message, NotificationTypes.Error);
				};
			}

			bool canGenerateIDs = !ParentForm.BusinessEntityForHasChanges.HasChanges;

			// auto-save if necessary (IDs can only be generated for a saved job)
			if (!canGenerateIDs)
			{
				ParentForm.FireSaveButton();
				canGenerateIDs = !ParentForm.BusinessEntityForHasChanges.HasChanges;
			}

			// generate the IDs
			if (canGenerateIDs)
			{
				Tree.BeginUpdate();

				try
				{
					PackageIDGenerator.GenerateIDsForAllPackagesAndConsumeFountainImmediately_DoNotUse(PackageJob, Tree.SelectedPackagesOrAllIfPackageJobSelected, notify, context);
				}
				finally
				{
					Tree.EndUpdate();
				}
			}
			else // auto-save had failed
			{
				notify.AddError(Res.GetString("f55ea4b8-7d37-4b23-9166-217159a3e61b", "Fix any Errors then Save before attempting to Generate IDs."));
			}
		}

		#region ScanningNotifications

		class ScanningNotificationsEventArgs : EventArgs
		{
			public ScanningNotificationsEventArgs(ZString message)
			{
				Message = message;
			}

			public readonly ZString Message;
		}

		class ScanningNotifications : INotifications
		{
			public event EventHandler<ScanningNotificationsEventArgs> NotificationAdded;

			public void Add(INotification notification)
			{
				if (NotificationAdded != null)
				{
					NotificationAdded(this, new ScanningNotificationsEventArgs(notification.Message));
				}
			}
		}

		#endregion

		#endregion

		#region Clear IDs

		void ClearIDsMenuItem_Click(object sender, EventArgs e)
		{
			ClearIDs();
		}

		void ClearIDs()
		{
			var msg = "\r\n" + Res.GetString("6f6998ce-b969-4309-834a-75dd818504d3",
				"Are you sure you wish to clear the IDs for the Selected Package and its inner-Packages?\r\n\r\nContainer IDs and Tracking Numbers from Carriers will *not* be cleared.");
			var caption = Res.GetString("740ae0e1-a35e-4895-9a10-1d81fc4a2512", "Warning!");

			if (Globals.Message.Show(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes)
			{
				var selectedPackages = Tree.SelectedPackagesOrAllIfPackageJobSelected;
				PackageIDGenerator.ClearIDs(selectedPackages, this);
			}
		}

		#endregion

		#region Break Down Packages

		void BreakDownPackagesMenuItem_Click(object sender, EventArgs e)
		{
			Tree.ActionsHelper.BreakDownPackages();
		}

		#endregion

		#region Remove

		void Tree_DeleteNodes(object sender, EventArgs e)
		{
			RemoveSelectedItems();
		}

		void RemoveMenuItem_Click(object sender, EventArgs e)
		{
			RemoveSelectedItems();
		}

		void RemoveSelectedItems()
		{
			if (!ReadOnly)
			{
				if (Tree.ActionsHelper.Unpack())
				{
					OnRemove();
				}
			}
		}

		void OnRemove()
		{
			if (Remove != null)
			{
				Remove(this, EventArgs.Empty);
			}
		}

		public event EventHandler Remove;

		#endregion

		#region Open / Close Package

		public void TogglePackageOpenClose()
		{
			if (Tree.IsSinglePackageSelected)
			{
				if (Tree.SelectedPackage.IsClosed)
				{
					OpenPackage();
				}
				else
				{
					ClosePackage();
				}
			}
		}

		public void OpenPackage()
		{
			if (Tree.IsSinglePackageSelected)
			{
				Tree.SelectedPackage.KP_ClosedTimeUtc = ZDateTime.Empty;
			}
			else // cannot actually click open if multi-packages selected, must be scanning *OPEN_PACKAGE*
			{
				ParentForm.ShowScanningMessage(Res.GetString("2fe4e487-ed27-4834-b541-74c1ff053ae8", "Select a Single Package to Open and then try again."), NotificationTypes.Error);
			}
		}

		public void ClosePackage(bool isScanning = false)
		{
			if (Tree.IsSinglePackageSelected)
			{
				var package = Tree.SelectedPackage;

				if (!package.IsClosed)
				{
					package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
				}

				if (!package.IsContainer && package.KP_PackageID.IsEmpty) // do we print container labels?
				{
					GenerateIDs(isScanning ? SSCCGenerationContext.ScanPacking : SSCCGenerationContext.GeneratingIDsViaUser);
				}

				if (PackageJob.ParentJob.IsAutoPrintAllowed)
				{
					package.PrintLabel();
				}
			}
			else if (isScanning)
			{
				ParentForm.ShowScanningMessage(Res.GetString("151a64ef-3565-4a0e-9134-dce1d40ae3a2", "Select a Single Package to Close and then try again."), NotificationTypes.Error);
			}
		}

		void ClosePackageMenuItem_Click(object sender, EventArgs e)
		{
			TogglePackageOpenClose();
		}

		#region PackageIsClosedChanged

		public event EventHandler PackageIsClosedChanged;

		void PackageDetailControl_PackageIsClosedChanged(object sender, EventArgs e)
		{
			PackageIsClosedChanged?.Invoke(sender, e);
		}

		#endregion

		#endregion

		#region Release Package

		void ReleasePackageMenuItem_Click(object sender, EventArgs e)
		{
			ReleasePackage();
		}

		void ReleasePackage()
		{
			if (Tree.IsSinglePackageSelected)
			{
				var packingParent = PackageJob.ParentJob;
				if (!Tree.SelectedPackage.IsReleased
					&& packingParent != null
					&& !packingParent.CanReleasePackage(Tree.SelectedPackage))
				{
					INotifications notify = this;
					notify.AddError(packingParent.GetCannotReleasePackageMessage(Tree.SelectedPackage));
				}
				else
				{
					Tree.SelectedPackage.KP_ReleasedTimeUtc = Tree.SelectedPackage.IsReleased ? ZDateTime.Empty : ZDateTime.UtcNow;
				}
			}
		}

		#endregion

		#region Hold Package

		void HoldPackageMenuItem_Click(object sender, EventArgs e)
		{
			HoldPackage();
		}

		void RemoveHoldAllPackagesMenuItem_Click(object sender, EventArgs e)
		{
			if (Tree.IsPackageJobSelected)
			{
				Tree.PackageJob.GetAllPackagesOnJob().ForEach(p => p.KP_IsHeld = false);
				RefreshPackageNode(Tree.SelectedNode);
			}
		}

		void HoldPackage()
		{
			if (Tree.IsSinglePackageSelected)
			{
				var selectedPackage = Tree.SelectedPackage;
				selectedPackage.KP_IsHeld = !selectedPackage.KP_IsHeld;
				RefreshPackageNode(Tree.SelectedNode);
			}
		}

		#endregion

		#region Print Package Label

		void PrintPackageLabelMenuItem_Click(object sender, EventArgs e)
		{
			if (Tree.IsSinglePackageSelected)
			{
				var selectedPackage = Tree.SelectedPackage;
				INotifications notify = this;

				var result = selectedPackage.PrintCarrierLabel();
				if (!result.Success)
				{
					notify.AddError(result.Message);
				}
			}
		}

		#endregion

		#region Cancel Package Label

		void CancelPackageLabelMenuItem_Click(object sender, EventArgs e)
		{
			INotifications notify = this;
			var result = PackageJob.CancelPackageCarrierLabel(Tree.SelectedPackages);

			if (result.Success)
			{
				notify.AddInformation(Res.GetString("9403d3d7-da78-4865-9890-0774af87245a", "Packages have been marked for package label cancellation. Please save the package job for the cancellation to take effect."));
			}
			else
			{
				notify.AddError(result.Message);
			}
		}

		#endregion

		#region Package Detail

		void PackageDetailMenuItem_Click(object sender, EventArgs e)
		{
			UpdatePackageDetailPanelVisibility(!PackageDetailMenuItem.Checked);
		}

		void UpdatePackageDetailPanelVisibility(bool isVisible)
		{
			PackageDetailMenuItem.Checked = isVisible;
			TreeSplitContainer.Panel2Collapsed = !isVisible;
		}

		#endregion

		#region Customize View

		void CustomizeViewMenuItem_Click(object sender, EventArgs e)
		{
			Tree.ActionsHelper.CustomizeView();
		}

		#endregion

		#region Expand / Collapse

		void ExpandMenuItem_Click(object sender, EventArgs e)
		{
			ExpandOrCollapseSelectedNodes(n => n.Expand());
		}

		void CollapseMenuItem_Click(object sender, EventArgs e)
		{
			ExpandOrCollapseSelectedNodes(n => n.Collapse());
		}

		void ExpandOrCollapseSelectedNodes(Action<TreeNode> performActionOnNode)
		{
			foreach (var node in Tree.SelectedNodes)
			{
				performActionOnNode(node);
			}
		}

		#endregion

		#endregion

		#region Opening the PopUp Menu

		void PackingMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			OnPackingMenuStripOpening();
		}

		void OnPackingMenuStripOpening()
		{
			AddDocumentsMenu();
			UpdatePopupMenu();
		}

		#region UpdatePopupMenu

		void UpdatePopupMenu()
		{
			if (ReadOnly)
			{
				foreach (var toolStripItem in EditingControls)
				{
					toolStripItem.Enabled = false;
				}
			}
			else
			{
				AddPackageMenuItem.Enabled = Tree.IsPackageJobSelected || Tree.IsSinglePackageSelected;
#if DEBUG
				AddEventMenuItem.Enabled = Tree.IsSinglePackageSelected;
#endif
				GenerateIDsMenuItem.Enabled = Tree.IsPackageJobSelected || Tree.IsPackageSelected;
				ClearIDsMenuItem.Enabled = GenerateIDsMenuItem.Enabled;
				BreakDownPackageMenuItem.Enabled = Tree.IsPackageSelected;
				ClosePackageMenuItem.Enabled = Tree.IsSinglePackageSelected;
				ReleasePackageMenuItem.Enabled = Tree.IsSinglePackageSelected && Tree.SelectedPackage.IsOuter;
				HoldPackageMenuItem.Enabled = Tree.IsSinglePackageSelected;
				RemoveHoldAllPackagesMenuItem.Visible = Tree.IsPackageJobSelected;
				RemoveMenuItem.Enabled = !Tree.IsPackageJobSelected && Tree.IsAnyNodeSelected;
				PrintPackageLabelMenuItem.Enabled = Tree.IsSinglePackageSelected;
				CancelPackageLabelMenuItem.Enabled = Tree.IsPackageSelected;
			}

			ExpandMenuItem.Enabled = IsBound && Tree.SelectedNodes.Any(n => !n.IsExpanded);
			CollapseMenuItem.Enabled = IsBound && Tree.SelectedNodes.Any(n => n.IsExpanded);
			CustomizeViewMenuItem.Enabled = IsBound;

			UpdateViewJobMenuItemVisibility();
			UpdateCloseMenuItem();
			UpdateReleaseMenuItem();
			UpdateHoldMenuItem();
			UpdatePrintPackageLabelMenuItem();
			UpdateCancelPackageLabelMenuItem();
		}

		void UpdateViewJobMenuItemVisibility()
		{
			// only show the view job menu item if a. the selected node is the PackageJob, and b. we are not plugged into the parent job form (eg Order on OrderForm)
			var showViewJobMenuItem = (Tree.IsPackageJobSelected && PackageJob != null && ParentForm != null && PackageJob.ParentJob != ParentForm.BusinessEntity && !(PackageJob.ParentJob is PkgHandlingUnit));
			ViewJobMenuItem.Visible = ViewJobMenuItemSeparator.Visible = showViewJobMenuItem;

			AddPackageMenuItem.CaptionResourceString = Tree.IsPackageJobSelected
				? Res.GetData("5FF3BA1A-7189-4D3E-8211-5EB419641C1C", "Add Package")
				: Res.GetData("CC6CA918-13D7-4579-B880-FE5AB9D955A9", "Add Inner Package");
		}

		void UpdateCloseMenuItem()
		{
			if (!Tree.IsSinglePackageSelected)
			{
				ClosePackageMenuItem.Text = Res.GetString("3a60c98f-d590-4146-9107-227c7d1700fe", "Select a Single Package to Close");
			}
			else
			{
				ClosePackageMenuItem.Checked = Tree.SelectedPackage.IsClosed;

				if (ClosePackageMenuItem.Checked)
				{
					ClosePackageMenuItem.Image = Properties.Resources.ClosePack.ToBitmap();
					ClosePackageMenuItem.Text = Res.GetData("22acd523-4bee-4cf8-9a00-d9691f0defc4", "", "Package is Closed. Click to Open.").FullDescription;
				}
				else
				{
					ClosePackageMenuItem.Image = Properties.Resources.OpenPack.ToBitmap();
					ClosePackageMenuItem.Text = Res.GetData("db74d1fa-9e83-4ecc-94c2-b52e03a8772d", "", "Package is Open. Click to Close.").FullDescription;
				}
			}
		}

		void UpdateReleaseMenuItem()
		{
			if (!Tree.IsSinglePackageSelected)
			{
				ReleasePackageMenuItem.Text = Res.GetString("0b26e9e5-9b2c-434b-b9b1-dd936faf680a", "Select a Single Package to Release");
			}
			else
			{
				ReleasePackageMenuItem.Checked = Tree.SelectedPackage.IsReleased;

				if (ReleasePackageMenuItem.Checked)
				{
					ReleasePackageMenuItem.Text = Res.GetString("de4e233e-896d-4e6e-aed4-edd01fdde59b", "Package is Released. Click to Cancel Release.");
				}
				else
				{
					ReleasePackageMenuItem.Text = Res.GetString("cc2a1d9a-1e08-4224-a467-523952f24468", "Release Package");
				}
			}
		}

		void UpdateHoldMenuItem()
		{
			if (!Tree.IsSinglePackageSelected)
			{
				HoldPackageMenuItem.Text = Res.GetString("31587DF9-BA6E-46FD-B119-A8B0F95C630E", "Select a Single Package to Hold");
			}
			else
			{
				HoldPackageMenuItem.Checked = Tree.SelectedPackage.KP_IsHeld;

				if (HoldPackageMenuItem.Checked)
				{
					HoldPackageMenuItem.Text = Res.GetString("982DF79E-6E22-4BFB-9596-FA9D63C65F38", "Remove Hold");
				}
				else
				{
					HoldPackageMenuItem.Text = Res.GetString("E1C86814-8B4F-4095-9E6B-E4E14899DEB3", "Hold");
				}
			}
		}

		void UpdatePrintPackageLabelMenuItem()
		{
			if (Tree.IsSinglePackageSelected)
			{
				var package = Tree.SelectedPackage;
				PrintPackageLabelMenuItem.Visible = package.CanPrintCarrierLabel;
				PrintPackageLabelMenuItem.Text = package.IsSentToRTUS
					? Res.GetString("35639A24-BF94-46EF-A0A9-F06948D5C5CB", "Reprint Package Label")
					: Res.GetString("C96EB7FC-DD33-48E6-BBAE-51143C43CCD2", "Print Package Label");
			}
			else
			{
				PrintPackageLabelMenuItem.Visible = Tree.SelectedPackages.All(package => package.CanPrintCarrierLabel);
				PrintPackageLabelMenuItem.Text = Res.GetString("906A59DC-2B9C-430B-A030-E225DFBEEA03", "Select a Single Package to Print Package Label");
			}
		}

		void UpdateCancelPackageLabelMenuItem()
		{
			CancelPackageLabelMenuItem.Visible = Tree.SelectedPackages.All(package => package.CanCancelPackageLabel);
		}

		IEnumerable<ToolStripItem> EditingControls
		{
			get
			{
				return new ToolStripItem[]
				{
#if DEBUG
					AddEventMenuItem,
#endif
					AddPackageMenuItem,
					GenerateIDsMenuItem,
					ClosePackageMenuItem,
					ReleasePackageMenuItem,
					RemoveHoldAllPackagesMenuItem,
					BreakDownPackageMenuItem,
					HoldPackageMenuItem,
					RemoveMenuItemSeparator1,
					RemoveMenuItem,
					RemoveMenuItemSeparator2,
					ClearIDsMenuItem,
					PrintPackageLabelMenuItem,
					CancelPackageLabelMenuItem
				};
			}
		}

		#endregion

		#region AddDocumentsMenu

		void AddDocumentsMenu()
		{
			RemoveDocumentsMenu();

			if (IsBound && !Tree.IsPackableItemSelected)
			{
				var packageJob = PackageJob;
				var docSupportable = packageJob.Selected.IsPackageSelected ? (BusinessObject)packageJob.Selected.SelectedPackages[0] : packageJob;
				var menuItem = Provider.GetMenuItem(ParentForm, docSupportable);
				if (menuItem != null)
				{
					PackingMenuStrip.Items.Add(new ToolStripSeparator());
					PackingMenuStrip.Items.Add(menuItem);
					DocumentsMenuItem = menuItem;
				}
			}
		}

		void RemoveDocumentsMenu()
		{
			if (DocumentsMenuItem != null)
			{
				var indexOfSeparator = PackingMenuStrip.Items.IndexOf(DocumentsMenuItem) - 1;
				if (indexOfSeparator >= 0)
				{
					PackingMenuStrip.Items.RemoveAt(indexOfSeparator);
					PackingMenuStrip.Items.Remove(DocumentsMenuItem);
					DocumentsMenuItem.Dispose();
					DocumentsMenuItem = null;
				}
			}
		}

		DocEngineDynamicToolStripMenuItemProvider Provider => provider ?? (provider = new DocEngineDynamicToolStripMenuItemProvider());
		DocEngineDynamicToolStripMenuItemProvider provider;

		#endregion

		#endregion

		#region Focus on Package Qty

		public void FocusOnPackageQty()
		{
			if (!PackageDetailMenuItem.Checked)
			{
				PackageDetailMenuItem.PerformClickEnableFirst();
			}
			PackageDetailControl.FocusOnPackageQty();
		}

		#endregion

		#region Assign / Unassign

		void AssignButton_Click(object sender, EventArgs e)
		{
			var selectedItems = PackageIDsGrid.SelectedElements;
			var packageHeaderPivots = selectedItems.Cast<PkgPackageJobPackageHeaderPivot>().ToArray();

			var package = Tree.IsSinglePackageSelected ? Tree.SelectedPackage : null;
			if (PackageJob != null && packageHeaderPivots != null && package != null)
			{
				PackageJob.AssignPackageIDs(package, packageHeaderPivots.Select(p => p.PackageHeader).ToList());
				var packageNode = Tree.FindNodeByPackage(package);
				RefreshPackageNode(packageNode);
			}
		}

		void UnassignButton_Click(object sender, EventArgs e)
		{
			var selectedPackages = Tree.SelectedNodes.Select(n => n.Package).ToArray();
			if (PackageJob != null && selectedPackages != null)
			{
				PackageJob.UnassignPackageIDs(selectedPackages);
				var packageNode = selectedPackages.Length == 1 ? Tree.FindNodeByPackage(selectedPackages.First()) : Tree.TopNode;
				RefreshPackageNode(packageNode);
			}
		}

		void RefreshPackageNode(PackingTreeNode packageNode)
		{
			if (packageNode != null)
			{
				Tree.NodeSelector.DeselectAllNodes();
				Tree.NodeSelector.SelectNode(packageNode);
			}
		}

		#endregion

		#region IsPackageIDsGridDragging

		public bool IsPackageIDsGridDragging => PackageIDsGrid != null && PackageIDsGrid.IsDragging;

		#endregion

		#region Dispose

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

		#endregion

		#region INotifications

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion

		#region INotificationSubscriberQueryUser

		public void QueryUser(IQueryUserEventArgs e)
		{
			var helper = new NotificationSubscriberGuiHelper();
			helper.QueryUser(e);
		}

		#endregion
	}
}
