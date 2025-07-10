using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EInvoicingBranchRegisterForm : ZChildForm
	{
		protected internal ZArchitecture.ZTextBox OTPTextBox;
		protected internal ZArchitecture.GUI.ZButton Cancel_Button;
		protected internal ZArchitecture.GUI.ZButton OKButton;
		protected internal ZArchitecture.ZTextBox progressLogTextBox;
		ZArchitecture.ZLabel HintLabel1;
		readonly EInvoicingBranchRegister register;
		ZGuidFindBox DebtorFindBox;
		ZArchitecture.ZLabel HintLabel2;
		ZArchitecture.ZLabel HintLabel3;
		readonly DigitalCertificateControl_p12_EInvoicing certificateControl;

		protected override void InitializeComponent()
		{
			this.OTPTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.progressLogTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HintLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.DebtorFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.HintLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.HintLabel3 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DebtorFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 374, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EInvoicingBranchRegister);
			// 
			// OTPTextBox
			// 
			this.BindingSource.SetBindingMember(this.OTPTextBox, "OTP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoicingBranchRegister)(null)).OTP)));
			this.OTPTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("88735bb7-e4db-4875-a9ba-65b94e3ed377", "One Time Password");
			this.OTPTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OTPTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OTPTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 74, true);
			this.OTPTextBox.Name = "OTPTextBox";
			this.OTPTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 17, true);
			this.OTPTextBox.TabIndex = 0;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("75afbbf0-653f-4034-9a71-1fd607269a3c", "Close");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 345, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.Cancel_Button.TabIndex = 3;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("938c3e15-e11b-4e3a-b303-613ee4bb48f0", "Submit");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 345, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// progressLogTextBox
			// 
			this.BindingSource.SetBindingMember(this.progressLogTextBox, "ProgressLog");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoicingBranchRegister)(null)).ProgressLog)));
			this.progressLogTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ae169db9-e554-4bca-b242-ee7aeda74292", "Logs");
			this.progressLogTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.progressLogTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.progressLogTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 129, true);
			this.progressLogTextBox.Multiline = true;
			this.progressLogTextBox.Name = "progressLogTextBox";
			this.progressLogTextBox.ReadOnly = true;
			this.progressLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.progressLogTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 200, true);
			this.progressLogTextBox.TabIndex = 4;
			this.progressLogTextBox.TabStop = false;
			this.progressLogTextBox.WordWrap = false;
			// 
			// HintLabel1
			// 
			this.HintLabel1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a7938b6f-31af-4bd4-8154-f11e94736fd9", "To register this Branch for e-Invoicing, enter the One Time Password shown on the" +
		" Fatoora Portal.");
			this.HintLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HintLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 4, true);
			this.HintLabel1.Name = "HintLabel1";
			this.HintLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 18, true);
			this.HintLabel1.TabIndex = 5;
			this.HintLabel1.UseMnemonic = false;
			// 
			// DebtorFindBox
			// 
			this.DebtorFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DebtorFindBox, "DebtorPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EInvoicingBranchRegister)(null)).DebtorPK)));
			this.DebtorFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3a464c9b-99df-4398-84d8-c26ba406bf32", "Debtor");
			this.DebtorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 99, true);
			this.DebtorFindBox.Name = "DebtorFindBox";
			this.DebtorFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DebtorFindBox.ParentType = null;
			this.DebtorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 19, true);
			this.DebtorFindBox.TabIndex = 1;
			// 
			// HintLabel2
			// 
			this.HintLabel2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("312f1e41-e25e-4be5-ad0f-cf230afded9e", "CargoWise must submit valid test transactions to complete registration.");
			this.HintLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HintLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 22, true);
			this.HintLabel2.Name = "HintLabel2";
			this.HintLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 18, true);
			this.HintLabel2.TabIndex = 6;
			this.HintLabel2.UseMnemonic = false;
			// 
			// HintLabel3
			// 
			this.HintLabel3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("af402b01-40de-4bc9-9ff0-4bd902951762", "Select a Saudi Arabia Receivables Organization to use as the Debtor for those tes" +
		"t transactions.");
			this.HintLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HintLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 40, true);
			this.HintLabel3.Name = "HintLabel3";
			this.HintLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 18, true);
			this.HintLabel3.TabIndex = 7;
			this.HintLabel3.UseMnemonic = false;
			// 
			// EInvoicingBranchRegisterForm
			// 
			this.AcceptButton = this.OKButton;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.CancelButton = this.Cancel_Button;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("362acec6-7a69-4431-aca0-326a1a6b47d8", "Register");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 398, true);
			this.Controls.Add(this.HintLabel3);
			this.Controls.Add(this.HintLabel2);
			this.Controls.Add(this.DebtorFindBox);
			this.Controls.Add(this.HintLabel1);
			this.Controls.Add(this.progressLogTextBox);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.OTPTextBox);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.EInvoicingBranchRegister);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "EInvoicingBranchRegisterForm";
			this.Controls.SetChildIndex(this.OTPTextBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.progressLogTextBox, 0);
			this.Controls.SetChildIndex(this.HintLabel1, 0);
			this.Controls.SetChildIndex(this.DebtorFindBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.HintLabel2, 0);
			this.Controls.SetChildIndex(this.HintLabel3, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DebtorFindBox.ResumeLayout(true);
			this.DebtorFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
