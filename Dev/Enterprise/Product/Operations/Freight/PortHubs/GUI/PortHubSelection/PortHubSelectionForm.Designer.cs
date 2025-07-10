using System.Windows.Forms;

namespace Enterprise.Freight.PortHubs.GUI
{
	partial class PortHubSelectionForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo3 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.PostingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.SelectionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DispatchDepotZAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.PackTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DGDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ServiceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.portHubFilterStripControl = new Enterprise.Freight.PortHubs.GUI.PortHubFilterStripControl();
			this.PortsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ZonesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ZonesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DepotAddressZAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.FreightRateModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.selectionDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.basePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtons.SuspendLayout();
			this.SelectionGroupBox.SuspendLayout();
			this.DispatchDepotZAddressControl.SuspendLayout();
			this.PackTypeDropEdit.SuspendLayout();
			this.DGDropEdit.SuspendLayout();
			this.ServiceLevelDropEdit.SuspendLayout();
			this.DirectionDropEdit.SuspendLayout();
			this.portHubFilterStripControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PortsGrid)).BeginInit();
			this.PortsGrid.SuspendLayout();
			this.ZonesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZonesGrid)).BeginInit();
			this.ZonesGrid.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.DepotAddressZAddressControl.SuspendLayout();
			this.FreightRateModeDropEdit.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.selectionDetailsPanel.SuspendLayout();
			this.basePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 662, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 24, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper);
			// 
			// PostingButtons
			// 
			this.PostingButtons.AllowDrop = true;
			this.PostingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(744, 633, true);
			this.PostingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtons.Name = "PostingButtons";
			this.PostingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtons.TabIndex = 5;
			// 
			// SelectionGroupBox
			// 
			this.SelectionGroupBox.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("34333a6d-8b22-4a16-bd99-4fe8dd4a27d5", "Details");
			this.SelectionGroupBox.Controls.Add(this.DispatchDepotZAddressControl);
			this.SelectionGroupBox.Controls.Add(this.PackTypeDropEdit);
			this.SelectionGroupBox.Controls.Add(this.DGDropEdit);
			this.SelectionGroupBox.Controls.Add(this.ServiceLevelDropEdit);
			this.SelectionGroupBox.Controls.Add(this.DirectionDropEdit);
			this.SelectionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.SelectionGroupBox.Name = "SelectionGroupBox";
			this.SelectionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 157, true);
			this.SelectionGroupBox.TabIndex = 2;
			this.SelectionGroupBox.TabStop = false;
			// 
			// DispatchDepotZAddressControl
			// 
			this.DispatchDepotZAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DispatchDepotZAddressControl, "Collection.TY_OA_DispatchDepotAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_OA_DispatchDepotAddress)));
			this.DispatchDepotZAddressControl.BindToOrgList = "Collection.Lookups.Depots";
			this.DispatchDepotZAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 130, true);
			this.DispatchDepotZAddressControl.Name = "DispatchDepotZAddressControl";
			this.DispatchDepotZAddressControl.PopupCaption = "";
			this.DispatchDepotZAddressControl.ReadOnly = false;
			this.DispatchDepotZAddressControl.ShowAddress = false;
			this.DispatchDepotZAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 18, true);
			this.DispatchDepotZAddressControl.TabIndex = 4;
			// 
			// PackTypeDropEdit
			// 
			this.PackTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackTypeDropEdit, "Collection.TY_F3_NKPackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_F3_NKPackType)));
			this.PackTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 102, true);
			this.PackTypeDropEdit.Name = "PackTypeDropEdit";
			this.PackTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.PackTypeDropEdit.TabIndex = 3;
			// 
			// DGDropEdit
			// 
			this.DGDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DGDropEdit, "Collection.TY_UndgClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_UndgClass)));
			this.DGDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 46, true);
			this.DGDropEdit.Name = "DGDropEdit";
			this.DGDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.DGDropEdit.TabIndex = 1;
			// 
			// ServiceLevelDropEdit
			// 
			this.ServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelDropEdit, "Collection.TY_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_RS_NKServiceLevel)));
			this.ServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 74, true);
			this.ServiceLevelDropEdit.Name = "ServiceLevelDropEdit";
			this.ServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.ServiceLevelDropEdit.TabIndex = 2;
			// 
			// DirectionDropEdit
			// 
			this.DirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DirectionDropEdit, "Collection.TY_Direction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_Direction)));
			this.DirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 17, true);
			this.DirectionDropEdit.Name = "DirectionDropEdit";
			this.DirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.DirectionDropEdit.TabIndex = 0;
			// 
			// portHubFilterStripControl
			// 
			this.portHubFilterStripControl.AllowDrop = true;
			this.portHubFilterStripControl.AutoScroll = true;
			this.portHubFilterStripControl.AutoSize = true;
			this.portHubFilterStripControl.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.portHubFilterStripControl, ".");
			this.portHubFilterStripControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.portHubFilterStripControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.portHubFilterStripControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.portHubFilterStripControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 70, true);
			this.portHubFilterStripControl.Name = "portHubFilterStripControl";
			this.portHubFilterStripControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 70, true);
			this.portHubFilterStripControl.TabIndex = 0;
			// 
			// PortsGrid
			// 
			this.PortsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PortsGrid, "Collection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_UndgClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_RS_NKServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).DispatchDepotPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).DispatchDepotPortCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_RatingFreightMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).DepotPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).DepotPortCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_OH_CarrierBookingAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_PackMode)));
			this.PortsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "TY_Direction";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "TY_UndgClass";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "TY_RS_NKServiceLevel";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "TY_F3_NKPackType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "DispatchDepotPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "DispatchDepotPortCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.ColumnName = "TY_RatingFreightMode";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "DepotPK";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "DepotPortCode";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo3.ColumnName = "TY_OH_CarrierBookingAgent";
			zOrganisationFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zDropEditColumnStyleInfo6.ColumnName = "TY_PackMode";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PortsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PortsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PortsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PortsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PortsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.PortsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PortsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.PortsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.PortsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.PortsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo3);
			this.PortsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.PortsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PortsGrid.GridId = "1ea6a3b0-1758-44ec-bece-c88bee0db49c";
			this.PortsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PortsGrid.LayoutKey = "PortsGrid";
			this.PortsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 70, true);
			this.PortsGrid.Name = "PortsGrid";
			this.PortsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 327, true);
			this.PortsGrid.TabIndex = 1;
			// 
			// ZonesGroupBox
			// 
			this.ZonesGroupBox.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("884ad9b4-6299-475e-8c30-4acb3278446b", "Zones");
			this.ZonesGroupBox.Controls.Add(this.ZonesGrid);
			this.ZonesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ZonesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 0, true);
			this.ZonesGroupBox.Name = "ZonesGroupBox";
			this.ZonesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 251, true);
			this.ZonesGroupBox.TabIndex = 3;
			this.ZonesGroupBox.TabStop = false;
			// 
			// ZonesGrid
			// 
			this.ZonesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ZonesGrid, "Collection.PortHubZonePivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).PortHubZonePivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubZonePivot)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).PortHubZonePivots)).SyncRoot)).TX_TZ_Zone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubZonePivot)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).PortHubZonePivots)).SyncRoot)).CarrierPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubZonePivot)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).PortHubZonePivots)).SyncRoot)).TX_PL_NKCarrierServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.PortHubs.Business.PortHubZonePivot)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).PortHubZonePivots)).SyncRoot)).TX_CarrierAccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.PortHubs.Business.PortHubZonePivot)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).PortHubZonePivots)).SyncRoot)).TX_PickupCutOffTimeVariance)));
			this.ZonesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TX_TZ_Zone";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "CarrierPK";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.ColumnName = "TX_PL_NKCarrierServiceLevel";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo8.ColumnName = "TX_CarrierAccountNumber";
			zDropEditColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("b51cafa7-761e-44df-9b4f-829e643c7622", "Carrier Account Number");
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TX_PickupCutOffTimeVariance";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ZonesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ZonesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ZonesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.ZonesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ZonesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.ZonesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ZonesGrid.GridId = "37f9454c-1ff6-4ebe-820b-ddbe888ceafb";
			this.ZonesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ZonesGrid.LayoutKey = "ZonesGrid";
			this.ZonesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ZonesGrid.Name = "ZonesGrid";
			this.ZonesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 234, true);
			this.ZonesGrid.TabIndex = 0;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Freight.PortHubs.GUI.Res.GetData("25ae1255-4c27-4b45-96c6-3de4124af308", "Resulting Depot and Mode");
			this.zGroupBox1.Controls.Add(this.DepotAddressZAddressControl);
			this.zGroupBox1.Controls.Add(this.FreightRateModeDropEdit);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 158, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 92, true);
			this.zGroupBox1.TabIndex = 4;
			this.zGroupBox1.TabStop = false;
			// 
			// DepotAddressZAddressControl
			// 
			this.DepotAddressZAddressControl.AllowDrop = true;
			this.DepotAddressZAddressControl.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.BindingSource.SetBindingMember(this.DepotAddressZAddressControl, "Collection.TY_OA_DepotAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_OA_DepotAddress)));
			this.DepotAddressZAddressControl.BindToOrgList = "Collection.Lookups.Depots";
			this.DepotAddressZAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 25, true);
			this.DepotAddressZAddressControl.Name = "DepotAddressZAddressControl";
			this.DepotAddressZAddressControl.PopupCaption = "";
			this.DepotAddressZAddressControl.ReadOnly = false;
			this.DepotAddressZAddressControl.ShowAddress = false;
			this.DepotAddressZAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 18, true);
			this.DepotAddressZAddressControl.TabIndex = 0;
			// 
			// FreightRateModeDropEdit
			// 
			this.FreightRateModeDropEdit.AllowDrop = true;
			this.FreightRateModeDropEdit.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.BindingSource.SetBindingMember(this.FreightRateModeDropEdit, "Collection.TY_RatingFreightMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.PortHubs.Business.PortHubSelection)(((System.Collections.IList)(((Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper)(null)).Collection)).SyncRoot)).TY_RatingFreightMode)));
			this.FreightRateModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 51, true);
			this.FreightRateModeDropEdit.Name = "FreightRateModeDropEdit";
			this.FreightRateModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 18, true);
			this.FreightRateModeDropEdit.TabIndex = 1;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.ZonesGroupBox);
			this.bottomPanel.Controls.Add(this.selectionDetailsPanel);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 376, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 251, true);
			this.bottomPanel.TabIndex = 8;
			// 
			// selectionDetailsPanel
			// 
			this.selectionDetailsPanel.Controls.Add(this.SelectionGroupBox);
			this.selectionDetailsPanel.Controls.Add(this.zGroupBox1);
			this.selectionDetailsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.selectionDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.selectionDetailsPanel.Name = "selectionDetailsPanel";
			this.selectionDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 251, true);
			this.selectionDetailsPanel.TabIndex = 5;
			// 
			// basePanel
			// 
			this.basePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.basePanel.Controls.Add(this.PortsGrid);
			this.basePanel.Controls.Add(this.portHubFilterStripControl);
			this.basePanel.Controls.Add(this.bottomPanel);
			this.basePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.basePanel.Name = "basePanel";
			this.basePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 627, true);
			this.basePanel.TabIndex = 9;
			// 
			// PortHubSelectionForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 686, true);
			this.Controls.Add(this.PostingButtons);
			this.Controls.Add(this.basePanel);
			this.DataSourceType = typeof(Enterprise.Freight.PortHubs.Business.PortHubSelectionCollectionWrapper);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 725, true);
			this.Name = "PortHubSelectionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.basePanel, 0);
			this.Controls.SetChildIndex(this.PostingButtons, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtons.ResumeLayout(true);
			this.PostingButtons.PerformLayout();
			this.SelectionGroupBox.ResumeLayout(false);
			this.SelectionGroupBox.PerformLayout();
			this.DispatchDepotZAddressControl.ResumeLayout(true);
			this.DispatchDepotZAddressControl.PerformLayout();
			this.PackTypeDropEdit.ResumeLayout(true);
			this.PackTypeDropEdit.PerformLayout();
			this.DGDropEdit.ResumeLayout(true);
			this.DGDropEdit.PerformLayout();
			this.ServiceLevelDropEdit.ResumeLayout(true);
			this.ServiceLevelDropEdit.PerformLayout();
			this.DirectionDropEdit.ResumeLayout(true);
			this.DirectionDropEdit.PerformLayout();
			this.portHubFilterStripControl.ResumeLayout(true);
			this.portHubFilterStripControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PortsGrid)).EndInit();
			this.PortsGrid.ResumeLayout(false);
			this.PortsGrid.PerformLayout();
			this.ZonesGroupBox.ResumeLayout(false);
			this.ZonesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZonesGrid)).EndInit();
			this.ZonesGrid.ResumeLayout(false);
			this.ZonesGrid.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.DepotAddressZAddressControl.ResumeLayout(true);
			this.DepotAddressZAddressControl.PerformLayout();
			this.FreightRateModeDropEdit.ResumeLayout(true);
			this.FreightRateModeDropEdit.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.selectionDetailsPanel.ResumeLayout(false);
			this.selectionDetailsPanel.PerformLayout();
			this.basePanel.ResumeLayout(false);
			this.basePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Core.Forms.ZPostingButtonsUserControl PostingButtons;
		private ZArchitecture.ZGrid PortsGrid;
		private ZArchitecture.GUI.ZGroupBox SelectionGroupBox;
		private ZArchitecture.GUI.ZDropEdit DirectionDropEdit;
		private ZArchitecture.GUI.ZDropEdit ServiceLevelDropEdit;
		private ZArchitecture.GUI.ZDropEdit DGDropEdit;
		private ZArchitecture.GUI.ZDropEdit PackTypeDropEdit;
		private ZArchitecture.GUI.ZAddressControl DispatchDepotZAddressControl;
		private PortHubFilterStripControl portHubFilterStripControl;
		private ZArchitecture.GUI.ZGroupBox ZonesGroupBox;
		private ZArchitecture.ZGrid ZonesGrid;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZAddressControl DepotAddressZAddressControl;
		private ZArchitecture.GUI.ZDropEdit FreightRateModeDropEdit;
		private ZArchitecture.GUI.ZPanel bottomPanel;
		private ZArchitecture.GUI.ZPanel selectionDetailsPanel;
		private ZArchitecture.GUI.ZPanel basePanel;
	}
}
