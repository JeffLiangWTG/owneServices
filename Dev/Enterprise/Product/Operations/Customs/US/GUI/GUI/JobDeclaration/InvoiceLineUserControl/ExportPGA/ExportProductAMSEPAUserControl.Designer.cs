namespace Enterprise.Customs.US.GUI
{
	partial class ExportProductAMSEPAUserControl
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
			this.AMSGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExportCertificateNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EPAGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HazWasteTrackingNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EPAConsentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AMSGroupBox.SuspendLayout();
			this.EPAGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusClassPartPivot);
			// 
			// AMSGroupBox
			// 
			this.AMSGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d6d9295e-b9b2-4412-9267-87291754b638", "AMS - Agricultural Marketing Service");
			this.AMSGroupBox.Controls.Add(this.ExportCertificateNoTextBox);
			this.AMSGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AMSGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 77, true);
			this.AMSGroupBox.Name = "AMSGroupBox";
			this.AMSGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 104, true);
			this.AMSGroupBox.TabIndex = 5;
			this.AMSGroupBox.TabStop = false;
			this.AMSGroupBox.Text = "AMS - Agricultural Marketing Service";
			// 
			// ExportCertificateNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportCertificateNoTextBox, "CD_ExportCertificateNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_ExportCertificateNo)));
			this.ExportCertificateNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("01361f7c-cea3-4d16-897c-b88ee0edcf2c", "Export Certificate Number");
			this.ExportCertificateNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 19, true);
			this.ExportCertificateNoTextBox.Name = "ExportCertificateNoTextBox";
			this.ExportCertificateNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.ExportCertificateNoTextBox.TabIndex = 0;
			// 
			// EPAGroupBox
			// 
			this.EPAGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("54252e00-80a8-4afe-87fa-e87ad4736a5e", "EPA - Environmental Protection Agency");
			this.EPAGroupBox.Controls.Add(this.HazWasteTrackingNoTextBox);
			this.EPAGroupBox.Controls.Add(this.EPAConsentNumberTextBox);
			this.EPAGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.EPAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EPAGroupBox.Name = "EPAGroupBox";
			this.EPAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 77, true);
			this.EPAGroupBox.TabIndex = 4;
			this.EPAGroupBox.TabStop = false;
			this.EPAGroupBox.Text = "EPA - Environmental Protection Agency";
			// 
			// HazWasteTrackingNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.HazWasteTrackingNoTextBox, "CD_HazWasteTrackingNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_HazWasteTrackingNo)));
			this.HazWasteTrackingNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4876464d-511d-4ae3-82f2-05fcdbb5b5a0", "Hazardous Waste Manifest Tracking No.");
			this.HazWasteTrackingNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 47, true);
			this.HazWasteTrackingNoTextBox.Name = "HazWasteTrackingNoTextBox";
			this.HazWasteTrackingNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.HazWasteTrackingNoTextBox.TabIndex = 1;
			// 
			// EPAConsentNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EPAConsentNumberTextBox, "CD_EPAConsentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_EPAConsentNumber)));
			this.EPAConsentNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2453b588-9cea-4cf6-9d6f-80b93da33ef3", "EPA Consent Number");
			this.EPAConsentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 23, true);
			this.EPAConsentNumberTextBox.Name = "EPAConsentNumberTextBox";
			this.EPAConsentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.EPAConsentNumberTextBox.TabIndex = 0;
			// 
			// ExportProductAMSEPAUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AMSGroupBox);
			this.Controls.Add(this.EPAGroupBox);
			this.Name = "ExportProductAMSEPAUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 181, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AMSGroupBox.ResumeLayout(false);
			this.AMSGroupBox.PerformLayout();
			this.EPAGroupBox.ResumeLayout(false);
			this.EPAGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AMSGroupBox;
		private ZArchitecture.ZTextBox ExportCertificateNoTextBox;
		private ZArchitecture.GUI.ZGroupBox EPAGroupBox;
		private ZArchitecture.ZTextBox HazWasteTrackingNoTextBox;
		private ZArchitecture.ZTextBox EPAConsentNumberTextBox;
	}
}
