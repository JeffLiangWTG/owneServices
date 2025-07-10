namespace Enterprise.Customs.US.GUI
{
	partial class ExportAMSEPAUserControl
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
			this.EPANetQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.HazWasteTrackingNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EPAConsentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AMSGroupBox.SuspendLayout();
			this.EPAGroupBox.SuspendLayout();
			this.EPANetQtyCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobComInvoiceLine);
			// 
			// AMSGroupBox
			// 
			this.AMSGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6701b2ee-3a07-49fd-85a0-79e59b48fa48", "AMS - Agricultural Marketing Service");
			this.AMSGroupBox.Controls.Add(this.ExportCertificateNoTextBox);
			this.AMSGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AMSGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 111, true);
			this.AMSGroupBox.Name = "AMSGroupBox";
			this.AMSGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 70, true);
			this.AMSGroupBox.TabIndex = 3;
			this.AMSGroupBox.TabStop = false;
			this.AMSGroupBox.Text = "AMS - Agricultural Marketing Service";
			// 
			// ExportCertificateNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportCertificateNoTextBox, "US_ExportCertificateNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_ExportCertificateNo)));
			this.ExportCertificateNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("27217420-c629-4dbc-bbdf-b4f7b2729c59", "Export Certificate Number");
			this.ExportCertificateNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 19, true);
			this.ExportCertificateNoTextBox.Name = "ExportCertificateNoTextBox";
			this.ExportCertificateNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.ExportCertificateNoTextBox.TabIndex = 0;
			// 
			// EPAGroupBox
			// 
			this.EPAGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("89a24277-0daa-45e1-9a67-75a54603609c", "EPA - Environmental Protection Agency");
			this.EPAGroupBox.Controls.Add(this.EPANetQtyCalcDropEdit);
			this.EPAGroupBox.Controls.Add(this.HazWasteTrackingNoTextBox);
			this.EPAGroupBox.Controls.Add(this.EPAConsentNumberTextBox);
			this.EPAGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.EPAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EPAGroupBox.Name = "EPAGroupBox";
			this.EPAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 111, true);
			this.EPAGroupBox.TabIndex = 2;
			this.EPAGroupBox.TabStop = false;
			this.EPAGroupBox.Text = "EPA - Environmental Protection Agency";
			// 
			// EPANetQtyCalcDropEdit
			// 
			this.EPANetQtyCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EPANetQtyCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_EPANetQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_EPANetQtyUQ)));
			this.EPANetQtyCalcDropEdit.BindToAmount = "US_EPANetQty";
			this.EPANetQtyCalcDropEdit.BindToUnit = "US_EPANetQtyUQ";
			this.EPANetQtyCalcDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("78e65546-83f0-4fb2-9913-6161cf8c0d87", "EPA Net Quantity");
			this.EPANetQtyCalcDropEdit.Decimals = 0;
			this.EPANetQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 71, true);
			this.EPANetQtyCalcDropEdit.Name = "EPANetQtyCalcDropEdit";
			this.EPANetQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.EPANetQtyCalcDropEdit.TabIndex = 3;
			this.EPANetQtyCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// HazWasteTrackingNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.HazWasteTrackingNoTextBox, "US_HazWasteTrackingNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_HazWasteTrackingNo)));
			this.HazWasteTrackingNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a34671dc-eddc-4064-97d2-2bad873f0d87", "Hazardous Waste Manifest Tracking No.");
			this.HazWasteTrackingNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 47, true);
			this.HazWasteTrackingNoTextBox.Name = "HazWasteTrackingNoTextBox";
			this.HazWasteTrackingNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.HazWasteTrackingNoTextBox.TabIndex = 1;
			// 
			// EPAConsentNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EPAConsentNumberTextBox, "US_EPAConsentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_EPAConsentNumber)));
			this.EPAConsentNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4b0ce0ba-5b86-426a-9b01-24a16fb98b02", "EPA Consent Number");
			this.EPAConsentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 23, true);
			this.EPAConsentNumberTextBox.Name = "EPAConsentNumberTextBox";
			this.EPAConsentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.EPAConsentNumberTextBox.TabIndex = 0;
			// 
			// ExportAMSEPAUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AMSGroupBox);
			this.Controls.Add(this.EPAGroupBox);
			this.Name = "ExportAMSEPAUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 181, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AMSGroupBox.ResumeLayout(false);
			this.AMSGroupBox.PerformLayout();
			this.EPAGroupBox.ResumeLayout(false);
			this.EPAGroupBox.PerformLayout();
			this.EPANetQtyCalcDropEdit.ResumeLayout(true);
			this.EPANetQtyCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AMSGroupBox;
		private ZArchitecture.ZTextBox ExportCertificateNoTextBox;
		private ZArchitecture.GUI.ZGroupBox EPAGroupBox;
		private ZArchitecture.GUI.ZCalcDropEdit EPANetQtyCalcDropEdit;
		private ZArchitecture.ZTextBox HazWasteTrackingNoTextBox;
		private ZArchitecture.ZTextBox EPAConsentNumberTextBox;
	}
}
