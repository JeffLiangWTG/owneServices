
namespace Enterprise.Customs.SG.V4.GUI
{
	partial class PromtAmendmentForm
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
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.ReasonForExtendingTemporaryImportPeriodInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReasonForExtendingTemporaryImportPeriodInfoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.DeclarationGroupBox.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.zGroupBox2.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 279, true);
			this.zGroupBox1.TabIndex = 2;
			// 
			// zGrid1
			// 
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 260, true);
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.SupportingDocument)(((object)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).SupportingDocuments)))).DocumentTypes)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.SG.V4.Business.SupportingDocument)(((object)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).SupportingDocuments)))).DocumentTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.SupportingDocument)(((object)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).SupportingDocuments)))).DocumentType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.SupportingDocument)(((object)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).SupportingDocuments)))).StorageDocs)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.SupportingDocument)(((object)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).SupportingDocuments)))).eDoc)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.SG.V4.Business.SupportingDocument)(((object)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).SupportingDocuments)))).eDocInfo)));
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 474, true);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 474, true);
			// 
			// DeclarationGroupBox
			// 
			this.DeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 326, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 312, true);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.zGroupBox2);
			this.DetailsTabPage.Controls.Add(this.zGroupBox3);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 285, true);
			this.DetailsTabPage.Controls.SetChildIndex(this.zGroupBox3, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.zGroupBox2, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.BrokerDetailsGroupBox, 0);
			// 
			// BrokerDetailsGroupBox
			// 
			this.BrokerDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 196, true);
			this.BrokerDetailsGroupBox.TabIndex = 2;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 504, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 24, true);
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("5D6D10B4-B3A4-4BAD-8832-8B75B165331E", "Reason:");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 24, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.zLabel1.TabIndex = 0;
			// 
			// zTextBox1
			// 
			this.zTextBox1.BindTo = "AM_ReasonForAmending";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).AM_ReasonForAmendingInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).AM_ReasonForAmending)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 19, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 40, true);
			this.zTextBox1.TabIndex = 1;
			// 
			// ReasonForExtendingTemporaryImportPeriodInfoLabel
			// 
			this.ReasonForExtendingTemporaryImportPeriodInfoLabel.AutoSize = true;
			this.ReasonForExtendingTemporaryImportPeriodInfoLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("036BD9CA-DE1A-412B-BDDF-EAE5E88FBF60", "Reason:");
			this.ReasonForExtendingTemporaryImportPeriodInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 54, true);
			this.ReasonForExtendingTemporaryImportPeriodInfoLabel.Name = "ReasonForExtendingTemporaryImportPeriodInfoLabel";
			this.ReasonForExtendingTemporaryImportPeriodInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.ReasonForExtendingTemporaryImportPeriodInfoLabel.TabIndex = 1;
			// 
			// ReasonForExtendingTemporaryImportPeriodInfoTextBox
			// 
			this.ReasonForExtendingTemporaryImportPeriodInfoTextBox.BindTo = "AM_ReasonForExtendingTemporaryImportPeriod";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).AM_ReasonForExtendingTemporaryImportPeriodInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).AM_ReasonForExtendingTemporaryImportPeriod)));
			this.ReasonForExtendingTemporaryImportPeriodInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 49, true);
			this.ReasonForExtendingTemporaryImportPeriodInfoTextBox.Multiline = true;
			this.ReasonForExtendingTemporaryImportPeriodInfoTextBox.Name = "ReasonForExtendingTemporaryImportPeriodInfoTextBox";
			this.ReasonForExtendingTemporaryImportPeriodInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 40, true);
			this.ReasonForExtendingTemporaryImportPeriodInfoTextBox.TabIndex = 2;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.zTextBox1);
			this.zGroupBox2.Controls.Add(this.zLabel1);
			this.zGroupBox2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("7E12F051-5FAD-4B43-AA24-EAF72BAFEDF2", "Amendment");
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 8, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 70, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Controls.Add(this.zCheckBox1);
			this.zGroupBox3.Controls.Add(this.ReasonForExtendingTemporaryImportPeriodInfoTextBox);
			this.zGroupBox3.Controls.Add(this.ReasonForExtendingTemporaryImportPeriodInfoLabel);
			this.zGroupBox3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("DD767CE1-5340-419E-834C-DB7E519F6C52", "Extend Permit Validity");
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 84, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 105, true);
			this.zGroupBox3.TabIndex = 1;
			this.zGroupBox3.TabStop = false;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.zCheckBox1.BindTo = "AM_ExtendingTemporaryImportPeriod";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).AM_ExtendingTemporaryImportPeriod)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).AM_ExtendingTemporaryImportPeriodInfo)));
			this.zCheckBox1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("718C83A6-D403-49AC-9318-036465FF1376", "Extend Permit Validity?");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 23, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 17, true);
			this.zCheckBox1.TabIndex = 0;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// PromtAmendmentForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 528, true);
			this.Name = "PromtAmendmentForm";
			this.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("16F3CCD4-500A-438A-970A-6119A848E5B5", "Send Amendment");
			this.zGroupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.DeclarationGroupBox.ResumeLayout(false);
			this.DeclarationGroupBox.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.ZArchitecture.ZLabel ReasonForExtendingTemporaryImportPeriodInfoLabel;
		private Enterprise.ZArchitecture.ZTextBox ReasonForExtendingTemporaryImportPeriodInfoTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		public Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox2;
		public Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox3;
	}
}
