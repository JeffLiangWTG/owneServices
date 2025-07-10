namespace Enterprise.MasterFiles.GUI
{
	partial class EventsUnavailableMessageControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.messageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.waitingMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.loadingPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.loadingPicContainer = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.containerKTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.loadingPictureBox)).BeginInit();
			this.loadingPicContainer.SuspendLayout();
			this.containerKTableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.EventsBannerModel);
			// 
			// messageLabel
			// 
			this.messageLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.messageLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.messageLabel, "ErrorMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.EventsBannerModel)(null)).ErrorMessage)));
			this.messageLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.messageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 208, true);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 16, true);
			this.messageLabel.TabIndex = 1;
			this.messageLabel.UseMnemonic = false;
			// 
			// waitingMessageLabel
			// 
			this.waitingMessageLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.waitingMessageLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.waitingMessageLabel, "LoadingEvents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.EventsBannerModel)(null)).LoadingEvents)));
			this.waitingMessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.waitingMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 96, true);
			this.waitingMessageLabel.Name = "waitingMessageLabel";
			this.waitingMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.waitingMessageLabel.TabIndex = 4;
			this.waitingMessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.waitingMessageLabel.UseMnemonic = false;
			// 
			// loadingPictureBox
			// 
			this.loadingPictureBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.loadingPictureBox.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.events_loading;
			this.loadingPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.loadingPictureBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.loadingPictureBox.Name = "loadingPictureBox";
			this.loadingPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 92, true);
			this.loadingPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.loadingPictureBox.TabIndex = 3;
			this.loadingPictureBox.TabStop = false;
			// 
			// loadingPicContainer
			// 
			this.loadingPicContainer.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.loadingPicContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.loadingPicContainer.Controls.Add(this.loadingPictureBox, 0, 0);
			this.loadingPicContainer.Controls.Add(this.waitingMessageLabel, 0, 1);
			this.loadingPicContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 15, true);
			this.loadingPicContainer.Name = "loadingPicContainer";
			this.loadingPicContainer.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.loadingPicContainer.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.loadingPicContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 114, true);
			this.loadingPicContainer.TabIndex = 2;
			this.loadingPicContainer.Visible = false;
			// 
			// containerKTableLayoutPanel
			// 
			this.containerKTableLayoutPanel.ColumnCount = 1;
			this.containerKTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.containerKTableLayoutPanel.Controls.Add(this.messageLabel, 0, 0);
			this.containerKTableLayoutPanel.Controls.Add(this.loadingPicContainer, 0, 0);
			this.containerKTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerKTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.containerKTableLayoutPanel.Name = "containerKTableLayoutPanel";
			this.containerKTableLayoutPanel.RowCount = 1;
			this.containerKTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.containerKTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 289, true);
			this.containerKTableLayoutPanel.TabIndex = 0;
			// 
			// EventsUnavailableMessageControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.containerKTableLayoutPanel);
			this.Name = "EventsUnavailableMessageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 289, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.loadingPictureBox)).EndInit();
			this.loadingPicContainer.ResumeLayout(false);
			this.loadingPicContainer.PerformLayout();
			this.containerKTableLayoutPanel.ResumeLayout(false);
			this.containerKTableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel containerKTableLayoutPanel;
		private ZArchitecture.ZLabel messageLabel;
		private ZArchitecture.ZLabel waitingMessageLabel;
		private CargoWise.Windows.UI.KTableLayoutPanel loadingPicContainer;
		private ZArchitecture.GUI.ZPictureBox loadingPictureBox;
	}
}
