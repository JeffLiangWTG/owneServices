namespace Enterprise.Customs.GUI
{
	partial class LongTextForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.LongTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LongTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.oKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LongTextGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 250, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// LongTextGroupBox
			// 
			this.LongTextGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("e593cd4d-5c4a-450a-b526-ebe72876e131", "Long Text");
			this.LongTextGroupBox.Controls.Add(this.LongTextTextBox);
			this.LongTextGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LongTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LongTextGroupBox.Name = "LongTextGroupBox";
			this.LongTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 223, true);
			this.LongTextGroupBox.TabIndex = 1;
			this.LongTextGroupBox.TabStop = false;
			// 
			// LongTextTextBox
			// 
			this.LongTextTextBox.CaptionResourceString = null;
			this.LongTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LongTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LongTextTextBox.Multiline = true;
			this.LongTextTextBox.Name = "LongTextTextBox";
			this.LongTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 204, true);
			this.LongTextTextBox.TabIndex = 0;
			// 
			// OKButton
			// 
			this.oKButton.CaptionResourceString = null;
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 227, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.oKButton.TabIndex = 2;
			this.oKButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("315760FC-B473-4DDB-8688-195EFD4E1021", "OK");
			this.oKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// LongTextForm
			// 

			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("91b740a7-af68-49c9-99f3-bbb854024c83", "Long Text Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 272, true);
			this.Controls.Add(this.oKButton);
			this.Controls.Add(this.LongTextGroupBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "LongTextForm";
			this.Text = "Long Text";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.LongTextGroupBox, 0);
			this.Controls.SetChildIndex(this.oKButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LongTextGroupBox.ResumeLayout(false);
			this.LongTextGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}

		private System.ComponentModel.Container components = null;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox LongTextGroupBox;
		internal Enterprise.ZArchitecture.ZTextBox LongTextTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton oKButton;

		#endregion
	}
}
