namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class SummaryViewUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.titleLabel = new Enterprise.ZArchitecture.ZLabel();
            this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.itemsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.scrollPanel = new System.Windows.Forms.Panel();
            this.scrollPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Largest;
            this.titleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 20, true);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 19, true);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "label1";
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(168)))), ((int)(((byte)(226)))));
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.ForeColor = System.Drawing.Color.White;
            this.closeButton.IsCaptionOverridden = true;
            this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 479, true);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 32, true);
            this.closeButton.TabIndex = 1;
            this.closeButton.Text = "Close";
            this.closeButton.ToolTipCaption = null;
            this.closeButton.UseVisualStyleBackColor = false;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // itemsLayoutPanel
            // 
            this.itemsLayoutPanel.AutoSize = true;
            this.itemsLayoutPanel.ColumnCount = 1;
            this.itemsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.itemsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.itemsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.itemsLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.itemsLayoutPanel.Name = "itemsLayoutPanel";
            this.itemsLayoutPanel.RowCount = 1;
            this.itemsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.itemsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 0, true);
			this.itemsLayoutPanel.TabIndex = 2;
            // 
            // scrollPanel
            // 
            this.scrollPanel.AutoScroll = true;
            this.scrollPanel.Controls.Add(this.itemsLayoutPanel);
            this.scrollPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 67, true);
            this.scrollPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.scrollPanel.Name = "scrollPanel";
            this.scrollPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 397, true);
            this.scrollPanel.TabIndex = 3;
            // 
            // SummaryViewUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.scrollPanel);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.titleLabel);
            this.Name = "SummaryViewUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 524, true);
            this.scrollPanel.ResumeLayout(false);
            this.scrollPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel titleLabel;
		private Enterprise.ZArchitecture.GUI.ZButton closeButton;
		private System.Windows.Forms.TableLayoutPanel itemsLayoutPanel;
		private System.Windows.Forms.Panel scrollPanel;
	}
}
