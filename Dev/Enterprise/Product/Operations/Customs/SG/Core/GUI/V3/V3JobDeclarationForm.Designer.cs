namespace Enterprise.Customs.SG.V3.GUI
{
	partial class V3JobDeclarationForm
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
		protected override void InitializeComponent()
		{
			this.Close_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 607, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// Close_Button
			// 
			this.Close_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Close_Button.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("V3JobDeclaration|3143B3B6-10A9-49F0-9B46-CFC2AF71A430", "Close");
			this.Close_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Close_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(766, 578, true);
			this.Close_Button.Name = "Close_Button";
			this.Close_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Close_Button.TabIndex = 1;
			this.Close_Button.UseVisualStyleBackColor = true;
			this.Close_Button.Click += new System.EventHandler(this.Close_Button_Click);
			// 
			// MainPanel
			// 
			this.MainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 565, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 565, true);
			this.MainPanel.TabIndex = 0;
			// 
			// V3JobDeclarationForm
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 631, true);
			this.Controls.Add(this.Close_Button);
			this.Controls.Add(this.MainPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 665, true);
			this.Name = "V3JobDeclarationForm";
			this.Text = "V3JobDeclarationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.Close_Button, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZButton Close_Button;
		public Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
	}
}
