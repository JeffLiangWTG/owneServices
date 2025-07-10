namespace Enterprise.Customs.US.GUI
{
	partial class DDTCProductUserControl
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
			this.CD_DDTCLicenseTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CD_DDTCLicenseNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CD_DDTCRegistrationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CD_DDTCExemptionCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CD_DDTCLicenseTypeDropEdit.SuspendLayout();
			this.CD_DDTCExemptionCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.InvoiceLineViewCollection);
			// 
			// CD_DDTCLicenseTypeDropEdit
			// 
			this.CD_DDTCLicenseTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CD_DDTCLicenseTypeDropEdit, "Details.CD_DDTCLicenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).Details.CD_DDTCLicenceType)));
			this.CD_DDTCLicenseTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("86658b23-1bc8-428a-94e0-d311583299f6", "License Type");
			this.CD_DDTCLicenseTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 29, true);
			this.CD_DDTCLicenseTypeDropEdit.BindToList = "USClassificationLookups.DDTCLicenseTypeCodes";
			this.CD_DDTCLicenseTypeDropEdit.Name = "CD_DDTCLicenseTypeDropEdit";
			this.CD_DDTCLicenseTypeDropEdit.PreBoundMaxLength = 3;
			this.CD_DDTCLicenseTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.CD_DDTCLicenseTypeDropEdit.TabIndex = 1;
			// 
			// CD_DDTCLicenseNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.CD_DDTCLicenseNoTextBox, "Details.CD_DDTCLicenceNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).Details.CD_DDTCLicenceNo)));
			this.CD_DDTCLicenseNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7c8838cc-b064-45af-b286-b3c9f08cbd64", "License Number");
			this.CD_DDTCLicenseNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 55, true);
			this.CD_DDTCLicenseNoTextBox.Name = "CD_DDTCLicenseNoTextBox";
			this.CD_DDTCLicenseNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.CD_DDTCLicenseNoTextBox.TabIndex = 2;
			// 
			// CD_DDTCRegistrationNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.CD_DDTCRegistrationNoTextBox, "CD_DDTCRegoNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_DDTCRegoNo)));
			this.CD_DDTCRegistrationNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("02aabdda-451e-429f-ab22-0e51b6fde0da", "Registration Number");
			this.CD_DDTCRegistrationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 81, true);
			this.CD_DDTCRegistrationNoTextBox.Name = "CD_DDTCRegistrationNoTextBox";
			this.CD_DDTCRegistrationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CD_DDTCRegistrationNoTextBox.TabIndex = 3;
			// 
			// CD_DDTCExemptionCodeDropEdit
			// 
			this.CD_DDTCExemptionCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CD_DDTCExemptionCodeDropEdit, "CD_ITARExemptionNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_ITARExemptionNo)));
			this.CD_DDTCExemptionCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("449214da-718f-4ae7-8a6b-197bb9711411", "Exemption Code");
			this.CD_DDTCExemptionCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 3, true);
			this.CD_DDTCExemptionCodeDropEdit.Name = "CD_DDTCExemptionCodeDropEdit";
			this.CD_DDTCExemptionCodeDropEdit.PreBoundMaxLength = 8;
			this.CD_DDTCExemptionCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.CD_DDTCExemptionCodeDropEdit.TabIndex = 0;
			// 
			// DDTCProductUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CD_DDTCExemptionCodeDropEdit);
			this.Controls.Add(this.CD_DDTCRegistrationNoTextBox);
			this.Controls.Add(this.CD_DDTCLicenseNoTextBox);
			this.Controls.Add(this.CD_DDTCLicenseTypeDropEdit);
			this.Name = "DDTCProductUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CD_DDTCLicenseTypeDropEdit.ResumeLayout(true);
			this.CD_DDTCLicenseTypeDropEdit.PerformLayout();
			this.CD_DDTCExemptionCodeDropEdit.ResumeLayout(true);
			this.CD_DDTCExemptionCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit CD_DDTCLicenseTypeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox CD_DDTCLicenseNoTextBox;
		private Enterprise.ZArchitecture.ZTextBox CD_DDTCRegistrationNoTextBox;
		private ZArchitecture.GUI.ZDropEdit CD_DDTCExemptionCodeDropEdit;
		

	}
}
