using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SetSQLPasswordForm : ZChildForm
	{
		protected internal ZArchitecture.ZTextBox SqlPasswordTextBox;
		protected internal ZArchitecture.GUI.ZButton Cancel_Button;
		protected internal ZArchitecture.GUI.ZButton OKButton;
		internal ZArchitecture.ZLabel ErrorLabel;

		protected override void InitializeComponent()
		{
			this.SqlPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ErrorLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 24, true);
			// 
			// SqlPasswordTextBox
			// 
			this.SqlPasswordTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("69fbe315-613e-4b2c-854d-b0185cf8b213", "New SQL Server Password");
			this.SqlPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SqlPasswordTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SqlPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 77, true);
			this.SqlPasswordTextBox.Name = "SqlPasswordTextBox";
			this.SqlPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 21, true);
			this.SqlPasswordTextBox.TabIndex = 0;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3ddaf05b-3ad0-41e3-8239-f230f1efe4cb", "Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 163, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.Cancel_Button.TabIndex = 3;
			this.Cancel_Button.ToolTipCaption = null;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("552c2dbf-e226-4530-a709-718b12784c7a", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 163, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// ErrorLabel
			// 
			this.ErrorLabel.BackColor = System.Drawing.Color.Transparent;
			this.ErrorLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f3badeea-81e7-4239-9af7-9b091625c417", "Error Label");
			this.ErrorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ErrorLabel.ForeColor = System.Drawing.Color.Red;
			this.ErrorLabel.IsFontBold = true;
			this.ErrorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 111, true);
			this.ErrorLabel.Name = "ErrorLabel";
			this.ErrorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 35, true);
			this.ErrorLabel.TabIndex = 1;
			// 
			// SetSQLPasswordForm
			// 
			this.AcceptButton = this.OKButton;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.CancelButton = this.Cancel_Button;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("378e06da-d11b-40fa-ae73-869c0bc3b3b6", "Set SQL Password");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 216, true);
			this.Controls.Add(this.ErrorLabel);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.SqlPasswordTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "SetSQLPasswordForm";
			this.Load += new System.EventHandler(this.SetSQLPasswordForm_Load);
			this.Controls.SetChildIndex(this.SqlPasswordTextBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.ErrorLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
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
