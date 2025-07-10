namespace Enterprise.Customs.GUI
{
	partial class TransportInlandAirUserControl
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
			this.TransportNationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AircraftIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportNationalityCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// TransportNationalityCodeFindBox
			// 
			this.TransportNationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportNationalityCodeFindBox, "JE_RN_NKTransportNationalityInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RN_NKTransportNationalityInland)));
			this.TransportNationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
			this.TransportNationalityCodeFindBox.Name = "TransportNationalityCodeFindBox";
			this.TransportNationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransportNationalityCodeFindBox.ParentType = null;
			this.TransportNationalityCodeFindBox.PreBoundMaxLength = 2;
			this.TransportNationalityCodeFindBox.ShowDescriptionBox = false;
			this.TransportNationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TransportNationalityCodeFindBox.TabIndex = 1;
			// 
			// FlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightTextBox, "JE_TransportIDInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TransportIDInland)));
			this.FlightTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("325176f4-37aa-41de-aaa2-e11113d76afd", "Flight", "Flight Num.", "Flight Number", "");
			this.FlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FlightTextBox.Name = "FlightTextBox";
			this.FlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.FlightTextBox.TabIndex = 0;
			// 
			// AircraftIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.AircraftIDTextBox, "JE_AircraftRegistrationInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_AircraftRegistrationInland)));
			this.AircraftIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 22, true);
			this.AircraftIDTextBox.Name = "AircraftIDTextBox";
			this.AircraftIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.AircraftIDTextBox.TabIndex = 2;
			// 
			// TransportInlandAirUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AircraftIDTextBox);
			this.Controls.Add(this.FlightTextBox);
			this.Controls.Add(this.TransportNationalityCodeFindBox);
			this.Name = "TransportInlandAirUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 44, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportNationalityCodeFindBox.ResumeLayout(true);
			this.TransportNationalityCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox TransportNationalityCodeFindBox;
		internal ZArchitecture.ZTextBox FlightTextBox;
		internal ZArchitecture.ZTextBox AircraftIDTextBox;
	}
}
