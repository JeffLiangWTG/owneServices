namespace Enterprise.MasterFiles.GUI
{
	partial class DocAddressPassportDetailForm
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
			this.PassportIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryOfIssueCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DateOfBirthDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 90, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.JobDocAddress);
			// 
			// PassportIDTextBox
			// 
			this.PassportIDTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PassportIDTextBox, "E2_PassportID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_PassportID)));
			this.PassportIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 12, true);
			this.PassportIDTextBox.Name = "PassportIDTextBox";
			this.PassportIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.PassportIDTextBox.TabIndex = 0;
			// 
			// CountryOfIssueCodeFindBox
			// 
			this.CountryOfIssueCodeFindBox.AllowDrop = true;
			this.CountryOfIssueCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CountryOfIssueCodeFindBox, "E2_PassportCountryOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_PassportCountryOfIssue)));
			this.CountryOfIssueCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 38, true);
			this.CountryOfIssueCodeFindBox.Name = "CountryOfIssueCodeFindBox";
			this.CountryOfIssueCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfIssueCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.CountryOfIssueCodeFindBox.TabIndex = 1;
			// 
			// DateOfBirthDateEdit
			// 
			this.DateOfBirthDateEdit.AllowDrop = true;
			this.DateOfBirthDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DateOfBirthDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfBirthDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateOfBirthDateEdit, "E2_PassportDateOfBirth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_PassportDateOfBirth)));
			this.DateOfBirthDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 64, true);
			this.DateOfBirthDateEdit.Name = "DateOfBirthDateEdit";
			this.DateOfBirthDateEdit.TabIndex = 2;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressPassportDetailForm|34d32c15-69b1-4076-96e7-be11c1f7d6be", "&Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 64, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// DocAddressPassportDetailForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 114, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressPassportDetailForm|30a32d31-62a8-4155-95bb-d94cc204ffd3", "Passport Details");
			this.Controls.Add(this.PassportIDTextBox);
			this.Controls.Add(this.CountryOfIssueCodeFindBox);
			this.Controls.Add(this.DateOfBirthDateEdit);
			this.Controls.Add(this.CloseButton);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.JobDocAddress);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DocAddressPassportDetailForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.DateOfBirthDateEdit, 0);
			this.Controls.SetChildIndex(this.CountryOfIssueCodeFindBox, 0);
			this.Controls.SetChildIndex(this.PassportIDTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox PassportIDTextBox;
		private ZArchitecture.GUI.ZCodeFindBox CountryOfIssueCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit DateOfBirthDateEdit;
		private ZArchitecture.GUI.ZButton CloseButton;
	}
}
