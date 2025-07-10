using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Confirmations.GUI
{
	partial class ConfirmationDetailsControl
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
			this.PickupDeliveryConfirmPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostcodeDistanceUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CalculateDistanceButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DistanceUDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DistanceTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BookingGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TransportCoAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ChangeConfirmAddressCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConfirmAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.TransportCoNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PickupDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DropModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsSignedByTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TruckRegistrationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DriversLicenseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DriversNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PickupDeliveryConfirmPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.CommonPickupDeliveryConfirm);
			// 
			// PickupDeliveryConfirmPanel
			// 
			this.PickupDeliveryConfirmPanel.Controls.Add(this.PostcodeDistanceUnitLabel);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.zTextBox2);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.CalculateDistanceButton);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.DistanceUDropEdit);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.DistanceTextBox);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.BookingGuidFindBox);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.TransportCoAddressControl);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.zTextBox1);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.zDateEdit2);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.zDateEdit1);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.ChangeConfirmAddressCheckBox);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.ConfirmAddressControl);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.TransportCoNameTextBox);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.PickupDateEdit);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.DropModeDropEdit);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.GoodsSignedByTextBox);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.TruckRegistrationTextBox);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.DriversLicenseTextBox);
			this.PickupDeliveryConfirmPanel.Controls.Add(this.DriversNameTextBox);
			this.PickupDeliveryConfirmPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PickupDeliveryConfirmPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.PickupDeliveryConfirmPanel.Name = "PickupDeliveryConfirmPanel";
			this.PickupDeliveryConfirmPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 182, true);
			this.PickupDeliveryConfirmPanel.TabIndex = 0;
			// 
			// PostcodeDistanceUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.PostcodeDistanceUnitLabel, "EU_DistanceUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_DistanceUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PostcodeDistanceUnitLabel, false);
			this.PostcodeDistanceUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 94, true);
			this.PostcodeDistanceUnitLabel.Name = "PostcodeDistanceUnitLabel";
			this.PostcodeDistanceUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.PostcodeDistanceUnitLabel.TabIndex = 13;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "PostcodeDistance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).PostcodeDistance)));
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 94, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zTextBox2.TabIndex = 12;
			// 
			// CalculateDistanceButton
			// 
			this.CalculateDistanceButton.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("ConfirmationDetailsControl|40ef4477-5724-4690-a25f-2221e8f34a63", "Calculate Distance", "Calculate Driving Distance.");
			this.CalculateDistanceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 116, true);
			this.CalculateDistanceButton.Name = "CalculateDistanceButton";
			this.CalculateDistanceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.CalculateDistanceButton.TabIndex = 16;
			this.CalculateDistanceButton.UseVisualStyleBackColor = true;
			this.CalculateDistanceButton.Click += new System.EventHandler(this.CalculateDistanceButton_Click);
			// 
			// DistanceUDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DistanceUDropEdit, "EU_DistanceUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_DistanceUnit)));
			this.DistanceUDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 116, true);
			this.DistanceUDropEdit.Name = "DistanceUDropEdit";
			this.DistanceUDropEdit.PreBoundMaxLength = 3;
			this.DistanceUDropEdit.ShowDescriptionBox = false;
			this.DistanceUDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.DistanceUDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.DistanceUDropEdit.TabIndex = 15;
			// 
			// DistanceTextBox
			// 
			this.BindingSource.SetBindingMember(this.DistanceTextBox, "EU_Distance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_Distance)));
			this.DistanceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 116, true);
			this.DistanceTextBox.Name = "DistanceTextBox";
			this.DistanceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.DistanceTextBox.TabIndex = 14;
			// 
			// BookingGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.BookingGuidFindBox, "EU_D1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_D1)));
			this.BookingGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 116, true);
			this.BookingGuidFindBox.Name = "BookingGuidFindBox";
			this.BookingGuidFindBox.ShowDescriptionBox = false;
			this.BookingGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.BookingGuidFindBox.TabIndex = 5;
			// 
			// TransportCoAddressControl
			// 
			this.BindingSource.SetBindingMember(this.TransportCoAddressControl, "EU_OA_TransportProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_OA_TransportProvider)));
			this.TransportCoAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 6, true);
			this.TransportCoAddressControl.Name = "TransportCoAddressControl";
			this.TransportCoAddressControl.PopupCaption = "";
			this.TransportCoAddressControl.ShowAddress = false;
			this.TransportCoAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 21, true);
			this.TransportCoAddressControl.TabIndex = 7;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "EU_PickupDeliveryInstruction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_PickupDeliveryInstruction)));
			this.LabelCaptionRenderProvider.SetLabelTop(this.zTextBox1, 0);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 138, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 37, true);
			this.zTextBox1.TabIndex = 6;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "EU_RequestedPickupDeliveryTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_RequestedPickupDeliveryTime)));
			this.zDateEdit2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 28, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 1;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "EU_PlannedPickupDeliveryTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_PlannedPickupDeliveryTime)));
			this.zDateEdit1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 6, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 0;
			// 
			// ChangeConfirmAddressCheckBox
			// 
			this.ChangeConfirmAddressCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ChangeConfirmAddressCheckBox, "ConfirmAddressOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).ConfirmAddressOverride)));
			this.ChangeConfirmAddressCheckBox.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("ConfirmationDetailsControl|23c4c37e-d486-40c5-8d02-cac0dbb11f64", "Change");
			this.ChangeConfirmAddressCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ChangeConfirmAddressCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 0, true);
			this.ChangeConfirmAddressCheckBox.Name = "ChangeConfirmAddressCheckBox";
			this.ChangeConfirmAddressCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 17, true);
			this.ChangeConfirmAddressCheckBox.TabIndex = 18;
			this.ChangeConfirmAddressCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConfirmAddressControl
			// 
			this.BindingSource.SetBindingMember(this.ConfirmAddressControl, "ConfirmAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).ConfirmAddress)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConfirmAddressControl, false);
			this.ConfirmAddressControl.BindToOrganisations = "BindToLists+Organisations";
			this.ConfirmAddressControl.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("ConfirmationDetailsControl|030bd527-c1da-457e-92cb-69570707e054", "Address", "Address", "Override Address.");
			this.ConfirmAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(687, 0, true);
			this.ConfirmAddressControl.Name = "ConfirmAddressControl";
			this.ConfirmAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConfirmAddressControl.TabIndex = 17;
			// 
			// TransportCoNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportCoNameTextBox, "EU_TransportCoName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_TransportCoName)));
			this.TransportCoNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 28, true);
			this.TransportCoNameTextBox.Name = "TransportCoNameTextBox";
			this.TransportCoNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.TransportCoNameTextBox.TabIndex = 8;
			// 
			// PickupDateEdit
			// 
			this.PickupDateEdit.AutoCompleteMonthThreshold = 1;
			this.PickupDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PickupDateEdit, "EU_PickupDeliveryTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_PickupDeliveryTime)));
			this.PickupDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PickupDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 50, true);
			this.PickupDateEdit.Name = "PickupDateEdit";
			this.PickupDateEdit.TabIndex = 2;
			// 
			// DropModeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DropModeDropEdit, "EU_DropMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_DropMode)));
			this.DropModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 94, true);
			this.DropModeDropEdit.Name = "DropModeDropEdit";
			this.DropModeDropEdit.PreBoundMaxLength = 3;
			this.DropModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.DropModeDropEdit.TabIndex = 4;
			// 
			// GoodsSignedByTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsSignedByTextBox, "EU_GoodsSignForBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_GoodsSignForBy)));
			this.GoodsSignedByTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 72, true);
			this.GoodsSignedByTextBox.Name = "GoodsSignedByTextBox";
			this.GoodsSignedByTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.GoodsSignedByTextBox.TabIndex = 3;
			// 
			// TruckRegistrationTextBox
			// 
			this.BindingSource.SetBindingMember(this.TruckRegistrationTextBox, "EU_VehicleRegistration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_VehicleRegistration)));
			this.TruckRegistrationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 72, true);
			this.TruckRegistrationTextBox.Name = "TruckRegistrationTextBox";
			this.TruckRegistrationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TruckRegistrationTextBox.TabIndex = 11;
			// 
			// DriversLicenseTextBox
			// 
			this.BindingSource.SetBindingMember(this.DriversLicenseTextBox, "EU_DriversLicence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_DriversLicence)));
			this.DriversLicenseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 50, true);
			this.DriversLicenseTextBox.Name = "DriversLicenseTextBox";
			this.DriversLicenseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.DriversLicenseTextBox.TabIndex = 10;
			// 
			// DriversNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.DriversNameTextBox, "EU_DriversName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonPickupDeliveryConfirm)(null)).EU_DriversName)));
			this.DriversNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 50, true);
			this.DriversNameTextBox.Name = "DriversNameTextBox";
			this.DriversNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.DriversNameTextBox.TabIndex = 9;
			// 
			// ConfirmationDetailsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PickupDeliveryConfirmPanel);
			this.Name = "ConfirmationDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 185, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PickupDeliveryConfirmPanel.ResumeLayout(false);
			this.PickupDeliveryConfirmPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel PickupDeliveryConfirmPanel;
		private Enterprise.ZArchitecture.ZTextBox TransportCoNameTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit PickupDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DropModeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox GoodsSignedByTextBox;
		private Enterprise.ZArchitecture.ZTextBox TruckRegistrationTextBox;
		private Enterprise.ZArchitecture.ZTextBox DriversLicenseTextBox;
		private Enterprise.ZArchitecture.ZTextBox DriversNameTextBox;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl ConfirmAddressControl;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ChangeConfirmAddressCheckBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit2;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit1;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.ZArchitecture.GUI.ZAddressControl TransportCoAddressControl;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox BookingGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZButton CalculateDistanceButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DistanceUDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit DistanceTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit zTextBox2;
		private ZLabel PostcodeDistanceUnitLabel;
	}
}
