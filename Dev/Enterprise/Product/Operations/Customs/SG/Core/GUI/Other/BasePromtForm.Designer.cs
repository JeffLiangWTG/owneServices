namespace Enterprise.Customs.SG.V4.GUI
{
	partial class BasePromtForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BasePromtForm));
			this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DeclarationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeclarationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCheckShowMessage = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BrokerDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BrokerPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			this.DeclarationGroupBox.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.BrokerDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 315, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation);
			// 
			// DeclarationCheckBox
			// 
			this.DeclarationCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationCheckBox.AutoSize = true;
			this.DeclarationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DeclarationCheckBox.Checked = true;
			this.DeclarationCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.DeclarationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DeclarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 114, true);
			this.DeclarationCheckBox.Name = "DeclarationCheckBox";
			this.DeclarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.DeclarationCheckBox.TabIndex = 2;
			this.DeclarationCheckBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BasePromtForm|880AC960-8FCF-4162-8CB8-54FFAB2A14FB", "I Agree");
			this.DeclarationCheckBox.UseVisualStyleBackColor = true;
			this.DeclarationCheckBox.CheckedChanged += new System.EventHandler(this.DeclarationCheckBox_CheckedChanged);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Enabled = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 284, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BasePromtForm|31E6FB11-B0A0-4091-A9FA-A38D85AC3A09", "OK");
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 284, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.Cancel_Button.TabIndex = 3;
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BasePromtForm|B46256A4-72CD-4E33-B36F-302232E89D0E", "Cancel");
			this.Cancel_Button.UseVisualStyleBackColor = true;
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// DeclarationTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclarationTextBox, "AM_Declaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).AM_Declaration)));
			this.DeclarationTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("76E23B86-C2F5-452D-9572-6F8BC9207A74", "Declaration");
			this.DeclarationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DeclarationTextBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.DeclarationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DeclarationTextBox.Multiline = true;
			this.DeclarationTextBox.Name = "DeclarationTextBox";
			this.DeclarationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DeclarationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 91, true);
			this.DeclarationTextBox.TabIndex = 0;
			this.DeclarationTextBox.Text = resources.GetString("DeclarationTextBox.Text");
			// 
			// DeclarationGroupBox
			// 
			this.DeclarationGroupBox.Controls.Add(this.zCheckShowMessage);
			this.DeclarationGroupBox.Controls.Add(this.DeclarationTextBox);
			this.DeclarationGroupBox.Controls.Add(this.DeclarationCheckBox);
			this.DeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 135, true);
			this.DeclarationGroupBox.Name = "DeclarationGroupBox";
			this.DeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 137, true);
			this.DeclarationGroupBox.TabIndex = 1;
			this.DeclarationGroupBox.TabStop = false;
			this.DeclarationGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BasePromtForm|C910238D-5F80-44F6-AD57-B2F384D36989", "Declaration");
			// 
			// zCheckShowMessage
			// 
			this.zCheckShowMessage.AutoSize = true;
			this.zCheckShowMessage.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckShowMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 114, true);
			this.zCheckShowMessage.Name = "zCheckShowMessage";
			this.zCheckShowMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 17, true);
			this.zCheckShowMessage.TabIndex = 1;
			this.zCheckShowMessage.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BasePromtForm|72F3FB18-A60A-4FFD-B2E6-CEA2FEB4EAD5", "Preview Message");
			this.zCheckShowMessage.UseVisualStyleBackColor = true;
			// 
			// MainTabControl
			//
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.DetailsTabPage);
			this.MainTabControl.Controls.Add(this.SupportingDocumentsTabPage);
			//this.MainTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 122, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// SupportingDocumentsTabPage
			// 
			this.SupportingDocumentsTabPage.Controls.Add(this.zGroupBox1);
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
			this.SupportingDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 95, true);
			this.SupportingDocumentsTabPage.TabIndex = 1;
			this.SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BasePromtForm|F4138840-E0C7-4F6F-8C89-DFB3D821ECCF", "Supporting Documents");
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.SupportingDocumentsGrid);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 89, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BasePromtForm|7604F2D4-77F7-450B-8765-A78195F607C2", "Select Supporting Documents (*.doc;*.pdf;*.xls;*.tif;*.emf)");
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, "SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).SupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).SupportingDocuments)).SyncRoot)).DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).SupportingDocuments)).SyncRoot)).DocumentTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).SupportingDocuments)).SyncRoot)).eDoc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).SupportingDocuments)).SyncRoot)).StorageDocs)));
			this.SupportingDocumentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "DocumentTypes";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SupportingDocumentsGrid|2D172BE8-73DB-4EF8-8A4B-52DE659671BE", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "DocumentType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.MaxDropDownItems = 12;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidDropEditColumnStyleInfo1.BindToList = "StorageDocs";
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SupportingDocumentsGrid|3551190E-FBBD-485B-97C2-5558A3ADA3FA", "eDoc");
			zGuidDropEditColumnStyleInfo1.ColumnName = "eDoc";
			zGuidDropEditColumnStyleInfo1.IsMandatory = true;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGrid.GridId = "b522c5b0-62cf-4d63-bafd-fe451fca4952";
			this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupportingDocumentsGrid.LayoutKey = "zGrid1";
			this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 70, true);
			this.SupportingDocumentsGrid.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.BrokerDetailsGroupBox);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 95, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BasePromtForm|C8058217-C652-4886-A7EF-6D0466E808D3", "Details");
			// 
			// BrokerDetailsGroupBox
			// 
			this.BrokerDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BrokerDetailsGroupBox.Controls.Add(this.BrokerCodeFindBox);
			this.BrokerDetailsGroupBox.Controls.Add(this.BrokerPasswordTextBox);
			this.BrokerDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 6, true);
			this.BrokerDetailsGroupBox.Name = "BrokerDetailsGroupBox";
			this.BrokerDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 80, true);
			this.BrokerDetailsGroupBox.TabIndex = 0;
			this.BrokerDetailsGroupBox.TabStop = false;
			this.BrokerDetailsGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BasePromtForm|32ABDB89-9A61-486D-8F4A-A7EA359E0538", "Broker Details");
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "AM_Broker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).AM_Broker)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).Lookups.Brokers)));
			this.BrokerCodeFindBox.BindToList = "Lookups+Brokers";
			this.BrokerCodeFindBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BasePromtForm|7942FD52-6FA7-419E-9A9E-0F08C02C28E6", "Broker:");
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 23, true);
			this.BrokerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.PreBoundMaxLength = 3;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 21, true);
			this.BrokerCodeFindBox.TabIndex = 1;
			// 
			// BrokerPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrokerPasswordTextBox, "AM_BrokerPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).AM_BrokerPassword)));
			this.BrokerPasswordTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BasePromtForm|53CF24B2-278C-45B1-AC49-67B78357AB74", "Password:");
			this.BrokerPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BrokerPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 50, true);
			this.BrokerPasswordTextBox.Name = "BrokerPasswordTextBox";
			this.BrokerPasswordTextBox.PasswordChar = '*';
			this.BrokerPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.BrokerPasswordTextBox.TabIndex = 3;
			// 
			// BasePromtForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 339, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.DeclarationGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.SG.V4.Business";
			this.DataSourceType = typeof(Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation);
			this.DataSourceTypeName = "Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "BasePromtForm";
			this.ShouldSerializeTabPageMethods = false;
			this.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BasePromtForm|60C7BD66-9D4B-41F2-8A9B-03656D0A64D5", "Send");
			this.Controls.SetChildIndex(this.DeclarationGroupBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			this.DeclarationGroupBox.ResumeLayout(false);
			this.DeclarationGroupBox.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			this.BrokerDetailsGroupBox.ResumeLayout(false);
			this.BrokerDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZButton OKButton;
		public Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		public Enterprise.ZArchitecture.GUI.ZGroupBox DeclarationGroupBox;
		public Enterprise.ZArchitecture.GUI.ZCheckBox DeclarationCheckBox;
		private Enterprise.ZArchitecture.ZTextBox DeclarationTextBox;
		public Enterprise.ZArchitecture.GUI.ZTabControl MainTabControl;
		public Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		public Enterprise.ZArchitecture.GUI.ZGroupBox BrokerDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		private Enterprise.ZArchitecture.ZTextBox BrokerPasswordTextBox;
		public Enterprise.ZArchitecture.GUI.ZCheckBox zCheckShowMessage;
		public Enterprise.ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		protected Enterprise.ZArchitecture.ZGrid SupportingDocumentsGrid;
	}
}
