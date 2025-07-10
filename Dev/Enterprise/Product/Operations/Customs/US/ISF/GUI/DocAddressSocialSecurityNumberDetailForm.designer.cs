namespace Enterprise.Customs.US.ISF.GUI
{
	partial class DocAddressSocialSecurityNumberDetailForm
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
			this.SocialSecurityNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SocialSecurityNumberDateOfBirthDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ISF.Business.ISFDocAddress);
			// 
			// SocialSecurityNumberTextBox
			// 
			this.SocialSecurityNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SocialSecurityNumberTextBox, "SocialSecurityNumberForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ISF.Business.ISFDocAddress)(null)).SocialSecurityNumberForDisplay)));
			this.SocialSecurityNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 12, true);
			this.SocialSecurityNumberTextBox.Name = "SocialSecurityNumberTextBox";
			this.SocialSecurityNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.SocialSecurityNumberTextBox.TabIndex = 0;
			// 
			// SocialSecurityNumberDateOfBirthDateEdit
			// 
			this.SocialSecurityNumberDateOfBirthDateEdit.AllowDrop = true;
			this.SocialSecurityNumberDateOfBirthDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SocialSecurityNumberDateOfBirthDateEdit.AutoCompleteMonthThreshold = 1;
			this.SocialSecurityNumberDateOfBirthDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SocialSecurityNumberDateOfBirthDateEdit, "E2_SocialSecurityNumberDateOfBirth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.ISF.Business.ISFDocAddress)(null)).E2_SocialSecurityNumberDateOfBirth)));
			this.SocialSecurityNumberDateOfBirthDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 38, true);
			this.SocialSecurityNumberDateOfBirthDateEdit.Name = "SocialSecurityNumberDateOfBirthDateEdit";
			this.SocialSecurityNumberDateOfBirthDateEdit.TabIndex = 2;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("DocAddressSocialSecurityNumberDetailForm|ab6c27cc-e16d-4d25-9cc5-773523f97565", "&Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 38, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// DocAddressSocialSecurityNumberDetailForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 93, true);
			this.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("DocAddressSocialSecurityNumberDetailForm|e1dd4705-b372-44c0-869c-3f298fbc62ef", "Social Security Number Details");
			this.Controls.Add(this.SocialSecurityNumberTextBox);
			this.Controls.Add(this.SocialSecurityNumberDateOfBirthDateEdit);
			this.Controls.Add(this.CloseButton);
			this.DataSourceType = typeof(Enterprise.Customs.US.ISF.Business.ISFDocAddress);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DocAddressSocialSecurityNumberDetailForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.SocialSecurityNumberDateOfBirthDateEdit, 0);
			this.Controls.SetChildIndex(this.SocialSecurityNumberTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox SocialSecurityNumberTextBox;
		private ZArchitecture.GUI.ZDateEdit SocialSecurityNumberDateOfBirthDateEdit;
		private ZArchitecture.GUI.ZButton CloseButton;
	}
}
