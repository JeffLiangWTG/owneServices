namespace Enterprise.Customs.US.GUI
{
	partial class DDTCUserControl
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
			this.US_DDTCLicenseTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_DDTCLicenseNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_DDTCRegistrationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_DDTCExemptionCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_DDTCArrivalDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.US_DDTCTrackingStatusDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.US_DDTCLicenseTypeDropEdit.SuspendLayout();
			this.US_DDTCExemptionCodeDropEdit.SuspendLayout();
			this.US_DDTCArrivalDateDateEdit.SuspendLayout();
			this.zDateEdit1.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobComInvoiceLine);
			// 
			// US_DDTCLicenseTypeDropEdit
			// 
			this.US_DDTCLicenseTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_DDTCLicenseTypeDropEdit, "US_DDTCLicenseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_DDTCLicenseType)));
			this.US_DDTCLicenseTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("86658b23-1bc8-428a-94e0-d311583299f6", "License Type");
			this.US_DDTCLicenseTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 55, true);
			this.US_DDTCLicenseTypeDropEdit.Name = "US_DDTCLicenseTypeDropEdit";
			this.US_DDTCLicenseTypeDropEdit.PreBoundMaxLength = 3;
			this.US_DDTCLicenseTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.US_DDTCLicenseTypeDropEdit.TabIndex = 2;
			// 
			// US_DDTCLicenseNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_DDTCLicenseNoTextBox, "US_DDTCLicenseNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_DDTCLicenseNo)));
			this.US_DDTCLicenseNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7c8838cc-b064-45af-b286-b3c9f08cbd64", "License Number");
			this.US_DDTCLicenseNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 81, true);
			this.US_DDTCLicenseNoTextBox.Name = "US_DDTCLicenseNoTextBox";
			this.US_DDTCLicenseNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.US_DDTCLicenseNoTextBox.TabIndex = 3;
			// 
			// US_DDTCRegistrationNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_DDTCRegistrationNoTextBox, "US_DDTCRegistrationNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_DDTCRegistrationNo)));
			this.US_DDTCRegistrationNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("02aabdda-451e-429f-ab22-0e51b6fde0da", "Registration Number");
			this.US_DDTCRegistrationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 107, true);
			this.US_DDTCRegistrationNoTextBox.Name = "US_DDTCRegistrationNoTextBox";
			this.US_DDTCRegistrationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.US_DDTCRegistrationNoTextBox.TabIndex = 4;
			// 
			// US_DDTCExemptionCodeDropEdit
			// 
			this.US_DDTCExemptionCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_DDTCExemptionCodeDropEdit, "US_DDTCExemptionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_DDTCExemptionCode)));
			this.US_DDTCExemptionCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("449214da-718f-4ae7-8a6b-197bb9711411", "Exemption Code");
			this.US_DDTCExemptionCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 29, true);
			this.US_DDTCExemptionCodeDropEdit.Name = "US_DDTCExemptionCodeDropEdit";
			this.US_DDTCExemptionCodeDropEdit.PreBoundMaxLength = 8;
			this.US_DDTCExemptionCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.US_DDTCExemptionCodeDropEdit.TabIndex = 1;
			// 
			// US_DDTCArrivalDateDateEdit
			// 
			this.US_DDTCArrivalDateDateEdit.AllowDrop = true;
			this.US_DDTCArrivalDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.US_DDTCArrivalDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.US_DDTCArrivalDateDateEdit, "US_DDTCArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_DDTCArrivalDate)));
			this.US_DDTCArrivalDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6af964c9-13c4-4c81-86a2-03eea2364fb7", "Anticipated Arrival Date");
			this.US_DDTCArrivalDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.US_DDTCArrivalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 133, true);
			this.US_DDTCArrivalDateDateEdit.Name = "US_DDTCArrivalDateDateEdit";
			this.US_DDTCArrivalDateDateEdit.TabIndex = 5;
			// 
			// US_DDTCTrackingStatusDescTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_DDTCTrackingStatusDescTextBox, "US_DDTCTrackingStatusDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_DDTCTrackingStatusDesc)));
			this.US_DDTCTrackingStatusDescTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4ae79877-3ff2-474f-887f-301f5b73384e", "Message Status");
			this.US_DDTCTrackingStatusDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 3, true);
			this.US_DDTCTrackingStatusDescTextBox.Name = "US_DDTCTrackingStatusDescTextBox";
			this.US_DDTCTrackingStatusDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.US_DDTCTrackingStatusDescTextBox.TabIndex = 0;
			// 
			// UpdateButton
			// 
			this.UpdateButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d2ddc718-b533-49a4-8aff-4cffde7b0791", "&Update");
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 216, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpdateButton.TabIndex = 8;
			this.UpdateButton.UseVisualStyleBackColor = true;
			this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// DeleteButton
			// 
			this.DeleteButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("73742548-4407-4934-b393-b48f7f5dad69", "&Delete");
			this.DeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 216, true);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DeleteButton.TabIndex = 9;
			this.DeleteButton.UseVisualStyleBackColor = true;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "DDTCStatusDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).DDTCStatusDate)));
			this.zDateEdit1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("08ccb0c8-d143-47cc-b4bd-713624486f86", "PGA Line Status Date");
			this.zDateEdit1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 185, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 7;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "DDTCStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).DDTCStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).DDTCStatusDesc)));
			this.zDropEdit1.BindToForDescription = "DDTCStatusDesc";
			this.zDropEdit1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9aec901e-4039-467c-aa6b-16bbea8941cf", "PGA Line Status");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 159, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.zDropEdit1.TabIndex = 6;
			// 
			// DDTCUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zDropEdit1);
			this.Controls.Add(this.zDateEdit1);
			this.Controls.Add(this.DeleteButton);
			this.Controls.Add(this.UpdateButton);
			this.Controls.Add(this.US_DDTCTrackingStatusDescTextBox);
			this.Controls.Add(this.US_DDTCArrivalDateDateEdit);
			this.Controls.Add(this.US_DDTCExemptionCodeDropEdit);
			this.Controls.Add(this.US_DDTCRegistrationNoTextBox);
			this.Controls.Add(this.US_DDTCLicenseNoTextBox);
			this.Controls.Add(this.US_DDTCLicenseTypeDropEdit);
			this.Name = "DDTCUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.US_DDTCLicenseTypeDropEdit.ResumeLayout(true);
			this.US_DDTCLicenseTypeDropEdit.PerformLayout();
			this.US_DDTCExemptionCodeDropEdit.ResumeLayout(true);
			this.US_DDTCExemptionCodeDropEdit.PerformLayout();
			this.US_DDTCArrivalDateDateEdit.ResumeLayout(true);
			this.US_DDTCArrivalDateDateEdit.PerformLayout();
			this.zDateEdit1.ResumeLayout(true);
			this.zDateEdit1.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit US_DDTCLicenseTypeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox US_DDTCLicenseNoTextBox;
		private Enterprise.ZArchitecture.ZTextBox US_DDTCRegistrationNoTextBox;
		private ZArchitecture.GUI.ZDropEdit US_DDTCExemptionCodeDropEdit;
		private ZArchitecture.GUI.ZDateEdit US_DDTCArrivalDateDateEdit;
		private ZArchitecture.ZTextBox US_DDTCTrackingStatusDescTextBox;
		internal ZArchitecture.GUI.ZButton UpdateButton;
		internal ZArchitecture.GUI.ZButton DeleteButton;
		private ZArchitecture.GUI.ZDateEdit zDateEdit1;
		private ZArchitecture.GUI.ZDropEdit zDropEdit1;
	}
}
