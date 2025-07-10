using Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration.FormalEntry
{
	public partial class SubmitToCustomsForm : Base.SubmitToCustomsForm
	{
		private ZLabel pinEntryLabel;
		internal ZTextBox declarationTextBox;
		internal ZTextBox pinEntryTextBox;
		private ZCheckBox overrideCheckBox;
		private ZLabel overrideLabel;
		private ZLabel requestPDOLabel;
		private ZCheckBox requestPDOCheckBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SubmitToCustomsForm));
			this.pinEntryLabel = new ZLabel();
			this.declarationTextBox = new ZTextBox();
			this.pinEntryTextBox = new ZTextBox();
			this.overrideCheckBox = new ZCheckBox();
			this.overrideLabel = new ZLabel();
			this.requestPDOLabel = new ZLabel();
			this.requestPDOCheckBox = new ZCheckBox();
			this.RemarksGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// CancelBtn
			// 
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 304, true);
			this.CancelBtn.TabIndex = 10;
			// 
			// OkButton
			// 
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 304, true);
			this.OkButton.TabIndex = 9;
			// 
			// SelectDefaultRemarksButton
			// 
			this.SelectDefaultRemarksButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 70, true);
			// 
			// RemarksTextBox
			// 
			this.RemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 15, true);
			this.RemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 49, true);
			// 
			// RemarksGroupBox
			// 
			this.RemarksGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 199, true);
			this.RemarksGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 99, true);
			this.RemarksGroupBox.TabIndex = 8;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 338, true);
			this.MainStatusBar.TabIndex = 11;
			// 
			// PinEntryLabel
			// 
			this.pinEntryLabel.AutoSize = true;
			this.pinEntryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 130, true);
			this.pinEntryLabel.Name = "PinEntryLabel";
			this.pinEntryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 13, true);
			this.pinEntryLabel.TabIndex = 2;
			this.pinEntryLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("64a113ed-8e08-44fb-9ff0-ee6dd30c92bf", "PIN Number:");
			// 
			// DeclarationTextBox
			// 
			this.declarationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.declarationTextBox.BindTo = "BrokersDeclaration";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((MessageManager)(null)).BrokersDeclarationInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((MessageManager)(null)).BrokersDeclaration);
			this.declarationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.declarationTextBox.ReadOnly = true;
			this.declarationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 34, true);
			this.declarationTextBox.Multiline = true;
			this.declarationTextBox.Name = "DeclarationTextBox";
			this.declarationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 87, true);
			this.declarationTextBox.TabIndex = 1;
			this.declarationTextBox.Text = resources.GetString("DeclarationTextBox.Text");
			// 
			// PinEntryTextBox
			// 
			this.pinEntryTextBox.BindTo = "EnteredPinNumber";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((MessageManager)(null)).EnteredPinNumberInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((MessageManager)(null)).EnteredPinNumber);
			this.pinEntryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 127, true);
			this.pinEntryTextBox.Name = "PinEntryTextBox";
			this.pinEntryTextBox.PasswordChar = '*';
			this.pinEntryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.pinEntryTextBox.TabIndex = 3;
			// 
			// OverrideCheckBox
			// 
			this.overrideCheckBox.AutoSize = true;
			this.overrideCheckBox.BindTo = "EnteredOverrideFlag";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((MessageManager)(null)).EnteredOverrideFlag);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((MessageManager)(null)).EnteredOverrideFlagInfo);
			this.overrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.overrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 153, true);
			this.overrideCheckBox.Name = "OverrideCheckBox";
			this.overrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
			this.overrideCheckBox.TabIndex = 5;
			this.overrideCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("a0403736-2d18-4996-a732-003a11a053c1", "   (Send for review by Customs Audit Team)");
			// 
			// OverrideLabel
			// 
			this.overrideLabel.AutoSize = true;
			this.overrideLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 154, true);
			this.overrideLabel.Name = "OverrideLabel";
			this.overrideLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.overrideLabel.TabIndex = 4;
			this.overrideLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("f8abaca1-51a1-4401-a24d-6094fccaa213", "Override:");
			// 
			// RequestPDOLabel
			// 
			this.requestPDOLabel.AutoSize = true;
			this.requestPDOLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 177, true);
			this.requestPDOLabel.Name = "RequestPDOLabel";
			this.requestPDOLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 13, true);
			this.requestPDOLabel.TabIndex = 6;
			this.requestPDOLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("ae162710-16f2-4e8d-a458-466596cb468b", "Request PDO:");
			// 
			// RequestPDOCheckBox
			// 
			this.requestPDOCheckBox.AutoSize = true;
			this.requestPDOCheckBox.BindTo = "EnteredPDOFlag";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((MessageManager)(null)).EnteredPDOFlag);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((MessageManager)(null)).EnteredPDOFlagInfo);
			this.requestPDOCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.requestPDOCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 176, true);
			this.requestPDOCheckBox.Name = "RequestPDOCheckBox";
			this.requestPDOCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.requestPDOCheckBox.TabIndex = 7;
			this.requestPDOCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("eb000297-eb3c-4399-979e-107cbc3d0a06", "   (Submit \'PDO\' Other Info code)");
			// 
			// SubmitToCustomsForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 363, true);
			this.Controls.Add(this.requestPDOLabel);
			this.Controls.Add(this.requestPDOCheckBox);
			this.Controls.Add(this.overrideLabel);
			this.Controls.Add(this.overrideCheckBox);
			this.Controls.Add(this.pinEntryTextBox);
			this.Controls.Add(this.declarationTextBox);
			this.Controls.Add(this.pinEntryLabel);
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.MessageManager";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 368, true);
			this.Name = "SubmitToCustomsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.CancelBtn, 0);
			this.Controls.SetChildIndex(this.MessageTypeLabel, 0);
			this.Controls.SetChildIndex(this.pinEntryLabel, 0);
			this.Controls.SetChildIndex(this.declarationTextBox, 0);
			this.Controls.SetChildIndex(this.pinEntryTextBox, 0);
			this.Controls.SetChildIndex(this.overrideCheckBox, 0);
			this.Controls.SetChildIndex(this.overrideLabel, 0);
			this.Controls.SetChildIndex(this.requestPDOCheckBox, 0);
			this.Controls.SetChildIndex(this.RemarksGroupBox, 0);
			this.Controls.SetChildIndex(this.requestPDOLabel, 0);
			this.RemarksGroupBox.ResumeLayout(false);
			this.RemarksGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
