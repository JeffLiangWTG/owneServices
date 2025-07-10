using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.NO.Manifest.GUI
{
	partial class EmailAddressesUserControl
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
		void InitializeComponent()
		{
            this.EmailAddressesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.Email1TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.Email2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.Email3TextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.EmailAddressesGroupBox.SuspendLayout();
            this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Manifest.Business.AsycudaBill);
			// 
			// EmailAddressesGroupBox
			// 
			this.EmailAddressesGroupBox.CaptionResourceString = Enterprise.Customs.NO.Manifest.GUI.Res.GetData("B8705ADE-FFB7-4DE5-8C97-1E1F6B7B3633", "Email addresses for border passing confirmation:");
            this.EmailAddressesGroupBox.Controls.Add(this.Email1TextBox);
            this.EmailAddressesGroupBox.Controls.Add(this.Email2TextBox);
            this.EmailAddressesGroupBox.Controls.Add(this.Email3TextBox);
            this.EmailAddressesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 6, true);
            this.EmailAddressesGroupBox.Name = "EmailAddressesGroupBox";
            this.EmailAddressesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 101, true);
            this.EmailAddressesGroupBox.TabIndex = 1;
            this.EmailAddressesGroupBox.TabStop = false;
            // 
            // Email1TextBox
            // 
            this.BindingSource.SetBindingMember(this.Email1TextBox, "EmailAddress1");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).EmailAddress1)));
            this.Email1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.Email1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 24, true);
            this.Email1TextBox.Name = "Email1TextBox";
            this.Email1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 15, true);
            this.Email1TextBox.TabIndex = 1;
            // 
            // Email2TextBox
            // 
            this.BindingSource.SetBindingMember(this.Email2TextBox, "EmailAddress2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).EmailAddress2)));
            this.Email2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.Email2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 48, true);
            this.Email2TextBox.Name = "Email2TextBox";
            this.Email2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 15, true);
            this.Email2TextBox.TabIndex = 1;
            // 
            // Email3TextBox
            // 
            this.BindingSource.SetBindingMember(this.Email3TextBox, "EmailAddress3");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).EmailAddress3)));
            this.Email3TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.Email3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 72, true);
            this.Email3TextBox.Name = "Email3TextBox";
            this.Email3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 15, true);
            this.Email3TextBox.TabIndex = 1;
            // 
            // EmailAddressesUserControl
            //
            this.CaptionRenderingEnabled = true;
            this.AutoSize = true;
            this.Controls.Add(this.EmailAddressesGroupBox);
            this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 10, true);
            this.Name = "EmailAddressesUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 120, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.EmailAddressesGroupBox.ResumeLayout(false);
            this.EmailAddressesGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox EmailAddressesGroupBox;
		internal ZTextBox Email1TextBox;
		internal ZTextBox Email2TextBox;
		internal ZTextBox Email3TextBox;
	}
}
