using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.GUI
{
	public partial class PackingTreeViewUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackingTreeViewUserControl));
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TreeSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TreeGridSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.Tree = new Enterprise.Packing.GUI.PackingTreeView();
			this.PackingMenuStrip = new CargoWise.Windows.UI.KContextMenuStrip(this.components);
			this.ViewJobMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.ViewJobMenuItemSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.AddPackageMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.GenerateIDsMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.ClearIDsMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.BreakDownPackageMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.ClosePackageMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.ReleasePackageMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.HoldPackageMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.PrintPackageLabelMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.CancelPackageLabelMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.RemoveHoldAllPackagesMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.RemoveMenuItemSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.RemoveMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.RemoveMenuItemSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.PackageDetailMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.CustomizeViewMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.ExpandMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.CollapseMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.GridSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MoveBarPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MoveBar = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.AssignButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.UnassignButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.PackageIDGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PackageIDsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackageDetailControl = new Enterprise.Packing.GUI.PackageDetailUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TreeSplitContainer)).BeginInit();
			this.TreeSplitContainer.Panel1.SuspendLayout();
			this.TreeSplitContainer.Panel2.SuspendLayout();
			this.TreeSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TreeGridSplitContainer)).BeginInit();
			this.TreeGridSplitContainer.Panel1.SuspendLayout();
			this.TreeGridSplitContainer.Panel2.SuspendLayout();
			this.TreeGridSplitContainer.SuspendLayout();
			this.PackingMenuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GridSplitContainer)).BeginInit();
			this.GridSplitContainer.Panel1.SuspendLayout();
			this.GridSplitContainer.Panel2.SuspendLayout();
			this.GridSplitContainer.SuspendLayout();
			this.MoveBarPanel.SuspendLayout();
			this.PackageIDGridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackageIDsGrid)).BeginInit();
			this.PackageIDsGrid.SuspendLayout();
			this.PackageDetailControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// TreeSplitContainer
			// 
			this.TreeSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TreeSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.TreeSplitContainer.IsSplitterFixed = false;
			this.TreeSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TreeSplitContainer.Name = "TreeSplitContainer";
			this.TreeSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.TreeSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 339, true);
			// 
			// TreeSplitContainer.Panel1
			// 
			this.TreeSplitContainer.Panel1.Controls.Add(this.TreeGridSplitContainer);
			this.TreeSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20);
			// 
			// TreeSplitContainer.Panel2
			// 
			this.TreeSplitContainer.Panel2.Controls.Add(this.PackageDetailControl);
			this.TreeSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(118);
			this.TreeSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(219);
			this.TreeSplitContainer.SplitterWidth = 2;
			this.TreeSplitContainer.TabIndex = 13;
			// 
			// TreeGridSplitContainer
			// 
			this.TreeGridSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TreeGridSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.TreeGridSplitContainer.IsSplitterFixed = false;
			this.TreeGridSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TreeGridSplitContainer.Name = "TreeGridSplitContainer";
			this.TreeGridSplitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.TreeGridSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 219, true);
			// 
			// TreeGridSplitContainer.Panel1
			// 
			this.TreeGridSplitContainer.Panel1.Controls.Add(this.Tree);
			this.TreeGridSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(570);
			// 
			// TreeGridSplitContainer.Panel2
			// 
			this.TreeGridSplitContainer.Panel2.Controls.Add(this.GridSplitContainer);
			this.TreeGridSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(270);
			this.TreeGridSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.TreeGridSplitContainer.SplitterWidth = 1;
			this.TreeGridSplitContainer.TabIndex = 15;
			// 
			// Tree
			// 
			this.Tree.AllowDrop = true;
			this.Tree.AllowMultiSelectFromDifferentParents = false;
			this.Tree.ContextMenuStrip = this.PackingMenuStrip;
			this.Tree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Tree.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
			this.Tree.HideSelection = false;
			this.Tree.ImageIndex = 0;
			this.Tree.ItemHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20);
			this.Tree.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
			this.Tree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Tree.Name = "Tree";
			this.Tree.SelectedImageIndex = 0;
			this.Tree.SelectedNode = null;
			this.Tree.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 219, true);
			this.Tree.Sorted = true;
			this.Tree.TabIndex = 18;
			// 
			// PackingMenuStrip
			// 
			this.PackingMenuStrip.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.PackingMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ViewJobMenuItem,
            this.ViewJobMenuItemSeparator,
            this.AddPackageMenuItem,
            this.GenerateIDsMenuItem,
            this.ClearIDsMenuItem,
            this.BreakDownPackageMenuItem,
            this.ToolStripSeparator1,
            this.ClosePackageMenuItem,
            this.ReleasePackageMenuItem,
            this.HoldPackageMenuItem,
			this.PrintPackageLabelMenuItem,
			this.CancelPackageLabelMenuItem,
			this.RemoveHoldAllPackagesMenuItem,
            this.RemoveMenuItemSeparator1,
            this.RemoveMenuItem,
            this.RemoveMenuItemSeparator2,
            this.PackageDetailMenuItem,
            this.CustomizeViewMenuItem,
            this.ExpandMenuItem,
            this.CollapseMenuItem});
			this.PackingMenuStrip.Name = "ContextMenuStrip";
			this.PackingMenuStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 448, true);
			// 
			// ViewJobMenuItem
			// 
			this.ViewJobMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("045497C8-A5F7-4E53-A1B0-306773B36AC1", "View Job");
			this.ViewJobMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ViewJobMenuItem.Image")));
			this.ViewJobMenuItem.Name = "ViewJobMenuItem";
			this.ViewJobMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.ViewJobMenuItem.Click += new System.EventHandler(this.ViewJobMenuItem_Click);
			// 
			// ViewJobMenuItemSeparator
			// 
			this.ViewJobMenuItemSeparator.Name = "ViewJobMenuItemSeparator";
			this.ViewJobMenuItemSeparator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 6, true);
			// 
			// AddPackageMenuItem
			// 
			this.AddPackageMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("5FF3BA1A-7189-4D3E-8211-5EB419641C1C", "Add Package");
			this.AddPackageMenuItem.Image = global::Enterprise.Packing.GUI.Properties.Resources.AddPack;
			this.AddPackageMenuItem.Name = "AddPackageMenuItem";
			this.AddPackageMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.AddPackageMenuItem.Click += new System.EventHandler(this.AddPackageMenuItem_Click);
			// 
			// GenerateIDsMenuItem
			// 
			this.GenerateIDsMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("CB41FDCF-72CE-4A84-8570-D29FC7E4180C", "Generate IDs");
			this.GenerateIDsMenuItem.Image = global::Enterprise.Packing.GUI.Properties.Resources.Barcode;
			this.GenerateIDsMenuItem.Name = "GenerateIDsMenuItem";
			this.GenerateIDsMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.GenerateIDsMenuItem.Click += new System.EventHandler(this.GenerateIDsMenuItem_Click);
			// 
			// ClearIDsMenuItem
			// 
			this.ClearIDsMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("19FD1BA2-1734-45EC-B41F-8979B1461C46", "Clear IDs");
			this.ClearIDsMenuItem.Name = "ClearIDsMenuItem";
			this.ClearIDsMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.ClearIDsMenuItem.Click += new System.EventHandler(this.ClearIDsMenuItem_Click);
			// 
			// BreakDownPackageMenuItem
			// 
			this.BreakDownPackageMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("2dacbf52-3d50-47aa-b307-13d4f5aad624", "Break Down Packages");
			this.BreakDownPackageMenuItem.Name = "BreakDownPackageMenuItem";
			this.BreakDownPackageMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.BreakDownPackageMenuItem.Click += new System.EventHandler(this.BreakDownPackagesMenuItem_Click);
			// 
			// ToolStripSeparator1
			// 
			this.ToolStripSeparator1.Name = "ToolStripSeparator1";
			this.ToolStripSeparator1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 6, true);
			// 
			// ClosePackageMenuItem
			// 
			this.ClosePackageMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("634e28da-ea23-4c7c-ade8-ed4d0fd2230d", "Close Package");
			this.ClosePackageMenuItem.CheckOnClick = true;
			this.ClosePackageMenuItem.Name = "ClosePackageMenuItem";
			this.ClosePackageMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.ClosePackageMenuItem.Click += new System.EventHandler(this.ClosePackageMenuItem_Click);
			// 
			// ReleasePackageMenuItem
			// 
			this.ReleasePackageMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("2b8e287f-0bf4-4cd7-97f8-9fc78b502f3e", "Release Package");
			this.ReleasePackageMenuItem.CheckOnClick = true;
			this.ReleasePackageMenuItem.Image = global::Enterprise.Packing.GUI.Properties.Resources.REL;
			this.ReleasePackageMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.ReleasePackageMenuItem.Name = "ReleasePackageMenuItem";
			this.ReleasePackageMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.ReleasePackageMenuItem.Click += new System.EventHandler(this.ReleasePackageMenuItem_Click);
			// 
			// HoldPackageMenuItem
			// 
			this.HoldPackageMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("f96d40ec-9b0a-440a-a53e-b61dd718389f", "Hold Package");
			this.HoldPackageMenuItem.CheckOnClick = true;
			this.HoldPackageMenuItem.Name = "HoldPackageMenuItem";
			this.HoldPackageMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.HoldPackageMenuItem.Click += new System.EventHandler(this.HoldPackageMenuItem_Click);
			// 
			// PrintPackageLabelMenuItem
			// 
			this.PrintPackageLabelMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("01F631DD-34FC-4E39-ADF3-EF7D811A66F2", "Print Package Label");
			this.PrintPackageLabelMenuItem.Name = "PrintPackageLabelMenuItem";
			this.PrintPackageLabelMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.PrintPackageLabelMenuItem.Click += new System.EventHandler(this.PrintPackageLabelMenuItem_Click);
			// 
			// CancelPackageLabelMenuItem
			// 
			this.CancelPackageLabelMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("63ACD6D1-A3E3-465F-9D17-67FA9FB62518", "Cancel Package Label");
			this.CancelPackageLabelMenuItem.Name = "CancelPackageLabelMenuItem";
			this.CancelPackageLabelMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.CancelPackageLabelMenuItem.Click += new System.EventHandler(this.CancelPackageLabelMenuItem_Click);
			// 
			// RemoveHoldAllPackagesMenuItem
			// 
			this.RemoveHoldAllPackagesMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("12f074c2-e120-4e75-a283-e6ac7c8185ed", "Remove Hold on All Packages");
			this.RemoveHoldAllPackagesMenuItem.CheckOnClick = true;
			this.RemoveHoldAllPackagesMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.RemoveHoldAllPackagesMenuItem.Name = "RemoveHoldAllPackagesMenuItem";
			this.RemoveHoldAllPackagesMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.RemoveHoldAllPackagesMenuItem.Click += new System.EventHandler(this.RemoveHoldAllPackagesMenuItem_Click);
			// 
			// RemoveMenuItemSeparator1
			// 
			this.RemoveMenuItemSeparator1.Name = "RemoveMenuItemSeparator1";
			this.RemoveMenuItemSeparator1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 6, true);
			// 
			// RemoveMenuItem
			// 
			this.RemoveMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("E0726465-A884-47D1-B52A-83F3884A6407", "Remove");
			this.RemoveMenuItem.Image = global::Enterprise.Packing.GUI.Properties.Resources.Unpack;
			this.RemoveMenuItem.Name = "RemoveMenuItem";
			this.RemoveMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.RemoveMenuItem.Click += new System.EventHandler(this.RemoveMenuItem_Click);
			// 
			// RemoveMenuItemSeparator2
			// 
			this.RemoveMenuItemSeparator2.Name = "RemoveMenuItemSeparator2";
			this.RemoveMenuItemSeparator2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 6, true);
			// 
			// PackageDetailMenuItem
			// 
			this.PackageDetailMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("9551CCFB-DB77-4451-B9E8-9E2F305DA21E", "Package Detail");
			this.PackageDetailMenuItem.Checked = true;
			this.PackageDetailMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.PackageDetailMenuItem.Name = "PackageDetailMenuItem";
			this.PackageDetailMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.PackageDetailMenuItem.Click += new System.EventHandler(this.PackageDetailMenuItem_Click);
			// 
			// CustomizeViewMenuItem
			// 
			this.CustomizeViewMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("7DDEB0FE-CE29-49F3-B0F8-B0A595BD11CA", "Customize View");
			this.CustomizeViewMenuItem.Name = "CustomizeViewMenuItem";
			this.CustomizeViewMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.CustomizeViewMenuItem.Click += new System.EventHandler(this.CustomizeViewMenuItem_Click);
			// 
			// ExpandMenuItem
			// 
			this.ExpandMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingTreeViewUserControl|Expand", "Expand");
			this.ExpandMenuItem.Name = "ExpandMenuItem";
			this.ExpandMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.ExpandMenuItem.Click += new System.EventHandler(this.ExpandMenuItem_Click);
			// 
			// CollapseMenuItem
			// 
			this.CollapseMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingTreeViewUserControl|Collapse", "Collapse");
			this.CollapseMenuItem.Name = "CollapseMenuItem";
			this.CollapseMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 30, true);
			this.CollapseMenuItem.Click += new System.EventHandler(this.CollapseMenuItem_Click);
			// 
			// GridSplitContainer
			// 
			this.GridSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.GridSplitContainer.IsSplitterFixed = true;
			this.GridSplitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.GridSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GridSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 219, true);
			this.GridSplitContainer.Name = "GridSplitContainer";
			// 
			// GridSplitContainer.Panel1
			// 
			this.GridSplitContainer.Panel1.Controls.Add(this.MoveBarPanel);
			this.GridSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			// 
			// GridSplitContainer.Panel2
			// 
			this.GridSplitContainer.Panel2.Controls.Add(this.PackageIDGridPanel);
			this.GridSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(151);
			this.GridSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(91);
			this.GridSplitContainer.TabIndex = 0;
			// 
			// MoveBarPanel
			// 
			this.MoveBarPanel.Controls.Add(this.MoveBar);
			this.MoveBarPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MoveBarPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MoveBarPanel.Name = "MoveBarPanel";
			this.MoveBarPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 219, true);
			this.MoveBarPanel.TabIndex = 20;
			// 
			// MoveBar
			// 
			this.MoveBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MoveBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.MoveBar.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.MoveBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AssignButton,
            this.UnassignButton});
			this.MoveBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.MoveBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MoveBar.Name = "MoveBar";
			this.MoveBar.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MoveBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 219, true);
			this.MoveBar.Stretch = true;
			this.MoveBar.TabIndex = 0;
			// 
			// AssignButton
			// 
			this.AssignButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("f9b1d3e4-e1eb-4f7b-9ce1-cb55750f65d6", "Assign");
			this.AssignButton.Image = global::Enterprise.Packing.GUI.Properties.Resources.Assign;
			this.AssignButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.AssignButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 130, 0, 20, true);
			this.AssignButton.Name = "AssignButton";
			this.AssignButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 63, true);
			this.AssignButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.AssignButton.Click += new System.EventHandler(this.AssignButton_Click);
			// 
			// UnassignButton
			// 
			this.UnassignButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("2a5b0ac6-e28e-4eb4-a166-5fb897f71f1b", "Un-assign");
			this.UnassignButton.Image = global::Enterprise.Packing.GUI.Properties.Resources.Unassign;
			this.UnassignButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.UnassignButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 20, 0, 130, true);
			this.UnassignButton.Name = "UnassignButton";
			this.UnassignButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 43, true);
			this.UnassignButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.UnassignButton.Click += new System.EventHandler(this.UnassignButton_Click);
			// 
			// PackageIDGridPanel
			// 
			this.PackageIDGridPanel.AutoSize = true;
			this.PackageIDGridPanel.Controls.Add(this.PackageIDsGrid);
			this.PackageIDGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackageIDGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackageIDGridPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PackageIDGridPanel.Name = "PackageIDGridPanel";
			this.PackageIDGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 219, true);
			this.PackageIDGridPanel.TabIndex = 21;
			// 
			// PackageIDsGrid
			// 
			this.PackageIDsGrid.AllowDrop = true;
			this.PackageIDsGrid.AllowNavigation = false;
			this.PackageIDsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "PackageHeader+KPH_PackageID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			this.PackageIDsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackageIDsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackageIDsGrid.GridId = "dab17777-23bf-4395-9111-031438e45715";
			this.PackageIDsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackageIDsGrid.LayoutKey = "PackageIDsGrid";
			this.PackageIDsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackageIDsGrid.Name = "PackageIDsGrid";
			this.PackageIDsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 219, true);
			this.PackageIDsGrid.TabIndex = 22;
			// 
			// PackageDetailControl
			// 
			this.PackageDetailControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageDetailControl, ".");
			this.PackageDetailControl.CaptionResourceString = null;
			this.PackageDetailControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackageDetailControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackageDetailControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 117, true);
			this.PackageDetailControl.Name = "PackageDetailControl";
			this.PackageDetailControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 118, true);
			this.PackageDetailControl.TabIndex = 0;
			// 
			// PackingTreeViewUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TreeSplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 140, true);
			this.Name = "PackingTreeViewUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 339, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TreeSplitContainer.Panel1.ResumeLayout(false);
			this.TreeSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TreeSplitContainer)).EndInit();
			this.TreeSplitContainer.ResumeLayout(false);
			this.TreeSplitContainer.PerformLayout();
			this.TreeGridSplitContainer.Panel1.ResumeLayout(false);
			this.TreeGridSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TreeGridSplitContainer)).EndInit();
			this.TreeGridSplitContainer.ResumeLayout(false);
			this.TreeGridSplitContainer.PerformLayout();
			this.PackingMenuStrip.ResumeLayout(false);
			this.PackingMenuStrip.PerformLayout();
			this.GridSplitContainer.Panel1.ResumeLayout(false);
			this.GridSplitContainer.Panel2.ResumeLayout(false);
			this.GridSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GridSplitContainer)).EndInit();
			this.GridSplitContainer.ResumeLayout(false);
			this.GridSplitContainer.PerformLayout();
			this.MoveBarPanel.ResumeLayout(false);
			this.MoveBarPanel.PerformLayout();
			this.PackageIDGridPanel.ResumeLayout(false);
			this.PackageIDGridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackageIDsGrid)).EndInit();
			this.PackageIDsGrid.ResumeLayout(false);
			this.PackageIDsGrid.PerformLayout();
			this.PackageDetailControl.ResumeLayout(true);
			this.PackageDetailControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer TreeSplitContainer;
		internal CargoWise.Windows.UI.KSplitContainer TreeGridSplitContainer;
		internal CargoWise.Windows.UI.KSplitContainer GridSplitContainer;
		internal PackageDetailUserControl PackageDetailControl;

		internal ZToolStripMenuItem PackageDetailMenuItem;
		internal ZToolStripMenuItem GenerateIDsMenuItem;
		internal ZToolStripMenuItem CustomizeViewMenuItem;
		internal ZToolStripMenuItem RemoveMenuItem;
		internal ToolStripMenuItem DocumentsMenuItem;

		internal KContextMenuStrip PackingMenuStrip;
		ZToolStripMenuItem ViewJobMenuItem;
		ToolStripSeparator ViewJobMenuItemSeparator;
		ZToolStripMenuItem AddPackageMenuItem;
		ZToolStripMenuItem ClearIDsMenuItem;
		ToolStripSeparator RemoveMenuItemSeparator1;
		ToolStripSeparator RemoveMenuItemSeparator2;
		ZToolStripMenuItem ExpandMenuItem;
		ZToolStripMenuItem CollapseMenuItem;
		ZToolStripMenuItem BreakDownPackageMenuItem;
		public PackingTreeView Tree;
		internal Enterprise.ZArchitecture.ZGrid PackageIDsGrid;
		ToolStripSeparator ToolStripSeparator1;
		ZToolStripMenuItem ClosePackageMenuItem;
		ZToolStripMenuItem ReleasePackageMenuItem;
		ZToolStripMenuItem RemoveHoldAllPackagesMenuItem;
		ZToolStripMenuItem HoldPackageMenuItem;
		ZToolStripMenuItem PrintPackageLabelMenuItem;
		ZToolStripMenuItem CancelPackageLabelMenuItem;
		ZPanel MoveBarPanel;
		ZToolStrip MoveBar;
		ZToolStripButton AssignButton;
		ZToolStripButton UnassignButton;
		ZPanel PackageIDGridPanel;
	}
}
