namespace Enterprise.MasterFiles.GUI
{
	partial class EventsBannerItemControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.eventDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.eventTipPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.eventTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.eventBannerItemTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.eventTipPictureBox)).BeginInit();
			this.eventBannerItemTableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.EventItemModel);
			// 
			// eventDateLabel
			// 
			this.BindingSource.SetBindingMember(this.eventDateLabel, "EventDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.EventItemModel)(null)).EventDate)));
			this.eventDateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eventDateLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.eventDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.eventDateLabel.Name = "eventDateLabel";
			this.eventDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 33, true);
			this.eventDateLabel.TabIndex = 2;
			this.eventDateLabel.UseMnemonic = false;
			// 
			// eventTipPictureBox
			// 
			this.eventTipPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.eventTipPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 0, true);
			this.eventTipPictureBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 11, 0, true);
			this.eventTipPictureBox.Name = "eventTipPictureBox";
			this.eventTipPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 33, true);
			this.eventTipPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.eventTipPictureBox.TabIndex = 1;
			this.eventTipPictureBox.TabStop = false;
			// 
			// eventTypeLabel
			// 
			this.BindingSource.SetBindingMember(this.eventTypeLabel, "EventDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.EventItemModel)(null)).EventDescription)));
			this.eventTypeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eventTypeLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.eventTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 0, true);
			this.eventTypeLabel.Name = "eventTypeLabel";
			this.eventTypeLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.eventTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 33, true);
			this.eventTypeLabel.TabIndex = 0;
			this.eventTypeLabel.UseMnemonic = false;
			// 
			// eventBannerItemTableLayoutPanel
			// 
			this.eventBannerItemTableLayoutPanel.ColumnCount = 3;
			this.eventBannerItemTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44F));
			this.eventBannerItemTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13F));
			this.eventBannerItemTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43F));
			this.eventBannerItemTableLayoutPanel.Controls.Add(this.eventDateLabel, 0, 0);
			this.eventBannerItemTableLayoutPanel.Controls.Add(this.eventTipPictureBox, 1, 0);
			this.eventBannerItemTableLayoutPanel.Controls.Add(this.eventTypeLabel, 2, 0);
			this.eventBannerItemTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eventBannerItemTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 0, true);
			this.eventBannerItemTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, 0, 0, 0, true);
			this.eventBannerItemTableLayoutPanel.Name = "eventBannerItemTableLayoutPanel";
			this.eventBannerItemTableLayoutPanel.RowCount = 1;
			this.eventBannerItemTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.eventBannerItemTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 33, true);
			this.eventBannerItemTableLayoutPanel.TabIndex = 0;
			// 
			// EventsBannerItemControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.eventBannerItemTableLayoutPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "EventsBannerItemControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, 0, 0, 0, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 33, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.eventTipPictureBox)).EndInit();
			this.eventBannerItemTableLayoutPanel.ResumeLayout(false);
			this.eventBannerItemTableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZLabel eventDateLabel;
		internal ZArchitecture.GUI.ZPictureBox eventTipPictureBox;
		internal Enterprise.ZArchitecture.ZLabel eventTypeLabel;
		internal CargoWise.Windows.UI.KTableLayoutPanel eventBannerItemTableLayoutPanel;
	}
}
