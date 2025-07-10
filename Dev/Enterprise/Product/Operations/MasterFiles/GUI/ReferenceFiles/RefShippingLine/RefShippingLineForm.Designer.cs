using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	partial class RefShippingLineForm
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
			this.components = new System.ComponentModel.Container();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 291, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 269, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 269, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 269, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 291, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefShippingLine);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_CarrierName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_StandardCarrierAlphaCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_CargoWiseOneCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_IsSystem)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_IsNVO)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).RSL_IsShippingLine)));
			// 
			// RefShippingLineForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefShippingLineForm|02A148FF-007C-4C99-AB15-3392EAABA76D", "Shipping Line");
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 347, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefShippingLineForm|7AAD7A41-E71F-406F-B838-0FC661DE9FA5", "Shipping Line");
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefShippingLine);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 447, true);
			this.Name = "RefShippingLineForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.DetailTemplateTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailsTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.DetailsHeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RSL_OceanCarrierNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RSL_StandardCarrierAlphaCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RSL_CargoWiseOneCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RSL_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RSL_IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RSL_IsNVOCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RSL_IsShippingLineCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AvailableIntegrationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagingRequirementsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EBLProviderTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabPage.SuspendLayout();
			this.DetailTemplateTabControl.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.DetailsTableLayoutPanel.SuspendLayout();
			this.DetailsHeaderPanel.SuspendLayout();
			this.AvailableIntegrationsTabPage.SuspendLayout();
			this.MessagingRequirementsTabPage.SuspendLayout();
			this.EBLProviderTabPage.SuspendLayout();
			this.MainTabPage.Controls.Add(this.DetailsGroupBox);
			// 
			// DetailTemplateTabControl
			// 
			this.DetailTemplateTabControl.Controls.Add(this.AvailableIntegrationsTabPage);
			this.DetailTemplateTabControl.Controls.Add(this.MessagingRequirementsTabPage);
			this.DetailTemplateTabControl.Controls.Add(this.EBLProviderTabPage);
			this.DetailTemplateTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 87, true);
			this.DetailTemplateTabControl.Name = "DetailTemplateTabControl";
			this.DetailTemplateTabControl.SelectedIndex = 0;
			this.DetailTemplateTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 163, true);
			this.DetailTemplateTabControl.TabIndex = 0;
			this.DetailTemplateTabControl.TabStop = false;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c724654e-657d-44f4-a9e5-94ae8c3aacf5", "Details");
			this.DetailsGroupBox.Controls.Add(this.DetailsTableLayoutPanel);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 269, true);
			this.DetailsGroupBox.TabIndex = 4;
			this.DetailsGroupBox.TabStop = false;
			// 
			// DetailsTableLayoutPanel
			// 
			this.DetailsTableLayoutPanel.AutoScroll = true;
			this.DetailsTableLayoutPanel.ColumnCount = 1;
			this.DetailsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.DetailsTableLayoutPanel.Controls.Add(this.DetailTemplateTabControl, 0, 1);
			this.DetailsTableLayoutPanel.Controls.Add(this.DetailsHeaderPanel, 0, 0);
			this.DetailsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.DetailsTableLayoutPanel.Name = "DetailsTableLayoutPanel";
			this.DetailsTableLayoutPanel.RowCount = 2;
			this.DetailsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(85)));
			this.DetailsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.DetailsTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 252, true);
			this.DetailsTableLayoutPanel.TabIndex = 0;
			// 
			// DetailsHeaderPanel
			// 
			this.DetailsHeaderPanel.AutoScroll = true;
			this.DetailsHeaderPanel.Controls.Add(this.RSL_OceanCarrierNameTextBox);
			this.DetailsHeaderPanel.Controls.Add(this.RSL_StandardCarrierAlphaCodeTextBox);
			this.DetailsHeaderPanel.Controls.Add(this.RSL_CargoWiseOneCodeTextBox);
			this.DetailsHeaderPanel.Controls.Add(this.RSL_IsActiveCheckBox);
			this.DetailsHeaderPanel.Controls.Add(this.RSL_IsSystemCheckBox);
			this.DetailsHeaderPanel.Controls.Add(this.RSL_IsNVOCheckBox);
			this.DetailsHeaderPanel.Controls.Add(this.RSL_IsShippingLineCheckBox);
			this.DetailsHeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsHeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.DetailsHeaderPanel.Name = "DetailsHeaderPanel";
			this.DetailsHeaderPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.DetailsHeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 81, true);
			this.DetailsHeaderPanel.TabIndex = 0;
			// 
			// RSL_OceanCarrierNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.RSL_OceanCarrierNameTextBox, "RSL_CarrierName");
			this.RSL_OceanCarrierNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ed2d644e-cf12-4ecb-894d-445cce875ad5", "Ocean Carrier Name");
			this.RSL_OceanCarrierNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RSL_OceanCarrierNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 30, true);
			this.RSL_OceanCarrierNameTextBox.Name = "RSL_OceanCarrierNameTextBox";
			this.RSL_OceanCarrierNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 17, true);
			this.RSL_OceanCarrierNameTextBox.TabIndex = 1;
			// 
			// RSL_StandardCarrierAlphaCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RSL_StandardCarrierAlphaCodeTextBox, "RSL_StandardCarrierAlphaCode");
			this.RSL_StandardCarrierAlphaCodeTextBox.CaptionResourceString = null;
			this.RSL_StandardCarrierAlphaCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 55, true);
			this.RSL_StandardCarrierAlphaCodeTextBox.Name = "RSL_StandardCarrierAlphaCodeTextBox";
			this.RSL_StandardCarrierAlphaCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.RSL_StandardCarrierAlphaCodeTextBox.TabIndex = 2;
			// 
			// RSL_CargoWiseOneCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RSL_CargoWiseOneCodeTextBox, "RSL_CargoWiseOneCode");
			this.RSL_CargoWiseOneCodeTextBox.CaptionResourceString = null;
			this.RSL_CargoWiseOneCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 5, true);
			this.RSL_CargoWiseOneCodeTextBox.Name = "RSL_CargoWiseOneCodeTextBox";
			this.RSL_CargoWiseOneCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.RSL_CargoWiseOneCodeTextBox.TabIndex = 0;
			// 
			// RSL_IsActiveCheckBox
			// 
			this.RSL_IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_IsActiveCheckBox, "RSL_IsActive");
			this.RSL_IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("468ff47d-592b-4e8d-9ff6-581b3aa2b0f6", "Is Active");
			this.RSL_IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(454, 5, true);
			this.RSL_IsActiveCheckBox.Name = "RSL_IsActiveCheckBox";
			this.RSL_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 16, true);
			this.RSL_IsActiveCheckBox.TabIndex = 3;
			this.RSL_IsActiveCheckBox.Click += new System.EventHandler(this.RSL_IsActiveCheckBox_Click);
			// 
			// RSL_IsSystemCheckBox
			// 
			this.RSL_IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_IsSystemCheckBox, "RSL_IsSystem");
			this.RSL_IsSystemCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("344c2693-b995-4ffb-9b77-5cdd123888fc", "Is System");
			this.RSL_IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(454, 25, true);
			this.RSL_IsSystemCheckBox.Name = "RSL_IsSystemCheckBox";
			this.RSL_IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 16, true);
			this.RSL_IsSystemCheckBox.TabIndex = 4;
			// 
			// RSL_IsNVOCheckBox
			// 
			this.RSL_IsNVOCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_IsNVOCheckBox, "RSL_IsNVO");
			this.RSL_IsNVOCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d8e69075-a711-457a-8675-3fe1c41613ba", "NVO", "Non Vessel Operator", "Is Non Vessel Operator");
			this.RSL_IsNVOCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_IsNVOCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(454, 45, true);
			this.RSL_IsNVOCheckBox.Name = "RSL_IsNVOCheckBox";
			this.RSL_IsNVOCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 16, true);
			this.RSL_IsNVOCheckBox.TabIndex = 5;
			// 
			// RSL_IsShippingLineCheckBox
			// 
			this.RSL_IsShippingLineCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RSL_IsShippingLineCheckBox, "RSL_IsShippingLine");
			this.RSL_IsShippingLineCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("920d0568-10c9-4f8b-b593-9c47ebb85186", "Shipping Line");
			this.RSL_IsShippingLineCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RSL_IsShippingLineCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(454, 65, true);
			this.RSL_IsShippingLineCheckBox.Name = "RSL_IsShippingLineCheckBox";
			this.RSL_IsShippingLineCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 16, true);
			this.RSL_IsShippingLineCheckBox.TabIndex = 6;
			// 
			// AvailableIntegrationsTabPage
			// 
			this.AvailableIntegrationsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefShippingLineForm|1401c9a4-5c0a-48e3-a85e-9cf171e4186c", "Available Integrations");
			this.AvailableIntegrationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.AvailableIntegrationsTabPage.Name = "AvailableIntegrationsTabPage";
			this.AvailableIntegrationsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AvailableIntegrationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 141, true);
			this.AvailableIntegrationsTabPage.TabIndex = 0;
			this.AvailableIntegrationsTabPage.UseVisualStyleBackColor = true;
			this.AvailableIntegrationsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AvailableIntegrationsTabPage_InitializeTab));
			// 
			// MessagingRequirementsTabPage
			// 
			this.MessagingRequirementsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefShippingLineForm|6a1b9437-3cb9-4994-b43e-c44374ac2f18", "Messaging Requirements");
			this.MessagingRequirementsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MessagingRequirementsTabPage.Name = "MessagingRequirementsTabPage";
			this.MessagingRequirementsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagingRequirementsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 141, true);
			this.MessagingRequirementsTabPage.TabIndex = 1;
			this.MessagingRequirementsTabPage.UseVisualStyleBackColor = true;
			this.MessagingRequirementsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MessagingRequirementsTabPage_InitializeTab));
			// 
			// EBLProviderTabPage
			// 
			this.EBLProviderTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefShippingLineForm|46a8c38b-4d09-497a-ad70-e786d015c2cc", "Available eBL Providers");
			this.EBLProviderTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.EBLProviderTabPage.Name = "EBLProviderTabPage";
			this.EBLProviderTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EBLProviderTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 141, true);
			this.EBLProviderTabPage.TabIndex = 2;
			this.EBLProviderTabPage.UseVisualStyleBackColor = true;
			this.EBLProviderTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.EBLProviderTabPage_InitializeTab));

			const string billOfLadingProvider = "BLP";
			var refShippingLine = (RefShippingLine)BusinessEntity;
			this.EBLProviderTabPage.TabVisible = refShippingLine.ShippingLineMessagingRequirements.Any(x => string.Equals(x.RSR_RST_NKType, billOfLadingProvider));

			this.MainTabPage.PerformLayout();
			this.DetailTemplateTabControl.ResumeLayout(false);
			this.DetailTemplateTabControl.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.DetailsTableLayoutPanel.ResumeLayout(false);
			this.DetailsTableLayoutPanel.PerformLayout();
			this.DetailsHeaderPanel.ResumeLayout(false);
			this.DetailsHeaderPanel.PerformLayout();
			this.AvailableIntegrationsTabPage.ResumeLayout(false);
			this.AvailableIntegrationsTabPage.PerformLayout();
			this.MessagingRequirementsTabPage.ResumeLayout(false);
			this.MessagingRequirementsTabPage.PerformLayout();
			this.EBLProviderTabPage.ResumeLayout(false);
			this.EBLProviderTabPage.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		private Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private CargoWise.Windows.UI.KTableLayoutPanel DetailsTableLayoutPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel DetailsHeaderPanel;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl DetailTemplateTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage AvailableIntegrationsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage MessagingRequirementsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage EBLProviderTabPage;
		private Enterprise.MasterFiles.GUI.RefShippingLineMessagingRequirementsControl MessagingRequirementsControl;
		private RefShippingLineEBLProviderControl EBLProviderControl;
		internal RefShippingLineIntegrationsControl IntegrationsControl;
		internal ZArchitecture.ZTextBox RSL_OceanCarrierNameTextBox;
		internal ZArchitecture.ZTextBox RSL_StandardCarrierAlphaCodeTextBox;
		internal ZArchitecture.ZTextBox RSL_CargoWiseOneCodeTextBox;
		internal ZArchitecture.GUI.ZCheckBox RSL_IsActiveCheckBox;
		internal ZArchitecture.GUI.ZCheckBox RSL_IsSystemCheckBox;
		internal ZArchitecture.GUI.ZCheckBox RSL_IsNVOCheckBox;
		internal ZArchitecture.GUI.ZCheckBox RSL_IsShippingLineCheckBox;
	}
}
