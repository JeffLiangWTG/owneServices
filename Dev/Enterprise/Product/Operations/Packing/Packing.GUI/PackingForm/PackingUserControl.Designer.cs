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
using CargoWiseOne.ResourceStrings;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Scanning;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Scanning;

namespace Enterprise.Packing.GUI
{
	public partial class PackingUserControl
	{
		#region Auto

		void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			UnpackedQtyColumnStyleInfo unpackedQtyColumnStyleInfo1 = new UnpackedQtyColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ComponentResourceManager resources = new ComponentResourceManager(typeof(PackingUserControl));
			this.SplitContainer = new KSplitContainer();
			this.Grid = new ZGrid();
			this.EmptyPanel = new ZPanel();
			this.EmptyPanelLabel = new ZLabel();
			this.TreeUserControl = new PackingTreeViewUserControl();
			this.Toolstrip = new ZToolStrip();
			this.AddPackageButton = new ZToolStripButton();
			this.PackButton = new ZToolStripDropDownButton();
			this.PackIntoSelectedPackageMenuItem = new ZToolStripMenuItem();
			this.PackIntoNewPackagesMenuItem = new ZToolStripMenuItem();
			this.ToolStripMenuItem1 = new ToolStripSeparator();
			this.AutoPackToolStripMenuItem = new ZToolStripMenuItem();
			this.RemoveButton = new ZToolStripButton();
			this.ToolStripSeparator1 = new ToolStripSeparator();
			this.ClosePackageButton = new ZToolStripButton();
			this.GenerateIDsButton = new ZToolStripButton();
			this.ToolStripSeparator2 = new ToolStripSeparator();
			this.PackageDetailButton = new ZToolStripButton();
			this.LooseIDsButton = new ZToolStripButton();
			this.CustomizeViewButton = new ZToolStripButton();
			this.ScanPackModeButton = new ZToolStripButton();
			this.ToolStripSeparator3 = new ToolStripSeparator();
			this.ScanQtyModeButton = new ZToolStripButton();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((ISupportInitialize)(this.Grid)).BeginInit();
			this.EmptyPanel.SuspendLayout();
			this.Toolstrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PkgPackageJob);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.Grid);
			this.SplitContainer.Panel1.Controls.Add(this.EmptyPanel);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.TreeUserControl);
			this.SplitContainer.Panel2.Controls.Add(this.Toolstrip);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 250, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			this.SplitContainer.TabIndex = 7;
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, "PackableItemParents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PkgPackageJob)(null)).PackableItemParents);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackableItemParentWrapper)(((IList)(((PkgPackageJob)(null)).PackableItemParents)).SyncRoot)).PackableItemParent.Code);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackableItemParentWrapper)(((IList)(((PkgPackageJob)(null)).PackableItemParents)).SyncRoot)).PackableItemParent.Description);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackableItemParentWrapper)(((IList)(((PkgPackageJob)(null)).PackableItemParents)).SyncRoot)).PackableItemParent.TotalQty);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackableItemParentWrapper)(((IList)(((PkgPackageJob)(null)).PackableItemParents)).SyncRoot)).PackedQty);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackableItemParentWrapper)(((IList)(((PkgPackageJob)(null)).PackableItemParents)).SyncRoot)).UnpackedQty);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackableItemParentWrapper)(((IList)(((PkgPackageJob)(null)).PackableItemParents)).SyncRoot)).PackableItemParent.TotalQtyUQ);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackableItemParentWrapper)(((IList)(((PkgPackageJob)(null)).PackableItemParents)).SyncRoot)).UnpackedWeight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackableItemParentWrapper)(((IList)(((PkgPackageJob)(null)).PackableItemParents)).SyncRoot)).PackableItemParent.WeightUQ);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackableItemParentWrapper)(((IList)(((PkgPackageJob)(null)).PackableItemParents)).SyncRoot)).PackableItemParent.AutoPackQtyPerPackage);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackableItemParentWrapper)(((IList)(((PkgPackageJob)(null)).PackableItemParents)).SyncRoot)).PackableItemParent.AutoPackPackageType);
			this.Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingUserControl|2fa5e20e-473b-4bdc-b96c-88a2e3e96247", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "PackableItemParent+Code";
			zTextBoxColumnStyleInfo1.GroupName = null;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingUserControl|e679b51b-b1bc-4e79-b916-5f5be45a4786", "Desc.", "Description", "");
			zTextBoxColumnStyleInfo2.ColumnName = "PackableItemParent+Description";
			zTextBoxColumnStyleInfo2.GroupName = null;
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingUserControl|8ea3a306-523e-441f-8135-fac67e92842e", "Total", "Total Qty", "");
			zCalcEditColumnStyleInfo1.ColumnName = "PackableItemParent+TotalQty";
			zCalcEditColumnStyleInfo1.GroupName = null;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingUserControl|7adc308c-efa8-49db-b509-7f7fd211cc52", "Packed", "Packed Qty", "");
			zCalcEditColumnStyleInfo2.ColumnName = "PackedQty";
			zCalcEditColumnStyleInfo2.GroupName = null;
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			unpackedQtyColumnStyleInfo1.BindToDecimalPlaces = null;
			unpackedQtyColumnStyleInfo1.Caption = null;
			unpackedQtyColumnStyleInfo1.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingUserControl|cc68d1a5-9d0d-4f62-8cde-cdbff406acdb", "Unpacked", "Unpacked Qty", "");
			unpackedQtyColumnStyleInfo1.ColumnName = "UnpackedQty";
			unpackedQtyColumnStyleInfo1.GroupName = null;
			unpackedQtyColumnStyleInfo1.IsMandatory = true;
			unpackedQtyColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingUserControl|63ee01ca-ba49-465d-95fc-11ee072cd470", "UQ");
			zTextBoxColumnStyleInfo3.ColumnName = "PackableItemParent+TotalQtyUQ";
			zTextBoxColumnStyleInfo3.GroupName = null;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingUserControl|968114a0-172d-4862-adb7-78ab796fecf2", "Wgt.", "Weight", "Unpacked Weight", "");
			zCalcEditColumnStyleInfo3.ColumnName = "UnpackedWeight";
			zCalcEditColumnStyleInfo3.GroupName = null;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingUserControl|db3da1be-b765-46f0-b3f0-7d18dc799ffa", "UQ", "Weight UQ", "");
			zTextBoxColumnStyleInfo4.ColumnName = "PackableItemParent+WeightUQ";
			zTextBoxColumnStyleInfo4.GroupName = null;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingUserControl|1eb67b2c-bfb1-4669-8d0b-794f9dacd144", "Auto-Pack Qty");
			zCalcEditColumnStyleInfo4.ColumnName = "PackableItemParent+AutoPackQtyPerPackage";
			zCalcEditColumnStyleInfo4.GroupName = null;
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingUserControl|f6f6b36a-dbd4-4f63-a0be-3084b039e941", "Auto-Pack Pkg.", "Auto-Pack Package", "");
			zTextBoxColumnStyleInfo5.ColumnName = "PackableItemParent+AutoPackPackageType";
			zTextBoxColumnStyleInfo5.GroupName = null;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(unpackedQtyColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.Grid.CopySelectedRowsAllowed = true;
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.GridId = "5439f38f-1a27-469c-8b12-9e5240439f07";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.IsWholeRowSelectedOnClick = true;
			this.Grid.LayoutKey = "PackagingGrid1";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.ReadOnly = true;
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 63, true);
			this.Grid.TabIndex = 10;
			// 
			// EmptyPanel
			// 
			this.EmptyPanel.Controls.Add(this.EmptyPanelLabel);
			this.EmptyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EmptyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EmptyPanel.Name = "EmptyPanel";
			this.EmptyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 63, true);
			this.EmptyPanel.TabIndex = 11;
			this.EmptyPanel.Visible = false;
			// 
			// EmptyPanelLabel
			// 
			this.EmptyPanelLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.EmptyPanelLabel.AutoSize = true;
			this.EmptyPanelLabel.CaptionResourceString = null;
			this.EmptyPanelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1704, 35, true);
			this.EmptyPanelLabel.Name = "EmptyPanelLabel";
			this.EmptyPanelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.EmptyPanelLabel.TabIndex = 0;
			// 
			// TreeUserControl
			// 
			this.TreeUserControl.AllowDrop = true;
			this.TreeUserControl.CaptionResourceString = null;
			this.TreeUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TreeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 33, true);
			this.TreeUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 140, true);
			this.TreeUserControl.Name = "TreeUserControl";
			this.TreeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 150, true);
			this.TreeUserControl.TabIndex = 10;
			this.TreeUserControl.ViewMode = Enterprise.Packing.GUI.PackingViewMode.Default;
			// 
			// Toolstrip
			// 
			this.Toolstrip.CanOverflow = false;
			this.Toolstrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.Toolstrip.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.Toolstrip.Items.AddRange(new ToolStripItem[] {
			this.AddPackageButton,
			this.PackButton,
			this.RemoveButton,
			this.ToolStripSeparator1,
			this.ClosePackageButton,
			this.GenerateIDsButton,
			this.ToolStripSeparator2,
			this.PackageDetailButton,
			this.LooseIDsButton,
			this.CustomizeViewButton,
			this.ScanPackModeButton,
			this.ToolStripSeparator3,
			this.ScanQtyModeButton });
			this.Toolstrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Toolstrip.Name = "Toolstrip";
			this.Toolstrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 1, 0, true);
			this.Toolstrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 33, true);
			this.Toolstrip.TabIndex = 9;
			// 
			// AddPackageButton
			// 
			this.AddPackageButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("FC1C15A2-9932-4209-903D-587B179BF236", "Add Package", "Add");
			this.AddPackageButton.Image = ((Image)(resources.GetObject("AddPackageButton.Image")));
			this.AddPackageButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AddPackageButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.AddPackageButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.AddPackageButton.Name = "AddPackageButton";
			this.AddPackageButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.AddPackageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 30, true);
			this.AddPackageButton.Click += new EventHandler(this.AddPackageButton_Click);
			// 
			// PackButton
			// 
			this.PackButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("ABD90B34-D3C9-453E-A7EE-C67310D0B814", "Pack");
			this.PackButton.DropDownItems.AddRange(new ToolStripItem[] {
			this.PackIntoSelectedPackageMenuItem,
			this.PackIntoNewPackagesMenuItem,
			this.ToolStripMenuItem1,
			this.AutoPackToolStripMenuItem });
			this.PackButton.Image = ((Image)(resources.GetObject("PackButton.Image")));
			this.PackButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PackButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.PackButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.PackButton.Name = "PackButton";
			this.PackButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.PackButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 30, true);
			// 
			// PackIntoSelectedPackageMenuItem
			// 
			this.PackIntoSelectedPackageMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("b82def6e-323d-4d45-a3e4-879dcdf5b7ba", "...into Selected Packages");
			this.PackIntoSelectedPackageMenuItem.Name = "PackIntoSelectedPackageMenuItem";
			this.PackIntoSelectedPackageMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 22, true);
			this.PackIntoSelectedPackageMenuItem.Click += new EventHandler(this.PackIntoSelectedPackageMenuItem_Click);
			// 
			// PackIntoNewPackagesMenuItem
			// 
			this.PackIntoNewPackagesMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("8DF91D4F-28B1-46D5-B294-1D84821F07F1", "...into New Package(s)");
			this.PackIntoNewPackagesMenuItem.Name = "PackIntoNewPackagesMenuItem";
			this.PackIntoNewPackagesMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 22, true);
			this.PackIntoNewPackagesMenuItem.Click += new EventHandler(this.PackIntoNewPackagesMenuItem_Click);
			// 
			// ToolStripMenuItem1
			// 
			this.ToolStripMenuItem1.Name = "ToolStripMenuItem1";
			this.ToolStripMenuItem1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 6, true);
			// 
			// AutoPackToolStripMenuItem
			// 
			this.AutoPackToolStripMenuItem.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("f575a489-d053-4df1-9930-df7daa052b24", "Auto-Pack");
			this.AutoPackToolStripMenuItem.Name = "AutoPackToolStripMenuItem";
			this.AutoPackToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 22, true);
			this.AutoPackToolStripMenuItem.Click += new EventHandler(this.AutoPackToolStripMenuItem_Click);
			// 
			// RemoveButton
			// 
			this.RemoveButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("27FF0DF5-0EBE-4A3B-AABD-CFBD0584C8F4", "Remove");
			this.RemoveButton.Image = ((Image)(resources.GetObject("RemoveButton.Image")));
			this.RemoveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.RemoveButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.RemoveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.RemoveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 30, true);
			this.RemoveButton.Click += new EventHandler(this.RemoveButton_Click);
			// 
			// ToolStripSeparator1
			// 
			this.ToolStripSeparator1.Name = "ToolStripSeparator1";
			this.ToolStripSeparator1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 33, true);
			// 
			// ClosePackageButton
			// 
			this.ClosePackageButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("6775c39e-ee83-4c38-8849-d470c0fa7ada", "", "Close Package");
			this.ClosePackageButton.Image = ((Image)(resources.GetObject("ClosePackageButton.Image")));
			this.ClosePackageButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.ClosePackageButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ClosePackageButton.Name = "ClosePackageButton";
			this.ClosePackageButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, 0, 0, 0, true);
			this.ClosePackageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 30, true);
			this.ClosePackageButton.Click += new EventHandler(this.ClosePackageButton_Click);
			// 
			// GenerateIDsButton
			// 
			this.GenerateIDsButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("AF2BA0D9-C5DA-4350-9687-B9992B9F4AA8", "Generate IDs");
			this.GenerateIDsButton.Image = ((Image)(resources.GetObject("GenerateIDsButton.Image")));
			this.GenerateIDsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.GenerateIDsButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.GenerateIDsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.GenerateIDsButton.Name = "GenerateIDsButton";
			this.GenerateIDsButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.GenerateIDsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 30, true);
			this.GenerateIDsButton.Click += new EventHandler(this.GenerateIDsButton_Click);
			// 
			// ToolStripSeparator2
			// 
			this.ToolStripSeparator2.Name = "ToolStripSeparator2";
			this.ToolStripSeparator2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 33, true);
			// 
			// PackageDetailButton
			// 
			this.PackageDetailButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("4D12E845-A4DA-4CE2-B206-57F8C5ED7751", "Package Detail");
			this.PackageDetailButton.Checked = true;
			this.PackageDetailButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.PackageDetailButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.PackageDetailButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.PackageDetailButton.Name = "PackageDetailButton";
			this.PackageDetailButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 30, true);
			this.PackageDetailButton.Click += new EventHandler(this.PackageDetailButton_Click);
			//
			// LooseIDsButton
			//
			this.LooseIDsButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("97da13fb-b53b-418e-bb2b-36c3875e7a72", "Package IDs");
			this.LooseIDsButton.Checked = true;
			this.LooseIDsButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.LooseIDsButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.LooseIDsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.LooseIDsButton.Name = "LooseIDsButton";
			this.LooseIDsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 30, true);
			this.LooseIDsButton.Click += new EventHandler(this.LooseIDsButton_Click);
			// 
			// CustomizeViewButton
			// 
			this.CustomizeViewButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("17CAEBA8-1A12-4023-830B-90DE82E2AED7", "Customize View", "Customize the Tree View");
			this.CustomizeViewButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.CustomizeViewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.CustomizeViewButton.Name = "CustomizeViewButton";
			this.CustomizeViewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 30, true);
			this.CustomizeViewButton.Click += new EventHandler(this.CustomizeViewButton_Click);
			// 
			// ScanPackModeButton
			// 
			this.ScanPackModeButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.ScanPackModeButton.AutoSize = false;
			this.ScanPackModeButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("25611161-4311-484f-bc2d-a40679fab29b", "Mode: Packing");
			this.ScanPackModeButton.CheckOnClick = true;
			this.ScanPackModeButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.ScanPackModeButton.Name = "ScanPackModeButton";
			this.ScanPackModeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 30, true);
			this.ScanPackModeButton.Click += new EventHandler(this.ScanPackModeButton_Click);
			// 
			// ToolStripSeparator3
			// 
			this.ToolStripSeparator3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.ToolStripSeparator3.Name = "ToolStripSeparator3";
			this.ToolStripSeparator3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 33, true);
			// 
			// ScanQtyModeButton
			// 
			this.ScanQtyModeButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.ScanQtyModeButton.AutoSize = false;
			this.ScanQtyModeButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("df30d481-0d44-4f49-9bd8-ff538c499418", "Scan: All");
			this.ScanQtyModeButton.CheckOnClick = true;
			this.ScanQtyModeButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.ScanQtyModeButton.Name = "ScanQtyModeButton";
			this.ScanQtyModeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 30, true);
			this.ScanQtyModeButton.Click += new EventHandler(this.ScanQtyModeButton_Click);
			// 
			// PackingUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 250, true);
			this.Name = "PackingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 250, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.Panel2.PerformLayout();
			((ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			((ISupportInitialize)(this.Grid)).EndInit();
			this.EmptyPanel.ResumeLayout(false);
			this.EmptyPanel.PerformLayout();
			this.Toolstrip.ResumeLayout(false);
			this.Toolstrip.PerformLayout();
			this.ResumeLayout(false);
		}

		KSplitContainer SplitContainer;
		KToolStrip Toolstrip;
		ZToolStripButton AddPackageButton;
		ZToolStripButton CustomizeViewButton;
		ZToolStripButton GenerateIDsButton;
		ZToolStripButton PackageDetailButton;
		internal ZToolStripButton LooseIDsButton;
		ZToolStripButton RemoveButton;
		ZToolStripDropDownButton PackButton;
		internal ZToolStripMenuItem AutoPackToolStripMenuItem;
		ZToolStripMenuItem PackIntoNewPackagesMenuItem;
		ZToolStripMenuItem PackIntoSelectedPackageMenuItem;
		ToolStripSeparator ToolStripMenuItem1;
		ToolStripSeparator ToolStripSeparator1;
		ToolStripSeparator ToolStripSeparator2;
		protected PackingTreeViewUserControl TreeUserControl;
		ZGrid Grid;
		ZLabel EmptyPanelLabel;
		ToolStripSeparator ToolStripSeparator3;
		protected ZToolStripButton ScanPackModeButton;
		protected ZToolStripButton ScanQtyModeButton;
		ZToolStripButton ClosePackageButton;
		ZPanel EmptyPanel;

		#endregion
	}
}
