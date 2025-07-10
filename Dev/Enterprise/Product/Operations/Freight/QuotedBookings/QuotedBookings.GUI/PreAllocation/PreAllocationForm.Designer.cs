
namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class PreAllocationForm
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
		new void InitializeComponent()
		{
			this.JS_GoodsDescriptionBoundTextBox = new Enterprise.Freight.GUI.GoodsDescriptionTextBox();
			this.ClientOrgControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.DeliveryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.OriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PickupDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ServiceLevelFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.CreateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.HouseBillCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TheCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.JS_HouseBillOfLadingTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsDomesticCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.WarningLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox2.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 461, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 24, true);
			this.MainStatusBar.TabIndex = 15;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.PreAllocation);
			// 
			// JS_GoodsDescriptionBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_GoodsDescriptionBoundTextBox, "QuotedBooking+Booking+JS_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).QuotedBooking.Booking.JS_GoodsDescription)));
			this.JS_GoodsDescriptionBoundTextBox.ButtonText = "Detail";
			this.JS_GoodsDescriptionBoundTextBox.CaptionResourceString = Res.GetData("PreAllocationForm|1d5e7217-561a-45ea-8b86-0c5f501edb23", "Description");
			this.JS_GoodsDescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 141, true);
			this.JS_GoodsDescriptionBoundTextBox.Name = "JS_GoodsDescriptionBoundTextBox";
			this.JS_GoodsDescriptionBoundTextBox.NoteTypeDescription = "Detailed Goods Description";
			this.JS_GoodsDescriptionBoundTextBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 1, true);
			this.JS_GoodsDescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 23, true);
			this.JS_GoodsDescriptionBoundTextBox.TabIndex = 10;
			// 
			// ClientOrgControl
			// 
			this.BindingSource.SetBindingMember(this.ClientOrgControl, "QuotedBooking.ClientPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).QuotedBooking.ClientPK)));
			this.ClientOrgControl.BindToOrganisations = "QuotedBooking.Clients";
			this.ClientOrgControl.CaptionResourceString = Res.GetData("PreAllocationForm|1E040726-A4D0-4d55-962F-BBE3E53DA8BC", "Client");
			this.ClientOrgControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.FullName;
			this.ClientOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 7, true);
			this.ClientOrgControl.Name = "ClientOrgControl";
			this.ClientOrgControl.PopupCaption = "";
			this.ClientOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 52, true);
			this.ClientOrgControl.TabIndex = 0;
			// 
			// DeliveryDocAddressControl
			// 
			this.BindingSource.SetBindingMember(this.DeliveryDocAddressControl, "QuotedBooking.ConsigneeDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).QuotedBooking.ConsigneeDocumentaryAddress)));
			this.DeliveryDocAddressControl.BindToOrganisations = "QuotedBooking.Consignee_List";
			this.DeliveryDocAddressControl.CaptionResourceString = Res.GetData("PreAllocationForm|7CE7F11D-1040-425a-9160-0F314DCC0ECE", "Consignee");
			this.DeliveryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 253, true);
			this.DeliveryDocAddressControl.Name = "DeliveryDocAddressControl";
			this.DeliveryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.DeliveryDocAddressControl.TabIndex = 2;
			// 
			// OriginCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.OriginCodeFindBox, "QuotedBooking.Origin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).QuotedBooking.Origin)));
			this.OriginCodeFindBox.CaptionResourceString = Res.GetData("PreAllocationForm|d6331478-6f70-4542-b375-e942337320d7", "Origin");
			this.OriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 63, true);
			this.OriginCodeFindBox.Name = "OriginCodeFindBox";
			this.OriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.OriginCodeFindBox.TabIndex = 4;
			// 
			// PickupDocAddressControl
			// 
			this.BindingSource.SetBindingMember(this.PickupDocAddressControl, "QuotedBooking.ConsignorDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).QuotedBooking.ConsignorDocumentaryAddress)));
			this.PickupDocAddressControl.BindToOrganisations = "QuotedBooking.Consignor_List";
			this.PickupDocAddressControl.CaptionResourceString = Res.GetData("PreAllocationForm|030bd527-c1da-457e-92cb-69570707e054", "Consignor");
			this.PickupDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 65, true);
			this.PickupDocAddressControl.Name = "PickupDocAddressControl";
			this.PickupDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.PickupDocAddressControl.TabIndex = 1;
			// 
			// DestinationCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.DestinationCodeFindBox, "QuotedBooking.Destination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).QuotedBooking.Destination)));
			this.DestinationCodeFindBox.CaptionResourceString = Res.GetData("PreAllocationForm|0dc8f174-135e-4f4b-b866-56e7f1e2bcb8", "Destination");
			this.DestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 89, true);
			this.DestinationCodeFindBox.Name = "DestinationCodeFindBox";
			this.DestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.DestinationCodeFindBox.TabIndex = 6;
			// 
			// ServiceLevelFindBox
			// 
			this.BindingSource.SetBindingMember(this.ServiceLevelFindBox, "QuotedBooking.ServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).QuotedBooking.ServiceLevel)));
			this.ServiceLevelFindBox.CaptionResourceString = Res.GetData("PreAllocationForm|1381fa8e-da42-491d-9ba1-ae95944d3a98", "Service Level");
			this.ServiceLevelFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 115, true);
			this.ServiceLevelFindBox.Name = "ServiceLevelFindBox";
			this.ServiceLevelFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.ServiceLevelFindBox.TabIndex = 8;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Res.GetData("PreAllocationForm|e58bdaff-1f18-4295-b398-042608fcd46f", "House Bill Number Range");
			this.zGroupBox2.Controls.Add(this.RefreshButton);
			this.zGroupBox2.Controls.Add(this.zTextBox2);
			this.zGroupBox2.Controls.Add(this.zTextBox1);
			this.zGroupBox2.Controls.Add(this.CreateButton);
			this.zGroupBox2.Controls.Add(this.HouseBillCountCalcEdit);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 211, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 105, true);
			this.zGroupBox2.TabIndex = 4;
			this.zGroupBox2.TabStop = false;
			// 
			// RefreshButton
			// 
			this.RefreshButton.CaptionResourceString = Res.GetData("PreAllocationForm|0311c17f-5174-4ce4-9437-04915ac417a3", "Refresh");
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 70, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 26, true);
			this.RefreshButton.TabIndex = 6;
			this.RefreshButton.UseVisualStyleBackColor = true;
			this.RefreshButton.Click += new System.EventHandler(this.zButton1_Click);
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "HouseBillTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).HouseBillTo)));
			this.zTextBox2.CaptionResourceString = Res.GetData("PreAllocationForm|3e653a39-a645-4269-83df-e039519dbe3d", "To");
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 44, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
			this.zTextBox2.TabIndex = 5;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "HouseBillFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).HouseBillFrom)));
			this.zTextBox1.CaptionResourceString = Res.GetData("PreAllocationForm|f5f80dba-7549-455c-8a43-1979c112b40e", "From");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 44, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
			this.zTextBox1.TabIndex = 3;
			// 
			// CreateButton
			// 
			this.CreateButton.CaptionResourceString = Res.GetData("PreAllocationForm|c883b68c-25cd-46cb-a8a8-c0bd7670bdb3", "Create");
			this.CreateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 70, true);
			this.CreateButton.Name = "CreateButton";
			this.CreateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 26, true);
			this.CreateButton.TabIndex = 7;
			this.CreateButton.UseVisualStyleBackColor = true;
			this.CreateButton.Click += new System.EventHandler(this.SaveAndCloseButton_Click);
			// 
			// HouseBillCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.HouseBillCountCalcEdit, "HouseBillCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).HouseBillCount)));
			this.HouseBillCountCalcEdit.CaptionResourceString = Res.GetData("PreAllocationForm|65cdcc4c-07fe-46a3-a506-95f62220b4bb", "Count");
			this.HouseBillCountCalcEdit.Decimals = 0;
			this.HouseBillCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 18, true);
			this.HouseBillCountCalcEdit.Name = "HouseBillCountCalcEdit";
			this.HouseBillCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.HouseBillCountCalcEdit.TabIndex = 1;
			this.HouseBillCountCalcEdit.Text = "0";
			this.HouseBillCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TheCancelButton
			// 
			this.TheCancelButton.CaptionResourceString = Res.GetData("PreAllocationForm|3caa8396-d582-46a3-a25b-af730191d0ef", "Cancel");
			this.TheCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.TheCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(543, 409, true);
			this.TheCancelButton.Name = "TheCancelButton";
			this.TheCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 26, true);
			this.TheCancelButton.TabIndex = 6;
			this.TheCancelButton.UseVisualStyleBackColor = true;
			this.TheCancelButton.Click += new System.EventHandler(this.TheCancelButton_Click);
			// 
			// JS_HouseBillOfLadingTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JS_HouseBillOfLadingTypeDropEdit, "QuotedBooking.Booking+JS_HouseBillOfLadingType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).QuotedBooking.Booking.JS_HouseBillOfLadingType)));
			this.JS_HouseBillOfLadingTypeDropEdit.CaptionResourceString = Res.GetData("PreAllocationForm|772d5e92-1914-49b2-9eb6-b3705259dd08", "HBL Type");
			this.JS_HouseBillOfLadingTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 167, true);
			this.JS_HouseBillOfLadingTypeDropEdit.Name = "JS_HouseBillOfLadingTypeDropEdit";
			this.JS_HouseBillOfLadingTypeDropEdit.PreBoundMaxLength = 3;
			this.JS_HouseBillOfLadingTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.JS_HouseBillOfLadingTypeDropEdit.TabIndex = 12;
			// 
			// zDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit1, "QuotedBooking.TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).QuotedBooking.TransportMode)));
			this.zDropEdit1.CaptionResourceString = Res.GetData("PreAllocationForm|f3d46410-0809-4403-87ca-562762ac355b", "Transport Mode");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 17, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.zDropEdit1.TabIndex = 1;
			// 
			// zDropEdit2
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit2, "QuotedBooking.ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).QuotedBooking.ContainerMode)));
			this.zDropEdit2.CaptionResourceString = Res.GetData("PreAllocationForm|c27d10f0-1ed1-48d4-bf39-938450534877", "Container Mode");
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 39, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.PreBoundMaxLength = 3;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			this.zDropEdit2.TabIndex = 2;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Res.GetData("PreAllocationForm|0a74ee0b-151b-482e-a9af-53853050d0d2", "Pre-Allocation Details");
			this.zGroupBox1.Controls.Add(this.IsDomesticCheckBox);
			this.zGroupBox1.Controls.Add(this.zDropEdit1);
			this.zGroupBox1.Controls.Add(this.zDropEdit2);
			this.zGroupBox1.Controls.Add(this.ServiceLevelFindBox);
			this.zGroupBox1.Controls.Add(this.JS_HouseBillOfLadingTypeDropEdit);
			this.zGroupBox1.Controls.Add(this.OriginCodeFindBox);
			this.zGroupBox1.Controls.Add(this.JS_GoodsDescriptionBoundTextBox);
			this.zGroupBox1.Controls.Add(this.DestinationCodeFindBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 7, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 198, true);
			this.zGroupBox1.TabIndex = 3;
			this.zGroupBox1.TabStop = false;
			// 
			// IsDomesticCheckBox
			// 
			this.IsDomesticCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsDomesticCheckBox, "QuotedBooking+IsDomesticFreight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).QuotedBooking.IsDomesticFreight)));
			this.IsDomesticCheckBox.CaptionResourceString = Res.GetData("PreAllocationForm|625428d5-56ef-46d2-a096-00db4c0e1808", "Is Domestic");
			this.IsDomesticCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsDomesticCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 39, true);
			this.IsDomesticCheckBox.Name = "IsDomesticCheckBox";
			this.IsDomesticCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 17, true);
			this.IsDomesticCheckBox.TabIndex = 3;
			this.IsDomesticCheckBox.UseVisualStyleBackColor = true;
			// 
			// PrintButton
			// 
			this.PrintButton.CaptionResourceString = Res.GetData("PreAllocationForm|ab5fa2d8-6eea-48f0-8965-5f9f38db902b", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 14, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 26, true);
			this.PrintButton.TabIndex = 1;
			this.PrintButton.UseVisualStyleBackColor = true;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.CaptionResourceString = Res.GetData("PreAllocationForm|b27bf88a-377a-4c9f-b6ad-001f8d533745", "Printing");
			this.zGroupBox3.Controls.Add(this.zCheckBox1);
			this.zGroupBox3.Controls.Add(this.PrintButton);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 319, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 48, true);
			this.zGroupBox3.TabIndex = 5;
			this.zGroupBox3.TabStop = false;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "IsPrePrinted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.QuotedBookings.Business.PreAllocation)(null)).IsPrePrinted)));
			this.zCheckBox1.CaptionResourceString = Res.GetData("PreAllocationForm|adb76366-c06a-48fd-997e-c951fc009b0c", "Print on Pre-Printed");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 20, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.zCheckBox1.TabIndex = 0;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// SaveButton
			// 
			this.SaveButton.CaptionResourceString = Res.GetData("PreAllocationForm|3d774382-8382-4baf-8e21-17f5d1e23250", "Save");
			this.SaveButton.Enabled = false;
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 409, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 26, true);
			this.SaveButton.TabIndex = 16;
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// WarningLabel
			// 
			this.WarningLabel.ForeColor = System.Drawing.Color.Red;
			this.WarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 369, true);
			this.WarningLabel.Name = "WarningLabel";
			this.WarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 38, true);
			this.WarningLabel.TabIndex = 17;
			this.WarningLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.WarningLabel.Visible = false;
			// 
			// PreAllocationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.TheCancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 485, true);
			this.ControlBox = false;
			this.CaptionResourceString = Res.GetData("PreAllocationForm|db67ba4b-e29d-4dea-85b7-96cec7f268d2", "Pre-Allocation");
			this.Controls.Add(this.WarningLabel);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.zGroupBox3);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.TheCancelButton);
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.ClientOrgControl);
			this.Controls.Add(this.PickupDocAddressControl);
			this.Controls.Add(this.DeliveryDocAddressControl);
			this.DataSourceAssemblyName = "Enterprise.Freight.QuotedBookings.Business";
			this.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.PreAllocation);
			this.DataSourceTypeName = "Enterprise.Freight.QuotedBookings.Business.PreAllocation";
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 501, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 501, true);
			this.Name = "PreAllocationForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.DeliveryDocAddressControl, 0);
			this.Controls.SetChildIndex(this.PickupDocAddressControl, 0);
			this.Controls.SetChildIndex(this.ClientOrgControl, 0);
			this.Controls.SetChildIndex(this.zGroupBox2, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TheCancelButton, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.zGroupBox3, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.WarningLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.Freight.GUI.GoodsDescriptionTextBox JS_GoodsDescriptionBoundTextBox;
		private Enterprise.MasterFiles.GUI.ZOrganisationControl ClientOrgControl;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl DeliveryDocAddressControl;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox OriginCodeFindBox;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl PickupDocAddressControl;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox DestinationCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ServiceLevelFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.ZArchitecture.ZCalcEdit HouseBillCountCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox zTextBox2;
		private Enterprise.ZArchitecture.GUI.ZButton CreateButton;
		private Enterprise.ZArchitecture.GUI.ZButton TheCancelButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JS_HouseBillOfLadingTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit2;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox3;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		private Enterprise.ZArchitecture.GUI.ZButton RefreshButton;
		private Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		private Enterprise.ZArchitecture.ZLabel WarningLabel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsDomesticCheckBox;

	}
}
