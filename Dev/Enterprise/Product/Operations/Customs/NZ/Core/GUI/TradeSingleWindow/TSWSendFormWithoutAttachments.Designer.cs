namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	partial class TSWSendFormWithoutAttachments
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
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FreeTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManualProcessingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ManualProcessingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalInformationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 316, true);
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
			this.OKButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("DB0BDF7B-AA23-4B0B-9ABE-972CDADA4B5D", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 285, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("146FFCE8-3D03-4B7E-BFFA-06BAC6B996D8", "Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 285, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.Cancel_Button.TabIndex = 3;
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
			this.AdditionalInformationGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("70889E31-8E29-4215-B81E-666646681119", "Additional Information");
			this.AdditionalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 8, true);
			this.AdditionalInformationGroupBox.Name = "AdditionalInformationGroupBox";
			this.AdditionalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 253, true);
			this.AdditionalInformationGroupBox.TabIndex = 1;
			this.AdditionalInformationGroupBox.TabStop = false;
			// 
			// ManualProcessingLabel
			// 
			this.ManualProcessingLabel.AutoSize = true;
			this.ManualProcessingLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("68ABC560-5BC5-4C51-A7F8-FD8B38081329", "Manual Processing Request (Override)");
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
			// TSWSendFormWithoutAttachments
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 340, true);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.AdditionalInformationGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business.TradeSingleWindow";
			this.DataSourceType = typeof(Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation);
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.TradeSingleWindow.AdditionalMessageInformation";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "TSWSendFormWithoutAttachments";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Send";
			this.Controls.SetChildIndex(this.AdditionalInformationGroupBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalInformationGroupBox.ResumeLayout(false);
			this.AdditionalInformationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZButton OKButton;
		public Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		public Enterprise.ZArchitecture.GUI.ZGroupBox AdditionalInformationGroupBox;
		public Enterprise.ZArchitecture.ZTextBox FreeTextTextBox;
		public ZArchitecture.ZTextBox ManualProcessingTextBox;
		public Enterprise.ZArchitecture.ZLabel ManualProcessingLabel;
	}
}
