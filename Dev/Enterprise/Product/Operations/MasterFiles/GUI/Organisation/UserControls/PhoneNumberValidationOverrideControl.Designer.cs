namespace Enterprise.MasterFiles.GUI
{
	partial class PhoneNumberValidationOverrideControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ManualVerifyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.HeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// Panel
			// 
			this.Panel.AutoScroll = true;
			this.Panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Panel.BackColor = System.Drawing.Color.Transparent;
			this.Panel.Controls.Add(this.InfoLabel);
			this.Panel.Controls.Add(this.ManualVerifyButton);
			this.Panel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, -110, true);
			this.Panel.Name = "Panel";
			this.Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 322, true);
			this.Panel.TabIndex = 2;
			// 
			// InfoLabel
			// 
			this.InfoLabel.BackColor = System.Drawing.Color.White;
			this.InfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 136, true);
			this.InfoLabel.Name = "InfoLabel";
			this.InfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 153, true);
			this.InfoLabel.TabIndex = 3;
			this.InfoLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// ManualVerifyButton
			// 
			this.ManualVerifyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ManualVerifyButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PhoneNumberValidationOverrideControl|57e52943-5cd5-42af-988c-5a1fb572f4f5", "Accept as Entered");
			this.ManualVerifyButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.Warning1616;
			this.ManualVerifyButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ManualVerifyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 295, true);
			this.ManualVerifyButton.Name = "ManualVerifyButton";
			this.ManualVerifyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 24, true);
			this.ManualVerifyButton.TabIndex = 4;
			this.ManualVerifyButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.ManualVerifyButton.UseVisualStyleBackColor = true;
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("PhoneNumberValidationOverrideControl|6e9b33fa-916d-4933-94a9-aa4faa1d1e7f", "Phone Number Validation");
			this.HeaderLabel.IsFontBold = true;
			this.HeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.HeaderLabel.Name = "HeaderLabel";
			this.HeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 22, true);
			this.HeaderLabel.TabIndex = 1;
			// 
			// PhoneNumberValidationOverrideControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.HeaderLabel);
			this.Controls.Add(this.Panel);
			this.Name = "PhoneNumberValidationOverrideControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 212, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Panel.ResumeLayout(false);
			this.Panel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public ZArchitecture.GUI.ZPanel Panel;
		public ZArchitecture.ZLabel HeaderLabel;
		internal ZArchitecture.GUI.ZButton ManualVerifyButton;
		public ZArchitecture.ZLabel InfoLabel;
	}
}
