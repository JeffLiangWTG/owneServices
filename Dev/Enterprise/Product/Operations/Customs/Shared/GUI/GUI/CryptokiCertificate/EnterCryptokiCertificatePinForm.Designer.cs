namespace Enterprise.Customs.GUI.Certificates
{
	partial class EnterCryptokiCertificatePinForm
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
		new void InitializeComponent()
		{
			this.PinLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PinTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AbortButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 95, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.UserEnterableTokenPin);
			// 
			// PinLabel
			// 
			this.PinLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("2fbd05d7-d16d-46b8-9f0e-6c97830715e7", "Please enter the PIN for your Digital Signature Certificate");
			this.PinLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PinLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 9, true);
			this.PinLabel.Name = "PinLabel";
			this.PinLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.PinLabel.TabIndex = 1;
			// 
			// PinTextBox
			// 
			this.BindingSource.SetBindingMember(this.PinTextBox, "Pin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.UserEnterableTokenPin)(null)).Pin)));
			this.PinTextBox.CaptionResourceString = null;
			this.PinTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PinTextBox, false);
			this.PinTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 34, true);
			this.PinTextBox.Name = "PinTextBox";
			this.PinTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.PinTextBox.TabIndex = 2;
			this.PinTextBox.UseSystemPasswordChar = true;
			// 
			// OkButton
			// 
			this.OkButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("d1f7f220-bbb2-4ff3-806c-1f9eb8fdfb1f", "OK");
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 60, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 3;
			this.OkButton.ToolTipCaption = null;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// AbortButton
			// 
			this.AbortButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("91337180-d38f-4e69-805b-eb600446a7f4", "Cancel");
			this.AbortButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.AbortButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 60, true);
			this.AbortButton.Name = "AbortButton";
			this.AbortButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AbortButton.TabIndex = 4;
			this.AbortButton.ToolTipCaption = null;
			this.AbortButton.Click += new System.EventHandler(this.AbortButton_Click);
			// 
			// EnterCryptokiCertificatePinForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.AbortButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 119, true);
			this.Controls.Add(this.PinLabel);
			this.Controls.Add(this.PinTextBox);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.AbortButton);
			this.DataSourceType = typeof(Enterprise.Customs.Business.UserEnterableTokenPin);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "EnterCryptokiCertificatePinForm";
			this.Text = "EnterCryptokiCertificatePinForm";
			this.Controls.SetChildIndex(this.AbortButton, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.PinTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PinLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.ZLabel PinLabel;
		internal Enterprise.ZArchitecture.ZTextBox PinTextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton OkButton;
		internal Enterprise.ZArchitecture.GUI.ZButton AbortButton;

		#endregion
	}
}
