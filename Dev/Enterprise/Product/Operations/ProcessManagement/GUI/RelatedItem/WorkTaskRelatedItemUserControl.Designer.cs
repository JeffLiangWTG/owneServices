
namespace Enterprise.ProcessManagement.GUI
{
	partial class WorkTaskRelatedItemUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.splitContainerMain = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainerTop = new CargoWise.Windows.UI.KSplitContainer();
			this.RelatedItemGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalRelatedItemsPanel = new CargoWise.Windows.UI.KPanel();
			this.RelatedItemGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DetachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AttachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ShowNonClosedItemsOnlyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NetworkDiagramGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.splitContainerBottom = new CargoWise.Windows.UI.KSplitContainer();
			this.ParentWorkflowGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChildWorkflowGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.menuStripNew = new CargoWise.Windows.UI.KContextMenuStrip(this.components);
			this.mehToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.menuStripAttach = new CargoWise.Windows.UI.KContextMenuStrip(this.components);
			this.toolStripMenuItem1 = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
			this.splitContainerMain.Panel1.SuspendLayout();
			this.splitContainerMain.Panel2.SuspendLayout();
			this.splitContainerMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).BeginInit();
			this.splitContainerTop.Panel1.SuspendLayout();
			this.splitContainerTop.Panel2.SuspendLayout();
			this.splitContainerTop.SuspendLayout();
			this.RelatedItemGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedItemGrid)).BeginInit();
			this.RelatedItemGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerBottom)).BeginInit();
			this.splitContainerBottom.Panel1.SuspendLayout();
			this.splitContainerBottom.Panel2.SuspendLayout();
			this.splitContainerBottom.SuspendLayout();
			this.menuStripNew.SuspendLayout();
			this.menuStripAttach.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource);
			// 
			// splitContainerMain
			// 
			this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerMain.Name = "splitContainerMain";
			this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerMain.Panel1
			// 
			this.splitContainerMain.Panel1.Controls.Add(this.splitContainerTop);
			// 
			// splitContainerMain.Panel2
			// 
			this.splitContainerMain.Panel2.Controls.Add(this.splitContainerBottom);
			this.splitContainerMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 687, true);
			this.splitContainerMain.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(334);
			this.splitContainerMain.SplitterWidth = 6;
			this.splitContainerMain.TabIndex = 0;
			// 
			// splitContainerTop
			// 
			this.splitContainerTop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerTop.Name = "splitContainerTop";
			this.splitContainerTop.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerTop.Panel1
			// 
			this.splitContainerTop.Panel1.Controls.Add(this.RelatedItemGroupBox);
			// 
			// splitContainerTop.Panel2
			// 
			this.splitContainerTop.Panel2.Controls.Add(this.NetworkDiagramGroupBox);
			this.splitContainerTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 334, true);
			this.splitContainerTop.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(185);
			this.splitContainerTop.SplitterWidth = 6;
			this.splitContainerTop.TabIndex = 0;
			// 
			// RelatedItemGroupBox
			// 
			this.RelatedItemGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("DF7E310B-15DF-4156-A33C-F80441606401", "Job Relationships");
			this.RelatedItemGroupBox.Controls.Add(this.AdditionalRelatedItemsPanel);
			this.RelatedItemGroupBox.Controls.Add(this.RelatedItemGrid);
			this.RelatedItemGroupBox.Controls.Add(this.DetachButton);
			this.RelatedItemGroupBox.Controls.Add(this.EditButton);
			this.RelatedItemGroupBox.Controls.Add(this.AttachButton);
			this.RelatedItemGroupBox.Controls.Add(this.NewButton);
			this.RelatedItemGroupBox.Controls.Add(this.ShowNonClosedItemsOnlyCheckBox);
			this.RelatedItemGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedItemGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedItemGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.RelatedItemGroupBox.Name = "RelatedItemGroupBox";
			this.RelatedItemGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 185, true);
			this.RelatedItemGroupBox.TabIndex = 0;
			this.RelatedItemGroupBox.TabStop = false;
			// 
			// AdditionalRelatedItemsPanel
			// 
			this.AdditionalRelatedItemsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AdditionalRelatedItemsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 154, true);
			this.AdditionalRelatedItemsPanel.Name = "AdditionalRelatedItemsPanel";
			this.AdditionalRelatedItemsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 27, true);
			this.AdditionalRelatedItemsPanel.TabIndex = 1;
			// 
			// RelatedItemGrid
			// 
			this.RelatedItemGrid.AllowNavigation = false;
			this.RelatedItemGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RelatedItemGrid, "FilteredRelatedItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).Criticality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).ItemDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).ClientCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).ClientName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).AssignedStaffCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).Source)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).SelectionCriterion1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).SelectionCriterion2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).SelectionCriterion3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).SelectionCriterion4)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).SelectionCriterion5)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ProcessManagement.Integration.IWorkTaskRelatedItem)(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).FilteredRelatedItems)).SyncRoot)).IsClosedOrCancelled)));
			this.RelatedItemGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("39af01c1-2c6a-4400-b7b7-f945cf718727", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "Type";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("740eafc3-e518-4fd2-aa66-9c8737139237", "Criticality");
			zTextBoxColumnStyleInfo2.ColumnName = "Criticality";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.Caption = "";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("cdce5572-92db-4ea9-bf08-6dd450b2eca9", "Number");
			zTextBoxColumnStyleInfo3.ColumnName = "Number";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = "";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("4c62cecd-9df4-4ba3-a3e1-be45749551a6", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "ItemDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.Caption = "";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("3dcb02a2-11c5-4787-adfe-b347e0729a9c", "Client Code");
			zTextBoxColumnStyleInfo5.ColumnName = "ClientCode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.Caption = "";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("71537cfc-4f70-426f-bbcd-483d92d3199b", "Client Name");
			zTextBoxColumnStyleInfo6.ColumnName = "ClientName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo7.Caption = "";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("9961320a-7504-4e9a-a659-72c59b821cf4", "Status Description");
			zTextBoxColumnStyleInfo7.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo8.Caption = "";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("4cb15ff0-5bb9-43be-943c-bc66c18956fe", "Assigned");
			zTextBoxColumnStyleInfo8.ColumnName = "AssignedStaffCode";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.Caption = "";
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("80cd4a8f-bfd1-4e63-ab57-3cf4c88157f0", "Source");
			zTextBoxColumnStyleInfo9.ColumnName = "Source";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.Caption = "";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("d814b04a-9add-45e0-bc8d-1fec837a02de", "Selection Criterion 1");
			zTextBoxColumnStyleInfo10.ColumnName = "SelectionCriterion1";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.Caption = "";
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("bc644cf5-5ac0-4704-af46-60105a150d30", "Selection Criterion 2");
			zTextBoxColumnStyleInfo11.ColumnName = "SelectionCriterion2";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.Caption = "";
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("3e6e336f-e60b-49f6-8db3-f3f135896b25", "Selection Criterion 3");
			zTextBoxColumnStyleInfo12.ColumnName = "SelectionCriterion3";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.Caption = "";
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("238d0992-ea81-4556-b939-19cd87df351f", "Selection Criterion 4");
			zTextBoxColumnStyleInfo13.ColumnName = "SelectionCriterion4";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.Caption = "";
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("c87f090f-4c60-4f4c-b5d0-3ae5f08fe90d", "Selection Criterion 5");
			zTextBoxColumnStyleInfo14.ColumnName = "SelectionCriterion5";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.Caption = "";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("407e5e08-9d9d-4b5d-9bcd-bebefbc092ba", "Closed");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsClosedOrCancelled";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.RelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.RelatedItemGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RelatedItemGrid.GridId = "f9e65176-16f6-4430-a6ad-29733af69eb5";
			this.RelatedItemGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedItemGrid.IsWholeRowSelectedOnClick = true;
			this.RelatedItemGrid.LayoutKey = "RelatedItemGrid";
			this.RelatedItemGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.RelatedItemGrid.Name = "RelatedItemGrid";
			this.RelatedItemGrid.ReadOnly = true;
			this.RelatedItemGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 134, true);
			this.RelatedItemGrid.TabIndex = 0;
			this.RelatedItemGrid.DoubleClick += new System.EventHandler(this.RelatedItemGrid_DoubleClick);
			// 
			// DetachButton
			// 
			this.DetachButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DetachButton.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("e6792b47-52f0-46da-905f-91e729973fa0", "Detach");
			this.DetachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(866, 154, true);
			this.DetachButton.Name = "DetachButton";
			this.DetachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DetachButton.TabIndex = 14;
			this.DetachButton.ToolTipCaption = null;
			this.DetachButton.UseVisualStyleBackColor = true;
			this.DetachButton.Click += new System.EventHandler(this.DetachButton_Click);
			// 
			// EditButton
			// 
			this.EditButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.EditButton.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("314be7cf-981d-4901-be26-08a1b62becbd", "Edit");
			this.EditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(787, 154, true);
			this.EditButton.Name = "EditButton";
			this.EditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.EditButton.TabIndex = 13;
			this.EditButton.ToolTipCaption = null;
			this.EditButton.UseVisualStyleBackColor = true;
			this.EditButton.Click += new System.EventHandler(this.EditButton_Click);
			// 
			// AttachButton
			// 
			this.AttachButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AttachButton.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("86993956-ddac-4bb2-8992-47cdcafe8025", "Attach");
			this.AttachButton.Image = global::Enterprise.ProcessManagement.GUI.Properties.Resources.arrow_drop1;
			this.AttachButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.AttachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(703, 154, true);
			this.AttachButton.Name = "AttachButton";
			this.AttachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AttachButton.TabIndex = 12;
			this.AttachButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AttachButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.AttachButton.ToolTipCaption = null;
			this.AttachButton.UseVisualStyleBackColor = true;
			this.AttachButton.Click += new System.EventHandler(this.AttachButton_Click);
			// 
			// NewButton
			// 
			this.NewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NewButton.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("9725ee45-9ddb-43a8-9c8a-1ce7dc415c0a", "New");
			this.NewButton.Image = global::Enterprise.ProcessManagement.GUI.Properties.Resources.arrow_drop1;
			this.NewButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.NewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(624, 154, true);
			this.NewButton.Name = "NewButton";
			this.NewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NewButton.TabIndex = 11;
			this.NewButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.NewButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.NewButton.ToolTipCaption = null;
			this.NewButton.UseVisualStyleBackColor = true;
			this.NewButton.Click += new System.EventHandler(this.NewButton_Click);
			// 
			// ShowNonClosedItemsOnlyCheckBox
			// 
			this.ShowNonClosedItemsOnlyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShowNonClosedItemsOnlyCheckBox, "ShowOnlyNonClosedItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ProcessManagement.Business.IWorkTaskRelatedItemSource)(null)).ShowOnlyNonClosedItems)));
			this.ShowNonClosedItemsOnlyCheckBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("c1220660-6370-48d9-b57d-65ddbb7926dd", "Show Non Closed Items Only");
			this.ShowNonClosedItemsOnlyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 154, true);
			this.ShowNonClosedItemsOnlyCheckBox.Name = "ShowNonClosedItemsOnlyCheckBox";
			this.ShowNonClosedItemsOnlyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 23, true);
			this.ShowNonClosedItemsOnlyCheckBox.TabIndex = 10;
			this.ShowNonClosedItemsOnlyCheckBox.UseVisualStyleBackColor = true;
			// 
			// NetworkDiagramGroupBox
			// 
			this.NetworkDiagramGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("A28335D0-D38C-466F-A85B-FA72391B855C", "Network Diagrams");
			this.NetworkDiagramGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NetworkDiagramGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NetworkDiagramGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.NetworkDiagramGroupBox.Name = "NetworkDiagramGroupBox";
			this.NetworkDiagramGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 144, true);
			this.NetworkDiagramGroupBox.TabIndex = 1;
			this.NetworkDiagramGroupBox.TabStop = false;
			// 
			// splitContainerBottom
			// 
			this.splitContainerBottom.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerBottom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerBottom.Name = "splitContainerBottom";
			this.splitContainerBottom.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerBottom.Panel1
			// 
			this.splitContainerBottom.Panel1.Controls.Add(this.ParentWorkflowGroupBox);
			// 
			// splitContainerBottom.Panel2
			// 
			this.splitContainerBottom.Panel2.Controls.Add(this.ChildWorkflowGroupBox);
			this.splitContainerBottom.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 347, true);
			this.splitContainerBottom.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(160);
			this.splitContainerBottom.SplitterWidth = 6;
			this.splitContainerBottom.TabIndex = 0;
			// 
			// ParentWorkflowGroupBox
			// 
			this.ParentWorkflowGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("837D27D2-B694-4249-9262-B9E4663051A8", "External Parent Workflows");
			this.ParentWorkflowGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentWorkflowGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ParentWorkflowGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ParentWorkflowGroupBox.Name = "ParentWorkflowGroupBox";
			this.ParentWorkflowGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 160, true);
			this.ParentWorkflowGroupBox.TabIndex = 2;
			this.ParentWorkflowGroupBox.TabStop = false;
			// 
			// ChildWorkflowGroupBox
			// 
			this.ChildWorkflowGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("BB4BED44-369F-445B-A2B6-01D7B335FFAB", "External Child Workflows");
			this.ChildWorkflowGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildWorkflowGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChildWorkflowGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ChildWorkflowGroupBox.Name = "ChildWorkflowGroupBox";
			this.ChildWorkflowGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 182, true);
			this.ChildWorkflowGroupBox.TabIndex = 2;
			this.ChildWorkflowGroupBox.TabStop = false;
			// 
			// menuStripNew
			// 
			this.menuStripNew.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mehToolStripMenuItem});
			this.menuStripNew.Name = "menuStripNewItem";
			this.menuStripNew.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			// 
			// mehToolStripMenuItem
			// 
			this.mehToolStripMenuItem.Name = "mehToolStripMenuItem";
			this.mehToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 18, true);
			// 
			// menuStripAttach
			// 
			this.menuStripAttach.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
			this.menuStripAttach.Name = "menuStripAttachItem";
			this.menuStripAttach.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 18, true);
			// 
			// WorkTaskRelatedItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainerMain);
			this.Name = "WorkTaskRelatedItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 687, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainerMain.Panel1.ResumeLayout(false);
			this.splitContainerMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
			this.splitContainerMain.ResumeLayout(false);
			this.splitContainerMain.PerformLayout();
			this.splitContainerTop.Panel1.ResumeLayout(false);
			this.splitContainerTop.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).EndInit();
			this.splitContainerTop.ResumeLayout(false);
			this.splitContainerTop.PerformLayout();
			this.RelatedItemGroupBox.ResumeLayout(false);
			this.RelatedItemGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedItemGrid)).EndInit();
			this.RelatedItemGrid.ResumeLayout(false);
			this.RelatedItemGrid.PerformLayout();
			this.splitContainerBottom.Panel1.ResumeLayout(false);
			this.splitContainerBottom.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerBottom)).EndInit();
			this.splitContainerBottom.ResumeLayout(false);
			this.splitContainerBottom.PerformLayout();
			this.menuStripNew.ResumeLayout(false);
			this.menuStripNew.PerformLayout();
			this.menuStripAttach.ResumeLayout(false);
			this.menuStripAttach.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainerMain;
		private CargoWise.Windows.UI.KSplitContainer splitContainerTop;
		private CargoWise.Windows.UI.KSplitContainer splitContainerBottom;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox RelatedItemGroupBox;
		protected Enterprise.ZArchitecture.ZGrid RelatedItemGrid;
		protected Enterprise.ZArchitecture.GUI.ZButton DetachButton;
		protected Enterprise.ZArchitecture.GUI.ZButton EditButton;
		protected Enterprise.ZArchitecture.GUI.ZButton NewButton;
		protected CargoWise.Windows.UI.KContextMenuStrip menuStripNew;
		private Enterprise.ZArchitecture.GUI.ZToolStripMenuItem mehToolStripMenuItem;
		protected CargoWise.Windows.UI.KContextMenuStrip menuStripAttach;
		private Enterprise.ZArchitecture.GUI.ZToolStripMenuItem toolStripMenuItem1;
		protected Enterprise.ZArchitecture.GUI.ZButton AttachButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ShowNonClosedItemsOnlyCheckBox;
		protected ZArchitecture.GUI.ZGroupBox NetworkDiagramGroupBox;
		protected ZArchitecture.GUI.ZGroupBox ParentWorkflowGroupBox;
		protected ZArchitecture.GUI.ZGroupBox ChildWorkflowGroupBox;
		private CargoWise.Windows.UI.KPanel AdditionalRelatedItemsPanel;
	}
}
