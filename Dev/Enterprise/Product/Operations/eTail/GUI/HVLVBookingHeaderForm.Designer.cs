namespace Enterprise.eTail.GUI
{
	partial class HVLVBookingHeaderForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.serviceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.deniedPartyScreeningStatusDropEdit = new Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit();
			this.deniedPartyScreeningButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.dispatchAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.shipperAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.headerDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.bookingStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.bookedByContactDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.originDepotAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.bookingHeaderConsignmentUserControl = new Enterprise.eTail.GUI.HVLVConsignmentUserControl();
			this.volumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.weightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.itemCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.workflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.isBookingReceivedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.pickupRequiredByDateEdit = new ZArchitecture.GUI.ZDateEdit();
			this.pickupRequiredFromDateEdit = new ZArchitecture.GUI.ZDateEdit();
			this.pickupCartageAdvisedDateEdit = new ZArchitecture.GUI.ZDateEdit();
			this.estimatedPickupDateEdit = new ZArchitecture.GUI.ZDateEdit();
			this.pickupCartageCompletedDateEdit = new ZArchitecture.GUI.ZDateEdit();
			this.headerDetailsPanel = new ZArchitecture.GUI.ZPanel();

			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.serviceLevelDropEdit.SuspendLayout();
			this.deniedPartyScreeningStatusDropEdit.SuspendLayout();
			this.deniedPartyScreeningButton.SuspendLayout();
			this.dispatchAddressControl.SuspendLayout();
			this.shipperAddressControl.SuspendLayout();
			this.headerDetailsGroupBox.SuspendLayout();
			this.bookingStatusDropEdit.SuspendLayout();
			this.bookedByContactDropEdit.SuspendLayout();
			this.originDepotAddressControl.SuspendLayout();
			this.bookingHeaderConsignmentUserControl.SuspendLayout();
			this.volumeCalcDropEdit.SuspendLayout();
			this.weightCalcDropEdit.SuspendLayout();
			this.workflowTabPage.SuspendLayout();
			this.isBookingReceivedCheckBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.workflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 631, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.workflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.bookingHeaderConsignmentUserControl);
			this.MainTabPage.Controls.Add(this.headerDetailsGroupBox);
			this.MainTabPage.AutoScroll = true;
			this.MainTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 586, true);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(929, 608, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(929, 587, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(929, 608, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 631, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.eTail.Business.HVLVBookingHeader);
			// 
			// serviceLevelDropEdit
			// 
			this.serviceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.serviceLevelDropEdit, "HVH_RS_NKBookingServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).HVH_RS_NKBookingServiceLevel)));
			this.serviceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 62, true);
			this.serviceLevelDropEdit.Name = "serviceLevelDropEdit";
			this.serviceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.serviceLevelDropEdit.TabIndex = 6;
			// 
			// deniedPartyScreeningStatusDropEdit
			// 
			this.deniedPartyScreeningStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.deniedPartyScreeningStatusDropEdit, "HVH_DeniedPartyScreeningStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).HVH_DeniedPartyScreeningStatus)));
			this.deniedPartyScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 84, true);
			this.deniedPartyScreeningStatusDropEdit.Name = "deniedPartyScreeningStatusDropEdit";
			this.deniedPartyScreeningStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.deniedPartyScreeningStatusDropEdit.TabIndex = 7;
			// 
			// deniedPartyScreeningButton
			// 
			this.deniedPartyScreeningButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.deniedPartyScreeningButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(787, 84, true);
			this.deniedPartyScreeningButton.Name = "deniedPartyScreeningButton";
			this.deniedPartyScreeningButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 18, true);
			this.deniedPartyScreeningButton.TabIndex = 8;
			this.deniedPartyScreeningButton.Text = "...";
			this.deniedPartyScreeningButton.UseVisualStyleBackColor = false;
			this.deniedPartyScreeningButton.Click += new System.EventHandler(this.DeniedPartyingScreeningButton_Click);
			// 
			// dispatchAddressControl
			// 
			this.dispatchAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dispatchAddressControl, "HVH_OA_DispatchAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).HVH_OA_DispatchAddress)));
			this.dispatchAddressControl.BindToOrgList = "Lookups.OrgList";
			this.dispatchAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 39, true);
			this.dispatchAddressControl.Name = "dispatchAddressControl";
			this.dispatchAddressControl.PopupCaption = "";
			this.dispatchAddressControl.ReadOnly = false;
			this.dispatchAddressControl.ShowAddress = false;
			this.dispatchAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 18, true);
			this.dispatchAddressControl.TabIndex = 1;
			// 
			// shipperAddressControl
			// 
			this.shipperAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.shipperAddressControl, "HVH_OA_BillToParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).HVH_OA_BillToParty)));
			this.shipperAddressControl.BindToOrgList = "Lookups.DebtorOrgList";
			this.shipperAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 17, true);
			this.shipperAddressControl.Name = "shipperAddressControl";
			this.shipperAddressControl.PopupCaption = "";
			this.shipperAddressControl.ReadOnly = false;
			this.shipperAddressControl.ShowAddress = false;
			this.shipperAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 18, true);
			this.shipperAddressControl.TabIndex = 0;
			//
			// headerDetails
			//
			this.headerDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.headerDetailsPanel.AutoScroll = true;
			this.headerDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 135, true);
			this.headerDetailsPanel.Controls.Add(this.bookingStatusDropEdit);
			this.headerDetailsPanel.Controls.Add(this.bookedByContactDropEdit);
			this.headerDetailsPanel.Controls.Add(this.originDepotAddressControl);
			this.headerDetailsPanel.Controls.Add(this.shipperAddressControl);
			this.headerDetailsPanel.Controls.Add(this.serviceLevelDropEdit);
			this.headerDetailsPanel.Controls.Add(this.deniedPartyScreeningStatusDropEdit);
			this.headerDetailsPanel.Controls.Add(this.deniedPartyScreeningButton);
			this.headerDetailsPanel.Controls.Add(this.dispatchAddressControl);
			this.headerDetailsPanel.Controls.Add(this.volumeCalcDropEdit);
			this.headerDetailsPanel.Controls.Add(this.weightCalcDropEdit);
			this.headerDetailsPanel.Controls.Add(this.itemCountCalcEdit);
			this.headerDetailsPanel.Controls.Add(this.isBookingReceivedCheckBox);
			this.headerDetailsPanel.Controls.Add(this.pickupRequiredByDateEdit);
			this.headerDetailsPanel.Controls.Add(this.pickupRequiredFromDateEdit);
			this.headerDetailsPanel.Controls.Add(this.pickupCartageAdvisedDateEdit);
			this.headerDetailsPanel.Controls.Add(this.estimatedPickupDateEdit);
			this.headerDetailsPanel.Controls.Add(this.pickupCartageCompletedDateEdit);
			// 
			// headerDetailsGroupBox
			// 
			this.headerDetailsGroupBox.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("9d4107d3-e0f4-41b8-97b9-61a96a014ee6", "Booking Header Details");
			this.headerDetailsGroupBox.Controls.Add(this.headerDetailsPanel);
			this.headerDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 2, true);
			this.headerDetailsGroupBox.Name = "headerDetailsGroupBox";
			this.headerDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 165, true);
			this.headerDetailsGroupBox.TabIndex = 0;
			this.headerDetailsGroupBox.TabStop = false;
			this.headerDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			// 
			// bookingStatusDropEdit
			// 
			this.bookingStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bookingStatusDropEdit, "BookingStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).BookingStatus)));
			this.bookingStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 39, true);
			this.bookingStatusDropEdit.Name = "bookingStatusDropEdit";
			this.bookingStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.bookingStatusDropEdit.TabIndex = 5;
			// 
			// bookedByContactDropEdit
			// 
			this.bookedByContactDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bookedByContactDropEdit, "HVH_OC_BookedBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).HVH_OC_BookedBy)));
			this.bookedByContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 17, true);
			this.bookedByContactDropEdit.Name = "bookedByContactDropEdit";
			this.bookedByContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.bookedByContactDropEdit.TabIndex = 4;
			// 
			// originDepotAddressControl
			// 
			this.originDepotAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.originDepotAddressControl, "HVH_OA_OriginDepot");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).HVH_OA_OriginDepot)));
			this.originDepotAddressControl.BindToOrgList = "Lookups.PackDepotOrgList";
			this.originDepotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 62, true);
			this.originDepotAddressControl.Name = "originDepotAddressControl";
			this.originDepotAddressControl.PopupCaption = "";
			this.originDepotAddressControl.ReadOnly = false;
			this.originDepotAddressControl.ShowAddress = false;
			this.originDepotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 18, true);
			this.originDepotAddressControl.TabIndex = 2;
			// 
			// bookingHeaderConsignmentUserControl
			// 
			this.bookingHeaderConsignmentUserControl.AllowDrop = true;
			this.bookingHeaderConsignmentUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.bookingHeaderConsignmentUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.eTail.Business.HVLVBookingHeader)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)))));
			this.bookingHeaderConsignmentUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 143, true);
			this.bookingHeaderConsignmentUserControl.Name = "bookingHeaderConsignmentUserControl";
			this.bookingHeaderConsignmentUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 444, true);
			this.bookingHeaderConsignmentUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 444, true);
			this.bookingHeaderConsignmentUserControl.TabIndex = 1;
			this.bookingHeaderConsignmentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// volumeCalcDropEdit
			// 
			this.volumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.volumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).HVH_GrossVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).HVH_GrossVolumeUQ)));
			this.volumeCalcDropEdit.BindToAmount = "HVH_GrossVolume";
			this.volumeCalcDropEdit.BindToUnit = "HVH_GrossVolumeUQ";
			this.volumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(446, 108, true);
			this.volumeCalcDropEdit.Name = "volumeCalcDropEdit";
			this.volumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 18, true);
			this.volumeCalcDropEdit.TabIndex = 11;
			this.volumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// pickupRequiredFromDateEdit
			// 
			this.pickupRequiredFromDateEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pickupRequiredFromDateEdit, "DocsAndCartage+JP_PickupRequiredFrom");
			this.pickupRequiredFromDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.pickupRequiredFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1000, 17, true);
			this.pickupRequiredFromDateEdit.Name = "pickupRequiredFromDateEdit";
			this.pickupRequiredFromDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 18, true);
			this.pickupRequiredFromDateEdit.TabIndex = 12;
			// 
			// pickupRequiredByDateEdit
			// 
			this.pickupRequiredByDateEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pickupRequiredByDateEdit, "DocsAndCartage+JP_PickupRequiredBy");
			this.pickupRequiredByDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.pickupRequiredByDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1000, 39, true);
			this.pickupRequiredByDateEdit.Name = "pickupRequiredByDateEdit";
			this.pickupRequiredByDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 18, true);
			this.pickupRequiredByDateEdit.TabIndex = 13;
			// 
			// pickupCartageAdvisedDateEdit
			// 
			this.pickupCartageAdvisedDateEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pickupCartageAdvisedDateEdit, "DocsAndCartage+JP_PickupCartageAdvised");
			this.pickupCartageAdvisedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.pickupCartageAdvisedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1000, 61, true);
			this.pickupCartageAdvisedDateEdit.Name = "pickupCartageAdvisedDateEdit";
			this.pickupCartageAdvisedDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 18, true);
			this.pickupCartageAdvisedDateEdit.TabIndex = 14;
			// 
			// estimatedPickupDateEdit
			// 
			this.estimatedPickupDateEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.estimatedPickupDateEdit, "DocsAndCartage+JP_EstimatedPickup");
			this.estimatedPickupDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.estimatedPickupDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1000, 83, true);
			this.estimatedPickupDateEdit.Name = "estimatedPickupDateEdit";
			this.estimatedPickupDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 18, true);
			this.estimatedPickupDateEdit.TabIndex = 15;
			// 
			// pickupCartageCompletedDateEdit
			// 
			this.pickupCartageCompletedDateEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pickupCartageCompletedDateEdit, "DocsAndCartage+JP_PickupCartageCompleted");
			this.pickupCartageCompletedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.pickupCartageCompletedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1000, 106, true);
			this.pickupCartageCompletedDateEdit.Name = "pickupCartageCompletedDateEdit";
			this.pickupCartageCompletedDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 18, true);
			this.pickupCartageCompletedDateEdit.TabIndex = 16;
			// 
			// weightCalcDropEdit
			// 
			this.weightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.weightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).HVH_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).HVH_GrossWeightUQ)));
			this.weightCalcDropEdit.BindToAmount = "HVH_GrossWeight";
			this.weightCalcDropEdit.BindToUnit = "HVH_GrossWeightUQ";
			this.weightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 108, true);
			this.weightCalcDropEdit.Name = "weightCalcDropEdit";
			this.weightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 18, true);
			this.weightCalcDropEdit.TabIndex = 10;
			this.weightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// itemCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.itemCountCalcEdit, "HVH_ItemCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).HVH_ItemCount)));
			this.itemCountCalcEdit.DecimalPlaces = 2;
			this.itemCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 108, true);
			this.itemCountCalcEdit.Name = "itemCountCalcEdit";
			this.itemCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 18, true);
			this.itemCountCalcEdit.TabIndex = 9;
			this.itemCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// isBookingReceivedCheckBox
			//
			this.BindingSource.SetBindingMember(this.isBookingReceivedCheckBox, "HVH_IsBookingReceived");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.eTail.Business.HVLVBookingHeader)(null)).HVH_IsBookingReceived)));
			this.isBookingReceivedCheckBox.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("C730F33E-B21F-4654-9717-B00DC7B416FD", "Received at Origin");
			this.isBookingReceivedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isBookingReceivedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 86, true);
			this.isBookingReceivedCheckBox.Name = "isBookingReceivedCheckBox";
			this.isBookingReceivedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 18, true);
			this.isBookingReceivedCheckBox.TabIndex = 3;
			this.isBookingReceivedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			// 
			// workflowTabPage
			// 
			this.workflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.workflowTabPage.Name = "workflowTabPage";
			this.workflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 553, true);
			this.workflowTabPage.TabIndex = 17;
			// 
			// HVLVBookingHeaderForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 730, true);
			this.DataSourceType = typeof(Enterprise.eTail.Business.HVLVBookingHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 725, true);
			this.Name = "HVLVBookingHeaderForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.serviceLevelDropEdit.ResumeLayout(true);
			this.serviceLevelDropEdit.PerformLayout();
			this.deniedPartyScreeningStatusDropEdit.ResumeLayout(true);
			this.deniedPartyScreeningStatusDropEdit.PerformLayout();
			this.deniedPartyScreeningButton.ResumeLayout(true);
			this.deniedPartyScreeningButton.PerformLayout();
			this.dispatchAddressControl.ResumeLayout(true);
			this.dispatchAddressControl.PerformLayout();
			this.shipperAddressControl.ResumeLayout(true);
			this.shipperAddressControl.PerformLayout();
			this.headerDetailsGroupBox.ResumeLayout(false);
			this.headerDetailsGroupBox.PerformLayout();
			this.bookingStatusDropEdit.ResumeLayout(true);
			this.bookingStatusDropEdit.PerformLayout();
			this.bookedByContactDropEdit.ResumeLayout(true);
			this.bookedByContactDropEdit.PerformLayout();
			this.originDepotAddressControl.ResumeLayout(true);
			this.originDepotAddressControl.PerformLayout();
			this.bookingHeaderConsignmentUserControl.ResumeLayout(true);
			this.bookingHeaderConsignmentUserControl.PerformLayout();
			this.volumeCalcDropEdit.ResumeLayout(true);
			this.volumeCalcDropEdit.PerformLayout();
			this.weightCalcDropEdit.ResumeLayout(true);
			this.weightCalcDropEdit.PerformLayout();
			this.workflowTabPage.ResumeLayout(false);
			this.workflowTabPage.PerformLayout();
			this.isBookingReceivedCheckBox.ResumeLayout(true);
			this.isBookingReceivedCheckBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZDropEdit serviceLevelDropEdit;
		Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit deniedPartyScreeningStatusDropEdit;
		ZArchitecture.GUI.ZButton deniedPartyScreeningButton;
		ZArchitecture.GUI.ZAddressControl dispatchAddressControl;
		ZArchitecture.GUI.ZGroupBox headerDetailsGroupBox;
		ZArchitecture.GUI.ZAddressControl shipperAddressControl;
		ZArchitecture.GUI.ZAddressControl originDepotAddressControl;
		ZArchitecture.GUI.ZGuidDropEdit bookedByContactDropEdit;
		ZArchitecture.ZCalcEdit itemCountCalcEdit;
		ZArchitecture.GUI.ZCheckBox isBookingReceivedCheckBox;

		ZArchitecture.GUI.ZCalcDropEdit weightCalcDropEdit;
		ZArchitecture.GUI.ZCalcDropEdit volumeCalcDropEdit;
		HVLVConsignmentUserControl bookingHeaderConsignmentUserControl;
		ZArchitecture.GUI.ZDropEdit bookingStatusDropEdit;
		MasterFiles.GUI.ZWorkflowTabPage workflowTabPage;

		ZArchitecture.GUI.ZPanel headerDetailsPanel;
		ZArchitecture.GUI.ZDateEdit pickupRequiredByDateEdit;
		ZArchitecture.GUI.ZDateEdit pickupRequiredFromDateEdit;
		ZArchitecture.GUI.ZDateEdit pickupCartageAdvisedDateEdit;
		ZArchitecture.GUI.ZDateEdit estimatedPickupDateEdit;
		ZArchitecture.GUI.ZDateEdit pickupCartageCompletedDateEdit;
	}
}
