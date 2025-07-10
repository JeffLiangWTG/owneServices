namespace Enterprise.MasterFiles.GUI
{
	partial class RefMessagingBussCarrierInfoForm
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
		private new void InitializeComponent()
		{
			this.ZMC_CarrierCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZMC_CarrierNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZMC_CountryCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZMP_PackageNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 180, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.ZMP_PackageNameTextBox);
			this.MainTabPage.Controls.Add(this.ZMC_CountryCodeTextBox);
			this.MainTabPage.Controls.Add(this.ZMC_CarrierNameTextBox);
			this.MainTabPage.Controls.Add(this.ZMC_CarrierCodeTextBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 153, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 153, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 180, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 6, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefMessagingBussCarrierInfo);
			// 
			// ZMC_CarrierCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZMC_CarrierCodeTextBox, "ZMC_CarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefMessagingBussCarrierInfo)(null)).ZMC_CarrierCode)));
			this.ZMC_CarrierCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 26, true);
			this.ZMC_CarrierCodeTextBox.Name = "ZMC_CarrierCodeTextBox";
			this.ZMC_CarrierCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 20, true);
			this.ZMC_CarrierCodeTextBox.TabIndex = 2;
			// 
			// ZMC_CarrierNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZMC_CarrierNameTextBox, "ZMC_CarrierName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefMessagingBussCarrierInfo)(null)).ZMC_CarrierName)));
			this.ZMC_CarrierNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 52, true);
			this.ZMC_CarrierNameTextBox.Name = "ZMC_CarrierNameTextBox";
			this.ZMC_CarrierNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 20, true);
			this.ZMC_CarrierNameTextBox.TabIndex = 3;
			// 
			// ZMC_CountryCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZMC_CountryCodeTextBox, "ZMC_CountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefMessagingBussCarrierInfo)(null)).ZMC_CountryCode)));
			this.ZMC_CountryCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 78, true);
			this.ZMC_CountryCodeTextBox.Name = "ZMC_CountryCodeTextBox";
			this.ZMC_CountryCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 20, true);
			this.ZMC_CountryCodeTextBox.TabIndex = 4;
			// 
			// ZMP_PackageNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZMP_PackageNameTextBox, "Package.ZMP_PackageName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefMessagingBussCarrierInfo)(null)).Package.ZMP_PackageName)));
			this.ZMP_PackageNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 104, true);
			this.ZMP_PackageNameTextBox.Name = "ZMP_PackageNameTextBox";
			this.ZMP_PackageNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 20, true);
			this.ZMP_PackageNameTextBox.TabIndex = 5;
			// 
			// RefMessagingBussCarrierInfoForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 236, true);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefMessagingBussCarrierInfo);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.RefMessagingBussCarrierInfo";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "RefMessagingBussCarrierInfo";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox ZMC_CarrierCodeTextBox;
		private ZArchitecture.ZTextBox ZMP_PackageNameTextBox;
		private ZArchitecture.ZTextBox ZMC_CountryCodeTextBox;
		private ZArchitecture.ZTextBox ZMC_CarrierNameTextBox;
	}
}
