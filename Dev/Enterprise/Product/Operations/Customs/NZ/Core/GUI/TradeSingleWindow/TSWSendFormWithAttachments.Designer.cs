namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	partial class TSWSendFormWithAttachments
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FreeTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManualProcessingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ManualProcessingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalInformationGroupBox.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 460, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("8ACAD20A-8962-468B-B173-8B753300C720", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 429, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("3B72D5A8-4597-4124-8559-3812DF0CC256", "Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 429, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.Cancel_Button.TabIndex = 3;
			this.Cancel_Button.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// FreeTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.FreeTextTextBox, "AM_FreeText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).AM_FreeText)));
			this.FreeTextTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("4BA2B645-91F7-42B6-BFC0-D8864CC6F324", "", "May be used by the sender to state additional information relating to the declaration.");
			this.FreeTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FreeTextTextBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FreeTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.FreeTextTextBox.Multiline = true;
			this.FreeTextTextBox.Name = "FreeTextTextBox";
			this.FreeTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.FreeTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 91, true);
			this.FreeTextTextBox.TabIndex = 0;
			// 
			// AdditionalInformationGroupBox
			// 
			this.AdditionalInformationGroupBox.Controls.Add(this.ManualProcessingLabel);
			this.AdditionalInformationGroupBox.Controls.Add(this.ManualProcessingTextBox);
			this.AdditionalInformationGroupBox.Controls.Add(this.FreeTextTextBox);
			this.AdditionalInformationGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("A6C46D22-C052-474E-BA96-A110AC4C14D8", "Additional Information");
			this.AdditionalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 172, true);
			this.AdditionalInformationGroupBox.Name = "AdditionalInformationGroupBox";
			this.AdditionalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 253, true);
			this.AdditionalInformationGroupBox.TabIndex = 1;
			this.AdditionalInformationGroupBox.TabStop = false;
			// 
			// ManualProcessingLabel
			// 
			this.ManualProcessingLabel.AutoSize = true;
			this.ManualProcessingLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("8BE3564E-3FD4-4FAF-8CDC-FBD5866A26CA", "Manual Processing Request (Override)");
			this.ManualProcessingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ManualProcessingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 123, true);
			this.ManualProcessingLabel.Name = "ManualProcessingLabel";
			this.ManualProcessingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 13, true);
			this.ManualProcessingLabel.TabIndex = 6;
			// 
			// ManualProcessingTextBox
			// 
			this.ManualProcessingTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.ManualProcessingTextBox, "AM_OverrideText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).AM_OverrideText)));
			this.ManualProcessingTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("e1a6f6e8-47b8-43f3-a604-b8df38c740e3", "", "May be transmitted to request override of a previously reported error or to direct a declaration to a border agency officer for manual processing.");
			this.ManualProcessingTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ManualProcessingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 138, true);
			this.ManualProcessingTextBox.Multiline = true;
			this.ManualProcessingTextBox.Name = "ManualProcessingTextBox";
			this.ManualProcessingTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ManualProcessingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 91, true);
			this.ManualProcessingTextBox.TabIndex = 5;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.DetailsTabPage);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 5, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 163, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.zGroupBox1);
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("5C23DC08-53A5-4701-8060-F7F307B949D9", "Additional Documents");
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 141, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.zGrid1);
			this.zGroupBox1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("DF7235DE-8600-48C8-8095-A404D18557DD", "Select Supporting Documents");
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 136, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).SupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).SupportingDocuments)).SyncRoot)).AttachmentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).SupportingDocuments)).SyncRoot)).AttachmentTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).SupportingDocuments)).SyncRoot)).eDoc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation)(null)).SupportingDocuments)).SyncRoot)).StorageDocs)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "AttachmentTypes";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("A49AF11C-5206-4EF3-8B8D-DB0DFBA0D816", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "AttachmentType";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.MaxDropDownItems = 12;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidDropEditColumnStyleInfo1.BindToList = "StorageDocs";
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("ABE23DBE-BA6E-435C-B752-E1F9AC36E0B0", "eDoc");
			zGuidDropEditColumnStyleInfo1.ColumnName = "eDoc";
			zGuidDropEditColumnStyleInfo1.IsMandatory = true;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "9750499c-36a8-427c-a47b-33931f732abf";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 119, true);
			this.zGrid1.TabIndex = 0;
			// 
			// TSWSendFormWithAttachments
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 484, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.AdditionalInformationGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business.TradeSingleWindow";
			this.DataSourceType = typeof(Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation);
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "TSWSendFormWithAttachments";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Send";
			this.Controls.SetChildIndex(this.AdditionalInformationGroupBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalInformationGroupBox.ResumeLayout(false);
			this.AdditionalInformationGroupBox.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZButton OKButton;
		public Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		public Enterprise.ZArchitecture.GUI.ZGroupBox AdditionalInformationGroupBox;
		public Enterprise.ZArchitecture.ZTextBox FreeTextTextBox;
		public Enterprise.ZArchitecture.GUI.ZTabControl MainTabControl;
		public Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		public ZArchitecture.GUI.ZGroupBox zGroupBox1;
		public ZArchitecture.ZGrid zGrid1;
		public ZArchitecture.ZTextBox ManualProcessingTextBox;
		public Enterprise.ZArchitecture.ZLabel ManualProcessingLabel;
	}
}
