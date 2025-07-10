namespace Enterprise.MasterFiles.GUI
{
	partial class LoadingForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.loadingPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.progressMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.loadingPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.LoadingFormModel<object>);
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.AutoSize = true;
			this.tableLayoutPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(224)))), ((int)(((byte)(227)))));
			this.tableLayoutPanel.ColumnCount = 1;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.Controls.Add(this.loadingPictureBox, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.progressMessageLabel, 0, 1);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 2;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 307, true);
			this.tableLayoutPanel.TabIndex = 0;
			// 
			// loadingPictureBox
			// 
			this.loadingPictureBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.loadingPictureBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(224)))), ((int)(((byte)(227)))));
			this.loadingPictureBox.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.Loading;
			this.loadingPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 68, true);
			this.loadingPictureBox.Name = "loadingPictureBox";
			this.loadingPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 160, true);
			this.loadingPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.loadingPictureBox.TabIndex = 1;
			this.loadingPictureBox.TabStop = false;
			// 
			// progressMessageLabel
			// 
			this.progressMessageLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.progressMessageLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.progressMessageLabel, "LoadingContent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.LoadingFormModel<object>)(null)).LoadingContent)));
			this.progressMessageLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.progressMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 260, true);
			this.progressMessageLabel.Name = "progressMessageLabel";
			this.progressMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 16, true);
			this.progressMessageLabel.TabIndex = 2;
			this.progressMessageLabel.Text = "progressMessageLabel";
			this.progressMessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.progressMessageLabel.UseMnemonic = false;
			// 
			// LoadingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 307, true);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "LoadingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "LoadingForm";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.loadingPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel;
		private ZArchitecture.GUI.ZPictureBox loadingPictureBox;
		private ZArchitecture.ZLabel progressMessageLabel;
	}
}
