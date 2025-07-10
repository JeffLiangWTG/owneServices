namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class BookedMoveDeliveryControl
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
			this.DeliveryBookingDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel5 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DropModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RequestedPickupPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ReqPickupStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReqPickupEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RequestedDeliveryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ReqDeliveryStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReqDeliveryEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel14 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel15 = new Enterprise.ZArchitecture.ZLabel();
			this.AddressSelectionDropDown = new Enterprise.ZArchitecture.GUI.ZDropButtonOnly();
			this.AddressGroupBoxCaption = new Enterprise.ZArchitecture.ZLabel();
			this.BookingAddressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeliveryBookingDetailsPanel.SuspendLayout();
			this.zPanel5.SuspendLayout();
			this.RequestedPickupPanel.SuspendLayout();
			this.RequestedDeliveryPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddressSelectionDropDown)).BeginInit();
			this.BookingAddressGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove);
			// 
			// DeliveryBookingDetailsPanel
			// 
			this.DeliveryBookingDetailsPanel.Controls.Add(this.zPanel5);
			this.DeliveryBookingDetailsPanel.Controls.Add(this.RequestedPickupPanel);
			this.DeliveryBookingDetailsPanel.Controls.Add(this.RequestedDeliveryPanel);
			this.DeliveryBookingDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 19, true);
			this.DeliveryBookingDetailsPanel.Name = "DeliveryBookingDetailsPanel";
			this.DeliveryBookingDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 105, true);
			this.DeliveryBookingDetailsPanel.TabIndex = 41;
			// 
			// zPanel5
			// 
			this.zPanel5.Controls.Add(this.DropModeDropEdit);
			this.zPanel5.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 84, true);
			this.zPanel5.Name = "zPanel5";
			this.zPanel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 21, true);
			this.zPanel5.TabIndex = 2;
			// 
			// DropModeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DropModeDropEdit, "EW_DropMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_DropMode)));
			this.DropModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 1, true);
			this.DropModeDropEdit.Name = "DropModeDropEdit";
			this.DropModeDropEdit.ShowDescriptionBox = false;
			this.DropModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.DropModeDropEdit.TabIndex = 0;
			// 
			// RequestedPickupPanel
			// 
			this.RequestedPickupPanel.Controls.Add(this.ReqPickupStartDateEdit);
			this.RequestedPickupPanel.Controls.Add(this.ReqPickupEndDateEdit);
			this.RequestedPickupPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.RequestedPickupPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 42, true);
			this.RequestedPickupPanel.Name = "RequestedPickupPanel";
			this.RequestedPickupPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 42, true);
			this.RequestedPickupPanel.TabIndex = 1;
			// 
			// ReqPickupStartDateEdit
			// 
			this.ReqPickupStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReqPickupStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReqPickupStartDateEdit, "EW_RequestedPickupTimeStart");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_RequestedPickupTimeStart)));
			this.ReqPickupStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReqPickupStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 1, true);
			this.ReqPickupStartDateEdit.Name = "ReqPickupStartDateEdit";
			this.ReqPickupStartDateEdit.TabIndex = 0;
			// 
			// ReqPickupEndDateEdit
			// 
			this.ReqPickupEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReqPickupEndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReqPickupEndDateEdit, "EW_RequestedPickupTimeEnd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_RequestedPickupTimeEnd)));
			this.ReqPickupEndDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReqPickupEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 22, true);
			this.ReqPickupEndDateEdit.Name = "ReqPickupEndDateEdit";
			this.ReqPickupEndDateEdit.TabIndex = 1;
			// 
			// RequestedDeliveryPanel
			// 
			this.RequestedDeliveryPanel.Controls.Add(this.ReqDeliveryStartDateEdit);
			this.RequestedDeliveryPanel.Controls.Add(this.ReqDeliveryEndDateEdit);
			this.RequestedDeliveryPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.RequestedDeliveryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RequestedDeliveryPanel.Name = "RequestedDeliveryPanel";
			this.RequestedDeliveryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 42, true);
			this.RequestedDeliveryPanel.TabIndex = 0;
			// 
			// ReqDeliveryStartDateEdit
			// 
			this.ReqDeliveryStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReqDeliveryStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReqDeliveryStartDateEdit, "EW_RequestedDeliveryTimeStart");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_RequestedDeliveryTimeStart)));
			this.ReqDeliveryStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReqDeliveryStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 1, true);
			this.ReqDeliveryStartDateEdit.Name = "ReqDeliveryStartDateEdit";
			this.ReqDeliveryStartDateEdit.TabIndex = 0;
			// 
			// ReqDeliveryEndDateEdit
			// 
			this.ReqDeliveryEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReqDeliveryEndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReqDeliveryEndDateEdit, "EW_RequestedDeliveryTimeEnd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_RequestedDeliveryTimeEnd)));
			this.ReqDeliveryEndDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReqDeliveryEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 22, true);
			this.ReqDeliveryEndDateEdit.Name = "ReqDeliveryEndDateEdit";
			this.ReqDeliveryEndDateEdit.TabIndex = 1;
			// 
			// zLabel14
			// 
			this.BindingSource.SetBindingMember(this.zLabel14, "WaitPointDocAddress+E2_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).WaitPointDocAddress.E2_CompanyName)));
			this.zLabel14.IsFontBold = true;
			this.zLabel14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.zLabel14.Name = "zLabel14";
			this.zLabel14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 13, true);
			this.zLabel14.TabIndex = 1;
			// 
			// zLabel15
			// 
			this.BindingSource.SetBindingMember(this.zLabel15, "WaitPointDocAddress+AddressSummaryWithCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).WaitPointDocAddress.AddressSummaryWithCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel15, false);
			this.zLabel15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 34, true);
			this.zLabel15.Name = "zLabel15";
			this.zLabel15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 87, true);
			this.zLabel15.TabIndex = 2;
			this.zLabel15.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// AddressSelectionDropDown
			// 
			this.AddressSelectionDropDown.AutoSize = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddressSelectionDropDown, false);
			this.AddressSelectionDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 1, true);
			this.AddressSelectionDropDown.Name = "AddressSelectionDropDown";
			this.AddressSelectionDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 18, true);
			this.AddressSelectionDropDown.TabIndex = 0;
			// 
			// AddressGroupBoxCaption
			// 
			this.AddressGroupBoxCaption.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AddressGroupBoxCaption, "WaitPointDocAddress+AddressCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).WaitPointDocAddress.AddressCaption)));
			this.AddressGroupBoxCaption.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddressGroupBoxCaption, false);
			this.AddressGroupBoxCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 1, true);
			this.AddressGroupBoxCaption.Name = "AddressGroupBoxCaption";
			this.AddressGroupBoxCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.AddressGroupBoxCaption.TabIndex = 0;
			// 
			// BookingAddressGroupBox
			// 
			this.BookingAddressGroupBox.Controls.Add(this.DeliveryBookingDetailsPanel);
			this.BookingAddressGroupBox.Controls.Add(this.AddressGroupBoxCaption);
			this.BookingAddressGroupBox.Controls.Add(this.zLabel14);
			this.BookingAddressGroupBox.Controls.Add(this.zLabel15);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BookingAddressGroupBox, false);
			this.BookingAddressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.BookingAddressGroupBox.Name = "BookingAddressGroupBox";
			this.BookingAddressGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, 3, 3, 20, true);
			this.BookingAddressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 128, true);
			this.BookingAddressGroupBox.TabIndex = 1;
			this.BookingAddressGroupBox.TabStop = false;
			// 
			// BookedMoveDeliveryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddressSelectionDropDown);
			this.Controls.Add(this.BookingAddressGroupBox);
			this.Name = "BookedMoveDeliveryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 131, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeliveryBookingDetailsPanel.ResumeLayout(false);
			this.zPanel5.ResumeLayout(false);
			this.RequestedPickupPanel.ResumeLayout(false);
			this.RequestedDeliveryPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AddressSelectionDropDown)).EndInit();
			this.BookingAddressGroupBox.ResumeLayout(false);
			this.BookingAddressGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel DeliveryBookingDetailsPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel5;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DropModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel RequestedDeliveryPanel;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ReqDeliveryStartDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ReqDeliveryEndDateEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel RequestedPickupPanel;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ReqPickupStartDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ReqPickupEndDateEdit;
		private Enterprise.ZArchitecture.ZLabel zLabel14;
		private Enterprise.ZArchitecture.ZLabel zLabel15;
		internal Enterprise.ZArchitecture.GUI.ZDropButtonOnly AddressSelectionDropDown;
		private Enterprise.ZArchitecture.ZLabel AddressGroupBoxCaption;
		private Enterprise.ZArchitecture.GUI.ZGroupBox BookingAddressGroupBox;


	}
}
