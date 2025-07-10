namespace Enterprise.MasterFiles.GUI
{
	partial class AddressValidationMessageMonitorForm
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
		private new void InitializeComponent()
		{
			this.PNLButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonClear = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonToggleStartStop = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TextBoxStackTrace = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PNLButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 616, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 24, true);
			// 
			// PNLButtons
			// 
			this.PNLButtons.Controls.Add(this.ButtonClear);
			this.PNLButtons.Controls.Add(this.ButtonToggleStartStop);
			this.PNLButtons.Dock = System.Windows.Forms.DockStyle.Top;
			this.PNLButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PNLButtons.Name = "PNLButtons";
			this.PNLButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 39, true);
			this.PNLButtons.TabIndex = 6;
			// 
			// ButtonClear
			// 
			this.ButtonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonClear.BackColor = System.Drawing.Color.Salmon;
			this.ButtonClear.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(730, 6, true);
			this.ButtonClear.Name = "ButtonClear";
			this.ButtonClear.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 30, true);
			this.ButtonClear.TabIndex = 6;
			this.ButtonClear.Text = "Clear";
			this.ButtonClear.UseVisualStyleBackColor = false;
			this.ButtonClear.Click += new System.EventHandler(this.BtnClear_Click);
			// 
			// ButtonToggleStartStop
			// 
			this.ButtonToggleStartStop.BackColor = System.Drawing.Color.DodgerBlue;
			this.ButtonToggleStartStop.ForeColor = System.Drawing.Color.White;
			this.ButtonToggleStartStop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.ButtonToggleStartStop.Name = "ButtonToggleStartStop";
			this.ButtonToggleStartStop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 30, true);
			this.ButtonToggleStartStop.TabIndex = 5;
			this.ButtonToggleStartStop.Text = "Start tracking address validation messages";
			this.ButtonToggleStartStop.UseVisualStyleBackColor = false;
			this.ButtonToggleStartStop.Click += new System.EventHandler(this.BtnCollect_Click);
			// 
			// TextBoxStackTrace
			// 
			this.TextBoxStackTrace.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBoxStackTrace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TextBoxStackTrace.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TextBoxStackTrace.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 39, true);
			this.TextBoxStackTrace.Multiline = true;
			this.TextBoxStackTrace.Name = "TextBoxStackTrace";
			this.TextBoxStackTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.TextBoxStackTrace.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 577, true);
			this.TextBoxStackTrace.TabIndex = 7;
			// 
			// AddressValidationMessageMonitorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 640, true);
			this.Controls.Add(this.TextBoxStackTrace);
			this.Controls.Add(this.PNLButtons);
			this.Name = "AddressValidationMessageMonitorForm";
			this.Text = "Address Validation Message Monitor";
			this.TopMost = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PNLButtons, 0);
			this.Controls.SetChildIndex(this.TextBoxStackTrace, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PNLButtons.ResumeLayout(false);
			this.PNLButtons.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZPanel PNLButtons;
		public ZArchitecture.GUI.ZButton ButtonClear;
		public ZArchitecture.GUI.ZButton ButtonToggleStartStop;
		public ZArchitecture.ZTextBox TextBoxStackTrace;
	}
}
