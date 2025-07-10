using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.CFS.GUI
{
	public partial class CFSContainerForm : ZTemplateForm
	{
		#region Windows Form Designer generated code

		public ZButton CreateNewContainerRegistrationButton;

		new void InitializeComponent()
		{
			this.CreateNewContainerRegistrationButton = new ZButton();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom)
						| AnchorStyles.Left)));
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.CreateNewContainerRegistrationButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = ControlDpiScalingHelper.NewScaledPoint(396, 4, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = ControlDpiScalingHelper.NewScaledSize(360, 27, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = true;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = ControlDpiScalingHelper.NewScaledSize(1014, 619, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = ControlDpiScalingHelper.NewScaledSize(1006, 592, true);
			this.MainTabPage.AutoScroll = true;
			this.MainTabPage.AutoScrollMinSize = ControlDpiScalingHelper.NewScaledSize(1006, 586, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = ControlDpiScalingHelper.NewScaledSize(1014, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(489);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(490);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CFSContainer);
			// 
			// CreateNewContainerRegistrationButton
			// 
			this.CreateNewContainerRegistrationButton.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			this.CreateNewContainerRegistrationButton.CaptionResourceString = Res.GetData("CFSContainerForm|aad2ffe9-1929-4225-8fc7-1f836185b166", "<Create Storage From CFS>");
			this.CreateNewContainerRegistrationButton.Location = ControlDpiScalingHelper.NewScaledPoint(114, 3, true);
			this.CreateNewContainerRegistrationButton.Name = "CreateNewContainerRegistrationButton";
			this.CreateNewContainerRegistrationButton.Size = ControlDpiScalingHelper.NewScaledSize(158, 23, true);
			this.CreateNewContainerRegistrationButton.TabIndex = 0;
			this.CreateNewContainerRegistrationButton.UseVisualStyleBackColor = true;
			this.CreateNewContainerRegistrationButton.Click += new EventHandler(this.CreateNewContainerRegistrationButton_Click);
			// 
			// CFSContainerForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(1014, 675, true);
			this.DataSourceAssemblyName = "Enterprise.Freight.CFS.Business";
			this.DataSourceType = typeof(CFSContainer);
			this.DataSourceTypeName = "Enterprise.Freight.CFS.Business.CFSContainer";
			this.MinimumSize = ControlDpiScalingHelper.NewScaledSize(1024, 714, true);
			this.Name = "CFSContainerForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "CFSContainerForm";
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.ResumeLayout(false);
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
