namespace Enterprise.Freight.Agency.GUI
{
	partial class SplitBookingsForm
	{
		new void InitializeComponent()
		{
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			bottomPanel = new CargoWise.Windows.UI.KPanel();
			okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			splitBookingControl = new Enterprise.Freight.Agency.GUI.SplitBookingsControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 599, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.SplitBookingsHeader);
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(okButton);
			bottomPanel.Controls.Add(this.cancelButton);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 570, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 29, true);
			bottomPanel.TabIndex = 1;
			// 
			// okButton
			// 
			okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			okButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SplitBookingsForm|1274820a-d8a4-4ad5-b553-3328031ce441", "Save");
			okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 3, true);
			okButton.Name = "okButton";
			okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			okButton.TabIndex = 0;
			okButton.UseVisualStyleBackColor = true;
			okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SplitBookingsForm|2d3ed50c-5632-407d-99a0-1c9de73c38a1", "Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(754, 3, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 0;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// splitBookingControl
			// 
			this.BindingSource.SetBindingMember(splitBookingControl, ".");
			splitBookingControl.Dock = System.Windows.Forms.DockStyle.Fill;
			splitBookingControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			splitBookingControl.Name = "splitBookingControl";
			splitBookingControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 570, true);
			splitBookingControl.TabIndex = 1;
			// 
			// SplitBookingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 623, true);
			this.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SplitBookingsForm|77a61a08-b0c0-468a-9ffb-b4f9f54d4509", "Split Booking");
			this.Controls.Add(splitBookingControl);
			this.Controls.Add(bottomPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.SplitBookingsHeader);
			this.Name = "SplitBookingsForm";
			this.Text = "SplitBookingsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(bottomPanel, 0);
			this.Controls.SetChildIndex(splitBookingControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		CargoWise.Windows.UI.KPanel bottomPanel;
		Enterprise.ZArchitecture.GUI.ZButton okButton;
		Enterprise.Freight.Agency.GUI.SplitBookingsControl splitBookingControl;
	}
}
