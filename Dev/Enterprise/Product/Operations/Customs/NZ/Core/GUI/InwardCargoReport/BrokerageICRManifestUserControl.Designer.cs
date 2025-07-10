namespace Enterprise.Customs.NZ.GUI
{
	partial class BrokerageICRManifestUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//protected override void Dispose(bool disposing)
		//{
		//    if (disposing && (components != null))
		//    {
		//        components.Dispose();
		//    }
		//    base.Dispose(disposing);
		//}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.interpretedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.eM_InterpretedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TopPanel.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			this.MessageTextGroupBox.SuspendLayout();
			this.HistoryPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessageTextPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.interpretedGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 40, true);
			// 
			// StatusLabel
			// 
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 8, true);
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 23, true);
			// 
			// MessageTextGroupBox
			// 
			this.MessageTextGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MessageTextGroupBox.Dock = System.Windows.Forms.DockStyle.None;
			this.MessageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 256, true);
			// 
			// MessageTextPanel
			// 
			this.MessageTextPanel.Controls.Add(this.interpretedGroupBox);
			this.MessageTextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 504, true);
			this.MessageTextPanel.Controls.SetChildIndex(this.interpretedGroupBox, 0);
			this.MessageTextPanel.Controls.SetChildIndex(this.MessageTextGroupBox, 0);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 237, true);
			// 
			// E2_MessageStatusBoundTextBox
			// 
			this.E2_MessageStatusBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.E2_MessageStatusBoundTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
			this.E2_MessageStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(661, 9, true);
			// 
			// CustomsEntryNumberTextBox
			// 
			this.CustomsEntryNumberTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("ZManifestMessageHistoryUserControl|F5825223-1813-4322-A73C-70010D460CBF", "ICR");
			this.CustomsEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 9, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.TradeSingleWindow.ICRManifestStatus);
			// 
			// InterpretedGroupBox
			// 
			this.interpretedGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.interpretedGroupBox.Controls.Add(this.eM_InterpretedTextBox);
			this.interpretedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 256, true);
			this.interpretedGroupBox.Name = "InterpretedGroupBox";
			this.interpretedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 248, true);
			this.interpretedGroupBox.TabIndex = 1;
			this.interpretedGroupBox.TabStop = false;
			this.interpretedGroupBox.Text = "Interpreted Message";
			// 
			// EM_InterpretedTextBox
			// 
			this.BindingSource.SetBindingMember(this.eM_InterpretedTextBox, "MessagesIncludingInterchangeRejections.EM_MessageInterpretation");
			this.eM_InterpretedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.eM_InterpretedTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eM_InterpretedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.eM_InterpretedTextBox.Multiline = true;
			this.eM_InterpretedTextBox.Name = "EM_InterpretedTextBox";
			this.eM_InterpretedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 229, true);
			this.eM_InterpretedTextBox.TabIndex = 0;
			// 
			// BrokerageICRManifestUserControl
			// 
			this.Name = "BrokerageICRManifestUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 544, true);
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.PerformLayout();
			this.HistoryPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessageTextPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.interpretedGroupBox.ResumeLayout(false);
			this.interpretedGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
