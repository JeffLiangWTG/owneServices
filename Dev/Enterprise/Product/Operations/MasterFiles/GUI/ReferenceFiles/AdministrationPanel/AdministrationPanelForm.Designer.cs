using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class AdministrationPanelForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.components = new Container();
			this.MainTabControl = new ZTemplateTabControl();
			this.DashboardTabPage = new ZTabPage();
			this.AddressesTabPage = new ZTabPage();
			this.DuplicatesTabPage = new ZTabPage();
			this.StandardsTabPage = new ZTabPage();
			this.ButtonsUserControl = new ZPostingButtonsUserControl();
			this.BackgroundValidationMenuItem = new ZMenuItem();
			this.CopyAddressInformationMenuItem = new ZMenuItem();
			this.SearchAddressOnlineMenuItem = new ZMenuItem();
			this.CopyCompanyInformationMenuItem = new ZMenuItem();
			this.SearchCompanyOnlineMenuItem = new ZMenuItem();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.AddressesTabPage.SuspendLayout();
			this.ButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = ControlDpiScalingHelper.NewScaledPoint(0, 788, true);
			this.MainStatusBar.Size = ControlDpiScalingHelper.NewScaledSize(994, 24, true);
			// 
			// EditMenuItem
			// 
			this.EditMenuItem.Visible = false;
			// 
			// ActionsMenuItem
			// 
			this.ActionsMenuItem.Visible = true;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AdministrationPanelManager);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.DashboardTabPage);
			this.MainTabControl.Controls.Add(this.AddressesTabPage);
			this.MainTabControl.Controls.Add(this.DuplicatesTabPage);
			this.MainTabControl.Controls.Add(this.StandardsTabPage);
			this.MainTabControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = ControlDpiScalingHelper.NewScaledSize(994, 758, true);
			this.MainTabControl.TabIndex = 1;
			this.MainTabControl.TabOrderExtendedToTabPages = true;
			// 
			// DashboardTabPage
			// 
			this.DashboardTabPage.BackColor = SystemColors.Control;
			this.DashboardTabPage.CaptionResourceString = Res.GetData("2c16ca83-c607-4b4f-8d5d-927791639ff5", "Dashboard");
			this.DashboardTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DashboardTabPage.Name = "DashboardTabPage";
			this.DashboardTabPage.TabVisible = false;
			this.DashboardTabPage.Padding = ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DashboardTabPage.Size = ControlDpiScalingHelper.NewScaledSize(986, 711, true);
			this.DashboardTabPage.TabIndex = 0;
			// 
			// AddressesTabPage
			// 
			this.AddressesTabPage.CaptionResourceString = Res.GetData("2502424a-49a4-40b5-a63f-37e4bab061ee", "Addresses");
			this.AddressesTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AddressesTabPage.Name = "AddressesTabPage";
			this.AddressesTabPage.Padding = ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AddressesTabPage.TabIndex = 1;
			this.AddressesTabPage.TabVisible = false;
			this.AddressesTabPage.UseVisualStyleBackColor = true;
			// 
			// DuplicatesTabPage
			// 
			this.DuplicatesTabPage.CaptionResourceString = Res.GetData("bc5ae94d-006c-4a89-8d2d-195d5e210544", "Duplicates");
			this.DuplicatesTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DuplicatesTabPage.Name = "DuplicatesTabPage";
			this.DuplicatesTabPage.Padding = ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DuplicatesTabPage.Size = ControlDpiScalingHelper.NewScaledSize(986, 711, true);
			this.DuplicatesTabPage.TabIndex = 2;
			this.DuplicatesTabPage.TabVisible = false;
			this.DuplicatesTabPage.UseVisualStyleBackColor = true;
			// 
			// StandardsTabPage
			// 
			this.StandardsTabPage.CaptionResourceString = Res.GetData("066e6475-969d-4a08-8716-4611766b9b7e", "Standards");
			this.StandardsTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StandardsTabPage.Name = "StandardsTabPage";
			this.StandardsTabPage.Padding = ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.StandardsTabPage.Size = ControlDpiScalingHelper.NewScaledSize(986, 731, true);
			this.StandardsTabPage.TabIndex = 4;
			this.StandardsTabPage.TabVisible = false;
			this.StandardsTabPage.UseVisualStyleBackColor = true;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			this.ButtonsUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(749, 759, true);
			this.ButtonsUserControl.MinimumSize = ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.TabIndex = 2;
			// 
			// AdministrationPanelForm
			// 
			this.AutoScaleMode = AutoScaleMode.None;
			this.MinimumSize = ControlDpiScalingHelper.NewScaledSize(1366, 725, true);
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Res.GetData("MDMAdminPanelForm|3be5f776-d8d8-32f6-b1f1-65b31d01091e", "MDM Admin Panel");
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(994, 812, true);
			this.Controls.Add(this.ButtonsUserControl);
			this.Controls.Add(this.MainTabControl);
			this.DataSourceType = typeof(AdministrationPanelManager);
			this.DisplayMode = ODisplayMode.Browse;
			this.Name = "AdministrationPanelForm";
			this.ShouldSerializeTabPageMethods = false;
			this.StartPosition = FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.AddressesTabPage.ResumeLayout(false);
			this.AddressesTabPage.PerformLayout();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZPostingButtonsUserControl ButtonsUserControl;
		internal ZMenuItem BackgroundValidationMenuItem;
		internal ZMenuItem CopyAddressInformationMenuItem;
		internal ZMenuItem SearchAddressOnlineMenuItem;
		internal ZMenuItem CopyCompanyInformationMenuItem;
		internal ZMenuItem SearchCompanyOnlineMenuItem;
		private ZTemplateTabControl MainTabControl;
		internal ZTabPage AddressesTabPage;
		internal ZTabPage DuplicatesTabPage;
		private ZTabPage DashboardTabPage;
		private ZTabPage StandardsTabPage;
	}
}
