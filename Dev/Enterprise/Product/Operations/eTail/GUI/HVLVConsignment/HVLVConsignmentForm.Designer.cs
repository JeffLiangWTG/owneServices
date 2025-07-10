namespace Enterprise.eTail.GUI
{
	partial class HVLVConsignmentForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.workflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.workflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1232, 946, true);
			this.workflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.workflowTabPage_InitializeTab));
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.workflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.AutoScroll = true;
			this.MainTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 586, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1224, 919, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1224, 919, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1224, 919, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1232, 946, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1232, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.eTail.Business.HVLVConsignment);
			// 
			// workflowTabPage
			// 
			this.workflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.workflowTabPage.Name = "workflowTabPage";
			this.workflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.workflowTabPage.TabIndex = 3;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.eTail.Business.HVLVItem)(((Enterprise.eTail.Business.HVLVItem)(((System.Collections.IList)(((Enterprise.eTail.Business.HVLVConsignment)(null)).Items)).SyncRoot)))));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.eTail.Business.HVLVItemLine)(((Enterprise.eTail.Business.HVLVItemLine)(((System.Collections.IList)(((Enterprise.eTail.Business.HVLVItem)(((System.Collections.IList)(((Enterprise.eTail.Business.HVLVConsignment)(null)).Items)).SyncRoot)).Lines)).SyncRoot)))));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVConsignment)(null)).ShipmentTransportMode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVConsignment)(null)).ShipmentPackingMode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).ConsolMasterBill)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).ConsolVoyageFlight)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).ConsolVessel)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVConsignment)(null)).ConsolETD)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVConsignment)(null)).ConsolETA)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).ConsolOrigin)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).ConsolDestination)));
			// 
			// HVLVConsignmentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1232, 1002, true);
			this.DataSourceType = typeof(Enterprise.eTail.Business.HVLVConsignment);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1215, 720, true);
			this.Name = "HVLVConsignmentForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void workflowTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.workflowTabPage.SuspendLayout();
			this.workflowTabPage.ResumeLayout(false);
			this.workflowTabPage.PerformLayout();

		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.consignmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.consignmentSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.consignmentDetailsUserControl = new Enterprise.eTail.GUI.HVLVConsignmentDetailsUserControl();
			this.itemSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.itemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.itemsUserControl = new Enterprise.eTail.GUI.HVLVItemsUserControl();
			this.itemLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.itemLinesUserControl = new Enterprise.eTail.GUI.HVLVItemLinesUserControl();
			this.shipmentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.transportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.packingModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.masterBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.voyageFlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.vesselTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.etdTextBox = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.etaTextBox = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.originCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.destinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MainTabPage.SuspendLayout();
			this.consignmentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.consignmentSplitContainer)).BeginInit();
			this.consignmentSplitContainer.Panel1.SuspendLayout();
			this.consignmentSplitContainer.Panel2.SuspendLayout();
			this.consignmentSplitContainer.SuspendLayout();
			this.consignmentDetailsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.itemSplitContainer)).BeginInit();
			this.itemSplitContainer.Panel1.SuspendLayout();
			this.itemSplitContainer.Panel2.SuspendLayout();
			this.itemSplitContainer.SuspendLayout();
			this.itemsGroupBox.SuspendLayout();
			this.itemsUserControl.SuspendLayout();
			this.itemLinesGroupBox.SuspendLayout();
			this.itemLinesUserControl.SuspendLayout();
			this.shipmentDetailsGroupBox.SuspendLayout();
			this.transportModeDropEdit.SuspendLayout();
			this.packingModeDropEdit.SuspendLayout();
			this.etdTextBox.SuspendLayout();
			this.etaTextBox.SuspendLayout();
			this.originCodeFindBox.SuspendLayout();
			this.destinationCodeFindBox.SuspendLayout();
			this.MainTabPage.Controls.Add(this.consignmentGroupBox);
			this.MainTabPage.Controls.Add(this.shipmentDetailsGroupBox);
			// 
			// consignmentGroupBox
			// 
			this.consignmentGroupBox.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("5f5d162a-36ea-466f-8bdb-099645afc2b1", "Consignment");
			this.consignmentGroupBox.Controls.Add(this.consignmentSplitContainer);
			this.consignmentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.consignmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 119, true);
			this.consignmentGroupBox.Name = "consignmentGroupBox";
			this.consignmentGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 3, 2, 3, true);
			this.consignmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1224, 800, true);
			this.consignmentGroupBox.TabIndex = 0;
			this.consignmentGroupBox.TabStop = false;
			// 
			// consignmentSplitContainer
			//
			this.consignmentSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.consignmentSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 16, true);
			this.consignmentSplitContainer.Name = "consignmentSplitContainer";
			this.consignmentSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// consignmentSplitContainer.Panel1
			// 
			this.consignmentSplitContainer.Panel1.Controls.Add(this.consignmentDetailsUserControl);
			this.consignmentSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1213, 768, true);
			this.consignmentSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(220);
			// 
			// consignmentSplitContainer.Panel2
			// 
			this.consignmentSplitContainer.Panel2.Controls.Add(this.itemSplitContainer);
			this.consignmentSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(418);
			this.consignmentSplitContainer.TabIndex = 1;
			// 
			// consignmentDetailsUserControl
			// 
			this.consignmentDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.consignmentDetailsUserControl, ".");
			this.consignmentDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.consignmentDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.consignmentDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 220, true);
			this.consignmentDetailsUserControl.Name = "consignmentDetailsUserControl";
			this.consignmentDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1213, 418, true);
			this.consignmentDetailsUserControl.TabIndex = 0;
			// 
			// itemSplitContainer
			// 
			this.itemSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.itemSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.itemSplitContainer.Name = "itemSplitContainer";
			this.itemSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// itemSplitContainer.Panel1
			// 
			this.itemSplitContainer.Panel1.Controls.Add(this.itemsGroupBox);
			// 
			// itemSplitContainer.Panel2
			// 
			this.itemSplitContainer.Panel2.Controls.Add(this.itemLinesGroupBox);
			this.itemSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1213, 310, true);
			this.itemSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(152);
			this.itemSplitContainer.TabIndex = 0;
			// 
			// itemsGroupBox
			// 
			this.itemsGroupBox.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("042de10a-dd57-4ec5-8002-61ab8ec1d243", "Items");
			this.itemsGroupBox.Controls.Add(this.itemsUserControl);
			this.itemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.itemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.itemsGroupBox.Name = "itemsGroupBox";
			this.itemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1213, 152, true);
			this.itemsGroupBox.TabIndex = 0;
			this.itemsGroupBox.TabStop = false;
			// 
			// itemsUserControl
			// 
			this.itemsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.itemsUserControl, "Items");
			this.itemsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.itemsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.itemsUserControl.Name = "itemsUserControl";
			this.itemsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1207, 131, true);
			this.itemsUserControl.TabIndex = 0;
			// 
			// itemLinesGroupBox
			// 
			this.itemLinesGroupBox.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("643d8c60-7326-4bd6-8e19-821bb2bf309c", "Item Lines");
			this.itemLinesGroupBox.Controls.Add(this.itemLinesUserControl);
			this.itemLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.itemLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.itemLinesGroupBox.Name = "itemLinesGroupBox";
			this.itemLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1213, 152, true);
			this.itemLinesGroupBox.TabIndex = 1;
			this.itemLinesGroupBox.TabStop = false;
			// 
			// itemLinesUserControl
			// 
			this.itemLinesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.itemLinesUserControl, "Items.Lines");
			this.itemLinesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.itemLinesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.itemLinesUserControl.Name = "itemLinesUserControl";
			this.itemLinesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1207, 131, true);
			this.itemLinesUserControl.TabIndex = 0;
			// 
			// shipmentDetailsGroupBox
			// 
			this.shipmentDetailsGroupBox.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("4146876f-70f2-4130-ada2-0491a65060e3", "Shipment Details");
			this.shipmentDetailsGroupBox.Controls.Add(this.masterBillTextBox);
			this.shipmentDetailsGroupBox.Controls.Add(this.transportModeDropEdit);
			this.shipmentDetailsGroupBox.Controls.Add(this.voyageFlightTextBox);
			this.shipmentDetailsGroupBox.Controls.Add(this.vesselTextBox);
			this.shipmentDetailsGroupBox.Controls.Add(this.etdTextBox);
			this.shipmentDetailsGroupBox.Controls.Add(this.etaTextBox);
			this.shipmentDetailsGroupBox.Controls.Add(this.packingModeDropEdit);
			this.shipmentDetailsGroupBox.Controls.Add(this.originCodeFindBox);
			this.shipmentDetailsGroupBox.Controls.Add(this.destinationCodeFindBox);
			this.shipmentDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.shipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.shipmentDetailsGroupBox.Name = "shipmentDetailsGroupBox";
			this.shipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1213, 119, true);
			this.shipmentDetailsGroupBox.TabIndex = 0;
			this.shipmentDetailsGroupBox.TabStop = false;
			// 
			// transportModeDropEdit
			// 
			this.transportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.transportModeDropEdit, "ShipmentTransportMode");
			this.transportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 37, true);
			this.transportModeDropEdit.Name = "transportModeDropEdit";
			this.transportModeDropEdit.PreBoundMaxLength = 3;
			this.transportModeDropEdit.ShouldResizeByMaxLength = true;
			this.transportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.transportModeDropEdit.TabIndex = 1;
			// 
			// packingModeDropEdit
			// 
			this.packingModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.packingModeDropEdit, "ShipmentPackingMode");
			this.packingModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(343, 59, true);
			this.packingModeDropEdit.Name = "packingModeDropEdit";
			this.packingModeDropEdit.PreBoundMaxLength = 3;
			this.packingModeDropEdit.ShouldResizeByMaxLength = true;
			this.packingModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.packingModeDropEdit.TabIndex = 6;
			// 
			// masterBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.masterBillTextBox, "ConsolMasterBill");
			this.masterBillTextBox.CaptionResourceString = null;
			this.masterBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 15, true);
			this.masterBillTextBox.Name = "masterBillTextBox";
			this.masterBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.masterBillTextBox.TabIndex = 0;
			// 
			// voyageFlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.voyageFlightTextBox, "ConsolVoyageFlight");
			this.voyageFlightTextBox.CaptionResourceString = null;
			this.voyageFlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 59, true);
			this.voyageFlightTextBox.Name = "voyageFlightTextBox";
			this.voyageFlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.voyageFlightTextBox.TabIndex = 2;
			// 
			// vesselTextBox
			// 
			this.BindingSource.SetBindingMember(this.vesselTextBox, "ConsolVessel");
			this.vesselTextBox.CaptionResourceString = null;
			this.vesselTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 81, true);
			this.vesselTextBox.Name = "vesselTextBox";
			this.vesselTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.vesselTextBox.TabIndex = 3;
			// 
			// etdTextBox
			// 
			this.etdTextBox.AllowDrop = true;
			this.etdTextBox.AutoCompleteMonthThreshold = 1;
			this.etdTextBox.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.etdTextBox, "ConsolETD");
			this.etdTextBox.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.etdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(343, 15, true);
			this.etdTextBox.Name = "etdTextBox";
			this.etdTextBox.TabIndex = 4;
			// 
			// etaTextBox
			// 
			this.etaTextBox.AllowDrop = true;
			this.etaTextBox.AutoCompleteMonthThreshold = 1;
			this.etaTextBox.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.etaTextBox, "ConsolETA");
			this.etaTextBox.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.etaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(343, 37, true);
			this.etaTextBox.Name = "etaTextBox";
			this.etaTextBox.TabIndex = 5;
			// 
			// originCodeFindBox
			// 
			this.originCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.originCodeFindBox, "ConsolOrigin");
			this.originCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(597, 15, true);
			this.originCodeFindBox.Name = "originCodeFindBox";
			this.originCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.originCodeFindBox.TabIndex = 7;
			// 
			// destinationCodeFindBox
			// 
			this.destinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.destinationCodeFindBox, "ConsolDestination");
			this.destinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(597, 37, true);
			this.destinationCodeFindBox.Name = "destinationCodeFindBox";
			this.destinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.destinationCodeFindBox.TabIndex = 8;
			this.MainTabPage.PerformLayout();
			this.consignmentGroupBox.ResumeLayout(false);
			this.consignmentGroupBox.PerformLayout();
			this.consignmentSplitContainer.Panel1.ResumeLayout(false);
			this.consignmentSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.consignmentSplitContainer)).EndInit();
			this.consignmentSplitContainer.ResumeLayout(false);
			this.consignmentSplitContainer.PerformLayout();
			this.consignmentDetailsUserControl.ResumeLayout(true);
			this.consignmentDetailsUserControl.PerformLayout();
			this.itemSplitContainer.Panel1.ResumeLayout(false);
			this.itemSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.itemSplitContainer)).EndInit();
			this.itemSplitContainer.ResumeLayout(false);
			this.itemSplitContainer.PerformLayout();
			this.itemsGroupBox.ResumeLayout(false);
			this.itemsGroupBox.PerformLayout();
			this.itemsUserControl.ResumeLayout(true);
			this.itemsUserControl.PerformLayout();
			this.itemLinesGroupBox.ResumeLayout(false);
			this.itemLinesGroupBox.PerformLayout();
			this.itemLinesUserControl.ResumeLayout(true);
			this.itemLinesUserControl.PerformLayout();
			this.shipmentDetailsGroupBox.ResumeLayout(false);
			this.shipmentDetailsGroupBox.PerformLayout();
			this.transportModeDropEdit.ResumeLayout(true);
			this.transportModeDropEdit.PerformLayout();
			this.packingModeDropEdit.ResumeLayout(true);
			this.packingModeDropEdit.PerformLayout();
			this.etdTextBox.ResumeLayout(true);
			this.etdTextBox.PerformLayout();
			this.etaTextBox.ResumeLayout(true);
			this.etaTextBox.PerformLayout();
			this.originCodeFindBox.ResumeLayout(true);
			this.originCodeFindBox.PerformLayout();
			this.destinationCodeFindBox.ResumeLayout(true);
			this.destinationCodeFindBox.PerformLayout();
			this.MainTabPage.ResumeLayout(true);

		}

		private void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		private CargoWise.Windows.UI.KSplitContainer consignmentSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer itemSplitContainer;
		private Enterprise.eTail.GUI.HVLVConsignmentDetailsUserControl consignmentDetailsUserControl;
		private Enterprise.eTail.GUI.HVLVItemsUserControl itemsUserControl;
		private Enterprise.eTail.GUI.HVLVItemLinesUserControl itemLinesUserControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox consignmentGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox shipmentDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox itemsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox itemLinesGroupBox;
		private MasterFiles.GUI.ZWorkflowTabPage workflowTabPage;
		private Enterprise.ZArchitecture.GUI.ZDropEdit transportModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit packingModeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox masterBillTextBox;
		private Enterprise.ZArchitecture.ZTextBox voyageFlightTextBox;
		private Enterprise.ZArchitecture.ZTextBox vesselTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit etdTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit etaTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox originCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox destinationCodeFindBox;


		#endregion
	}
}
