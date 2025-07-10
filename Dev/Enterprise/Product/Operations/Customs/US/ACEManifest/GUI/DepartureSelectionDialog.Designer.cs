namespace Enterprise.Customs.US.ACEManifest.GUI
{
	partial class DepartureSelectionDialog
	{
		new void InitializeComponent()
		{
			this.DatePicker = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TimeZoneLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FlightDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FlightDetailsGrid)).BeginInit();
			this.FlightDetailsGrid.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.DatePicker.SuspendLayout();
			this.SuspendLayout();
			// 
			// DatePicker
			// 
			this.DatePicker.AllowDrop = true;
			this.DatePicker.AutoCompleteMonthThreshold = 1;
			this.DatePicker.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DatePicker, "AM_FlightDepartureTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.ACEManifest.Business.AdditionalMessageInformation)(null)).AM_FlightDepartureTime)));
			this.DatePicker.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("8c21680f-6043-4f36-8097-2be497c046fb", "Lift Off Time", "The Lift Off Time in the Time Zone of the load port");
			this.DatePicker.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DatePicker.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 15, true);
			this.DatePicker.Name = "DatePicker";
			this.DatePicker.TabIndex = 1;
			// 
			// TimeZoneLabel
			// 
			this.TimeZoneLabel.AutoSize = true;
			this.TimeZoneLabel.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("e2c34c06-b80a-47ae-be20-e378055ae0cf", "in the Time Zone of the load port");
			this.TimeZoneLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TimeZoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 18, true);
			this.TimeZoneLabel.Name = "TimeZoneLabel";
			this.TimeZoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 13, true);
			this.TimeZoneLabel.TabIndex = 4;
			// 
			// AdditionalInformationPanel
			// 
			this.AdditionalInformationPanel.Controls.Add(this.TimeZoneLabel);
			this.AdditionalInformationPanel.Controls.Add(this.DatePicker);
			// 
			// DepartureSelectionDialog
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 221, true);
			this.Name = "DepartureSelectionDialog";
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FlightDetailsGroupBox.ResumeLayout(false);
			this.FlightDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FlightDetailsGrid)).EndInit();
			this.FlightDetailsGrid.ResumeLayout(false);
			this.FlightDetailsGrid.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.DatePicker.ResumeLayout(true);
			this.DatePicker.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZArchitecture.GUI.ZDateEdit DatePicker;
		private ZArchitecture.ZLabel TimeZoneLabel;
	}
}
