using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class CombineTranslatedAddressForm
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
		new protected void InitializeComponent()
		{
			this.SaveAsButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CombineTranslatedAddressAUserControl = new Enterprise.MasterFiles.GUI.CombineTranslatedAddressUserControl();
			this.CombineTranslatedAddressBUserControl = new Enterprise.MasterFiles.GUI.CombineTranslatedAddressUserControl();
			this.SaveAsButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CombineTranslatedAddressAUserControl.SuspendLayout();
			this.CombineTranslatedAddressBUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 253, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AddressCombiner);
			// 
			// SaveAsButton1
			// 
			this.SaveAsButton1.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.SaveAsButton1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9DD7F951-CF5B-4EE6-9BD2-84D532ADEA70", "Save {0} as Translated Address");
			this.SaveAsButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 217, true);
			this.SaveAsButton1.Name = "SaveAsButton1";
			this.SaveAsButton1.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveAsButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 26, true);
			this.SaveAsButton1.TabIndex = 2;
			this.SaveAsButton1.ToolTipCaption = null;
			this.SaveAsButton1.UseVisualStyleBackColor = true;
			this.SaveAsButton1.Click += new System.EventHandler(this.SaveAsButton1_Click);
			// 
			// CombineTranslatedAddressAUserControl
			// 
			this.CombineTranslatedAddressAUserControl.AllowDrop = true;
			this.CombineTranslatedAddressAUserControl.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.BindingSource.SetBindingMember(this.CombineTranslatedAddressAUserControl, "Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgAddress)(((Enterprise.MasterFiles.Business.AddressCombiner)(null)).Address1)));
			this.CombineTranslatedAddressAUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.CombineTranslatedAddressAUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, true);
			this.CombineTranslatedAddressAUserControl.Name = "CombineTranslatedAddressAUserControl";
			this.CombineTranslatedAddressAUserControl.ReadOnly = false;
			this.CombineTranslatedAddressAUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 200, true);
			this.CombineTranslatedAddressAUserControl.TabIndex = 0;
			this.CombineTranslatedAddressAUserControl.ValidateAddressButtonVisibility = false;
			this.CombineTranslatedAddressAUserControl.ValidationJustForced = false;
			// 
			// CombineTranslatedAddressBUserControl
			// 
			this.CombineTranslatedAddressBUserControl.AllowDrop = true;
			this.CombineTranslatedAddressBUserControl.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.BindingSource.SetBindingMember(this.CombineTranslatedAddressBUserControl, "Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgAddress)(((Enterprise.MasterFiles.Business.AddressCombiner)(null)).Address2)));
			this.CombineTranslatedAddressBUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 19, true);
			this.CombineTranslatedAddressBUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, true);
			this.CombineTranslatedAddressBUserControl.Name = "CombineTranslatedAddressBUserControl";
			this.CombineTranslatedAddressBUserControl.ReadOnly = false;
			this.CombineTranslatedAddressBUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 200, true);
			this.CombineTranslatedAddressBUserControl.TabIndex = 1;
			this.CombineTranslatedAddressBUserControl.ValidateAddressButtonVisibility = false;
			this.CombineTranslatedAddressBUserControl.ValidationJustForced = false;
			// 
			// SaveAsButton2
			// 
			this.SaveAsButton2.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.SaveAsButton2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4a678e59-dfe4-4919-ae25-aed1b7fc3fec", "Save {0} as Translated Address");
			this.SaveAsButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(637, 217, true);
			this.SaveAsButton2.Name = "SaveAsButton2";
			this.SaveAsButton2.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveAsButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 26, true);
			this.SaveAsButton2.TabIndex = 3;
			this.SaveAsButton2.ToolTipCaption = null;
			this.SaveAsButton2.UseVisualStyleBackColor = true;
			this.SaveAsButton2.Click += new System.EventHandler(this.SaveAsButton2_Click);
			// 
			// CombineTranslatedAddressForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("899F10AE-0084-4833-A91B-C26535EEA0BE", "Combine address as a translated address");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 277, true);
			this.Controls.Add(this.SaveAsButton2);
			this.Controls.Add(this.SaveAsButton1);
			this.Controls.Add(this.CombineTranslatedAddressAUserControl);
			this.Controls.Add(this.CombineTranslatedAddressBUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AddressCombiner);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "CombineTranslatedAddressForm";
			this.Text = "Combine address as a translated address";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CombineTranslatedAddressBUserControl, 0);
			this.Controls.SetChildIndex(this.CombineTranslatedAddressAUserControl, 0);
			this.Controls.SetChildIndex(this.SaveAsButton1, 0);
			this.Controls.SetChildIndex(this.SaveAsButton2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CombineTranslatedAddressAUserControl.ResumeLayout(true);
			this.CombineTranslatedAddressAUserControl.PerformLayout();
			this.CombineTranslatedAddressBUserControl.ResumeLayout(true);
			this.CombineTranslatedAddressBUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		protected ZArchitecture.GUI.ZButton SaveAsButton1;
		protected ZArchitecture.GUI.ZButton SaveAsButton2;
		protected CombineTranslatedAddressUserControl CombineTranslatedAddressAUserControl;
		protected CombineTranslatedAddressUserControl CombineTranslatedAddressBUserControl;
	}
}